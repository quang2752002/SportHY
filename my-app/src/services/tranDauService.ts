import { api } from '../lib/api';
import { PagedResult } from '../types/common';
import {
  TranDau,
  CreateUpdateTranDau,
  AutoScheduleRequest,
  AutoScheduleResult,
  ConflictCheckRequest,
  ConflictCheckResult,
  TournamentConflictReport,
} from '../types/tranDau';

export const tranDauService = {
  /** Lấy danh sách trận đấu có phân trang, tìm kiếm và lọc */
  getPaged: async (params?: {
    pageIndex?: number;
    pageSize?: number;
    keyword?: string;
    giaiDauId?: number;
    giaiDauMonTheThaoId?: number;
    noiDungThiDauId?: number;
    vongDauId?: number;
    bangDauId?: number;
    sanDauId?: number;
    ngay?: string;
    trangThai?: string;
  }) => {
    const query = {
      ...params,
      giaiDauMonTheThaoId: params?.giaiDauMonTheThaoId ?? params?.noiDungThiDauId,
    };
    const res = await api.get<PagedResult<TranDau>>('/api/trandau/paged', { params: query });
    return res.data;
  },

  /** Lấy toàn bộ trận đấu theo bộ lọc */
  getAll: async (params?: {
    giaiDauId?: number;
    giaiDauMonTheThaoId?: number;
    noiDungThiDauId?: number;
    vongDauId?: number;
    bangDauId?: number;
    sanDauId?: number;
    ngay?: string;
  }) => {
    const query = {
      ...params,
      giaiDauMonTheThaoId: params?.giaiDauMonTheThaoId ?? params?.noiDungThiDauId,
    };
    const res = await api.get<TranDau[]>('/api/trandau', { params: query });
    return res.data;
  },

  /** Lấy chi tiết trận đấu */
  getById: async (id: number) => {
    const res = await api.get<TranDau>(`/api/trandau/${id}`);
    return res.data;
  },

  /** Tạo trận đấu mới thủ công */
  create: async (data: CreateUpdateTranDau) => {
    const res = await api.post<TranDau>('/api/trandau', data);
    return res.data;
  },

  /** Cập nhật trận đấu */
  update: async (id: number, data: CreateUpdateTranDau) => {
    const res = await api.put<TranDau>(`/api/trandau/${id}`, data);
    return res.data;
  },

  /** Xóa trận đấu */
  delete: async (id: number) => {
    const res = await api.delete(`/api/trandau/${id}`);
    return res.data;
  },

  /** Xóa toàn bộ lịch thi đấu của môn thể thao trong giải */
  clearByNoiDung: async (giaiDauMonTheThaoId: number) => {
    const res = await api.delete(`/api/trandau/clear-by-giai-dau-mon/${giaiDauMonTheThaoId}`);
    return res.data;
  },

  /** Xếp lịch thi đấu tự động */
  autoSchedule: async (data: AutoScheduleRequest) => {
    const res = await api.post<AutoScheduleResult>('/api/trandau/auto-generate', data);
    return res.data;
  },

  /** Kiểm tra xung đột lịch sân, trọng tài, đội & VĐV */
  checkConflict: async (data: ConflictCheckRequest) => {
    const res = await api.post<ConflictCheckResult>('/api/trandau/check-conflict', data);
    return res.data;
  },

  /** Rà soát kiểm tra toàn bộ xung đột lịch trong giải đấu */
  checkAllConflicts: async (giaiDauId: number) => {
    const res = await api.get<TournamentConflictReport>(`/api/trandau/check-all-conflicts/${giaiDauId}`);
    return res.data;
  },
};
