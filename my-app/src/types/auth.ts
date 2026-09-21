export interface User {
  id: number;
  username: string;
  email?: string;
  fullName: string;
  roles: string[];
  permissions: string[];
  donViId?: number | null;
  trongTaiId?: number | null;
  thuKyId?: number | null;
}

export interface AuthResponse {
  accessToken: string;
  refreshToken: string;
  accessTokenExpiry: string;
  userId: number;
  username: string;
  fullName: string;
  role: string;
  permissions: string[];
  donViId?: number | null;
  trongTaiId?: number | null;
  thuKyId?: number | null;
}

export interface LoginRequest {
  username: string;
  password: string;
}

export interface RegisterRequest {
  username: string;
  password: string;
  email: string;
  fullName: string;
  phoneNumber?: string;
}
