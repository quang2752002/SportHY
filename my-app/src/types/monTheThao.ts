export interface MonTheThao {
  id: number;
  danhMucId: number;
  tenDanhMuc?: string;
  ma: string;
  ten: string;
  moTa?: string;
  laMonDongDoi: boolean;
  gioiTinh: string;
  hinhThucThiDau?: string;
  soLuongVanDongVienToiThieu?: number;
  soLuongVanDongVienToiDa?: number;
  soDoiToiDa?: number;
  trangThai: boolean;
  created?: string;
  lastModified?: string;
}

export interface CreateUpdateMonTheThao {
  danhMucId: number;
  ma: string;
  ten: string;
  moTa?: string;
  laMonDongDoi: boolean;
  gioiTinh?: string;
  hinhThucThiDau?: string;
  soLuongVanDongVienToiThieu?: number;
  soLuongVanDongVienToiDa?: number;
  soDoiToiDa?: number;
  trangThai: boolean;
}
