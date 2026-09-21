import { api } from '../lib/api';
import { PagedResult } from '../types/common';
import { SanDau, CreateUpdateSanDau } from '../types/sanDau';

export const sanDauService = {
  /** Lấy danh sách sân đấu có phân trang, tìm kiếm và lọc theo cụm sân, môn thể thao, trạng thái */
  getPaged: async (params?: {
    pageIndex?: number;
    pageSize?: number;
    keyword?: string;
    cumSanId?: number;
    monTheThaoId?: number;
    trangThai?: boolean;
  }) => {
    const res = await api.get<PagedResult<SanDau>>('/api/sandau/paged', { params });
    return res.data;
  },

  /** Lấy tất cả sân đấu (có thể lọc theo cụm sân hoặc môn thể thao) */
  getAll: async (cumSanId?: number, monTheThaoId?: number) => {
    const res = await api.get<SanDau[]>('/api/sandau', { params: { cumSanId, monTheThaoId } });
    return res.data;
  },

  /** Lấy chi tiết sân đấu theo ID */
  getById: async (id: number) => {
    const res = await api.get<SanDau>(`/api/sandau/${id}`);
    return res.data;
  },

  /** Tạo sân đấu mới */
  create: async (data: CreateUpdateSanDau) => {
    const res = await api.post<SanDau>('/api/sandau', data);
    return res.data;
  },

  /** Cập nhật sân đấu */
  update: async (id: number, data: CreateUpdateSanDau) => {
    const res = await api.put<SanDau>(`/api/sandau/${id}`, data);
    return res.data;
  },

  /** Xóa sân đấu */
  delete: async (id: number) => {
    const res = await api.delete(`/api/sandau/${id}`);
    return res.data;
  },
};
