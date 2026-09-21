import { api } from '../lib/api';
import { PagedResult } from '../types/common';
import { CumSan, CreateUpdateCumSan } from '../types/cumSan';

export const cumSanService = {
  /** Lấy danh sách cụm sân có phân trang, tìm kiếm và lọc trạng thái */
  getPaged: async (params?: {
    pageIndex?: number;
    pageSize?: number;
    keyword?: string;
    trangThai?: boolean;
  }) => {
    const res = await api.get<PagedResult<CumSan>>('/api/cumsan/paged', { params });
    return res.data;
  },

  /** Lấy tất cả cụm sân đang hoạt động */
  getAll: async () => {
    const res = await api.get<CumSan[]>('/api/cumsan');
    return res.data;
  },

  /** Lấy chi tiết cụm sân theo ID */
  getById: async (id: number) => {
    const res = await api.get<CumSan>(`/api/cumsan/${id}`);
    return res.data;
  },

  /** Tạo cụm sân mới */
  create: async (data: CreateUpdateCumSan) => {
    const res = await api.post<CumSan>('/api/cumsan', data);
    return res.data;
  },

  /** Cập nhật cụm sân */
  update: async (id: number, data: CreateUpdateCumSan) => {
    const res = await api.put<CumSan>(`/api/cumsan/${id}`, data);
    return res.data;
  },

  /** Xóa cụm sân */
  delete: async (id: number) => {
    const res = await api.delete(`/api/cumsan/${id}`);
    return res.data;
  },
};
