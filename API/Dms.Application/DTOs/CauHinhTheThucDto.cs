using System.Collections.Generic;

namespace Dms.Application.DTOs
{
    /// <summary>
    /// DTO thông tin chi tiết cấu hình thể thức thi đấu
    /// </summary>
    public class CauHinhTheThucDto
    {
        public int Id { get; set; }
        public int MonTheThaoId { get; set; }
        public string? TenMonTheThao { get; set; }
        public int? GiaiDauMonTheThaoId { get; set; }
        public string? TenGiaiDau { get; set; }
        public string? HinhThucThiDau { get; set; }

        public string LoaiTheThuc { get; set; } = "SetDiem"; // "SetDiem", "ThoiGianHiep", "TinhDiemXepHang"

        // Cấu hình đối kháng
        public int SoHiepToiDa { get; set; } = 3;
        public int? SoHiepThangDeThangTran { get; set; } = 2;
        public int? DiemMoiHiep { get; set; } = 21;
        public int? DiemHiepQuyetDinh { get; set; } = 21;
        public int CachBietDiemToiThieu { get; set; } = 2;
        public int? DiemToiDaMoiHiep { get; set; } = 30;
        public int? ThoiGianHiepChinhPhut { get; set; } = 0;

        public bool ChoPhepHoaVongBang { get; set; } = true;
        public bool ChoPhepHoaKnockout { get; set; } = false;
        public bool CoHiepPhu { get; set; } = false;
        public int? SoHiepPhu { get; set; } = 2;
        public int? ThoiGianHiepPhuPhut { get; set; } = 15;
        public bool CoPenalty { get; set; } = false;
        public int? SoLuotPenaltyMoiDoi { get; set; } = 5;
        public bool CoTieBreak { get; set; } = false;
        public bool CoThePhat { get; set; } = false;

        public decimal DiemThang { get; set; } = 3;
        public decimal DiemHoa { get; set; } = 1;
        public decimal DiemThua { get; set; } = 0;
        public decimal DiemThuaBocCuoc { get; set; } = 0;
        public bool CachTinhDiemTheoSet { get; set; } = false;
        public string TieuChiXepHangJson { get; set; } = "[\"Diem\",\"HieuSo\",\"DiemGhiDuoc\",\"DoiDau\",\"SoTranThang\"]";

        // Cấu hình đo thành tích
        public string? LoaiDoThanhTich { get; set; } = "ThoiGian";
        public string? DonViThanhTich { get; set; } = "giay";
        public string? TieuChiXepHangThanhTich { get; set; } = "CangNhoCangTot";
        public string? TenTieuChiPhuThanhTich { get; set; } = "Chỉ số phụ";
        public bool TieuChiPhuCangNhoCangTot { get; set; } = true;
        public int SoVdvMoiLuotThi { get; set; } = 8;
        public string? QuyCachTienVaoChungKet { get; set; } = "TopNToanVong";
        public int? SoVdvVaoChungKet { get; set; } = 8;
        public decimal? KyLucHienTai { get; set; }
        public string? KyLucHienTaiText { get; set; }

        // Cấu hình mở rộng cho các môn đo lường
        public string? HinhThucXuatPhat { get; set; } = "ChiaLan";
        public int? SoLuotThucHien { get; set; } = 3;
        public string? CachTinhKetQuaLuotThi { get; set; } = "LanTotNhat";
        public decimal? ThangDiemToiDa { get; set; } = 10.0m;
        public bool CoDiemTruBieuDien { get; set; } = true;

        // Cấu hình thời lượng & xếp lịch
        public int ThoiLuongTranPhut { get; set; } = 60;
        public int NghiGiuaTranPhut { get; set; } = 15;
        public int SoBang { get; set; } = 0;
        public int SoDoiMoiBang { get; set; } = 4;
        public int SoDoiMoiBangVaoVongTrong { get; set; } = 2;
        public int SoVongThi { get; set; } = 2;
        public string PhuongThucPhanNhom { get; set; } = "random";

