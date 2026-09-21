import { api } from '../lib/api';
import {
  BangDau,
  CreateUpdateBangDau,
  AssignTeamsToBang,
  AutoDistributeBang,
} from '../types/bangDau';

export const bangDauService = {
  /** Lấy danh sách bảng đấu của môn thể thao trong giải */
  getAll: async (giaiDauMonTheThaoId?: number) => {
    const res = await api.get<BangDau[]>('/api/bangdau', { params: { giaiDauMonTheThaoId } });
    return res.data;
  },

  /** Lấy chi tiết bảng đấu */
  getById: async (id: number) => {
    const res = await api.get<BangDau>(`/api/bangdau/${id}`);
    return res.data;
  },

  /** Tạo mới bảng đấu */
  create: async (data: CreateUpdateBangDau) => {
    const res = await api.post<BangDau>('/api/bangdau', data);
    return res.data;
  },

  /** Cập nhật bảng đấu */
  update: async (id: number, data: CreateUpdateBangDau) => {
    const res = await api.put<BangDau>(`/api/bangdau/${id}`, data);
    return res.data;
  },

  /** Xóa bảng đấu */
  delete: async (id: number) => {
    const res = await api.delete(`/api/bangdau/${id}`);
    return res.data;
  },

  /** Gán danh sách đội vào bảng */
  assignTeams: async (data: AssignTeamsToBang) => {
    const res = await api.post('/api/bangdau/assign-teams', data);
    return res.data;
  },

  /** Tự động chia bảng ngẫu nhiên cho các đội */
  autoDistribute: async (data: AutoDistributeBang) => {
    const res = await api.post<BangDau[]>('/api/bangdau/auto-distribute', data);
    return res.data;
  },
};
