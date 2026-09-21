export interface NoiDungThiDau {
  id: number;
  giaiDauMonTheThaoId: number;
  giaiDauId?: number;
  monTheThaoId?: number;
  tenMonTheThao?: string;
  tenGiaiDau?: string;
  ma: string;
  ten: string;
  gioiTinh: string;        // 'Nam' | 'Nu' | 'HonHop'
  loaiThiDau: string;      // 'CaNhan' | 'DongDoi'
  hinhThucThiDau?: string; // HinhThucThiDau enum string
  soLuongToiThieu?: number;
  soLuongToiDa?: number;
  moTa?: string;
  trangThai: boolean;
  soDangKy?: number;
}

export interface CreateUpdateNoiDungThiDau {
  giaiDauId?: number;
  monTheThaoId?: number;
  giaiDauMonTheThaoId?: number;
  ma: string;
  ten: string;
  gioiTinh: string;
  loaiThiDau: string;
  hinhThucThiDau?: string;
  soLuongToiThieu?: number;
  soLuongToiDa?: number;
  moTa?: string;
  trangThai: boolean;
}
