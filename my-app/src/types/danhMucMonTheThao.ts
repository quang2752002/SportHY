export interface DanhMucMonTheThao {
  id: number;
  ma: string;
  ten: string;
  moTa?: string;
  trangThai: boolean;
  soMonTheThao?: number;
  created?: string;
  lastModified?: string;
}

export interface CreateUpdateDanhMucMonTheThao {
  ma: string;
  ten: string;
  moTa?: string;
  trangThai: boolean;
}
