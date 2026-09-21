import { api } from '../lib/api';
import { PagedResult } from '../types/common';
import { Khoi, CreateUpdateKhoi } from '../types/khoi';

export const khoiService = {
  /** Lấy danh sách khối có phân trang, tìm kiếm và lọc trạng thái */
  getPaged: async (params?: {
    pageIndex?: number;
    pageSize?: number;
    keyword?: string;
    trangThai?: boolean;
  }) => {
    const res = await api.get<PagedResult<Khoi>>('/api/khoi/paged', { params });
    return res.data;
  },

  /** Lấy tất cả khối hoạt động */
  getAll: async () => {
    const res = await api.get<Khoi[]>('/api/khoi');
    return res.data;
  },

  /** Lấy chi tiết khối theo ID */
  getById: async (id: number) => {
    const res = await api.get<Khoi>(`/api/khoi/${id}`);
    return res.data;
  },

  /** Tạo khối mới */
  create: async (data: CreateUpdateKhoi) => {
    const res = await api.post<Khoi>('/api/khoi', data);
    return res.data;
  },

  /** Cập nhật khối */
  update: async (id: number, data: CreateUpdateKhoi) => {
    const res = await api.put<Khoi>(`/api/khoi/${id}`, data);
    return res.data;
  },

  /** Xóa khối */
  delete: async (id: number) => {
    const res = await api.delete(`/api/khoi/${id}`);
    return res.data;
  },
};
