export interface LoaiHuyChuong {
  id: number;
  ma: string;
  ten: string;
  thuTu: number;
  soLuongDaTrao?: number;
  created?: string;
  lastModified?: string;
}

export interface CreateUpdateLoaiHuyChuong {
  ma: string;
  ten: string;
  thuTu: number;
}
