import { api } from '../lib/api';
import { PagedResult } from '../types/common';
import { TrongTai, CreateUpdateTrongTai } from '../types/trongTai';

export const trongTaiService = {
  /** Lấy danh sách trọng tài có phân trang, tìm kiếm và lọc trạng thái */
  getPaged: async (params?: {
    pageIndex?: number;
    pageSize?: number;
    keyword?: string;
    trangThai?: boolean;
  }) => {
    const res = await api.get<PagedResult<TrongTai>>('/api/trongtai/paged', { params });
    return res.data;
  },

  /** Lấy tất cả trọng tài đang hoạt động */
  getAll: async () => {
    const res = await api.get<TrongTai[]>('/api/trongtai');
    return res.data;
  },

  /** Lấy chi tiết trọng tài theo ID */
  getById: async (id: number) => {
    const res = await api.get<TrongTai>(`/api/trongtai/${id}`);
    return res.data;
  },

  /** Tạo trọng tài mới */
  create: async (data: CreateUpdateTrongTai) => {
    const res = await api.post<TrongTai>('/api/trongtai', data);
    return res.data;
  },

  /** Cập nhật trọng tài */
  update: async (id: number, data: CreateUpdateTrongTai) => {
    const res = await api.put<TrongTai>(`/api/trongtai/${id}`, data);
    return res.data;
  },

  /** Xóa trọng tài */
  delete: async (id: number) => {
    const res = await api.delete(`/api/trongtai/${id}`);
    return res.data;
  },
};