        /// <summary>
        /// Xác định xem môn này có thuộc nhóm thể thức đo lường / thành tích hay không (Đua thời gian, Lượt thử, Biểu diễn, Legacy).
        /// </summary>
        public bool IsPerformanceSport =>
            string.Equals(LoaiTheThuc, "TinhDiemXepHang", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(HinhThucThiDau, "DuaThoiGian", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(HinhThucThiDau, "DoLuotThi", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(HinhThucThiDau, "BieuDienChamDiem", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(HinhThucThiDau, "TinhDiemXepHang", StringComparison.OrdinalIgnoreCase) ||
            HinhThucThiDau == "6" || HinhThucThiDau == "8" || HinhThucThiDau == "9" || HinhThucThiDau == "10";

        /// <summary>
        /// Xác định xem môn này có thuộc thể thức đua tính thời gian (chạy, bơi, xe đạp) hay không.
        /// </summary>
        public bool IsDuaThoiGian =>
            string.Equals(HinhThucThiDau, "DuaThoiGian", StringComparison.OrdinalIgnoreCase) || HinhThucThiDau == "8";

        /// <summary>
        /// Xác định xem môn này có thuộc thể thức đo theo lần thực hiện (cử tạ, nhảy xa, ném tạ) hay không.
        /// </summary>
        public bool IsDoLuotThi =>
            string.Equals(HinhThucThiDau, "DoLuotThi", StringComparison.OrdinalIgnoreCase) || HinhThucThiDau == "9";

        /// <summary>
        /// Xác định xem môn này có thuộc thể thức biểu diễn / chấm điểm (võ quyền, thể dục) hay không.
        /// </summary>
        public bool IsBieuDienChamDiem =>
            string.Equals(HinhThucThiDau, "BieuDienChamDiem", StringComparison.OrdinalIgnoreCase) || HinhThucThiDau == "10";
    }

    /// <summary>
    /// DTO tạo mới hoặc cập nhật cấu hình thể thức
    /// </summary>
    public class CreateUpdateCauHinhTheThucDto
    {
        public int MonTheThaoId { get; set; }
        public int? GiaiDauMonTheThaoId { get; set; }
        public string? HinhThucThiDau { get; set; }
        public string LoaiTheThuc { get; set; } = "SetDiem";

        public int SoHiepToiDa { get; set; } = 3;
        public int? SoHiepThangDeThangTran { get; set; } = 2;
        public int? DiemMoiHiep { get; set; } = 21;
        public int? DiemHiepQuyetDinh { get; set; } = 21;
        public int CachBietDiemToiThieu { get; set; } = 2;
        public int? DiemToiDaMoiHiep { get; set; } = 30;
        public int? ThoiGianHiepChinhPhut { get; set; } = 0;

        public bool ChoPhepHoaVongBang { get; set; } = true;
        public bool ChoPhepHoaKnockout { get; set; } = false;
        public bool CoHiepPhu { get; set; } = false;
        public int? SoHiepPhu { get; set; } = 2;
        public int? ThoiGianHiepPhuPhut { get; set; } = 15;
        public bool CoPenalty { get; set; } = false;
        public int? SoLuotPenaltyMoiDoi { get; set; } = 5;
        public bool CoTieBreak { get; set; } = false;
        public bool CoThePhat { get; set; } = false;

        public decimal DiemThang { get; set; } = 3;
        public decimal DiemHoa { get; set; } = 1;
        public decimal DiemThua { get; set; } = 0;
        public decimal DiemThuaBocCuoc { get; set; } = 0;
        public bool CachTinhDiemTheoSet { get; set; } = false;
        public string TieuChiXepHangJson { get; set; } = "[\"Diem\",\"HieuSo\",\"DiemGhiDuoc\",\"DoiDau\",\"SoTranThang\"]";

        public string? LoaiDoThanhTich { get; set; } = "ThoiGian";
        public string? DonViThanhTich { get; set; } = "giay";
        public string? TieuChiXepHangThanhTich { get; set; } = "CangNhoCangTot";
        public string? TenTieuChiPhuThanhTich { get; set; } = "Chỉ số phụ";
        public bool TieuChiPhuCangNhoCangTot { get; set; } = true;
        public int SoVdvMoiLuotThi { get; set; } = 8;
        public string? QuyCachTienVaoChungKet { get; set; } = "TopNToanVong";
        public int? SoVdvVaoChungKet { get; set; } = 8;
        public decimal? KyLucHienTai { get; set; }
        public string? KyLucHienTaiText { get; set; }

        // Cấu hình mở rộng cho các môn đo lường
        public string? HinhThucXuatPhat { get; set; } = "ChiaLan";
        public int? SoLuotThucHien { get; set; } = 3;
        public string? CachTinhKetQuaLuotThi { get; set; } = "LanTotNhat";
        public decimal? ThangDiemToiDa { get; set; } = 10.0m;
        public bool CoDiemTruBieuDien { get; set; } = true;

        // Cấu hình thời lượng & xếp lịch
        public int ThoiLuongTranPhut { get; set; } = 60;
        public int NghiGiuaTranPhut { get; set; } = 15;
        public int SoBang { get; set; } = 0;
        public int SoDoiMoiBang { get; set; } = 4;
        public int SoDoiMoiBangVaoVongTrong { get; set; } = 2;
        public int SoVongThi { get; set; } = 2;
        public string PhuongThucPhanNhom { get; set; } = "random";
    }

    /// <summary>
    /// DTO yêu cầu ghi nhận và kết thúc trận đấu từ phía trọng tài
    /// </summary>
    public class CompleteMatchRequestDto
    {
        public int TranDauId { get; set; }
        public int Score1 { get; set; }
        public int Score2 { get; set; }
        public int? PenaltyScore1 { get; set; }
        public int? PenaltyScore2 { get; set; }
        public int? ExtraTimeScore1 { get; set; }
        public int? ExtraTimeScore2 { get; set; }
        public string? Winner { get; set; } // "1", "2", "draw"
        public string TrangThai { get; set; } = "KetThuc";
        public List<SetScoreDto>? SetScores { get; set; }
        public List<MatchEventItemDto>? Events { get; set; }
        public List<HeatParticipantResultDto>? HeatResults { get; set; } // Dành cho Bơi lội / Điền kinh
        public string? GhiChu { get; set; }
    }

    /// <summary>
    /// DTO kết quả của từng VĐV trên làn bơi / làn chạy trong lượt thi
    /// </summary>
    public class HeatParticipantResultDto
    {
        public int ThanhPhanTranDauId { get; set; }
        public int DangKyThiDauId { get; set; }
        public int SoLane { get; set; }
        public decimal? GiaTri { get; set; } // Giây hoặc mét
        public decimal? GiaTriPhu { get; set; }
        public string? KetQuaText { get; set; } // "10.45s", "01:02.34", "DNS", "DNF", "DQ"
        public string TrangThai { get; set; } = "ThamGia"; // "ThamGia", "DNS", "DNF", "DQ"
        public int? XepHang { get; set; }
        public string? ChiTietKetQuaJson { get; set; }
        public string? SoDeoBIB { get; set; }
        public int? ThuTuThiDau { get; set; }
    }

    /// <summary>
    /// DTO phản hồi sau khi hoàn tất trận đấu
    /// </summary>
    public class CompleteMatchResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int TranDauId { get; set; }
        public string? WinnerName { get; set; }
        public int? WinnerId { get; set; }
        public bool IsDraw { get; set; }
        public bool IsKnockout { get; set; }
        public bool IsGroupStage { get; set; }
        public string? NextMatchNotice { get; set; }
        public string? StandingsUpdateNotice { get; set; }
        public List<string> MedalsAwarded { get; set; } = new();
    }
}
