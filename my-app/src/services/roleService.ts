import { api } from '../lib/api';
import { Role, PermissionGroup, UpdateRolePermissionsPayload } from '@/types';

export const roleService = {
  // Lấy danh sách toàn bộ các Role kèm permissions
  getRoles: async () => {
    const res = await api.get<Role[]>('/api/roles');
    return res.data;
  },

  // Lấy cây phân loại Permissions theo nhóm
  getPermissionsTree: async () => {
    const res = await api.get<PermissionGroup[]>('/api/roles/permissions-tree');
    return res.data;
  },

  // Cập nhật danh sách permissions cho 1 role
  updateRolePermissions: async (data: UpdateRolePermissionsPayload) => {
    const res = await api.post<{ message: string }>('/api/roles/update-permissions', data);
    return res.data;
  },

  // Lấy danh sách toàn bộ người dùng
  getUsers: async () => {
    const res = await api.get<import('@/types').UserManagement[]>('/api/roles/users');
    return res.data;
  },

  // Cấp mới tài khoản và gán role
  createUserWithRole: async (data: import('@/types').CreateUserPayload) => {
    const res = await api.post<{ message: string }>('/api/roles/create-user', data);
    return res.data;
  },

  // Cập nhật đổi vai trò cho tài khoản
  updateUserRole: async (data: import('@/types').UpdateUserRolePayload) => {
    const res = await api.post<{ message: string }>('/api/roles/update-user-role', data);
    return res.data;
  },

  // Lấy danh sách ánh xạ Đơn vị & Trọng tài để cấp tài khoản
  getMapSources: async () => {
    const res = await api.get<import('@/types').MapSources>('/api/roles/map-sources');
    return res.data;
  },
};
