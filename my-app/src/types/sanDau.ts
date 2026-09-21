export interface SanDau {
  id: number;
  cumSanId: number;
  tenCumSan?: string;
  monTheThaoId?: number;
  tenMonTheThao?: string;
  ma: string;
  ten: string;
  loaiSan?: string;
  soSan?: number;
  sucChua?: number;
  moTa?: string;
  trangThai: boolean;
  created?: string;
  lastModified?: string;
}

export interface CreateUpdateSanDau {
  cumSanId: number;
  monTheThaoId?: number;
  ma: string;
  ten: string;
  loaiSan?: string;
  soSan?: number;
  sucChua?: number;
  moTa?: string;
  trangThai: boolean;
}
