export interface CumSan {
  id: number;
  ma: string;
  ten: string;
  diaChi?: string;
  soLuongSan?: number;
  moTa?: string;
  trangThai: boolean;
  soSanHienCo?: number;
  created?: string;
  lastModified?: string;
}

export interface CreateUpdateCumSan {
  ma: string;
  ten: string;
  diaChi?: string;
  soLuongSan?: number;
  moTa?: string;
  trangThai: boolean;
}
