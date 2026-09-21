import { api } from '../lib/api';
import { PagedResult } from '../types/common';
import { VanDongVien, CreateUpdateVanDongVien } from '../types/vanDongVien';

export const vanDongVienService = {
  /** Lấy danh sách VĐV có phân trang, tìm kiếm, lọc theo đơn vị và trạng thái */
  getPaged: async (params?: {
    pageIndex?: number;
    pageSize?: number;
    keyword?: string;
    donViId?: number;
    trangThai?: boolean;
  }) => {
    const res = await api.get<PagedResult<VanDongVien>>('/api/vandongvien/paged', { params });
    return res.data;
  },

  /** Lấy tất cả VĐV, có thể lọc theo đơn vị */
  getAll: async (donViId?: number) => {
    const res = await api.get<VanDongVien[]>('/api/vandongvien', {
      params: donViId ? { donViId } : undefined,
    });
    return res.data;
  },

  /** Lấy danh sách VĐV của đoàn (gọi endpoint /api/vandongvien/doan hoặc fallback /api/vandongvien như /don-vi/dang-ky/1) */
  getByDoan: async (donViId?: number) => {
    try {
      const res = await api.get<VanDongVien[]>('/api/vandongvien/doan', {
        params: donViId ? { donViId } : undefined,
      });
      return res.data;
    } catch {
      const res = await api.get<VanDongVien[]>('/api/vandongvien', {
        params: donViId ? { donViId } : undefined,
      });
      return res.data;
    }
  },

  /** Lấy chi tiết VĐV theo ID */
  getById: async (id: number) => {
    const res = await api.get<VanDongVien>(`/api/vandongvien/${id}`);
    return res.data;
  },

  /** Tạo VĐV mới */
  create: async (data: CreateUpdateVanDongVien) => {
    const res = await api.post<VanDongVien>('/api/vandongvien', data);
    return res.data;
  },

  /** Cập nhật VĐV */
  update: async (id: number, data: CreateUpdateVanDongVien) => {
    const res = await api.put<VanDongVien>(`/api/vandongvien/${id}`, data);
    return res.data;
  },

  /** Xóa VĐV */
  delete: async (id: number) => {
    const res = await api.delete(`/api/vandongvien/${id}`);
    return res.data;
  },

  /** Upload hình ảnh chân dung VĐV lưu vào wwwroot/vdv */
  uploadAvatar: async (file: File) => {
    const formData = new FormData();
    formData.append('file', file);
    const res = await api.post<{ url: string }>('/api/vandongvien/upload-avatar', formData, {
      headers: {
        'Content-Type': 'multipart/form-data',
      },
    });
    return res.data;
  },
};
