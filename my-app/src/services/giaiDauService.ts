import { api } from '../lib/api';
import { PagedResult } from '../types/common';
import { GiaiDau, CreateUpdateGiaiDau, TrangThaiGiaiDau, PhamViGiaiDau } from '../types/giaiDau';

export const giaiDauService = {
  /** Lấy danh sách giải đấu có phân trang, tìm kiếm và lọc trạng thái / phạm vi (hỗ trợ enum) */
  getPaged: async (params?: {
    pageIndex?: number;
    pageSize?: number;
    keyword?: string;
    trangThai?: TrangThaiGiaiDau;
    phamVi?: PhamViGiaiDau;
  }) => {
    const res = await api.get<PagedResult<GiaiDau>>('/api/giaidau/paged', { params });
    return res.data;
  },

  /** Lấy tất cả giải đấu */
  getAll: async () => {
    const res = await api.get<GiaiDau[]>('/api/giaidau');
    return res.data;
  },

  /** Lấy chi tiết giải đấu theo ID */
  getById: async (id: number) => {
    const res = await api.get<GiaiDau>(`/api/giaidau/${id}`);
    return res.data;
  },

  /** Lấy chi tiết giải đấu theo Slug URL (SEO) */
  getBySlug: async (slug: string) => {
    const res = await api.get<GiaiDau>(`/api/giaidau/slug/${slug}`);
    return res.data;
  },

  /** Tạo giải đấu mới */
  create: async (data: CreateUpdateGiaiDau) => {
    const res = await api.post<GiaiDau>('/api/giaidau', data);
    return res.data;
  },

  /** Cập nhật giải đấu */
  update: async (id: number, data: CreateUpdateGiaiDau) => {
    const res = await api.put<GiaiDau>(`/api/giaidau/${id}`, data);
    return res.data;
  },

  /** Xóa giải đấu */
  delete: async (id: number) => {
    const res = await api.delete(`/api/giaidau/${id}`);
    return res.data;
  },

  /** Upload hình ảnh banner lên thư mục root /banner của backend */
  uploadBanner: async (file: File) => {
    const formData = new FormData();
    formData.append('file', file);
    const res = await api.post<{ url: string }>('/api/giaidau/upload-banner', formData, {
      headers: {
        'Content-Type': 'multipart/form-data',
      },
    });
    return res.data;
  },

  /** Upload tệp đính kèm điều lệ giải đấu (PDF, Word, Excel...) lưu vào /dieu-le */
  uploadDieuLeFile: async (file: File) => {
    const formData = new FormData();
    formData.append('file', file);
    const res = await api.post<{ url: string; fileName: string }>('/api/giaidau/upload-dieule', formData, {
      headers: {
        'Content-Type': 'multipart/form-data',
      },
    });
    return res.data;
  },
};
