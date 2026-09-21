import { api } from '../lib/api';
import { VongDau, CreateUpdateVongDau } from '../types/vongDau';

export const vongDauService = {
  /** Lấy danh sách vòng đấu của môn thể thao trong giải */
  getAll: async (giaiDauMonTheThaoId?: number) => {
    const res = await api.get<VongDau[]>('/api/vongdau', { params: { giaiDauMonTheThaoId } });
    return res.data;
  },

  /** Lấy chi tiết vòng đấu */
  getById: async (id: number) => {
    const res = await api.get<VongDau>(`/api/vongdau/${id}`);
    return res.data;
  },

  /** Tạo mới vòng đấu */
  create: async (data: CreateUpdateVongDau) => {
    const res = await api.post<VongDau>('/api/vongdau', data);
    return res.data;
  },

  /** Cập nhật vòng đấu */
  update: async (id: number, data: CreateUpdateVongDau) => {
    const res = await api.put<VongDau>(`/api/vongdau/${id}`, data);
    return res.data;
  },

  /** Xóa vòng đấu */
  delete: async (id: number) => {
    const res = await api.delete(`/api/vongdau/${id}`);
    return res.data;
  },
};
