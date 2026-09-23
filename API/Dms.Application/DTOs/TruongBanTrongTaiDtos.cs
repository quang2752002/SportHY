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

    /// <summary>
    /// Tham số chạy chức năng tự động phân công trọng tài cho các trận đã có lịch.
    /// </summary>
    public class AutoAssignRefereesRequestDto
    {
        /// <summary>ID giải đấu cần phân công.</summary>
        public int GiaiDauId { get; set; }

        /// <summary>ID môn thể thao cần phân công; để trống sẽ áp dụng cho toàn giải.</summary>
        public int? MonTheThaoId { get; set; }

        /// <summary>Ngày thi đấu cần phân công; để trống sẽ áp dụng cho mọi ngày.</summary>
        public DateTime? Date { get; set; }

        /// <summary>
        /// Chỉ xử lý các trận chưa đấu. Mặc định bật để không tác động tới trận đang diễn ra hoặc đã kết thúc.
        /// </summary>
        public bool ChiPhanCongTranChuaDau { get; set; } = true;

        /// <summary>Chỉ lấp các vị trí còn trống, không ghi đè phân công hiện tại.</summary>
        public bool ChiLapViTriTrong { get; set; } = true;

        /// <summary>
        /// Danh sách vai trò cần tự động phân công. Nếu để trống, hệ thống dùng đủ 5 vị trí tiêu chuẩn.
        /// </summary>
        public List<string> VaiTros { get; set; } = new();

        /// <summary>
        /// Danh sách trọng tài người dùng chọn để thuật toán sử dụng. Nếu để trống, hệ thống sử dụng toàn bộ trọng tài hợp lệ, trừ Trưởng ban trọng tài.
        /// </summary>
        public List<int> TrongTaiIds { get; set; } = new();

        /// <summary>
        /// Các thay đổi đang ở trạng thái nháp trên giao diện. Thuật toán dùng chúng để lập bản nháp tiếp theo nhưng không ghi dữ liệu.
        /// </summary>
        public List<RefereeAssignmentDraftItemDto> DraftChanges { get; set; } = new();
    }

    /// <summary>
    /// Kết quả tổng hợp của một lần tự động phân công trọng tài.
    /// </summary>
    public class AutoAssignRefereesResultDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int MatchesConsidered { get; set; }
        public int MatchesAssigned { get; set; }
        public int PositionsAssigned { get; set; }
        public int PositionsUnassigned { get; set; }
        public List<AutoAssignRefereeItemDto> Assignments { get; set; } = new();
        public List<AutoAssignRefereeUnassignedDto> Unassigned { get; set; } = new();
    }

    /// <summary>
    /// Một vị trí trọng tài đã được tự động phân công.
    /// </summary>
    public class AutoAssignRefereeItemDto
    {
        public int TranDauId { get; set; }
        public int SoTran { get; set; }
        public string TenTran { get; set; } = string.Empty;
        public string VaiTro { get; set; } = string.Empty;
        public int TrongTaiId { get; set; }
        public string HoTenTrongTai { get; set; } = string.Empty;
    }

    /// <summary>
    /// Một vị trí chưa thể phân công và lý do chi tiết.
    /// </summary>
    public class AutoAssignRefereeUnassignedDto
    {
        public int TranDauId { get; set; }
        public int SoTran { get; set; }
        public string TenTran { get; set; } = string.Empty;
        public string VaiTro { get; set; } = string.Empty;
        public string LyDo { get; set; } = string.Empty;
    }

    /// <summary>
    /// Một thay đổi phân công trong bản nháp. TrongTaiId bằng null nghĩa là hủy phân công ở vị trí đó khi lưu.
    /// </summary>
    public class RefereeAssignmentDraftItemDto
    {
        /// <summary>ID trận đấu cần thay đổi.</summary>
        public int TranDauId { get; set; }

        /// <summary>Vai trò phân công cần thêm, đổi hoặc hủy.</summary>
        public string VaiTro { get; set; } = string.Empty;

        /// <summary>ID trọng tài được chọn; null biểu thị hủy phân công vị trí.</summary>
        public int? TrongTaiId { get; set; }

        /// <summary>Cờ cho biết người dùng đã chủ động chấp nhận cảnh báo trùng lịch khi tạo nháp thủ công.</summary>
        public bool Force { get; set; }
    }

    /// <summary>
    /// Dữ liệu xác nhận lưu toàn bộ thay đổi phân công đang ở trạng thái nháp.
    /// </summary>
    public class SaveRefereeAssignmentDraftRequestDto
    {
        /// <summary>ID giải đấu chứa các trận đấu cần lưu.</summary>
        public int GiaiDauId { get; set; }

        /// <summary>Danh sách vị trí cần thêm, thay đổi hoặc xóa mềm.</summary>
        public List<RefereeAssignmentDraftItemDto> Changes { get; set; } = new();
    }

    /// <summary>
    /// Kết quả xác nhận lưu bản nháp phân công trọng tài.
    /// </summary>
    public class SaveRefereeAssignmentDraftResultDto
    {
        /// <summary>Cho biết thao tác lưu có thành công hay không.</summary>
        public bool Success { get; set; }

        /// <summary>Thông báo tóm tắt kết quả lưu.</summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>Số vị trí đã xử lý từ bản nháp.</summary>
        public int ChangesSaved { get; set; }

        /// <summary>Số phân công mới được tạo.</summary>
        public int AssignmentsCreated { get; set; }

        /// <summary>Số phân công cũ được xóa mềm.</summary>
        public int AssignmentsRemoved { get; set; }
    }
}
