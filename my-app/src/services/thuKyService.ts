import { api } from '../lib/api';
import { PagedResult } from '../types/common';
import { ThuKy, CreateUpdateThuKy } from '../types/thuKy';

export const thuKyService = {
  /** Lấy danh sách thư ký có phân trang, tìm kiếm và lọc trạng thái */
  getPaged: async (params?: {
    pageIndex?: number;
    pageSize?: number;
    keyword?: string;
    trangThai?: boolean;
  }) => {
    const res = await api.get<PagedResult<ThuKy>>('/api/thuky/paged', { params });
    return res.data;
  },

  /** Lấy tất cả thư ký đang hoạt động */
  getAll: async () => {
    const res = await api.get<ThuKy[]>('/api/thuky');
    return res.data;
  },

  /** Lấy chi tiết thư ký theo ID */
  getById: async (id: number) => {
    const res = await api.get<ThuKy>(`/api/thuky/${id}`);
    return res.data;
  },

  /** Tạo thư ký mới */
  create: async (data: CreateUpdateThuKy) => {
    const res = await api.post<ThuKy>('/api/thuky', data);
    return res.data;
  },

  /** Cập nhật thông tin thư ký */
  update: async (id: number, data: CreateUpdateThuKy) => {
    const res = await api.put<ThuKy>(`/api/thuky/${id}`, data);
    return res.data;
  },

  /** Xóa thư ký */
  delete: async (id: number) => {
    const res = await api.delete(`/api/thuky/${id}`);
    return res.data;
  },
};
