export interface VanDongVien {
  id: number;
  ma: string;
  hoTen: string;
  donViId?: number;
  tenDonVi?: string;
  ngaySinh?: string;
  gioiTinh: string;
  soDienThoai?: string;
  email?: string;
  soCCCD?: string;
  diaChi?: string;
  hinhAnh?: string;
  trangThai: boolean;
  created?: string;
  lastModified?: string;
}

export interface CreateUpdateVanDongVien {
  ma: string;
  hoTen: string;
  donViId?: number;
  ngaySinh?: string;
  gioiTinh: string;
  soDienThoai?: string;
  email?: string;
  soCCCD?: string;
  diaChi?: string;
  hinhAnh?: string;
  trangThai: boolean;
}
