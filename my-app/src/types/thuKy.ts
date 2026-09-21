export interface ThuKy {
  id: number;
  ma: string;
  hoTen: string;
  gioiTinh?: string;
  soDienThoai?: string;
  email?: string;
  chucVu?: string;
  donViCongTac?: string;
  trangThai: boolean;
  created?: string;
  lastModified?: string;
}

export interface CreateUpdateThuKy {
  ma: string;
  hoTen: string;
  gioiTinh?: string;
  soDienThoai?: string;
  email?: string;
  chucVu?: string;
  donViCongTac?: string;
  trangThai: boolean;
}
