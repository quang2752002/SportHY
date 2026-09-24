using Dms.Application.DTOs;
using Dms.Application.Interfaces;
using Dms.Domain.Entities;
using Dms.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dms.Application.Services
{
    public class DieuHanhMonService : IDieuHanhMonService
    {
        private readonly IUnitOfWork _unitOfWork;

        public DieuHanhMonService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<TrongTai?> GetRefereeByUserIdOrNameAsync(int? trongTaiId, string? userName, string? email)
        {
            if (trongTaiId.HasValue)
            {
                var referee = (await _unitOfWork.TrongTais.FindAsync(t => t.Id == trongTaiId.Value && t.IsDeleted != true)).FirstOrDefault();
                if (referee != null) return referee;
            }

            var byMa = (await _unitOfWork.TrongTais.FindAsync(t => (t.Ma == userName || t.Email == email) && t.IsDeleted != true)).FirstOrDefault();
            return byMa;
        }

        public async Task<List<CoordinatorAssignmentDto>> GetAssignedDisciplinesAsync(int? refereeId, bool isAdminOrManager)
        {
            var gdms = (await _unitOfWork.GiaiDauMonTheThaos.FindAsync(g => g.IsDeleted != true)).ToList();

            if (!isAdminOrManager)
            {
                if (!refereeId.HasValue) return new List<CoordinatorAssignmentDto>();
                gdms = gdms.Where(g => g.NguoiDieuHanhId == refereeId.Value).ToList();
            }

            if (gdms.Count == 0) return new List<CoordinatorAssignmentDto>();

            var giaiDauIds = gdms.Select(g => g.GiaiDauId).Distinct().ToList();
            var giaiDaus = (await _unitOfWork.GiaiDaus.FindAsync(g => giaiDauIds.Contains(g.Id) && g.IsDeleted != true)).ToDictionary(g => g.Id);

            var monTheThaoIds = gdms.Select(g => g.MonTheThaoId).Distinct().ToList();
            var monTheThaos = (await _unitOfWork.MonTheThaos.FindAsync(m => monTheThaoIds.Contains(m.Id) && m.IsDeleted != true)).ToDictionary(m => m.Id);

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
            var allReferees = (await _unitOfWork.TrongTais.FindAsync(t => true)).ToDictionary(t => t.Id);

            var vongDauIds = matches.Select(m => m.VongDauId).Distinct().ToList();
            var vongDaus = (await _unitOfWork.VongDaus.FindAsync(v => vongDauIds.Contains(v.Id))).ToDictionary(v => v.Id);

            var sanDauIds = matches.Where(m => m.SanDauId.HasValue).Select(m => m.SanDauId!.Value).Distinct().ToList();
            var sanDaus = (await _unitOfWork.SanDaus.FindAsync(s => sanDauIds.Contains(s.Id))).ToDictionary(s => s.Id);

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
                    SoTran = m.SoTran,
                    TenTran = m.TenTran ?? $"Trận số {m.SoTran}",
                    TenMon = mon?.Ten ?? "Môn",
                    TenVongDau = vong,
                    TenSanDau = san,
                    ThoiGianDuKien = m.ThoiGianDuKien,
                    ThoiGianBatDau = m.ThoiGianBatDau,
                    ThoiGianKetThuc = m.ThoiGianKetThuc,
                    TrangThai = m.TrangThai,
                    TenTrongTaiChinh = (ttChinh != null && allReferees.TryGetValue(ttChinh.TrongTaiId, out var tc)) ? tc.HoTen : "Chưa gán",
                    TenTrongTaiBan = (ttBan != null && allReferees.TryGetValue(ttBan.TrongTaiId, out var tb)) ? tb.HoTen : "Chưa gán",
                    GhiChu = m.GhiChu
                };
            }).ToList();
        }

        public async Task<(bool success, string message)> UpdateMatchStatusAsync(int tranDauId, string status)
        {
            var match = (await _unitOfWork.TranDaus.FindAsync(t => t.Id == tranDauId && t.IsDeleted != true)).FirstOrDefault();
            if (match == null)
            {
                return (false, "Không tìm thấy trận đấu.");
            }

            match.TrangThai = status;
            if (status == "DangDau" && !match.ThoiGianBatDau.HasValue)
            {
                match.ThoiGianBatDau = DateTime.Now;
            }
            else if (status == "KetThuc" && !match.ThoiGianKetThuc.HasValue)
            {
                match.ThoiGianKetThuc = DateTime.Now;
            }

            match.LastModified = DateTime.UtcNow;
            _unitOfWork.TranDaus.Update(match);
            await _unitOfWork.CompleteAsync();

            return (true, $"Cập nhật trạng thái trận đấu thành '{status}' thành công!");
        }

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

                    return new CoordinatorResultItemDto
                    {
                        TranDauId = m.Id,
                        SoTran = m.SoTran,
                        TenTran = m.TenTran ?? $"Trận {m.SoTran}",
                        TenMon = mon?.Ten ?? "Môn",
                        ThoiGian = m.ThoiGianKetThuc ?? m.ThoiGianDuKien,
                        TrangThai = m.TrangThai,
                        KetQuaTySo = string.IsNullOrEmpty(resultSummary) ? "Chưa có tỉ số" : resultSummary,
                        GhiChu = m.GhiChu
                    };
                }).ToList();
        }

        public async Task<(bool success, string message)> ApproveResultAsync(int tranDauId)
        {
            var match = (await _unitOfWork.TranDaus.FindAsync(t => t.Id == tranDauId && t.IsDeleted != true)).FirstOrDefault();
            if (match == null)
            {
                return (false, "Không tìm thấy trận đấu.");
            }

            match.GhiChu = (match.GhiChu ?? "") + " [Đã kiểm tra & duyệt kết quả bởi Điều hành môn]";
            match.LastModified = DateTime.UtcNow;
            _unitOfWork.TranDaus.Update(match);
            await _unitOfWork.CompleteAsync();

            return (true, "Đã xác nhận và duyệt kết quả trận đấu thành công!");
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
