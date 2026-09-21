export interface Khoi {
  id: number;
  ma: string;
  ten: string;
  moTa?: string;
  trangThai: boolean;
  soDonVi?: number;
  created?: string;
  lastModified?: string;
}

export interface CreateUpdateKhoi {
  ma: string;
  ten: string;
  moTa?: string;
  trangThai: boolean;
}
