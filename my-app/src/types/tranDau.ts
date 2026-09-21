export interface PhanCongTrongTaiItem {
  id: number;
  tranDauId: number;
  trongTaiId: number;
  tenTrongTai?: string;
  soDienThoai?: string;
  capBac?: string;
  vaiTro?: string; // 'TrongTaiChinh' | 'TrongTaiPhu' | 'TrongTaiBan' | 'GiamSat'
  ghiChu?: string;
}

export interface AssignTrongTai {
  trongTaiId: number;
  vaiTro?: string;
  ghiChu?: string;
}

export interface ThanhPhanTranDauItem {
  id: number;
  tranDauId: number;
  dangKyThiDauId: number;
  tenDangKy?: string;
  tenDoi?: string;
  tenDonVi?: string;
  soLane?: number;
  viTri?: number; // 1: Đội 1 (Nhà), 2: Đội 2 (Khách)
  trangThai: string;
  ghiChu?: string;
}

export interface TranDau {
  id: number;
  giaiDauMonTheThaoId?: number;
  noiDungThiDauId?: number;
  tenNoiDung?: string;
  giaiDauId?: number;
  tenGiaiDau?: string;
  monTheThaoId?: number;
  tenMonTheThao?: string;
  vongDauId: number;
  tenVongDau?: string;
  bangDauId?: number;
  tenBangDau?: string;
  sanDauId?: number;
  tenSanDau?: string;
  tenCumSan?: string;
  soTran: number;
  tenTran?: string;
  thoiGianDuKien?: string;
  thoiGianBatDau?: string;
  thoiGianKetThuc?: string;
  trangThai: string; // 'ChuaDau' | 'DangDienRa' | 'DaKetThuc' | 'Hoan' | 'Huy'
  ghiChu?: string;

  doi1DangKyId?: number;
  tenDoi1?: string;
  donViDoi1?: string;

  doi2DangKyId?: number;
  tenDoi2?: string;
  donViDoi2?: string;

  thanhPhanTranDaus?: ThanhPhanTranDauItem[];
  danhSachTrongTai?: PhanCongTrongTaiItem[];

  created?: string;
  lastModified?: string;
}

export interface CreateUpdateTranDau {
  giaiDauMonTheThaoId?: number;
  noiDungThiDauId?: number;
  vongDauId: number;
  bangDauId?: number | null;
  sanDauId?: number | null;
  soTran: number;
  tenTran?: string;
  thoiGianDuKien?: string;
  thoiGianBatDau?: string;
  thoiGianKetThuc?: string;
  trangThai?: string;
  ghiChu?: string;
  doi1DangKyId?: number;
  doi2DangKyId?: number;
  danhSachTrongTai?: AssignTrongTai[];
}

export interface AutoScheduleRequest {
  giaiDauMonTheThaoId?: number;
  noiDungThiDauId?: number;
  ngayBatDau: string;
  gioBatDauMoiNgay: string;
  gioKetThucMoiNgay: string;
  thoiLuongTranPhut: number;
  nghiGiuaTranPhut: number;
  sanDauIds: number[];
  trongTaiIds: number[];
  soTrongTaiMoiTran: number;
  taoBangDauNeuChuaCo: boolean;
  soDoiMoiBang: number;
  xoaLichCu: boolean;
  tranhTrungLichVdv?: boolean;
  soHiepDau?: number;
  thoiGianMoiHiepPhut?: number;
  thoiGianNghiToiThieuVdvPhut?: number;
  khoangCachGiuaCacVongGio?: number;
  canBangTaiTrongTai?: boolean;
  canBangTaiSanDau?: boolean;
  soDoiMoiBangVaoVongTrong?: number;
  soDoiThu3TotNhat?: number;
  // Leaderboard (TinhDiemXepHang)
  soVdvMoiLuotThi?: number;
  soVongThi?: number;
  phuongThucPhanNhom?: string;
  // Round Robin (VongBang = 2)
  soLuotDau?: number;
  cheDoVongBang?: string;
  soBang?: number;
  diemThang?: number;
  diemHoa?: number;
  diemThua?: number;
}

export interface AutoScheduleResult {
  success: boolean;
  totalMatchesCreated: number;
  message: string;
  matches: TranDau[];
  warnings?: string[];
  thongKeSanDau?: Record<string, number>;
  thongKeTrongTai?: Record<string, number>;
  soNgayThiDau?: number;
}

export interface ConflictCheckRequest {
  tranDauId?: number;
  sanDauId?: number;
  thoiGianBatDau: string;
  thoiGianKetThuc: string;
  trongTaiIds?: number[];
  dangKyThiDauIds?: number[];
}

export interface ConflictDetail {
  loaiXungDot: 'VanDongVien' | 'SanDau' | 'TrongTai' | 'Doi';
  thongBao: string;
  vanDongVienId?: number;
  tenVanDongVien?: string;
  maVanDongVien?: string;
  tenDoiHienTai?: string;
  tranDauBiTrungId?: number;
  tenTranBiTrung?: string;
  tenMonTheThao?: string;
  tenNoiDung?: string;
  tenSanDau?: string;
  thoiGianBatDau?: string;
  thoiGianKetThuc?: string;
}

export interface ConflictCheckResult {
  hasConflict: boolean;
  conflicts: string[];
  chiTietXungDot?: ConflictDetail[];
}

export interface TournamentConflictReport {
  hasConflict: boolean;
  giaiDauId: number;
  tenGiaiDau?: string;
  totalMatchesChecked: number;
  totalConflicts: number;
  conflicts: string[];
  chiTietXungDot?: ConflictDetail[];
}

export interface MatchEvent {
  id: string;
  minute: number;
  type:
    | 'goal'
    | 'point'
    | 'yellow_card'
    | 'red_card'
    | 'substitution'
    | 'foul'
    | 'timeout'
    | 'injury'
    | 'incident'
    | 'penalty';
  team: 1 | 2;
  athleteName?: string;
  assistName?: string;
  details?: string;
  timestamp?: string;
}

export interface SetScore {
  setNumber: number;
  score1: number;
  score2: number;
}

export interface MatchScoreDetails {
  score1: number;
  score2: number;
  winner?: 1 | 2 | 'draw';
  setScores: SetScore[];
  events: MatchEvent[];
  notes?: string;

  // Thông tin hoàn thiện trận đấu
  actualStartTime?: string;
  actualEndTime?: string;
  durationMinutes?: number;
  extraTimeMinutes?: number;
  weatherCondition?: string; // Ví dụ: Nắng ráo, Mưa nhẹ, Trong nhà thi đấu
  pitchCondition?: string; // Mặt sân tốt, Bình thường, Trơn ướt
  spectatorCount?: number; // Số lượng khán giả
  mvpAthlete?: string; // Vận động viên xuất sắc nhất
  winMethod?: 'normal' | 'extra_time' | 'penalties' | 'walkover' | 'disqualification';
  refereeNotes?: string; // Nhận xét của tổ trọng tài
  supervisorNotes?: string; // Nhận xét của giám sát
  isFinalized?: boolean; // Đã chốt hoàn thiện trận đấu
  finalizedAt?: string; // Thời gian chốt
  finalizedBy?: string; // Người chốt

  signedReferee?: {
    name: string;
    signedAt: string;
  };
  signedSecretary?: {
    name: string;
    signedAt: string;
  };
  signedTeam1?: {
    name: string;
    signedAt: string;
  };
  signedTeam2?: {
    name: string;
    signedAt: string;
  };
}

