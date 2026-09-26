using Dms.Application.DTOs;
using Dms.Application.Interfaces;
using Dms.Domain.Entities;
using Dms.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Dms.Application.Services
{
    public class DieuHanhMonService : IDieuHanhMonService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITranDauService _tranDauService;

        /// <summary>Khởi tạo dịch vụ điều hành môn với kho dữ liệu giải đấu và dịch vụ kiểm tra xung đột lịch trận.</summary>
        /// <param name="unitOfWork">Unit of Work cung cấp truy cập đến các dữ liệu môn, lịch, kết quả và sự cố.</param>
        /// <param name="tranDauService">Dịch vụ nghiệp vụ trận đấu dùng để xác thực sân, đội/VĐV và trọng tài không trùng lịch.</param>
        public DieuHanhMonService(IUnitOfWork unitOfWork, ITranDauService tranDauService)
        {
            _unitOfWork = unitOfWork;
            _tranDauService = tranDauService;
        }

        /// <summary>Lấy phạm vi giải và danh mục môn mà tài khoản điều hành được phép truy cập.</summary>
        /// <param name="applicationUserId">ID tài khoản ApplicationUser của người điều hành.</param>
        /// <param name="isAdminOrManager">True nếu người gọi là Admin/Manager và được xem mọi phạm vi.</param>
        /// <returns>Danh sách các cặp giải/danh mục cùng những môn thi đấu tương ứng.</returns>
        public async Task<List<CoordinatorAssignmentDto>> GetAssignedDisciplinesAsync(int? applicationUserId, bool isAdminOrManager)
        {
            var gdms = (await _unitOfWork.GiaiDauMonTheThaos.FindAsync(g => g.IsDeleted != true)).ToList();
            var monTheThaoIds = gdms.Select(g => g.MonTheThaoId).Distinct().ToList();
            var monTheThaos = (await _unitOfWork.MonTheThaos.FindAsync(m => monTheThaoIds.Contains(m.Id) && m.IsDeleted != true)).ToDictionary(m => m.Id);

            if (!isAdminOrManager)
            {
                if (!applicationUserId.HasValue) return new List<CoordinatorAssignmentDto>();

                var currentUser = await _unitOfWork.ApplicationUsers.GetByIdAsync(applicationUserId.Value);
                if (currentUser?.NguoiDieuHanhMonId is not int coordinatorProfileId)
                {
                    return new List<CoordinatorAssignmentDto>();
                }

                var coordinatorProfile = await _unitOfWork.NguoiDieuHanhMons.GetByIdAsync(coordinatorProfileId);
                if (coordinatorProfile == null || coordinatorProfile.IsDeleted == true || !coordinatorProfile.TrangThai)
                {
                    return new List<CoordinatorAssignmentDto>();
                }

                var coordinatorAssignments = (await _unitOfWork.PhanCongDieuHanhMons.FindAsync(
                    assignment => assignment.NguoiDieuHanhMonId == coordinatorProfileId && assignment.IsDeleted != true)).ToList();
                var assignedScopes = coordinatorAssignments
                    .Select(assignment => (assignment.GiaiDauId, assignment.DanhMucId))
                    .ToHashSet();
                if (assignedScopes.Count == 0) return new List<CoordinatorAssignmentDto>();

                gdms = gdms.Where(g => monTheThaos.TryGetValue(g.MonTheThaoId, out var mon) &&
                    assignedScopes.Contains((g.GiaiDauId, mon.DanhMucId))).ToList();
            }

            if (gdms.Count == 0) return new List<CoordinatorAssignmentDto>();

            var giaiDauIds = gdms.Select(g => g.GiaiDauId).Distinct().ToList();
            var giaiDaus = (await _unitOfWork.GiaiDaus.FindAsync(g => giaiDauIds.Contains(g.Id) && g.IsDeleted != true)).ToDictionary(g => g.Id);

            var danhMucIds = monTheThaos.Values.Select(m => m.DanhMucId).Distinct().ToList();
            var danhMucs = (await _unitOfWork.DanhMucMonTheThaos.FindAsync(d => danhMucIds.Contains(d.Id) && d.IsDeleted != true)).ToDictionary(d => d.Id);

            var result = new List<CoordinatorAssignmentDto>();

            var grouped = gdms.GroupBy(g =>
            {
                var mon = monTheThaos.TryGetValue(g.MonTheThaoId, out var m) ? m : null;
                return new { g.GiaiDauId, DanhMucId = mon?.DanhMucId ?? 0 };
            });

            foreach (var grp in grouped)
            {
                if (!giaiDaus.TryGetValue(grp.Key.GiaiDauId, out var gd)) continue;
                if (!danhMucs.TryGetValue(grp.Key.DanhMucId, out var dm)) continue;

                var relatedMons = grp.Select(g => monTheThaos.TryGetValue(g.MonTheThaoId, out var m) ? m : null).Where(m => m != null).ToList();

                result.Add(new CoordinatorAssignmentDto
                {
                    GiaiDauId = gd.Id,
                    TenGiaiDau = gd.Ten,
                    DanhMucId = dm.Id,
                    TenDanhMuc = dm.Ten,
                    MaDanhMuc = dm.Ma,
                    MonTheThaos = relatedMons!,
                    GiaiDauMonTheThaoIds = grp.Select(g => g.Id).ToList()
                });
            }

            return result;
        }

        public async Task<CoordinatorDashboardDto> GetDashboardAsync(int giaiDauId, int danhMucId, List<CoordinatorAssignmentDto> assignments)
        {
            var assignment = assignments.FirstOrDefault(a => a.GiaiDauId == giaiDauId && a.DanhMucId == danhMucId);
            if (assignment == null) return new CoordinatorDashboardDto();

            var gdmIds = assignment.GiaiDauMonTheThaoIds;

            // Dùng GiaiDauMonTheThao thay vì NoiDungThiDau
            var gdmList = (await _unitOfWork.GiaiDauMonTheThaos.FindAsync(g => gdmIds.Contains(g.Id) && g.IsDeleted != true)).ToList();
            var matches = (await _unitOfWork.TranDaus.FindAsync(t => gdmIds.Contains(t.GiaiDauMonTheThaoId) && t.IsDeleted != true)).ToList();
            var matchIds = matches.Select(m => m.Id).ToList();

            var phanCongs = (await _unitOfWork.PhanCongTrongTais.FindAsync(pc => matchIds.Contains(pc.TranDauId) && pc.IsDeleted != true)).ToList();
            var assignedRefereeIds = phanCongs.Select(pc => pc.TrongTaiId).Distinct().ToList();

            int totalMatches = matches.Count;
            int completedMatches = matches.Count(m => m.TrangThai == "KetThuc" || m.TrangThai == "DaDau");
            int ongoingMatches = matches.Count(m => m.TrangThai == "DangDau");
            int upcomingMatches = matches.Count(m => m.TrangThai == "ChuaDau" || string.IsNullOrEmpty(m.TrangThai));

            var allMons = assignment.MonTheThaos.ToDictionary(m => m.Id);
            var gdmMap = gdmList.ToDictionary(g => g.Id);
            var allRefs = (await _unitOfWork.TrongTais.FindAsync(t => assignedRefereeIds.Contains(t.Id))).ToDictionary(t => t.Id);

            var recentMatches = matches
                .OrderBy(m => m.TrangThai == "DangDau" ? 0 : 1)
                .ThenBy(m => m.ThoiGianDuKien ?? DateTime.MaxValue)
                .Take(10)
                .Select(m =>
                {
                    var gdm = gdmMap.TryGetValue(m.GiaiDauMonTheThaoId, out var g) ? g : null;
                    var mon = (gdm != null && allMons.TryGetValue(gdm.MonTheThaoId, out var s)) ? s : null;
                    var pcs = phanCongs.Where(pc => pc.TranDauId == m.Id).ToList();
                    var ttChinh = pcs.FirstOrDefault(p => p.VaiTro == "Trọng tài chính");
                    string tenTtChinh = (ttChinh != null && allRefs.TryGetValue(ttChinh.TrongTaiId, out var r)) ? r.HoTen : "Chưa phân công";

                    return new CoordinatorRecentMatchDto
                    {
                        Id = m.Id,
                        SoTran = m.SoTran,
                        TenTran = m.TenTran,
                        TenMon = mon?.Ten ?? "Môn",
                        ThoiGianDuKien = m.ThoiGianDuKien,
                        TrangThai = m.TrangThai,
                        TrongTaiChinh = tenTtChinh,
                        SoTrongTai = pcs.Count
                    };
                })
                .ToList();

            return new CoordinatorDashboardDto
            {
                Assignment = assignment,
                TotalSports = assignment.MonTheThaos.Count,
                TotalEvents = gdmList.Count, // Số nội dung thi đấu = số GiaiDauMonTheThao
                TotalMatches = totalMatches,
                CompletedMatches = completedMatches,
                OngoingMatches = ongoingMatches,
                UpcomingMatches = upcomingMatches,
                TotalReferees = assignedRefereeIds.Count,
                RecentMatches = recentMatches
            };
        }

        public async Task<List<CoordinatorEventDto>> GetEventsAsync(int giaiDauId, int danhMucId, int? monTheThaoId, List<CoordinatorAssignmentDto> assignments)
        {
            var assignment = assignments.FirstOrDefault(a => a.GiaiDauId == giaiDauId && a.DanhMucId == danhMucId);
            if (assignment == null) return new List<CoordinatorEventDto>();

            var gdmList = (await _unitOfWork.GiaiDauMonTheThaos.FindAsync(g => assignment.GiaiDauMonTheThaoIds.Contains(g.Id) && g.IsDeleted != true)).ToList();
            if (monTheThaoId.HasValue)
            {
                gdmList = gdmList.Where(g => g.MonTheThaoId == monTheThaoId.Value).ToList();
            }

            var targetGdmIds = gdmList.Select(g => g.Id).ToList();

            // Dùng GiaiDauMonTheThao + MonTheThao thay vì NoiDungThiDau
            var monIds = gdmList.Select(g => g.MonTheThaoId).Distinct().ToList();
            var monMap2 = (await _unitOfWork.MonTheThaos.FindAsync(m => monIds.Contains(m.Id) && m.IsDeleted != true)).ToDictionary(m => m.Id);
            var dangKys = (await _unitOfWork.DangKyThiDaus.FindAsync(dk => targetGdmIds.Contains(dk.GiaiDauMonTheThaoId) && dk.IsDeleted != true)).ToList();
            var matches = (await _unitOfWork.TranDaus.FindAsync(t => targetGdmIds.Contains(t.GiaiDauMonTheThaoId) && t.IsDeleted != true)).ToList();

            var gdmMap2 = gdmList.ToDictionary(g => g.Id);

            return gdmList.Select(gdm =>
            {
                monMap2.TryGetValue(gdm.MonTheThaoId, out var mon);

                int regCount = dangKys.Count(dk => dk.GiaiDauMonTheThaoId == gdm.Id);
                int matchCount = matches.Count(t => t.GiaiDauMonTheThaoId == gdm.Id);
                int finishedCount = matches.Count(t => t.GiaiDauMonTheThaoId == gdm.Id && (t.TrangThai == "KetThuc" || t.TrangThai == "DaDau"));

                return new CoordinatorEventDto
                {
                    Id = gdm.Id,
                    Ma = mon?.Ma ?? gdm.Id.ToString(),
                    Ten = mon?.Ten ?? "Nội dung thi đấu",
                    TenMon = mon?.Ten ?? "Môn",
                    TheThuc = mon?.HinhThucThiDau.ToString(),
                    GioiTinh = mon?.GioiTinh,
                    SoVdvToiDa = mon?.SoLuongVanDongVienToiDa,
                    SoDangKy = regCount,
                    SoTranDau = matchCount,
                    SoTranDaDau = finishedCount,
                    TrangThai = "Hoạt động"
                };
            }).OrderBy(x => x.TenMon).ThenBy(x => x.Ten).ToList();
        }

        /// <summary>Lấy lịch các trận thuộc danh mục môn được giao và áp dụng bộ lọc môn, trạng thái, ngày.</summary>
        /// <param name="giaiDauId">ID giải đấu.</param>
        /// <param name="danhMucId">ID danh mục môn đã được phân công.</param>
        /// <param name="monTheThaoId">ID môn con cần lọc; null để lấy toàn bộ môn trong danh mục.</param>
        /// <param name="status">Trạng thái trận cần lọc; null hoặc rỗng để lấy mọi trạng thái.</param>
        /// <param name="date">Ngày thi đấu cần lọc; null để không giới hạn ngày.</param>
        /// <param name="assignments">Các danh mục/môn mà tài khoản được quyền xem.</param>
        /// <returns>Danh sách lịch trận đã giới hạn theo phân công và bộ lọc.</returns>
        public async Task<List<CoordinatorScheduleMatchDto>> GetScheduleAsync(int giaiDauId, int danhMucId, int? monTheThaoId, string? status, DateTime? date, List<CoordinatorAssignmentDto> assignments)
        {
            var assignment = assignments.FirstOrDefault(a => a.GiaiDauId == giaiDauId && a.DanhMucId == danhMucId);
            if (assignment == null) return new List<CoordinatorScheduleMatchDto>();

            var gdmList = (await _unitOfWork.GiaiDauMonTheThaos.FindAsync(g => assignment.GiaiDauMonTheThaoIds.Contains(g.Id) && g.IsDeleted != true)).ToList();
            if (monTheThaoId.HasValue)
            {
                gdmList = gdmList.Where(g => g.MonTheThaoId == monTheThaoId.Value).ToList();
            }

            var targetGdmIds = gdmList.Select(g => g.Id).ToList();

            var matches = (await _unitOfWork.TranDaus.FindAsync(t => targetGdmIds.Contains(t.GiaiDauMonTheThaoId) && t.IsDeleted != true)).ToList();

            if (!string.IsNullOrWhiteSpace(status))
            {
                matches = matches.Where(m => m.TrangThai == status).ToList();
            }

            if (date.HasValue)
            {
                matches = matches.Where(m => m.ThoiGianDuKien.HasValue && m.ThoiGianDuKien.Value.Date == date.Value.Date).ToList();
            }

            var matchIds = matches.Select(m => m.Id).ToList();
            var phanCongs = (await _unitOfWork.PhanCongTrongTais.FindAsync(pc => matchIds.Contains(pc.TranDauId) && pc.IsDeleted != true)).ToList();
            var allReferees = (await _unitOfWork.TrongTais.FindAsync(t => t.IsDeleted != true)).ToDictionary(t => t.Id);

            var vongDauIds = matches.Select(m => m.VongDauId).Distinct().ToList();
            var vongDaus = (await _unitOfWork.VongDaus.FindAsync(v => vongDauIds.Contains(v.Id) && v.IsDeleted != true)).ToDictionary(v => v.Id);

            var sanDauIds = matches.Where(m => m.SanDauId.HasValue).Select(m => m.SanDauId!.Value).Distinct().ToList();
            var sanDaus = (await _unitOfWork.SanDaus.FindAsync(s => sanDauIds.Contains(s.Id) && s.IsDeleted != true)).ToDictionary(s => s.Id);

            var gdmMap = gdmList.ToDictionary(g => g.Id);
            var monMap = assignment.MonTheThaos.ToDictionary(m => m.Id);

            return matches.OrderBy(m => m.ThoiGianDuKien ?? DateTime.MaxValue).Select(m =>
            {
                var gdm = gdmMap.TryGetValue(m.GiaiDauMonTheThaoId, out var g) ? g : null;
                var mon = (gdm != null && monMap.TryGetValue(gdm.MonTheThaoId, out var s)) ? s : null;
                var vong = vongDaus.TryGetValue(m.VongDauId, out var vd) ? vd.Ten : "Vòng";
                var san = (m.SanDauId.HasValue && sanDaus.TryGetValue(m.SanDauId.Value, out var sd)) ? sd.Ten : "Chưa xếp sân";

                var matchPcs = phanCongs.Where(p => p.TranDauId == m.Id).ToList();
                var ttChinh = matchPcs.FirstOrDefault(p => p.VaiTro == "Trọng tài chính");
                var ttBan = matchPcs.FirstOrDefault(p => p.VaiTro == "Trọng tài bàn");

                return new CoordinatorScheduleMatchDto
                {
                    TranDauId = m.Id,
                    GiaiDauMonTheThaoId = m.GiaiDauMonTheThaoId,
                    SanDauId = m.SanDauId,
                    SoTran = m.SoTran,
                    TenTran = m.TenTran ?? $"Trận số {m.SoTran}",
                    TenMon = mon?.Ten ?? "Môn",
                    TenVongDau = vong,
                    TenSanDau = san,
                    ThoiGianDuKien = m.ThoiGianDuKien,
                    ThoiGianBatDau = m.ThoiGianBatDau,
                    ThoiGianKetThuc = m.ThoiGianKetThuc,
                    TrangThai = m.TrangThai,
                    IsLichCoDinh = m.IsLichCoDinh,
                    TenTrongTaiChinh = (ttChinh != null && allReferees.TryGetValue(ttChinh.TrongTaiId, out var tc)) ? tc.HoTen : "Chưa gán",
                    TenTrongTaiBan = (ttBan != null && allReferees.TryGetValue(ttBan.TrongTaiId, out var tb)) ? tb.HoTen : "Chưa gán",
                    GhiChu = m.GhiChu
                };
            }).ToList();
        }

        /// <summary>Lấy các sân đang hoạt động và dành riêng hoặc dùng chung cho môn thuộc giải, sau khi kiểm tra quyền truy cập.</summary>
        /// <param name="giaiDauMonTheThaoId">ID liên kết giải đấu và môn thể thao.</param>
        /// <param name="allowedGiaiDauMonTheThaoIds">Danh sách môn trong giải được phép điều hành.</param>
        /// <returns>Danh sách sân hợp lệ kèm tên cụm sân, hoặc danh sách rỗng nếu ngoài phạm vi.</returns>
        public async Task<List<CoordinatorScheduleCourtDto>> GetAvailableCourtsAsync(int giaiDauMonTheThaoId, List<int> allowedGiaiDauMonTheThaoIds)
        {
            if (!allowedGiaiDauMonTheThaoIds.Contains(giaiDauMonTheThaoId)) return new List<CoordinatorScheduleCourtDto>();

            var eventSport = (await _unitOfWork.GiaiDauMonTheThaos.FindAsync(g =>
                g.Id == giaiDauMonTheThaoId && g.IsDeleted != true && g.TrangThai)).FirstOrDefault();
            if (eventSport == null) return new List<CoordinatorScheduleCourtDto>();

            var courts = (await _unitOfWork.SanDaus.FindAsync(s =>
                s.TrangThai && s.IsDeleted != true &&
                (!s.MonTheThaoId.HasValue || s.MonTheThaoId == eventSport.MonTheThaoId))).ToList();
            var venueIds = courts.Select(c => c.CumSanId).Distinct().ToList();
            var venues = (await _unitOfWork.CumSans.FindAsync(v => venueIds.Contains(v.Id) && v.IsDeleted != true))
                .ToDictionary(v => v.Id, v => v.Ten);

            return courts.OrderBy(c => c.Ten).Select(c => new CoordinatorScheduleCourtDto
            {
                Id = c.Id,
                Ten = c.Ten,
                TenCumSan = venues.GetValueOrDefault(c.CumSanId)
            }).ToList();
        }

        /// <summary>Lập lịch cho toàn bộ trận chưa có giờ thi đấu của một môn được giao, phân bổ sân và loại trừ các khung giờ xung đột.</summary>
        /// <param name="request">Ngày bắt đầu/kết thúc, khung giờ mỗi ngày, thời lượng trận, thời gian nghỉ và sân được chọn.</param>
        /// <param name="allowedGiaiDauMonTheThaoIds">Danh sách môn trong giải được phép điều hành.</param>
        /// <param name="username">Tài khoản thực hiện để lưu dấu vết cập nhật.</param>
        /// <returns>Kết quả lập lịch; chỉ lưu khi xếp được toàn bộ trận chưa có lịch.</returns>
        public async Task<CoordinatorSchedulePlanResultDto> PlanScheduleAsync(CoordinatorAutoScheduleRequestDto request, List<int> allowedGiaiDauMonTheThaoIds, string username)
        {
            var result = new CoordinatorSchedulePlanResultDto();
            if (!allowedGiaiDauMonTheThaoIds.Contains(request.GiaiDauMonTheThaoId))
            {
                result.Message = "Bạn không được phân công điều hành môn này.";
                return result;
            }

            if (request.NgayKetThuc.Date < request.NgayBatDau.Date ||
                request.NgayKetThuc.Date > request.NgayBatDau.Date.AddDays(90) ||
                !TimeSpan.TryParse(request.GioBatDau, CultureInfo.InvariantCulture, out var startTime) ||
                !TimeSpan.TryParse(request.GioKetThuc, CultureInfo.InvariantCulture, out var endTime) ||
                startTime < TimeSpan.Zero || endTime > TimeSpan.FromDays(1) || startTime >= endTime ||
                request.ThoiLuongTranPhut is < 5 or > 600 || request.NghiGiuaTranPhut is < 0 or > 300)
            {
                result.Message = "Ngày lập lịch phải trong khoảng tối đa 90 ngày; khung giờ, thời lượng trận và thời gian nghỉ cũng phải hợp lệ.";
                return result;
            }

            var availableCourts = await GetAvailableCourtsAsync(request.GiaiDauMonTheThaoId, allowedGiaiDauMonTheThaoIds);
            var requestedCourtIds = request.SanDauIds ?? new List<int>();
            if (requestedCourtIds.Count > 0)
            {
                var selectedIds = requestedCourtIds.Distinct().ToHashSet();
                if (selectedIds.Any(id => availableCourts.All(c => c.Id != id)))
                {
                    result.Message = "Danh sách sân có sân không hoạt động hoặc không phù hợp với môn thi đấu.";
                    return result;
                }
                availableCourts = availableCourts.Where(c => selectedIds.Contains(c.Id)).ToList();
            }
            if (availableCourts.Count == 0)
            {
                result.Message = "Môn này chưa có sân hoạt động phù hợp; hãy cấu hình sân hoặc chọn sân khác.";
                return result;
            }

            var allEventMatches = (await _unitOfWork.TranDaus.FindAsync(t =>
                t.GiaiDauMonTheThaoId == request.GiaiDauMonTheThaoId && t.IsDeleted != true)).ToList();
            var preservedScheduleCount = allEventMatches.Count(t =>
                t.ThoiGianDuKien.HasValue || t.IsLichCoDinh || t.TrangThai == "DangDau" || t.TrangThai == "KetThuc" || t.TrangThai == "DaDau");
            var matches = allEventMatches.Where(t =>
                t.GiaiDauMonTheThaoId == request.GiaiDauMonTheThaoId && t.IsDeleted != true &&
                (t.TrangThai == "ChuaDau" || string.IsNullOrEmpty(t.TrangThai)) &&
                !t.IsLichCoDinh && !t.ThoiGianDuKien.HasValue).ToList();
            if (matches.Count == 0)
            {
                result.Message = "Không có trận chưa xếp lịch. Các trận đang thi đấu, đã kết thúc, đã khóa lịch hoặc đã có giờ được giữ nguyên.";
                return result;
            }

            var roundIds = matches.Select(m => m.VongDauId).Distinct().ToList();
            var rounds = (await _unitOfWork.VongDaus.FindAsync(v => roundIds.Contains(v.Id) && v.IsDeleted != true))
                .ToDictionary(v => v.Id);
            matches = matches.OrderBy(m => rounds.TryGetValue(m.VongDauId, out var round) ? round.ThuTu : int.MaxValue)
                .ThenBy(m => m.SoTran).ToList();

            var matchIds = matches.Select(m => m.Id).ToList();
            var participants = (await _unitOfWork.ThanhPhanTranDaus.FindAsync(tp =>
                matchIds.Contains(tp.TranDauId) && tp.IsDeleted != true)).ToList();
            var assignments = (await _unitOfWork.PhanCongTrongTais.FindAsync(pc =>
                matchIds.Contains(pc.TranDauId) && pc.IsDeleted != true)).ToList();
            var participantsByMatch = participants.GroupBy(p => p.TranDauId)
                .ToDictionary(g => g.Key, g => g.Select(p => p.DangKyThiDauId).Distinct().ToHashSet());
            var refereesByMatch = assignments.GroupBy(p => p.TranDauId)
                .ToDictionary(g => g.Key, g => g.Select(p => p.TrongTaiId).Distinct().ToHashSet());

            var planned = new List<(TranDau Match, int CourtId, DateTime Start, DateTime End)>();
            var warnings = new List<string>();
            var remaining = matches.ToList();
            var slotStep = TimeSpan.FromMinutes(request.ThoiLuongTranPhut + request.NghiGiuaTranPhut);
            var duration = TimeSpan.FromMinutes(request.ThoiLuongTranPhut);

            for (var day = request.NgayBatDau.Date; day <= request.NgayKetThuc.Date && remaining.Count > 0; day = day.AddDays(1))
            {
                var slot = day.Add(startTime);
                var dayEnd = day.Add(endTime);
                while (slot.Add(duration) <= dayEnd && remaining.Count > 0)
                {
                    var usedCourts = new HashSet<int>();
                    var usedParticipants = new HashSet<int>();
                    var usedReferees = new HashSet<int>();
                    var scheduledThisSlot = new List<TranDau>();

                    foreach (var match in remaining.ToList())
                    {
                        var participantIds = participantsByMatch.GetValueOrDefault(match.Id) ?? new HashSet<int>();
                        var refereeIds = refereesByMatch.GetValueOrDefault(match.Id) ?? new HashSet<int>();
                        if (participantIds.Overlaps(usedParticipants) || refereeIds.Overlaps(usedReferees)) continue;

                        foreach (var court in availableCourts.Where(c => !usedCourts.Contains(c.Id)))
                        {
                            var check = await _tranDauService.CheckConflictAsync(new ConflictCheckRequestDto
                            {
                                TranDauId = match.Id,
                                SanDauId = court.Id,
                                ThoiGianBatDau = slot,
                                ThoiGianKetThuc = slot.Add(duration),
                                TrongTaiIds = refereeIds.ToList(),
                                DangKyThiDauIds = participantIds.ToList()
                            });
                            if (check.HasConflict)
                            {
                                if (warnings.Count < 8 && check.Conflicts.Count > 0) warnings.AddRange(check.Conflicts.Take(1));
                                continue;
                            }

                            planned.Add((match, court.Id, slot, slot.Add(duration)));
                            scheduledThisSlot.Add(match);
                            usedCourts.Add(court.Id);
                            usedParticipants.UnionWith(participantIds);
                            usedReferees.UnionWith(refereeIds);
                            break;
                        }
                    }

                    foreach (var match in scheduledThisSlot) remaining.Remove(match);
                    slot = slot.Add(slotStep);
                }
            }

            if (remaining.Count > 0)
            {
                result.Message = $"Chưa lưu lịch: chỉ tìm được khung phù hợp cho {planned.Count}/{matches.Count} trận. Hãy kéo dài ngày thi đấu, tăng số sân hoặc nới khung giờ.";
                result.CanhBao = warnings.Distinct().Take(8).ToList();
                return result;
            }

            foreach (var item in planned)
            {
                item.Match.SanDauId = item.CourtId;
                item.Match.ThoiGianDuKien = item.Start;
                // Giữ các cột thời gian lịch để bộ dò trùng lịch hiện có kiểm tra được sân, đội và trọng tài.
                item.Match.ThoiGianBatDau = item.Start;
                item.Match.ThoiGianKetThuc = item.End;
                item.Match.LastModified = DateTime.UtcNow;
                item.Match.LastModifiedBy = username;
                _unitOfWork.TranDaus.Update(item.Match);
            }
            await _unitOfWork.CompleteAsync();

            result.Success = true;
            result.SoTranDaXep = planned.Count;
            result.SoTranDuocGiuLich = preservedScheduleCount;
            result.Message = $"Đã lập lịch cho {planned.Count} trận; lịch cố định, trận đã bắt đầu/kết thúc và lịch đã có được giữ nguyên.";
            result.CanhBao = warnings.Distinct().Take(8).ToList();
            return result;
        }

        /// <summary>Cập nhật giờ dự kiến và sân của một trận chưa bắt đầu, chỉ trong phạm vi môn được giao và không cho lưu khi xung đột.</summary>
        /// <param name="request">ID trận, thời gian mới và sân mới; để trống cả thời gian/sân nhằm bỏ lịch.</param>
        /// <param name="allowedGiaiDauMonTheThaoIds">Danh sách môn trong giải được phép điều hành.</param>
        /// <param name="username">Tài khoản thực hiện để lưu dấu vết cập nhật.</param>
        /// <returns>Kết quả cập nhật lịch và nội dung lỗi nếu có.</returns>
        public async Task<(bool success, string message)> UpdateMatchScheduleAsync(CoordinatorUpdateScheduleRequestDto request, List<int> allowedGiaiDauMonTheThaoIds, string username)
        {
            var match = (await _unitOfWork.TranDaus.FindAsync(t => t.Id == request.TranDauId && t.IsDeleted != true)).FirstOrDefault();
            if (match == null) return (false, "Không tìm thấy trận đấu.");
            if (!allowedGiaiDauMonTheThaoIds.Contains(match.GiaiDauMonTheThaoId)) return (false, "Bạn không được phân công điều hành môn của trận này.");
            if (match.IsLichCoDinh || match.TrangThai != "ChuaDau") return (false, "Chỉ được điều chỉnh lịch của trận chưa đấu và chưa bị khóa lịch.");

            var eventSport = (await _unitOfWork.GiaiDauMonTheThaos.FindAsync(g =>
                g.Id == match.GiaiDauMonTheThaoId && g.IsDeleted != true)).FirstOrDefault();
            if (eventSport == null) return (false, "Không tìm thấy môn thi đấu của trận.");

            if (request.SanDauId.HasValue)
            {
                var courts = await GetAvailableCourtsAsync(match.GiaiDauMonTheThaoId, allowedGiaiDauMonTheThaoIds);
                if (!courts.Any(c => c.Id == request.SanDauId.Value)) return (false, "Sân đã chọn không hoạt động hoặc không phục vụ môn này.");
            }

            if (request.ThoiGianDuKien.HasValue)
            {
                var scheduleConfig = (await _unitOfWork.CauHinhLichThiDaus.FindAsync(c =>
                    c.MonTheThaoId == eventSport.MonTheThaoId && c.IsDeleted != true)).FirstOrDefault();
                var duration = Math.Clamp(scheduleConfig?.ThoiLuongTranMacDinhPhut ?? 60, 5, 600);
                var participantIds = (await _unitOfWork.ThanhPhanTranDaus.FindAsync(p =>
                    p.TranDauId == match.Id && p.IsDeleted != true)).Select(p => p.DangKyThiDauId).Distinct().ToList();
                var refereeIds = (await _unitOfWork.PhanCongTrongTais.FindAsync(p =>
                    p.TranDauId == match.Id && p.IsDeleted != true)).Select(p => p.TrongTaiId).Distinct().ToList();
                var check = await _tranDauService.CheckConflictAsync(new ConflictCheckRequestDto
                {
                    TranDauId = match.Id,
                    SanDauId = request.SanDauId,
                    ThoiGianBatDau = request.ThoiGianDuKien.Value,
                    ThoiGianKetThuc = request.ThoiGianDuKien.Value.AddMinutes(duration),
                    TrongTaiIds = refereeIds,
                    DangKyThiDauIds = participantIds
                });
                if (check.HasConflict) return (false, string.Join(" ", check.Conflicts));

                match.ThoiGianDuKien = request.ThoiGianDuKien.Value;
                match.ThoiGianBatDau = request.ThoiGianDuKien.Value;
                match.ThoiGianKetThuc = request.ThoiGianDuKien.Value.AddMinutes(duration);
            }
            else
            {
                match.ThoiGianDuKien = null;
                match.ThoiGianBatDau = null;
                match.ThoiGianKetThuc = null;
            }

            match.SanDauId = request.ThoiGianDuKien.HasValue ? request.SanDauId : null;
            match.LastModified = DateTime.UtcNow;
            match.LastModifiedBy = username;
            _unitOfWork.TranDaus.Update(match);
            await _unitOfWork.CompleteAsync();
            return (true, request.ThoiGianDuKien.HasValue ? "Đã cập nhật lịch trận đấu." : "Đã bỏ lịch của trận đấu.");
        }

        /// <summary>Cập nhật tiến độ trận thuộc môn được phân công và ghi nhận thời điểm bắt đầu/kết thúc thực tế.</summary>
        /// <param name="tranDauId">ID trận cần cập nhật.</param>
        /// <param name="status">Trạng thái mới: ChuaDau, DangDau hoặc KetThuc.</param>
        /// <param name="allowedGiaiDauMonTheThaoIds">Các môn trong giải mà tài khoản được quyền điều hành.</param>
        /// <param name="username">Tài khoản thực hiện thao tác.</param>
        /// <returns>Kết quả cập nhật và thông báo nghiệp vụ.</returns>
        public async Task<(bool success, string message)> UpdateMatchStatusAsync(int tranDauId, string status, List<int> allowedGiaiDauMonTheThaoIds, string username)
        {
            if (!new[] { "ChuaDau", "DangDau", "KetThuc" }.Contains(status))
            {
                return (false, "Trạng thái trận đấu không hợp lệ.");
            }

            var match = (await _unitOfWork.TranDaus.FindAsync(t => t.Id == tranDauId && t.IsDeleted != true)).FirstOrDefault();
            if (match == null)
            {
                return (false, "Không tìm thấy trận đấu.");
            }

            if (!allowedGiaiDauMonTheThaoIds.Contains(match.GiaiDauMonTheThaoId))
            {
                return (false, "Bạn không được phân công điều hành môn của trận đấu này.");
            }

            if (match.TrangThai == "KetThuc" && status != "KetThuc")
            {
                return (false, "Trận đã kết thúc; không thể đổi ngược trạng thái tại màn điều hành môn.");
            }

            var previousStatus = match.TrangThai;
            var now = DateTime.UtcNow;
            if (status == "DangDau" && previousStatus != "DangDau")
            {
                match.ThoiGianBatDau = now;
            }
            else if (status == "KetThuc")
            {
                match.ThoiGianKetThuc = now;
            }
            match.TrangThai = status;

            match.LastModified = DateTime.UtcNow;
            match.LastModifiedBy = username;
            _unitOfWork.TranDaus.Update(match);
            await _unitOfWork.CompleteAsync();

            return (true, $"Cập nhật trạng thái trận đấu thành '{status}' thành công!");
        }

        /// <summary>Lấy kết quả và trạng thái rà soát của các trận đã bắt đầu trong danh mục môn được giao.</summary>
        /// <param name="giaiDauId">ID giải đấu.</param>
        /// <param name="danhMucId">ID danh mục môn được phân công.</param>
        /// <param name="monTheThaoId">ID môn con cần lọc; null để lấy toàn bộ danh mục.</param>
        /// <param name="assignments">Các danh mục/môn mà tài khoản được quyền xem.</param>
        /// <returns>Danh sách kết quả trận kèm trạng thái, người và thời điểm rà soát.</returns>
        public async Task<List<CoordinatorResultItemDto>> GetResultsAsync(int giaiDauId, int danhMucId, int? monTheThaoId, List<CoordinatorAssignmentDto> assignments)
        {
            var assignment = assignments.FirstOrDefault(a => a.GiaiDauId == giaiDauId && a.DanhMucId == danhMucId);
            if (assignment == null) return new List<CoordinatorResultItemDto>();

            var gdmList = (await _unitOfWork.GiaiDauMonTheThaos.FindAsync(g => assignment.GiaiDauMonTheThaoIds.Contains(g.Id) && g.IsDeleted != true)).ToList();
            if (monTheThaoId.HasValue)
            {
                gdmList = gdmList.Where(g => g.MonTheThaoId == monTheThaoId.Value).ToList();
            }

            var targetGdmIds = gdmList.Select(g => g.Id).ToList();

            var matches = (await _unitOfWork.TranDaus.FindAsync(t => targetGdmIds.Contains(t.GiaiDauMonTheThaoId) && t.IsDeleted != true)).ToList();
            var matchIds = matches.Select(m => m.Id).ToList();

            var thanhPhans = (await _unitOfWork.ThanhPhanTranDaus.FindAsync(tp => matchIds.Contains(tp.TranDauId) && tp.IsDeleted != true)).ToList();
            var tpIds = thanhPhans.Select(x => x.Id).ToList();

            var ketQuas = (await _unitOfWork.KetQuaTranDaus.FindAsync(kq => tpIds.Contains(kq.ThanhPhanTranDauId) && kq.IsDeleted != true)).ToList();

            var gdmMap = gdmList.ToDictionary(g => g.Id);
            var monMap = assignment.MonTheThaos.ToDictionary(m => m.Id);

            return matches
                .Where(m => m.TrangThai == "KetThuc" || m.TrangThai == "DaDau" || m.TrangThai == "DangDau")
                .OrderByDescending(m => m.ThoiGianKetThuc ?? m.ThoiGianDuKien ?? DateTime.MinValue)
                .Select(m =>
                {
                    var gdm = gdmMap.TryGetValue(m.GiaiDauMonTheThaoId, out var g) ? g : null;
                    var mon = (gdm != null && monMap.TryGetValue(gdm.MonTheThaoId, out var s)) ? s : null;

                    var mThanhPhans = thanhPhans.Where(tp => tp.TranDauId == m.Id).ToList();

                    var resultSummary = string.Join(" - ", mThanhPhans.Select(tp =>
                    {
                        var kq = ketQuas.FirstOrDefault(k => k.ThanhPhanTranDauId == tp.Id);
                        return kq?.KetQuaText ?? (kq?.Diem?.ToString("0.#") ?? "0");
                    }));
                    if (m.TySoDoi1.HasValue || m.TySoDoi2.HasValue)
                    {
                        resultSummary = $"{m.TySoDoi1 ?? 0} - {m.TySoDoi2 ?? 0}";
                    }

                    return new CoordinatorResultItemDto
                    {
                        TranDauId = m.Id,
                        SoTran = m.SoTran,
                        TenTran = m.TenTran ?? $"Trận {m.SoTran}",
                        TenMon = mon?.Ten ?? "Môn",
                        ThoiGian = m.ThoiGianKetThuc ?? m.ThoiGianDuKien,
                        TrangThai = m.TrangThai,
                        KetQuaTySo = string.IsNullOrEmpty(resultSummary) ? "Chưa có tỉ số" : resultSummary,
                        GhiChu = m.GhiChu,
                        TrangThaiDuyet = m.TrangThaiDuyetKetQua,
                        ThoiGianDuyet = m.ThoiGianDuyetKetQua,
                        NguoiDuyet = m.NguoiDuyetKetQua,
                        GhiChuDuyet = m.GhiChuDuyetKetQua
                    };
                }).ToList();
        }

        /// <summary>Duyệt kết quả hoặc yêu cầu chỉnh sửa, lưu trạng thái và thông tin người rà soát trực tiếp trên trận.</summary>
        /// <param name="request">ID trận, hành động Duyet/YeuCauDieuChinh và ghi chú.</param>
        /// <param name="allowedGiaiDauMonTheThaoIds">Các môn mà người gọi được quyền điều hành.</param>
        /// <param name="username">Tên tài khoản rà soát kết quả.</param>
        /// <returns>Kết quả rà soát và thông báo nếu trạng thái hoặc quyền không hợp lệ.</returns>
        public async Task<(bool success, string message)> ReviewMatchResultAsync(CoordinatorReviewResultRequestDto request, List<int> allowedGiaiDauMonTheThaoIds, string username)
        {
            var match = (await _unitOfWork.TranDaus.FindAsync(t => t.Id == request.TranDauId && t.IsDeleted != true)).FirstOrDefault();
            if (match == null)
            {
                return (false, "Không tìm thấy trận đấu.");
            }

            if (!allowedGiaiDauMonTheThaoIds.Contains(match.GiaiDauMonTheThaoId))
            {
                return (false, "Bạn không được phân công điều hành môn của trận này.");
            }

            if (match.TrangThai != "KetThuc" && match.TrangThai != "DaDau")
            {
                return (false, "Chỉ có thể rà soát sau khi trọng tài kết thúc trận đấu.");
            }

            if (request.HanhDong != "Duyet" && request.HanhDong != "YeuCauDieuChinh")
            {
                return (false, "Thao tác rà soát không hợp lệ.");
            }
            if (request.HanhDong == "YeuCauDieuChinh" && string.IsNullOrWhiteSpace(request.GhiChu))
            {
                return (false, "Cần nêu lý do khi yêu cầu trọng tài chỉnh sửa kết quả.");
            }
            if (request.GhiChu?.Length > 1000)
            {
                return (false, "Ghi chú rà soát không được vượt quá 1000 ký tự.");
            }

            match.TrangThaiDuyetKetQua = request.HanhDong == "Duyet" ? "DaDuyet" : "YeuCauDieuChinh";
            match.ThoiGianDuyetKetQua = DateTime.UtcNow;
            match.NguoiDuyetKetQua = username;
            match.GhiChuDuyetKetQua = string.IsNullOrWhiteSpace(request.GhiChu) ? null : request.GhiChu.Trim();
            match.LastModified = DateTime.UtcNow;
            match.LastModifiedBy = username;
            _unitOfWork.TranDaus.Update(match);
            await _unitOfWork.CompleteAsync();

            return request.HanhDong == "Duyet"
                ? (true, "Đã duyệt kết quả trận đấu.")
                : (true, "Đã gửi yêu cầu chỉnh sửa kết quả tới trọng tài.");
        }

        /// <summary>Lấy sổ sự cố thuộc các môn được giao và ghép tên môn, trận để người điều hành theo dõi xử lý.</summary>
        /// <param name="allowedGiaiDauMonTheThaoIds">Các môn trong giải mà người gọi được quyền điều hành.</param>
        /// <param name="giaiDauMonTheThaoId">ID môn cần lọc; null để lấy mọi môn được giao.</param>
        /// <param name="status">Trạng thái cần lọc; null hoặc rỗng để lấy mọi trạng thái.</param>
        /// <returns>Danh sách sự cố chưa xóa mềm, sắp theo thời gian tạo giảm dần.</returns>
        public async Task<List<CoordinatorIssueDto>> GetIssuesAsync(List<int> allowedGiaiDauMonTheThaoIds, int? giaiDauMonTheThaoId, string? status)
        {
            if (allowedGiaiDauMonTheThaoIds.Count == 0) return new List<CoordinatorIssueDto>();

            var scopedIds = allowedGiaiDauMonTheThaoIds.Distinct().ToList();
            if (giaiDauMonTheThaoId.HasValue)
            {
                if (!scopedIds.Contains(giaiDauMonTheThaoId.Value)) return new List<CoordinatorIssueDto>();
                scopedIds = new List<int> { giaiDauMonTheThaoId.Value };
            }

            var issues = (await _unitOfWork.SuCoDieuHanhMons.FindAsync(i =>
                scopedIds.Contains(i.GiaiDauMonTheThaoId) && i.IsDeleted != true &&
                (string.IsNullOrEmpty(status) || i.TrangThai == status))).ToList();
            var eventIds = issues.Select(i => i.GiaiDauMonTheThaoId).Distinct().ToList();
            var events = (await _unitOfWork.GiaiDauMonTheThaos.FindAsync(e => eventIds.Contains(e.Id) && e.IsDeleted != true)).ToList();
            var sportIds = events.Select(e => e.MonTheThaoId).Distinct().ToList();
            var sports = (await _unitOfWork.MonTheThaos.FindAsync(s => sportIds.Contains(s.Id) && s.IsDeleted != true))
                .ToDictionary(s => s.Id, s => s.Ten);
            var matchIds = issues.Where(i => i.TranDauId.HasValue).Select(i => i.TranDauId!.Value).Distinct().ToList();
            var matches = (await _unitOfWork.TranDaus.FindAsync(m => matchIds.Contains(m.Id) && m.IsDeleted != true))
                .ToDictionary(m => m.Id, m => m.TenTran ?? $"Trận {m.SoTran}");
            var eventMap = events.ToDictionary(e => e.Id);

            return issues.OrderByDescending(i => i.Created).Select(i =>
            {
                var sportName = eventMap.TryGetValue(i.GiaiDauMonTheThaoId, out var eventSport) && sports.TryGetValue(eventSport.MonTheThaoId, out var name)
                    ? name
                    : "Môn thi đấu";
                return new CoordinatorIssueDto
                {
                    Id = i.Id,
                    GiaiDauMonTheThaoId = i.GiaiDauMonTheThaoId,
                    TenMon = sportName,
                    TranDauId = i.TranDauId,
                    TenTran = i.TranDauId.HasValue && matches.TryGetValue(i.TranDauId.Value, out var matchName) ? matchName : null,
                    TieuDe = i.TieuDe,
                    MoTa = i.MoTa,
                    MucDo = i.MucDo,
                    TrangThai = i.TrangThai,
                    NguoiPhuTrach = i.NguoiPhuTrach,
                    GhiChuXuLy = i.GhiChuXuLy,
                    NguoiBaoCao = i.CreatedBy,
                    NgayTao = i.Created,
                    ThoiGianGiaiQuyet = i.ThoiGianGiaiQuyet
                };
            }).ToList();
        }

        /// <summary>Tạo sự cố cho một môn được giao, kiểm tra tùy chọn trận liên quan thuộc đúng môn trước khi lưu.</summary>
        /// <param name="request">Tiêu đề, mô tả, mức độ, môn và trận liên quan tùy chọn.</param>
        /// <param name="allowedGiaiDauMonTheThaoIds">Các môn trong giải mà người gọi được quyền điều hành.</param>
        /// <param name="username">Tài khoản báo cáo sự cố.</param>
        /// <returns>Kết quả tạo sự cố và thông báo nghiệp vụ.</returns>
        public async Task<(bool success, string message)> CreateIssueAsync(CreateCoordinatorIssueRequestDto request, List<int> allowedGiaiDauMonTheThaoIds, string username)
        {
            if (!allowedGiaiDauMonTheThaoIds.Contains(request.GiaiDauMonTheThaoId))
                return (false, "Bạn không được phân công điều hành môn này.");
            if (string.IsNullOrWhiteSpace(request.TieuDe) || request.TieuDe.Trim().Length > 200 || string.IsNullOrWhiteSpace(request.MoTa))
                return (false, "Tiêu đề (tối đa 200 ký tự) và mô tả sự cố là bắt buộc.");
            if (!new[] { "BinhThuong", "Cao", "KhanCap" }.Contains(request.MucDo))
                return (false, "Mức độ sự cố không hợp lệ.");

            if (request.TranDauId.HasValue)
            {
                var match = (await _unitOfWork.TranDaus.FindAsync(m => m.Id == request.TranDauId.Value && m.IsDeleted != true)).FirstOrDefault();
                if (match == null || match.GiaiDauMonTheThaoId != request.GiaiDauMonTheThaoId)
                    return (false, "Trận liên quan không tồn tại hoặc không thuộc môn đã chọn.");
            }

            await _unitOfWork.SuCoDieuHanhMons.AddAsync(new SuCoDieuHanhMon
            {
                GiaiDauMonTheThaoId = request.GiaiDauMonTheThaoId,
                TranDauId = request.TranDauId,
                TieuDe = request.TieuDe.Trim(),
                MoTa = request.MoTa.Trim(),
                MucDo = request.MucDo,
                TrangThai = "Moi",
                Created = DateTime.UtcNow,
                CreatedBy = username,
                IsDeleted = false
            });
            await _unitOfWork.CompleteAsync();
            return (true, "Đã ghi nhận sự cố.");
        }

        /// <summary>Cập nhật trạng thái xử lý, người phụ trách và ghi chú của sự cố thuộc môn được giao.</summary>
        /// <param name="request">ID sự cố, trạng thái mới, người phụ trách và ghi chú xử lý.</param>
        /// <param name="allowedGiaiDauMonTheThaoIds">Các môn trong giải mà người gọi được quyền điều hành.</param>
        /// <param name="username">Tài khoản thực hiện cập nhật.</param>
        /// <returns>Kết quả cập nhật và thông báo tương ứng.</returns>
        public async Task<(bool success, string message)> UpdateIssueAsync(UpdateCoordinatorIssueRequestDto request, List<int> allowedGiaiDauMonTheThaoIds, string username)
        {
            var issue = (await _unitOfWork.SuCoDieuHanhMons.FindAsync(i => i.Id == request.Id && i.IsDeleted != true)).FirstOrDefault();
            if (issue == null) return (false, "Không tìm thấy sự cố.");
            if (!allowedGiaiDauMonTheThaoIds.Contains(issue.GiaiDauMonTheThaoId)) return (false, "Bạn không có quyền xử lý sự cố này.");
            if (!new[] { "Moi", "DangXuLy", "DaGiaiQuyet", "Dong" }.Contains(request.TrangThai)) return (false, "Trạng thái xử lý không hợp lệ.");
            if (request.NguoiPhuTrach?.Length > 256) return (false, "Tên người phụ trách không được vượt quá 256 ký tự.");

            issue.TrangThai = request.TrangThai;
            issue.NguoiPhuTrach = string.IsNullOrWhiteSpace(request.NguoiPhuTrach) ? null : request.NguoiPhuTrach.Trim();
            issue.GhiChuXuLy = string.IsNullOrWhiteSpace(request.GhiChuXuLy) ? null : request.GhiChuXuLy.Trim();
            issue.ThoiGianGiaiQuyet = request.TrangThai is "DaGiaiQuyet" or "Dong" ? DateTime.UtcNow : null;
            issue.LastModified = DateTime.UtcNow;
            issue.LastModifiedBy = username;
            _unitOfWork.SuCoDieuHanhMons.Update(issue);
            await _unitOfWork.CompleteAsync();
            return (true, "Đã cập nhật tình trạng xử lý sự cố.");
        }

        public async Task<List<CoordinatorRefereeItemDto>> GetMonRefereesAsync(int giaiDauId, int danhMucId, int? monTheThaoId, List<CoordinatorAssignmentDto> assignments)
        {
            var assignment = assignments.FirstOrDefault(a => a.GiaiDauId == giaiDauId && a.DanhMucId == danhMucId);
            if (assignment == null) return new List<CoordinatorRefereeItemDto>();

            var gdmList = (await _unitOfWork.GiaiDauMonTheThaos.FindAsync(g => assignment.GiaiDauMonTheThaoIds.Contains(g.Id) && g.IsDeleted != true)).ToList();
            if (monTheThaoId.HasValue)
            {
                gdmList = gdmList.Where(g => g.MonTheThaoId == monTheThaoId.Value).ToList();
            }

            var targetGdmIds = gdmList.Select(g => g.Id).ToList();

            var matches = (await _unitOfWork.TranDaus.FindAsync(t => targetGdmIds.Contains(t.GiaiDauMonTheThaoId) && t.IsDeleted != true)).ToList();
            var matchIds = matches.Select(m => m.Id).ToList();

            var phanCongs = (await _unitOfWork.PhanCongTrongTais.FindAsync(pc => matchIds.Contains(pc.TranDauId) && pc.IsDeleted != true)).ToList();
            var refIds = phanCongs.Select(pc => pc.TrongTaiId).Distinct().ToList();

            var referees = (await _unitOfWork.TrongTais.FindAsync(t => refIds.Contains(t.Id) && t.IsDeleted != true)).ToList();
            var refMap = referees.ToDictionary(r => r.Id);

            return phanCongs.GroupBy(pc => pc.TrongTaiId)
                .Where(grp => refMap.ContainsKey(grp.Key))
                .Select(grp =>
                {
                    var r = refMap[grp.Key];
                    int soTran = grp.Count();
                    var roles = grp.Select(p => p.VaiTro).Distinct().Where(x => !string.IsNullOrEmpty(x)).ToList();

                    return new CoordinatorRefereeItemDto
                    {
                        TrongTaiId = r.Id,
                        Ma = r.Ma,
                        HoTen = r.HoTen,
                        GioiTinh = r.GioiTinh,
                        CapBac = r.CapBac,
                        SoDienThoai = r.SoDienThoai,
                        Email = r.Email,
                        SoTranDieuHanh = soTran,
                        CacVaiTro = string.Join(", ", roles)
                    };
                }).OrderByDescending(x => x.SoTranDieuHanh).ThenBy(x => x.HoTen).ToList();
        }
    }
}
