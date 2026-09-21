export interface VongDau {
  id: number;
  giaiDauMonTheThaoId?: number;
  noiDungThiDauId?: number;
  tenNoiDung?: string;
  ma?: string;
  ten: string;
  thuTu: number;
  loaiVongDau?: string; // 'VongBang' | 'LoaiTrucTiep' | 'BanKet' | 'ChungKet'
  soTran?: number;
  created?: string;
  lastModified?: string;
}

export interface CreateUpdateVongDau {
  giaiDauMonTheThaoId?: number;
  noiDungThiDauId?: number;
  ma?: string;
  ten: string;
  thuTu: number;
  loaiVongDau?: string;
  moTa?: string;
}
