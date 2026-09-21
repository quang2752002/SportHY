export interface DonVi {
  id: number;
  ma: string;
  ten: string;
  khoiId?: number;
  tenKhoi?: string;
  donViChaId?: number;
  tenDonViCha?: string;
  loaiDonVi?: string;
  diaChi?: string;
  nguoiDaiDien?: string;
  soDienThoai?: string;
  email?: string;
  moTa?: string;
  hinhAnh?: string;
  trangThai: boolean;
  soVanDongVien?: number;
  soDoi?: number;
  created?: string;
  lastModified?: string;
}

export interface CreateUpdateDonVi {
  ma: string;
  ten: string;
  khoiId?: number;
  donViChaId?: number;
  loaiDonVi?: string;
  diaChi?: string;
  nguoiDaiDien?: string;
  soDienThoai?: string;
  email?: string;
  moTa?: string;
  hinhAnh?: string;
  trangThai: boolean;
}
