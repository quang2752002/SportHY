import { api } from '../lib/api';
import { PagedResult } from '../types/common';
import { MonTheThao, CreateUpdateMonTheThao } from '../types/monTheThao';

export const monTheThaoService = {
  /** Lấy danh sách môn thể thao có phân trang, tìm kiếm, lọc danh mục và trạng thái */
  getPaged: async (params?: {
    pageIndex?: number;
    pageSize?: number;
    keyword?: string;
    danhMucId?: number;
    trangThai?: boolean;
    gioiTinh?: string;
  }) => {
    const res = await api.get<PagedResult<MonTheThao>>('/api/monthethao/paged', { params });
    return res.data;
  },

  /** Lấy tất cả môn thể thao hoạt động, có thể lọc theo danh mục hoặc giới tính */
  getAll: async (danhMucId?: number, gioiTinh?: string) => {
    const res = await api.get<MonTheThao[]>('/api/monthethao', {
      params: {
        ...(danhMucId ? { danhMucId } : {}),
        ...(gioiTinh ? { gioiTinh } : {}),
      },
    });
    return res.data;
  },

  /** Lấy chi tiết môn thể thao theo ID */
  getById: async (id: number) => {
    const res = await api.get<MonTheThao>(`/api/monthethao/${id}`);
    return res.data;
  },

  /** Tạo môn thể thao mới */
  create: async (data: CreateUpdateMonTheThao) => {
    const res = await api.post<MonTheThao>('/api/monthethao', data);
    return res.data;
  },

  /** Cập nhật môn thể thao */
  update: async (id: number, data: CreateUpdateMonTheThao) => {
    const res = await api.put<MonTheThao>(`/api/monthethao/${id}`, data);
    return res.data;
  },

  /** Xóa môn thể thao */
  delete: async (id: number) => {
    const res = await api.delete(`/api/monthethao/${id}`);
    return res.data;
  },
};
