import { api } from '../lib/api';
import { PagedResult } from '../types/common';
import {
  DanhMucMonTheThao,
  CreateUpdateDanhMucMonTheThao,
} from '../types/danhMucMonTheThao';

export const danhMucMonTheThaoService = {
  /** Lấy danh sách danh mục môn có phân trang, tìm kiếm và lọc trạng thái */
  getPaged: async (params?: {
    pageIndex?: number;
    pageSize?: number;
    keyword?: string;
    trangThai?: boolean;
  }) => {
    const res = await api.get<PagedResult<DanhMucMonTheThao>>('/api/danhmucmonthethao/paged', { params });
    return res.data;
  },

  /** Lấy tất cả danh mục môn hoạt động */
  getAll: async () => {
    const res = await api.get<DanhMucMonTheThao[]>('/api/danhmucmonthethao');
    return res.data;
  },

  /** Lấy chi tiết danh mục theo ID */
  getById: async (id: number) => {
    const res = await api.get<DanhMucMonTheThao>(`/api/danhmucmonthethao/${id}`);
    return res.data;
  },

  /** Tạo danh mục mới */
  create: async (data: CreateUpdateDanhMucMonTheThao) => {
    const res = await api.post<DanhMucMonTheThao>('/api/danhmucmonthethao', data);
    return res.data;
  },

  /** Cập nhật danh mục */
  update: async (id: number, data: CreateUpdateDanhMucMonTheThao) => {
    const res = await api.put<DanhMucMonTheThao>(`/api/danhmucmonthethao/${id}`, data);
    return res.data;
  },

  /** Xóa danh mục */
  delete: async (id: number) => {
    const res = await api.delete(`/api/danhmucmonthethao/${id}`);
    return res.data;
  },
};
