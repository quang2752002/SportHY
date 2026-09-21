import { api } from '../lib/api';
import { PagedResult } from '../types/common';
import { DonVi, CreateUpdateDonVi } from '../types/donVi';

export const donViService = {
  /** Lấy danh sách đơn vị có phân trang, tìm kiếm, lọc khối và trạng thái */
  getPaged: async (params?: {
    pageIndex?: number;
    pageSize?: number;
    keyword?: string;
    khoiId?: number;
    trangThai?: boolean;
  }) => {
    const res = await api.get<PagedResult<DonVi>>('/api/donvi/paged', { params });
    return res.data;
  },

  /** Lấy tất cả đơn vị hoạt động, có thể lọc theo khối */
  getAll: async (khoiId?: number) => {
    const res = await api.get<DonVi[]>('/api/donvi', {
      params: khoiId ? { khoiId } : undefined,
    });
    return res.data;
  },

  /** Lấy chi tiết đơn vị theo ID */
  getById: async (id: number) => {
    const res = await api.get<DonVi>(`/api/donvi/${id}`);
    return res.data;
  },

  /** Tạo đơn vị mới */
  create: async (data: CreateUpdateDonVi) => {
    const res = await api.post<DonVi>('/api/donvi', data);
    return res.data;
  },

  /** Cập nhật đơn vị */
  update: async (id: number, data: CreateUpdateDonVi) => {
    const res = await api.put<DonVi>(`/api/donvi/${id}`, data);
    return res.data;
  },

  /** Xóa đơn vị */
  delete: async (id: number) => {
    const res = await api.delete(`/api/donvi/${id}`);
    return res.data;
  },

  /** Upload hình ảnh/logo của Đơn vị lưu vào wwwroot/don-vi */
  uploadImage: async (file: File) => {
    const formData = new FormData();
    formData.append('file', file);
    const res = await api.post<{ url: string }>('/api/donvi/upload-image', formData, {
      headers: {
        'Content-Type': 'multipart/form-data',
      },
    });
    return res.data;
  },
};
