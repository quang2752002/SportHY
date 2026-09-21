export interface DangKyThiDau {
  id: number;
  giaiDauMonTheThaoId?: number;
  noiDungThiDauId?: number;
  tenNoiDung?: string;
  giaiDauId?: number;
  tenGiaiDau?: string;
  monTheThaoId?: number;
  tenMonTheThao?: string;
  doiId?: number;
  tenDoi?: string;
  donViId?: number;
  tenDonVi?: string;
  soDangKy: string;
  tenDangKy?: string;
  trangThai: string;
  ngayDangKy: string;
  ghiChu?: string;
  soVdv: number;
  vanDongVienIds?: number[];
  vanDongVienNames?: string[];
  created?: string;
  lastModified?: string;
}

export interface CreateUpdateDangKyThiDau {
  giaiDauMonTheThaoId?: number;
  noiDungThiDauId?: number;
  doiId?: number;
  /** Nếu true: backend tự tạo Doi từ tenDoi + vanDongVienIds */
  tuDongTaoDoi?: boolean;
  tenDoi?: string;
  donViId?: number;
  soDangKy?: string;
  tenDangKy?: string;
  trangThai?: string;
  ngayDangKy?: string;
  ghiChu?: string;
  vanDongVienIds?: number[];
}

