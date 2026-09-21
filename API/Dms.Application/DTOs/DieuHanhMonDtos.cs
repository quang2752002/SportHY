using Dms.Domain.Entities;
using System;
using System.Collections.Generic;

namespace Dms.Application.DTOs
{
    public class CoordinatorAssignmentDto
    {
        public int GiaiDauId { get; set; }
        public string TenGiaiDau { get; set; } = string.Empty;
        public int DanhMucId { get; set; }
        public string TenDanhMuc { get; set; } = string.Empty;
        public string MaDanhMuc { get; set; } = string.Empty;
        public List<MonTheThao> MonTheThaos { get; set; } = new();
        public List<int> GiaiDauMonTheThaoIds { get; set; } = new();
    }

    public class CoordinatorDashboardDto
    {
        public CoordinatorAssignmentDto? Assignment { get; set; }
        public int TotalSports { get; set; }
        public int TotalEvents { get; set; }
        public int TotalMatches { get; set; }
        public int CompletedMatches { get; set; }
        public int OngoingMatches { get; set; }
        public int UpcomingMatches { get; set; }
        public int TotalReferees { get; set; }
        public List<CoordinatorRecentMatchDto> RecentMatches { get; set; } = new();
    }

    public class CoordinatorRecentMatchDto
    {
        public int Id { get; set; }
        public int SoTran { get; set; }
        public string? TenTran { get; set; }
        public string TenMon { get; set; } = string.Empty;
        public DateTime? ThoiGianDuKien { get; set; }
        public string? TrangThai { get; set; }
        public string TrongTaiChinh { get; set; } = string.Empty;
        public int SoTrongTai { get; set; }
    }

    public class CoordinatorEventDto
    {
        public int Id { get; set; }
        public string Ma { get; set; } = string.Empty;
        public string Ten { get; set; } = string.Empty;
        public string TenMon { get; set; } = string.Empty;
        public string? TheThuc { get; set; }
        public string? GioiTinh { get; set; }
        public int? SoVdvToiDa { get; set; }
        public int SoDangKy { get; set; }
        public int SoTranDau { get; set; }
        public int SoTranDaDau { get; set; }
        public string? TrangThai { get; set; }
    }

    public class CoordinatorScheduleMatchDto
    {
        public int TranDauId { get; set; }
        public int SoTran { get; set; }
        public string TenTran { get; set; } = string.Empty;
        public string TenMon { get; set; } = string.Empty;
        public string TenVongDau { get; set; } = string.Empty;
        public string TenSanDau { get; set; } = string.Empty;
        public DateTime? ThoiGianDuKien { get; set; }
        public DateTime? ThoiGianBatDau { get; set; }
        public DateTime? ThoiGianKetThuc { get; set; }
        public string TrangThai { get; set; } = "ChuaDau";
        public string TenTrongTaiChinh { get; set; } = string.Empty;
        public string TenTrongTaiBan { get; set; } = string.Empty;
        public string? GhiChu { get; set; }
    }

    public class CoordinatorResultItemDto
    {
        public int TranDauId { get; set; }
        public int SoTran { get; set; }
        public string TenTran { get; set; } = string.Empty;
        public string TenMon { get; set; } = string.Empty;
        public DateTime? ThoiGian { get; set; }
        public string TrangThai { get; set; } = "ChuaDau";
        public string KetQuaTySo { get; set; } = string.Empty;
        public string? GhiChu { get; set; }
    }

    public class CoordinatorRefereeItemDto
    {
        public int TrongTaiId { get; set; }
        public string Ma { get; set; } = string.Empty;
        public string HoTen { get; set; } = string.Empty;
        public string? GioiTinh { get; set; }
        public string? CapBac { get; set; }
        public string? SoDienThoai { get; set; }
        public string? Email { get; set; }
        public int SoTranDieuHanh { get; set; }
        public string CacVaiTro { get; set; } = string.Empty;
    }
}
