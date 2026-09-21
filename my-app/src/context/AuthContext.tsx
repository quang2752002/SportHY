'use client';

import React, { createContext, useContext, useState, useEffect, useCallback } from 'react';
import { useRouter } from 'next/navigation';
import { jwtDecode } from 'jwt-decode';
import { User, LoginRequest, RegisterRequest, AuthResponse } from '../types/auth';
import { api, tokenStorage } from '../lib/api';

import { Role, PermissionGroup } from '../types';
import { roleService } from '../services/roleService';

interface JwtPayload {
  sub?: string;
  nameid?: string;
  username?: string;
  fullName?: string;
  email?: string;
  role?: string | string[];
  permission?: string | string[];
  exp?: number;
}

interface AuthContextType {
  user: User | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  permissionGroups: PermissionGroup[];
  login: (credentials: LoginRequest) => Promise<User | null>;
  register: (data: RegisterRequest) => Promise<User | null>;
  logout: () => Promise<void>;
  hasPermission: (permission: string) => boolean;
  hasRole: (role: string) => boolean;
  can: (module: string, action: string) => boolean;
  loadPermissionTree: () => Promise<void>;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

// Hàm giải mã trực tiếp từ chuỗi Access Token JWT
function parseUserFromToken(token: string): User | null {
  try {
    const rawDecoded = jwtDecode<Record<string, any>>(token);

    // Chuẩn hóa roles từ jwt (hỗ trợ cả claim 'role' thông thường và claim URI của .NET Identity)
    const rawRole =
      rawDecoded['role'] ||
      rawDecoded['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] ||
      rawDecoded['roles'];

    const roles: string[] = [];
    if (rawRole) {
      if (Array.isArray(rawRole)) roles.push(...rawRole.map(String));
      else roles.push(String(rawRole));
    }

    // Chuẩn hóa permissions từ jwt
    const rawPerms = rawDecoded['permission'] || rawDecoded['permissions'];
    const permissions: string[] = [];
    if (rawPerms) {
      if (Array.isArray(rawPerms)) permissions.push(...rawPerms.map(String));
      else permissions.push(String(rawPerms));
    }

    const userId = Number(
      rawDecoded.sub ||
      rawDecoded.nameid ||
      rawDecoded['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'] ||
      0
    );

    const rawDonViId =
      rawDecoded['donViId'] ||
      rawDecoded['DonViId'] ||
      rawDecoded['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/donViId'];
    const donViId =
      rawDonViId !== undefined && rawDonViId !== null && rawDonViId !== ''
        ? Number(rawDonViId)
        : null;

    const rawTrongTaiId = rawDecoded['trongTaiId'] || rawDecoded['TrongTaiId'];
    const trongTaiId =
      rawTrongTaiId !== undefined && rawTrongTaiId !== null && rawTrongTaiId !== ''
        ? Number(rawTrongTaiId)
        : null;

    const rawThuKyId = rawDecoded['thuKyId'] || rawDecoded['ThuKyId'];
    const thuKyId =
      rawThuKyId !== undefined && rawThuKyId !== null && rawThuKyId !== ''
        ? Number(rawThuKyId)
        : null;

    return {
      id: userId,
      username:
        rawDecoded.username ||
        rawDecoded['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'] ||
        '',
      fullName: rawDecoded.fullName || '',
      email: rawDecoded.email || '',
      roles,
      permissions,
      donViId,
      trongTaiId,
      thuKyId,
    };
  } catch {
    return null;
  }
}

export function AuthProvider({ children }: { children: React.ReactNode }) {
  const [user, setUser] = useState<User | null>(null);
  const [isLoading, setIsLoading] = useState<boolean>(true);
  const [permissionGroups, setPermissionGroups] = useState<PermissionGroup[]>([]);
  const router = useRouter();

  // Tải danh mục phân nhóm quyền từ Backend API: /api/roles/permissions-tree
  const loadPermissionTree = useCallback(async () => {
    try {
      const data = await roleService.getPermissionsTree();
      setPermissionGroups(data || []);
    } catch {
      // Bỏ qua lỗi nếu chưa có quyền truy cập
    }
  }, []);

  // Khởi tạo kiểm tra token lưu trong storage khi mở app
  useEffect(() => {
    const token = tokenStorage.getAccessToken();
    if (token) {
      const parsedUser = parseUserFromToken(token);
      if (parsedUser) {
        setUser(parsedUser);
        loadPermissionTree();
      } else {
        tokenStorage.clearTokens();
        setUser(null);
      }
    }
    setIsLoading(false);

    const handleUnauthorized = () => {
      setUser(null);
      if (typeof window !== 'undefined') {
        const path = window.location.pathname;
        const isPublicRoute = path === '/' || path.startsWith('/login') || path.startsWith('/giai-dau');
        if (!isPublicRoute) {
          router.push('/login');
        }
      }
    };

    window.addEventListener('auth:unauthorized', handleUnauthorized);
    return () => {
      window.removeEventListener('auth:unauthorized', handleUnauthorized);
    };
  }, [router]);

  const login = async (credentials: LoginRequest): Promise<User | null> => {
    setIsLoading(true);
    try {
      const { data } = await api.post<AuthResponse>('/api/auth/login', credentials);
      tokenStorage.setTokens(data.accessToken, data.refreshToken);

      // Giải mã trực tiếp từ token vừa nhận
      const parsedUser = parseUserFromToken(data.accessToken);
      if (parsedUser && data.donViId && !parsedUser.donViId) {
        parsedUser.donViId = data.donViId;
      }
      setUser(parsedUser);
      loadPermissionTree();
      return parsedUser;
    } finally {
      setIsLoading(false);
    }
  };

  const register = async (dataPayload: RegisterRequest): Promise<User | null> => {
    setIsLoading(true);
    try {
      const { data } = await api.post<AuthResponse>('/api/auth/register', dataPayload);
      tokenStorage.setTokens(data.accessToken, data.refreshToken);

      const parsedUser = parseUserFromToken(data.accessToken);
      if (parsedUser && data.donViId && !parsedUser.donViId) {
        parsedUser.donViId = data.donViId;
      }
      setUser(parsedUser);
      loadPermissionTree();
      return parsedUser;
    } finally {
      setIsLoading(false);
    }
  };

  const logout = async () => {
    try {
      await api.post('/api/auth/logout').catch(() => { });
    } finally {
      tokenStorage.clearTokens();
      setUser(null);
      setPermissionGroups([]);
      router.push('/login');
    }
  };

  const hasPermission = (permission: string): boolean => {
    if (!user) return false;
    // Role Admin luôn có toàn quyền
    if (user.roles.includes('Admin')) return true;
    return user.permissions.includes(permission);
  };

  const can = (module: string, action: string): boolean => {
    if (!user) return false;
    if (user.roles.includes('Admin')) return true;
    return user.permissions.includes(`Permissions.${module}.${action}`);
  };

  const hasRole = (role: string): boolean => {
    if (!user) return false;
    return user.roles.some((r) => r.toLowerCase() === role.toLowerCase());
  };

  return (
    <AuthContext.Provider
      value={{
        user,
        isAuthenticated: !!user,
        isLoading,
        permissionGroups,
        login,
        register,
        logout,
        hasPermission,
        hasRole,
        can,
        loadPermissionTree,
      }}
    >
      <ToastProvider>
        {children}
      </ToastProvider>
    </AuthContext.Provider>
  );
}

export function useAuth() {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
}

// ==================== TOAST POPUP NOTIFICATION SYSTEM ====================
export type ToastType = 'success' | 'error' | 'warning' | 'info';

export interface ToastItem {
  id: string;
  type: ToastType;
  title?: string;
  message: string;
  duration?: number;
}

interface ToastContextType {
  toast: (message: string, type?: ToastType, title?: string, duration?: number) => void;
  success: (message: string, title?: string) => void;
  error: (message: string, title?: string) => void;
  warning: (message: string, title?: string) => void;
  info: (message: string, title?: string) => void;
}

const ToastContext = createContext<ToastContextType | undefined>(undefined);

export function ToastProvider({ children }: { children: React.ReactNode }) {
  const [toasts, setToasts] = useState<ToastItem[]>([]);

  const removeToast = useCallback((id: string) => {
    setToasts((prev) => prev.filter((t) => t.id !== id));
  }, []);

  const toast = useCallback(
    (message: string, type: ToastType = 'info', title?: string, duration = 3800) => {
      const id = `${Date.now()}_${Math.random().toString(36).substring(2, 9)}`;
      const newToast: ToastItem = { id, type, title, message, duration };

      setToasts((prev) => [...prev, newToast]);

      if (duration > 0) {
        setTimeout(() => {
          removeToast(id);
        }, duration);
      }
    },
    [removeToast]
  );

  const success = useCallback(
    (message: string, title: string = 'Thành công') => toast(message, 'success', title),
    [toast]
  );
  const error = useCallback(
    (message: string, title: string = 'Lỗi bắt buộc') => toast(message, 'error', title, 4500),
    [toast]
  );
  const warning = useCallback(
    (message: string, title: string = 'Cảnh báo') => toast(message, 'warning', title),
    [toast]
  );
  const info = useCallback(
    (message: string, title: string = 'Thông báo') => toast(message, 'info', title),
    [toast]
  );

  return (
    <ToastContext.Provider value={{ toast, success, error, warning, info }}>
      {children}
      {/* Toast Popup Container - Nổi góc trên bên phải */}
      <div
        className="toast-container position-fixed top-0 end-0 p-3"
        style={{ zIndex: 999999, pointerEvents: 'none' }}
      >
        {toasts.map((t) => {
          const bgStyle = {
            success: { bg: '#ffffff', border: '#bbf7d0', iconBg: '#dcfce7', iconColor: '#16a34a', barBg: '#16a34a' },
            error: { bg: '#ffffff', border: '#fecaca', iconBg: '#fee2e2', iconColor: '#dc2626', barBg: '#dc2626' },
            warning: { bg: '#ffffff', border: '#fef08a', iconBg: '#fef9c3', iconColor: '#ca8a04', barBg: '#ca8a04' },
            info: { bg: '#ffffff', border: '#bfdbfe', iconBg: '#dbeafe', iconColor: '#2563eb', barBg: '#2563eb' },
          }[t.type];

          const iconClass = {
            success: 'bi-check-circle-fill',
            error: 'bi-exclamation-octagon-fill',
            warning: 'bi-exclamation-triangle-fill',
            info: 'bi-info-circle-fill',
          }[t.type];

          return (
            <div
              key={t.id}
              className="toast show border shadow-lg rounded-4 mb-3 overflow-hidden position-relative"
              role="alert"
              aria-live="assertive"
              aria-atomic="true"
              style={{
                pointerEvents: 'auto',
                minWidth: '320px',
                maxWidth: '420px',
                backgroundColor: bgStyle.bg,
                borderColor: bgStyle.border,
                transition: 'all 0.2s ease',
              }}
            >
              {/* Top Accent Color Line */}
              <div style={{ height: '4px', backgroundColor: bgStyle.barBg }} />
              
              <div className="d-flex align-items-start p-3">
                <div
                  className="rounded-circle d-flex align-items-center justify-content-center me-3 flex-shrink-0"
                  style={{
                    width: '36px',
                    height: '36px',
                    backgroundColor: bgStyle.iconBg,
                    color: bgStyle.iconColor,
                  }}
                >
                  <i className={`bi ${iconClass} fs-5`}></i>
                </div>

                <div className="flex-grow-1">
                  {t.title && (
                    <div className="fw-bold small text-dark mb-0.5" style={{ fontSize: '0.9rem' }}>
                      {t.title}
                    </div>
                  )}
                  <div className="small text-secondary" style={{ lineHeight: '1.45', fontSize: '0.83rem' }}>
                    {t.message}
                  </div>
                </div>

                <button
                  type="button"
                  className="btn-close ms-2 mt-0.5"
                  style={{ fontSize: '0.75rem' }}
                  onClick={() => removeToast(t.id)}
                  aria-label="Close"
                ></button>
              </div>
            </div>
          );
        })}
      </div>
    </ToastContext.Provider>
  );
}

export function useToast() {
  const context = useContext(ToastContext);
  if (!context) {
    throw new Error('useToast must be used within an AuthProvider/ToastProvider');
  }
  return context;
}
