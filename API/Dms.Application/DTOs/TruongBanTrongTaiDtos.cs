using Dms.Domain.Entities;
using System;
using System.Collections.Generic;

namespace Dms.Application.DTOs
{
    public class TruongBanDashboardDto
    {
        public GiaiDau? CurrentTournament { get; set; }
        public int TotalReferees { get; set; }
        public int AssignedReferees { get; set; }
        public int TotalSports { get; set; }
        public int TotalMatches { get; set; }
        public int CompletedMatches { get; set; }
        public int OngoingMatches { get; set; }
        public int UpcomingMatches { get; set; }
        public int ConflictCount { get; set; }
        public List<UpcomingMatchItemDto> UpcomingMatchesList { get; set; } = new();
    }

    public class UpcomingMatchItemDto
    {
        public int Id { get; set; }
        public int SoTran { get; set; }
        public string? TenTran { get; set; }
        public DateTime? ThoiGianDuKien { get; set; }
        public string? TrangThai { get; set; }
        public string TenMon { get; set; } = string.Empty;
        public string TrongTaiChinh { get; set; } = string.Empty;
        public string TrongTaiBan { get; set; } = string.Empty;
        public int TotalAssigned { get; set; }
    }

    public class RefereeListDto
    {
        public List<TrongTai> Referees { get; set; } = new();
        public Dictionary<int, int> MatchCountMap { get; set; } = new();
    }

    public class RefereeDetailsDto
    {
        public TrongTai Referee { get; set; } = null!;
        public List<RefereeHistoryItemDto> History { get; set; } = new();
    }

    public class RefereeHistoryItemDto
    {
        public string? VaiTro { get; set; }
        public string? GhiChu { get; set; }
        public int SoTran { get; set; }
        public string? TenTran { get; set; }
        public DateTime? ThoiGianDuKien { get; set; }
        public string? TrangThai { get; set; }
        public string TenMon { get; set; } = string.Empty;
        public string TenGiaiDau { get; set; } = string.Empty;
    }

    public class MatchAssignmentDto
    {
        public int TranDauId { get; set; }
        public int SoTran { get; set; }
        public string TenTran { get; set; } = string.Empty;
        public string TenMon { get; set; } = string.Empty;
        public string TenVongDau { get; set; } = string.Empty;
        public string TenSanDau { get; set; } = string.Empty;
        public DateTime? ThoiGianDuKien { get; set; }
        public string TrangThai { get; set; } = "ChuaDau";

        public int? TrongTaiChinhId { get; set; }
        public string? TenTrongTaiChinh { get; set; }

        public int? TrongTaiPhu1Id { get; set; }
        public string? TenTrongTaiPhu1 { get; set; }

        public int? TrongTaiPhu2Id { get; set; }
        public string? TenTrongTaiPhu2 { get; set; }

        public int? TrongTaiBanId { get; set; }
        public string? TenTrongTaiBan { get; set; }

        public int? GiamSatId { get; set; }
        public string? TenGiamSat { get; set; }

        public int TotalAssigned { get; set; }
    }

    public class ConflictResultDto
    {
        public bool HasConflict { get; set; }
        /// <summary>Cờ báo hiệu trọng tài bị quá tải số trận trong ngày</summary>
        public bool IsOverloaded { get; set; }
        /// <summary>Chi tiết cảnh báo quá tải thể lực (nếu có)</summary>
        public string? OverloadMessage { get; set; }
        public List<ConflictItemDto> Conflicts { get; set; } = new();
    }

    public class ConflictItemDto
    {
        public int TranDauId { get; set; }
        public int SoTran { get; set; }
        public string TenTran { get; set; } = string.Empty;
        public string ThoiGian { get; set; } = string.Empty;
        public string TenMon { get; set; } = string.Empty;
        public string VaiTro { get; set; } = string.Empty;
        public int DiffMinutes { get; set; }
        /// <summary>Thời gian đệm nghỉ tối thiểu yêu cầu (phút)</summary>
        public int RequiredRestMinutes { get; set; } = 15;
        /// <summary>Phân loại: "TrungGio" hoặc "ThieuThoiGianNghi"</summary>
        public string LoaiXungDot { get; set; } = "ThieuThoiGianNghi";
        /// <summary>Mô tả chi tiết xung đột</summary>
        public string MoTa { get; set; } = string.Empty;
    }

    public class RefereeScheduleGroupDto
    {
        public int TrongTaiId { get; set; }
        public string MaTrongTai { get; set; } = string.Empty;
        public string HoTen { get; set; } = string.Empty;
        public string? CapBac { get; set; }
        public string? SoDienThoai { get; set; }
        public int TongSoTran { get; set; }
        public List<RefereeTaskDto> Tasks { get; set; } = new();
    }

    public class RefereeTaskDto
    {
        public int TranDauId { get; set; }
        public int SoTran { get; set; }
        public string TenTran { get; set; } = string.Empty;
        public string TenMon { get; set; } = string.Empty;
        public string TenSan { get; set; } = string.Empty;
        public DateTime? ThoiGianDuKien { get; set; }
        public string TrangThai { get; set; } = "ChuaDau";
        public string VaiTro { get; set; } = string.Empty;
        public string? GhiChu { get; set; }
    }

    public class ScanConflictItemDto
    {
        public int TrongTaiId { get; set; }
        public string MaTrongTai { get; set; } = string.Empty;
        public string HoTenTrongTai { get; set; } = string.Empty;
        public string? SoDienThoai { get; set; }
        public int ChenhLechPhut { get; set; }
        /// <summary>Phân loại lỗi: "TrungGio", "ThieuThoiGianNghi", "QuaTaiSoTran"</summary>
        public string LoaiXungDot { get; set; } = "ThieuThoiGianNghi";
        /// <summary>Mô tả chi tiết nguyên nhân vi phạm cấu hình</summary>
        public string MoTaLoi { get; set; } = string.Empty;

        public int Tran1Id { get; set; }
        public int Tran1So { get; set; }
        public string Tran1Ten { get; set; } = string.Empty;
        public string Tran1Mon { get; set; } = string.Empty;
        public string Tran1San { get; set; } = string.Empty;
        public DateTime Tran1ThoiGian { get; set; }
        public string Tran1VaiTro { get; set; } = string.Empty;

        public int Tran2Id { get; set; }
        public int Tran2So { get; set; }
        public string Tran2Ten { get; set; } = string.Empty;
        public string Tran2Mon { get; set; } = string.Empty;
        public string Tran2San { get; set; } = string.Empty;
        public DateTime Tran2ThoiGian { get; set; }
        public string Tran2VaiTro { get; set; } = string.Empty;
    }

    public class CategoryAssignmentViewModelDto
    {
        public int DanhMucId { get; set; }
        public string TenDanhMuc { get; set; } = string.Empty;
        public string MaDanhMuc { get; set; } = string.Empty;
        public int? NguoiDieuHanhId { get; set; }
        public string? TenNguoiDieuHanh { get; set; }
        public List<string> DanhSachMon { get; set; } = new();
        public int SoMonCon { get; set; }
    }
}
