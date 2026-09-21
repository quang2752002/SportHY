export interface Role {
  id: number;
  name: string;
  permissions: string[];
}

export interface PermissionItem {
  name: string;
  value: string;
  description?: string;
}

export interface PermissionGroup {
  groupName: string;
  description: string;
  permissions: PermissionItem[];
}

export interface UpdateRolePermissionsPayload {
  roleName: string;
  permissions: string[];
}

export interface UserManagement {
  id: number;
  username: string;
  email: string;
  fullName: string;
  phoneNumber?: string;
  roles: string[];
  donViId?: number | null;
  tenDonVi?: string | null;
  trongTaiId?: number | null;
  tenTrongTai?: string | null;
  thuKyId?: number | null;
  tenThuKy?: string | null;
  createdAt?: string;
}

export interface CreateUserPayload {
  username: string;
  email: string;
  password: string;
  fullName: string;
  phoneNumber?: string;
  role: string;
  donViId?: number | null;
  trongTaiId?: number | null;
  thuKyId?: number | null;
}

export interface UpdateUserRolePayload {
  userId: number;
  role: string;
  donViId?: number | null;
  trongTaiId?: number | null;
  thuKyId?: number | null;
}

export interface LookupItem {
  id: number;
  name: string;
  code?: string;
}

export interface MapSources {
  donVis: LookupItem[];
  trongTais: LookupItem[];
  thuKys: LookupItem[];
}
