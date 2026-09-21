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
        public int SoVdvMoiLuotThi { get; set; } = 8;
        public string? QuyCachTienVaoChungKet { get; set; } = "TopNToanVong";
        public int? SoVdvVaoChungKet { get; set; } = 8;
        public decimal? KyLucHienTai { get; set; }
        public string? KyLucHienTaiText { get; set; }
    }

    /// <summary>
    /// DTO tạo mới hoặc cập nhật cấu hình thể thức
    /// </summary>
    public class CreateUpdateCauHinhTheThucDto
    {
        public int MonTheThaoId { get; set; }
        public int? GiaiDauMonTheThaoId { get; set; }
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

        public decimal DiemThang { get; set; } = 3;
        public decimal DiemHoa { get; set; } = 1;
        public decimal DiemThua { get; set; } = 0;
        public decimal DiemThuaBocCuoc { get; set; } = 0;
        public bool CachTinhDiemTheoSet { get; set; } = false;
        public string TieuChiXepHangJson { get; set; } = "[\"Diem\",\"HieuSo\",\"DiemGhiDuoc\",\"DoiDau\",\"SoTranThang\"]";

        public string? LoaiDoThanhTich { get; set; } = "ThoiGian";
        public string? DonViThanhTich { get; set; } = "giay";
        public string? TieuChiXepHangThanhTich { get; set; } = "CangNhoCangTot";
        public int SoVdvMoiLuotThi { get; set; } = 8;
        public string? QuyCachTienVaoChungKet { get; set; } = "TopNToanVong";
        public int? SoVdvVaoChungKet { get; set; } = 8;
        public decimal? KyLucHienTai { get; set; }
        public string? KyLucHienTaiText { get; set; }
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
        public string? KetQuaText { get; set; } // "10.45s", "01:02.34", "DNS", "DNF", "DQ"
        public string TrangThai { get; set; } = "ThamGia"; // "ThamGia", "DNS", "DNF", "DQ"
        public int? XepHang { get; set; }
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
