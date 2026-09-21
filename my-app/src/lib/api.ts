import axios, { AxiosError, InternalAxiosRequestConfig } from 'axios';
import { AuthResponse } from '../types/auth';

const TOKEN_KEY = 'dms_access_token';
const REFRESH_KEY = 'dms_refresh_token';

// Helper lưu cookie để Next.js Middleware (chạy ở Edge/Server) có thể đọc được
const setCookie = (name: string, value: string, days: number = 7) => {
  if (typeof document === 'undefined') return;
  const expires = new Date(Date.now() + days * 864e5).toUTCString();
  document.cookie = `${name}=${encodeURIComponent(value)}; expires=${expires}; path=/; SameSite=Lax`;
};

const deleteCookie = (name: string) => {
  if (typeof document === 'undefined') return;
  document.cookie = `${name}=; expires=Thu, 01 Jan 1970 00:00:00 GMT; path=/; SameSite=Lax`;
};

export const tokenStorage = {
  getAccessToken: (): string | null =>
    typeof window !== 'undefined' ? localStorage.getItem(TOKEN_KEY) : null,
  getRefreshToken: (): string | null =>
    typeof window !== 'undefined' ? localStorage.getItem(REFRESH_KEY) : null,
  setTokens: (accessToken: string, refreshToken: string) => {
    if (typeof window === 'undefined') return;
    localStorage.setItem(TOKEN_KEY, accessToken);
    localStorage.setItem(REFRESH_KEY, refreshToken);
    // Đồng bộ vào cookie cho Next.js middleware
    setCookie(TOKEN_KEY, accessToken);
    setCookie(REFRESH_KEY, refreshToken);
  },
  clearTokens: () => {
    if (typeof window === 'undefined') return;
    localStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem(REFRESH_KEY);
    // Xóa cookie
    deleteCookie(TOKEN_KEY);
    deleteCookie(REFRESH_KEY);
  },
};

const API_BASE_URL = process.env.NEXT_PUBLIC_API_URL || 'https://localhost:7085';

// Bỏ qua kiểm tra chứng chỉ SSL tự ký trên server Node.js khi gọi https localhost trong môi trường dev
let httpsAgent: any = undefined;
if (typeof window === 'undefined') {
  try {
    const https = require('https');
    httpsAgent = new https.Agent({
      rejectUnauthorized: false,
    });
  } catch {
    // Không thể khởi tạo https agent
  }
}

export const api = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
  httpsAgent,
});

// Request Interceptor: Tự động gắn Bearer Token
api.interceptors.request.use((config: InternalAxiosRequestConfig) => {
  const token = tokenStorage.getAccessToken();
  if (token && config.headers) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

// Xử lý refresh token song song
let isRefreshing = false;
let failedQueue: Array<{
  resolve: (value?: unknown) => void;
  reject: (reason?: unknown) => void;
}> = [];

const processQueue = (error: any, token: string | null = null) => {
  failedQueue.forEach((prom) => {
    if (error) {
      prom.reject(error);
    } else {
      prom.resolve(token);
    }
  });
  failedQueue = [];
};

// Response Interceptor: Bắt mã 401 và tự động refresh token
api.interceptors.response.use(
  (response) => response,
  async (error: AxiosError) => {
    const originalRequest = error.config as InternalAxiosRequestConfig & {
      _retry?: boolean;
    };

    if (error.response?.status === 401 && !originalRequest._retry) {
      if (isRefreshing) {
        return new Promise((resolve, reject) => {
          failedQueue.push({ resolve, reject });
        })
          .then((token) => {
            if (originalRequest.headers) {
              originalRequest.headers.Authorization = `Bearer ${token}`;
            }
            return api(originalRequest);
          })
          .catch((err) => Promise.reject(err));
      }

      originalRequest._retry = true;
      isRefreshing = true;

      const refreshToken = tokenStorage.getRefreshToken();
      const accessToken = tokenStorage.getAccessToken();

      if (!refreshToken || !accessToken) {
        tokenStorage.clearTokens();
        if (typeof window !== 'undefined') {
          window.dispatchEvent(new CustomEvent('auth:unauthorized'));
        }
        return Promise.reject(error);
      }

      try {
        const { data } = await axios.post<AuthResponse>(
          `${API_BASE_URL}/api/auth/refresh`,
          {
            accessToken,
            refreshToken,
          }
        );

        tokenStorage.setTokens(data.accessToken, data.refreshToken);
        processQueue(null, data.accessToken);

        if (originalRequest.headers) {
          originalRequest.headers.Authorization = `Bearer ${data.accessToken}`;
        }
        return api(originalRequest);
      } catch (refreshError) {
        processQueue(refreshError, null);
        tokenStorage.clearTokens();
        if (typeof window !== 'undefined') {
          window.dispatchEvent(new CustomEvent('auth:unauthorized'));
        }
        return Promise.reject(refreshError);
      } finally {
        isRefreshing = false;
      }
    }

    return Promise.reject(error);
  }
);
