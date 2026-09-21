/**
 * Enum Phạm vi áp dụng của giải đấu
 */
export enum PhamViGiaiDau {
  TatCa = 1,
  TheoKhoi = 2,
}

export const PhamViGiaiDauLabels: Record<PhamViGiaiDau, string> = {
  [PhamViGiaiDau.TatCa]: 'Tất cả đơn vị',
  [PhamViGiaiDau.TheoKhoi]: 'Theo khối ngành',
};

/**
 * Enum Trạng thái của giải đấu
 */
export enum TrangThaiGiaiDau {
  Nhap = 1,
  SapDienRa = 2,
  DangDienRa = 3,
  KetThuc = 4,
  Huy = 5,
}

export const TrangThaiGiaiDauLabels: Record<TrangThaiGiaiDau, string> = {
  [TrangThaiGiaiDau.Nhap]: 'Bản nháp',
  [TrangThaiGiaiDau.SapDienRa]: 'Sắp diễn ra',
  [TrangThaiGiaiDau.DangDienRa]: 'Đang diễn ra',
  [TrangThaiGiaiDau.KetThuc]: 'Đã kết thúc',
  [TrangThaiGiaiDau.Huy]: 'Đã hủy',
};

export interface DieuLeGiaiDau {
  id: number;
  giaiDauId: number;
  tieuDe: string;
  noiDung: string;
  tepDinhKem?: string;
  thuTu: number;
  trangThai: boolean;
}

export interface CreateUpdateDieuLeGiaiDau {
  id?: number;
  tieuDe: string;
  noiDung: string;
  tepDinhKem?: string;
  thuTu: number;
  trangThai: boolean;
}

export interface GiaiDau {
  id: number;
  ma: string;
  ten: string;
  slug?: string;
  hinhAnh?: string;
  moTa?: string;
  ngayBatDau: string;
  ngayKetThuc: string;
  hanDangKy?: string;
  diaDiem?: string;
  phamVi: PhamViGiaiDau;
  trangThai: TrangThaiGiaiDau;
  phamViText?: string;     // Chuỗi tiếng Việt backend trả về (VD: 'Tất cả đơn vị', 'Theo khối ngành')
  trangThaiText?: string;  // Chuỗi tiếng Việt backend trả về (VD: 'Đang diễn ra', 'Sắp diễn ra'...)
  created?: string;
  lastModified?: string;
  khoiIds?: number[];
  monTheThaoIds?: number[];
  monTheThaos?: GiaiDauMonTheThao[];
  dieuLeGiaiDaus?: DieuLeGiaiDau[];
}

export interface GiaiDauMonTheThao {
  id: number;
  monTheThaoId: number;
  ma: string;
  ten: string;
  moTa?: string;
  laMonDongDoi: boolean;
  tenDanhMuc?: string;
  hinhThucThiDau?: string;
  gioiTinh?: string;
}

export interface CreateUpdateGiaiDau {
  ma: string;
  ten: string;
  slug?: string;
  hinhAnh?: string;
  moTa?: string;
  ngayBatDau: string;
  ngayKetThuc: string;
  hanDangKy?: string;
  diaDiem?: string;
  phamVi: PhamViGiaiDau;
  trangThai: TrangThaiGiaiDau;
  khoiIds?: number[];
  monTheThaoIds?: number[];
  dieuLes?: CreateUpdateDieuLeGiaiDau[];
}
