export enum HinhThucThiDau {
  LoaiTrucTiep = 'LoaiTrucTiep',
  VongBang = 'VongBang',
  KetHopVongBangVaLoaiTrucTiep = 'KetHopVongBangVaLoaiTrucTiep',
  NhanhThangNhanhThua = 'NhanhThangNhanhThua',
  HeThuySi = 'HeThuySi',
  TinhDiemXepHang = 'TinhDiemXepHang',
  Khac = 'Khac',
}

export const HinhThucThiDauLabels: Record<string, string> = {
  LoaiTrucTiep: 'Loại trực tiếp (Knockout)',
  VongBang: 'Vòng tròn tính điểm / Vòng bảng (Round Robin)',
  KetHopVongBangVaLoaiTrucTiep: 'Kết hợp vòng bảng & loại trực tiếp',
  NhanhThangNhanhThua: 'Nhánh thắng - nhánh thua (Double Elimination)',
  HeThuySi: 'Hệ Thụy Sĩ (Swiss System)',
  TinhDiemXepHang: 'Tính điểm xếp hạng / Tính giờ (Leaderboard)',
  Khac: 'Khác',
};
