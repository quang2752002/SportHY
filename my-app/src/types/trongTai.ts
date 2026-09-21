export interface TrongTai {
  id: number;
  ma: string;
  hoTen: string;
  gioiTinh?: string;
  soDienThoai?: string;
  email?: string;
  capBac?: string;
  trangThai: boolean;
  created?: string;
  lastModified?: string;
}

export interface CreateUpdateTrongTai {
  ma: string;
  hoTen: string;
  gioiTinh?: string;
  soDienThoai?: string;
  email?: string;
  capBac?: string;
  trangThai: boolean;
}
