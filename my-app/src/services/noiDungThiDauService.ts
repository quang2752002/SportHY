import { api } from '../lib/api';
import { NoiDungThiDau, CreateUpdateNoiDungThiDau } from '../types/noiDungThiDau';

export const noiDungThiDauService = {
  /** Lấy danh sách nội dung thi đấu, lọc theo giải đấu hoặc GiaiDauMonTheThaoId */
  getAll: async (params?: {
    giaiDauId?: number;
    giaiDauMonTheThaoId?: number;
  }) => {
    const res = await api.get<NoiDungThiDau[]>('/api/noidungthidau', { params });
    return res.data;
  },

  getById: async (id: number) => {
    const res = await api.get<NoiDungThiDau>(`/api/noidungthidau/${id}`);
    return res.data;
  },

  /** Tạo mới nội dung thi đấu */
  create: async (data: CreateUpdateNoiDungThiDau) => {
    const res = await api.post<NoiDungThiDau>('/api/noidungthidau', data);
    return res.data;
  },

  /** Cập nhật nội dung thi đấu */
  update: async (id: number, data: CreateUpdateNoiDungThiDau) => {
    const res = await api.put<NoiDungThiDau>(`/api/noidungthidau/${id}`, data);
    return res.data;
  },

  /** Xóa nội dung thi đấu */
  delete: async (id: number) => {
    const res = await api.delete(`/api/noidungthidau/${id}`);
    return res.data;
  },
};
