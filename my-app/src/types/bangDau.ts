export interface ThanhVienBang {
  id: number;
  bangDauId: number;
  dangKyThiDauId: number;
  tenDangKy?: string;
  tenDoi?: string;
  tenDonVi?: string;
  hatGiong?: number;
  soTran: number;
  soThang: number;
  soHoa: number;
  soThua: number;
  diemGhiDuoc: number;
  diemBiGhi: number;
  diem: number;
  xepHang?: number;
}

export interface BangDau {
  id: number;
  giaiDauMonTheThaoId?: number;
  noiDungThiDauId?: number;
  tenNoiDung?: string;
  ma: string;
  ten: string;
  thuTu: number;
  soDoi: number;
  thanhViens?: ThanhVienBang[];
  created?: string;
  lastModified?: string;
}

export interface CreateUpdateBangDau {
  giaiDauMonTheThaoId?: number;
  noiDungThiDauId?: number;
  ma?: string;
  ten: string;
  thuTu: number;
  dangKyThiDauIds?: number[];
}

export interface AssignTeamsToBang {
  bangDauId: number;
  dangKyThiDauIds: number[];
}

export interface AutoDistributeBang {
  giaiDauMonTheThaoId?: number;
  noiDungThiDauId?: number;
  soBang: number;
  tienToBang?: string;
}
