export interface CauHinhLichThiDau {
  id?: number;
  monTheThaoId: number;
  tenMonTheThao?: string;

  // ① DÀN TRẢI LỊCH THI ĐẤU
  moiVongMotNgay: boolean;
  khoangCachGiuaCacVongGio: number;
  uuTienChungKetNgayCuoi: boolean;

  // ② GIẢM TẢI THỂ LỰC VĐV
  soTranToiDaMoiDoiMoiNgay: number;
  nghiToiThieuGiua2TranPhut: number;

  // ③ KHUNG GIỜ & CA THI ĐẤU
  chiaCaThiDau: boolean;
  caSangBatDau: string;
  caSangKetThuc: string;
  caChieuBatDau: string;
  caChieuKetThuc: string;
  caToBatDau?: string | null;
  caToKetThuc?: string | null;

  // ④ SÂN ĐẤU & TRỌNG TÀI
  thoiGianDemDonSanPhut: number;
  soTranToiDaMoiTrongTaiMoiNgay: number;
  nghiToiThieuTrongTaiPhut: number;

  // ⑤ VĐV THI ĐẤU NHIỀU MÔN
  thoiGianDemDiChuyenPhut: number;

  // ⑥ GIÁ TRỊ MẶC ĐỊNH KHI MỞ MODAL XẾP LỊCH
  thoiLuongTranMacDinhPhut: number;
  soHiepDauMacDinh: number;
  thoiGianMoiHiepPhut: number;

  ghiChu?: string | null;
  created?: string;
  lastModified?: string;
}

export interface CreateUpdateCauHinhLichThiDauRequest {
  monTheThaoId: number;

  moiVongMotNgay: boolean;
  khoangCachGiuaCacVongGio: number;
  uuTienChungKetNgayCuoi: boolean;

  soTranToiDaMoiDoiMoiNgay: number;
  nghiToiThieuGiua2TranPhut: number;

  chiaCaThiDau: boolean;
  caSangBatDau: string;
  caSangKetThuc: string;
  caChieuBatDau: string;
  caChieuKetThuc: string;
  caToBatDau?: string | null;
  caToKetThuc?: string | null;

  thoiGianDemDonSanPhut: number;
  soTranToiDaMoiTrongTaiMoiNgay: number;
  nghiToiThieuTrongTaiPhut: number;

  thoiGianDemDiChuyenPhut: number;

  thoiLuongTranMacDinhPhut: number;
  soHiepDauMacDinh: number;
  thoiGianMoiHiepPhut: number;

  ghiChu?: string | null;
}
