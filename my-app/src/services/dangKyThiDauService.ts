import { api } from '../lib/api';
import { PagedResult } from '../types/common';
import { DangKyThiDau, CreateUpdateDangKyThiDau } from '../types/dangKyThiDau';

export const dangKyThiDauService = {
  /** Lấy danh sách hồ sơ đăng ký thi đấu có phân trang, hỗ trợ tìm kiếm và lọc */
  getPaged: async (params?: {
    pageIndex?: number;
    pageSize?: number;
    keyword?: string;
    giaiDauId?: number;
    giaiDauMonTheThaoId?: number;
    noiDungThiDauId?: number;
    donViId?: number;
    trangThai?: string;
  }) => {
    const query = {
      ...params,
      giaiDauMonTheThaoId: params?.giaiDauMonTheThaoId ?? params?.noiDungThiDauId,
    };
    const res = await api.get<PagedResult<DangKyThiDau>>('/api/dangkythidau/paged', { params: query });
    return res.data;
  },

  /** Lấy toàn bộ danh sách hồ sơ đăng ký thi đấu */
  getAll: async (params?: {
    giaiDauId?: number;
    giaiDauMonTheThaoId?: number;
    noiDungThiDauId?: number;
    donViId?: number;
  }) => {
    const query = {
      ...params,
      giaiDauMonTheThaoId: params?.giaiDauMonTheThaoId ?? params?.noiDungThiDauId,
    };
    const res = await api.get<DangKyThiDau[]>('/api/dangkythidau', { params: query });
    return res.data;
  },

  /** Lấy chi tiết hồ sơ đăng ký */
  getById: async (id: number) => {
    const res = await api.get<DangKyThiDau>(`/api/dangkythidau/${id}`);
    return res.data;
  },

  /** Tạo mới hồ sơ đăng ký thi đấu */
  create: async (data: CreateUpdateDangKyThiDau) => {
    const res = await api.post<DangKyThiDau>('/api/dangkythidau', data);
    return res.data;
  },

  /** Cập nhật hồ sơ đăng ký thi đấu */
  update: async (id: number, data: CreateUpdateDangKyThiDau) => {
    const res = await api.put<DangKyThiDau>(`/api/dangkythidau/${id}`, data);
    return res.data;
  },

  /** Hủy / xóa hồ sơ đăng ký */
  delete: async (id: number) => {
    const res = await api.delete(`/api/dangkythidau/${id}`);
    return res.data;
  },
};
