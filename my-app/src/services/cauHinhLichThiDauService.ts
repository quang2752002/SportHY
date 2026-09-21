import { api } from '../lib/api';
import { CauHinhLichThiDau, CreateUpdateCauHinhLichThiDauRequest } from '../types/cauHinhLichThiDau';

export const cauHinhLichThiDauService = {
  /** Lấy cấu hình xếp lịch theo ID môn thể thao */
  getByMonTheThao: async (monTheThaoId: number) => {
    try {
      const res = await api.get<CauHinhLichThiDau>(`/api/cauhinhlichthidau/${monTheThaoId}`);
      return res.data;
    } catch {
      return null;
    }
  },

  /** Tạo hoặc cập nhật (Upsert) cấu hình xếp lịch cho môn thể thao */
  upsert: async (data: CreateUpdateCauHinhLichThiDauRequest) => {
    const res = await api.post<CauHinhLichThiDau>('/api/cauhinhlichthidau', data);
    return res.data;
  },

  /** Xóa cấu hình xếp lịch (Reset về mặc định hệ thống) */
  delete: async (monTheThaoId: number) => {
    const res = await api.delete(`/api/cauhinhlichthidau/${monTheThaoId}`);
    return res.data;
  },
};
