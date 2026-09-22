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
    public class TruongBanTrongTaiService : ITruongBanTrongTaiService
    {
        private readonly IUnitOfWork _unitOfWork;

        public TruongBanTrongTaiService(IUnitOfWork unitOfWork)
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

            // Tim theo Ma, Email cua TrongTai
            var byMa = (await _unitOfWork.TrongTais.FindAsync(t =>
                (t.Ma == userName || t.Email == email || t.Ma == email) && t.IsDeleted != true)).FirstOrDefault();
            return byMa;
        }

        public async Task<List<GiaiDau>> GetManagedTournamentsAsync(int? refereeId, bool isAdminOrManager)
        {
            var allTournaments = (await _unitOfWork.GiaiDaus.FindAsync(g => g.IsDeleted != true))
                .OrderByDescending(g => g.NgayBatDau)
                .ToList();

            if (isAdminOrManager)
            {
                return allTournaments;
            }

            if (!refereeId.HasValue)
            {
                return new List<GiaiDau>();
            }

            return allTournaments.Where(g => g.TruongBanTrongTaiId == refereeId.Value).ToList();
        }

        public async Task<TruongBanDashboardDto> GetDashboardAsync(int giaiDauId)
        {
            var tournament = (await _unitOfWork.GiaiDaus.FindAsync(g => g.Id == giaiDauId && g.IsDeleted != true)).FirstOrDefault();

            var gdmList = (await _unitOfWork.GiaiDauMonTheThaos.FindAsync(gm => gm.GiaiDauId == giaiDauId && gm.IsDeleted != true)).ToList();
            var gdmIds = gdmList.Select(gm => gm.Id).ToList();

            var allMons = (await _unitOfWork.MonTheThaos.FindAsync(m => m.IsDeleted != true)).ToDictionary(m => m.Id);

            var matches = (await _unitOfWork.TranDaus.FindAsync(t => gdmIds.Contains(t.GiaiDauMonTheThaoId) && t.IsDeleted != true)).ToList();
            var matchIds = matches.Select(m => m.Id).ToList();

            var phanCongs = (await _unitOfWork.PhanCongTrongTais.FindAsync(pc => matchIds.Contains(pc.TranDauId) && pc.IsDeleted != true)).ToList();

            var allReferees = (await _unitOfWork.TrongTais.FindAsync(t => t.IsDeleted != true)).ToList();
            var refMap = allReferees.ToDictionary(t => t.Id);

            int totalReferees = allReferees.Count;
            int assignedReferees = phanCongs.Select(pc => pc.TrongTaiId).Distinct().Count();
            int totalMatches = matches.Count;
            int completedMatches = matches.Count(m => m.TrangThai == "KetThuc" || m.TrangThai == "DaDau");
            int ongoingMatches = matches.Count(m => m.TrangThai == "DangDau");
            int upcomingMatches = matches.Count(m => m.TrangThai == "ChuaDau" || string.IsNullOrEmpty(m.TrangThai));

            // Quét trùng lịch
            int conflictCount = 0;
            var assignedMatches = (from pc in phanCongs
                                   join m in matches on pc.TranDauId equals m.Id
                                   where m.ThoiGianDuKien.HasValue
                                   select new { pc.TrongTaiId, Match = m }).ToList();

            var groupedByReferee = assignedMatches.GroupBy(x => x.TrongTaiId);
            foreach (var grp in groupedByReferee)
            {
                var refMatches = grp.Select(x => x.Match).OrderBy(m => m.ThoiGianDuKien).ToList();
                for (int i = 0; i < refMatches.Count - 1; i++)
                {
                    for (int j = i + 1; j < refMatches.Count; j++)
                    {
                        var diff = Math.Abs((refMatches[i].ThoiGianDuKien!.Value - refMatches[j].ThoiGianDuKien!.Value).TotalMinutes);
                        if (diff < 90)
                        {
                            conflictCount++;
                        }
                    }
                }
            }

            var upcomingList = matches
                .Where(m => m.TrangThai == "ChuaDau" || string.IsNullOrEmpty(m.TrangThai) || m.TrangThai == "DangDau")
                .OrderBy(m => m.ThoiGianDuKien ?? DateTime.MaxValue)
                .Take(10)
                .Select(m =>
                {
                    var gdm = gdmList.FirstOrDefault(x => x.Id == m.GiaiDauMonTheThaoId);
                    string tenMon = (gdm != null && allMons.TryGetValue(gdm.MonTheThaoId, out var mon)) ? mon.Ten : "Môn thi đấu";
                    var matchPcs = phanCongs.Where(pc => pc.TranDauId == m.Id).ToList();

                    string ttChinh = matchPcs.FirstOrDefault(pc => pc.VaiTro == "Trọng tài chính" || pc.VaiTro == "Chính") is var c && c != null && refMap.TryGetValue(c.TrongTaiId, out var refC) ? refC.HoTen : "Chưa phân công";
                    string ttBan = matchPcs.FirstOrDefault(pc => pc.VaiTro == "Trọng tài bàn" || pc.VaiTro == "Bàn") is var b && b != null && refMap.TryGetValue(b.TrongTaiId, out var refB) ? refB.HoTen : "Chưa phân công";

                    return new UpcomingMatchItemDto
                    {
                        Id = m.Id,
                        SoTran = m.SoTran,
                        TenTran = m.TenTran,
                        ThoiGianDuKien = m.ThoiGianDuKien,
                        TrangThai = m.TrangThai,
                        TenMon = tenMon,
                        TrongTaiChinh = ttChinh,
                        TrongTaiBan = ttBan,
                        TotalAssigned = matchPcs.Count
                    };
                })
                .ToList();

            return new TruongBanDashboardDto
            {
                CurrentTournament = tournament,
                TotalReferees = totalReferees,
                AssignedReferees = assignedReferees,
                TotalSports = gdmList.Count,
                TotalMatches = totalMatches,
                CompletedMatches = completedMatches,
                OngoingMatches = ongoingMatches,
                UpcomingMatches = upcomingMatches,
                ConflictCount = conflictCount,
                UpcomingMatchesList = upcomingList
            };
        }

        public async Task<RefereeListDto> GetRefereeListAsync(int? giaiDauId, string? keyword, string? capBac, bool? trangThai)
        {
            var allReferees = (await _unitOfWork.TrongTais.FindAsync(t => t.IsDeleted != true)).ToList();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                string kw = keyword.Trim().ToLower();
                allReferees = allReferees.Where(t =>
                    t.HoTen.ToLower().Contains(kw) ||
                    t.Ma.ToLower().Contains(kw) ||
                    (t.Email != null && t.Email.ToLower().Contains(kw)) ||
                    (t.SoDienThoai != null && t.SoDienThoai.Contains(kw))
                ).ToList();
            }

            if (!string.IsNullOrWhiteSpace(capBac))
            {
                allReferees = allReferees.Where(t => t.CapBac == capBac).ToList();
            }

            if (trangThai.HasValue)
            {
                allReferees = allReferees.Where(t => t.TrangThai == trangThai.Value).ToList();
            }

            Dictionary<int, int> matchCountMap = new();
            if (giaiDauId.HasValue)
            {
                var gdmIds = (await _unitOfWork.GiaiDauMonTheThaos.FindAsync(gm => gm.GiaiDauId == giaiDauId.Value && gm.IsDeleted != true)).Select(x => x.Id).ToList();
                var matchIds = (await _unitOfWork.TranDaus.FindAsync(t => gdmIds.Contains(t.GiaiDauMonTheThaoId) && t.IsDeleted != true)).Select(x => x.Id).ToList();
                var pcs = (await _unitOfWork.PhanCongTrongTais.FindAsync(pc => matchIds.Contains(pc.TranDauId) && pc.IsDeleted != true)).ToList();

                foreach (var tt in allReferees)
                {
                    matchCountMap[tt.Id] = pcs.Count(pc => pc.TrongTaiId == tt.Id);
                }
            }

            return new RefereeListDto
            {
                Referees = allReferees.OrderBy(t => t.HoTen).ToList(),
                MatchCountMap = matchCountMap
            };
        }

        public async Task<RefereeDetailsDto?> GetRefereeDetailsAsync(int id)
        {
            var referee = (await _unitOfWork.TrongTais.FindAsync(t => t.Id == id && t.IsDeleted != true)).FirstOrDefault();
            if (referee == null) return null;

            var pcs = (await _unitOfWork.PhanCongTrongTais.FindAsync(pc => pc.TrongTaiId == id && pc.IsDeleted != true)).ToList();
            var matchIds = pcs.Select(x => x.TranDauId).ToList();
            var matches = (await _unitOfWork.TranDaus.FindAsync(m => matchIds.Contains(m.Id) && m.IsDeleted != true)).ToList();

            var gdmIds = matches.Select(m => m.GiaiDauMonTheThaoId).Distinct().ToList();
            var gdms = (await _unitOfWork.GiaiDauMonTheThaos.FindAsync(gm => gdmIds.Contains(gm.Id))).ToDictionary(x => x.Id);

            var monIds = gdms.Values.Select(x => x.MonTheThaoId).Distinct().ToList();
            var mons = (await _unitOfWork.MonTheThaos.FindAsync(m => monIds.Contains(m.Id))).ToDictionary(x => x.Id);

            var giaiDauIds = gdms.Values.Select(x => x.GiaiDauId).Distinct().ToList();
            var giaiDaus = (await _unitOfWork.GiaiDaus.FindAsync(g => giaiDauIds.Contains(g.Id))).ToDictionary(x => x.Id);

            var history = (from pc in pcs
                           join m in matches on pc.TranDauId equals m.Id
                           let gdm = gdms.ContainsKey(m.GiaiDauMonTheThaoId) ? gdms[m.GiaiDauMonTheThaoId] : null
                           let mon = (gdm != null && mons.ContainsKey(gdm.MonTheThaoId)) ? mons[gdm.MonTheThaoId] : null
                           let giai = (gdm != null && giaiDaus.ContainsKey(gdm.GiaiDauId)) ? giaiDaus[gdm.GiaiDauId] : null
                           orderby m.ThoiGianDuKien descending
                           select new RefereeHistoryItemDto
                           {
                               VaiTro = pc.VaiTro,
                               GhiChu = pc.GhiChu,
                               SoTran = m.SoTran,
                               TenTran = m.TenTran,
                               ThoiGianDuKien = m.ThoiGianDuKien,
                               TrangThai = m.TrangThai,
                               TenMon = mon?.Ten ?? "N/A",
                               TenGiaiDau = giai?.Ten ?? "N/A"
                           }).ToList();

            return new RefereeDetailsDto
            {
                Referee = referee,
                History = history
            };
        }

        public async Task<(bool success, string message)> CreateOrUpdateRefereeAsync(TrongTai model)
        {
            if (string.IsNullOrWhiteSpace(model.HoTen))
            {
                return (false, "Vui lòng nhập họ và tên trọng tài.");
            }

            if (model.Id == 0)
            {
                if (string.IsNullOrWhiteSpace(model.Ma))
                {
                    model.Ma = "TT_" + DateTime.Now.ToString("yyyyMMddHHmmss");
                }

                var existing = (await _unitOfWork.TrongTais.FindAsync(t => t.Ma == model.Ma && t.IsDeleted != true)).FirstOrDefault();
                if (existing != null)
                {
                    return (false, "Mã trọng tài này đã tồn tại!");
                }

                model.Created = DateTime.UtcNow;
                await _unitOfWork.TrongTais.AddAsync(model);
                await _unitOfWork.CompleteAsync();

                return (true, "Thêm trọng tài mới thành công!");
            }
            else
            {
                var existing = (await _unitOfWork.TrongTais.FindAsync(t => t.Id == model.Id && t.IsDeleted != true)).FirstOrDefault();
                if (existing == null)
                {
                    return (false, "Không tìm thấy thông tin trọng tài.");
                }

                existing.HoTen = model.HoTen;
                existing.GioiTinh = model.GioiTinh;
                existing.SoDienThoai = model.SoDienThoai;
                existing.Email = model.Email;
                existing.CapBac = model.CapBac;
                existing.TrangThai = model.TrangThai;
                existing.LastModified = DateTime.UtcNow;

                _unitOfWork.TrongTais.Update(existing);
                await _unitOfWork.CompleteAsync();

                return (true, "Cập nhật thông tin trọng tài thành công!");
            }
        }

        public async Task<List<MatchAssignmentDto>> GetMatchAssignmentsAsync(int giaiDauId, int? monTheThaoId, string? status, DateTime? date)
        {
            var gdmList = (await _unitOfWork.GiaiDauMonTheThaos.FindAsync(gm => gm.GiaiDauId == giaiDauId && gm.IsDeleted != true)).ToList();
            if (monTheThaoId.HasValue)
            {
                gdmList = gdmList.Where(gm => gm.MonTheThaoId == monTheThaoId.Value).ToList();
            }

            var gdmIds = gdmList.Select(gm => gm.Id).ToList();

            var matches = (await _unitOfWork.TranDaus.FindAsync(t => gdmIds.Contains(t.GiaiDauMonTheThaoId) && t.IsDeleted != true)).ToList();

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

            var allReferees = (await _unitOfWork.TrongTais.FindAsync(t => t.IsDeleted != true)).ToList();
            var refMap = allReferees.ToDictionary(t => t.Id);

            var allMons = (await _unitOfWork.MonTheThaos.FindAsync(m => m.IsDeleted != true)).ToDictionary(m => m.Id);

            var vongDauIds = matches.Select(m => m.VongDauId).Distinct().ToList();
            var vongDaus = (await _unitOfWork.VongDaus.FindAsync(v => vongDauIds.Contains(v.Id))).ToDictionary(v => v.Id);

            var sanDauIds = matches.Where(m => m.SanDauId.HasValue).Select(m => m.SanDauId!.Value).Distinct().ToList();
            var sanDaus = (await _unitOfWork.SanDaus.FindAsync(s => sanDauIds.Contains(s.Id))).ToDictionary(s => s.Id);

            return matches.OrderBy(m => m.ThoiGianDuKien ?? DateTime.MaxValue).Select(m =>
            {
                var gdm = gdmList.FirstOrDefault(x => x.Id == m.GiaiDauMonTheThaoId);
                string tenMon = (gdm != null && allMons.TryGetValue(gdm.MonTheThaoId, out var s)) ? s.Ten : "Môn";
                string tenVong = vongDaus.TryGetValue(m.VongDauId, out var vd) ? vd.Ten : "Vòng";
                string tenSan = (m.SanDauId.HasValue && sanDaus.TryGetValue(m.SanDauId.Value, out var sd)) ? sd.Ten : "Chưa xếp sân";

                var matchPcs = phanCongs.Where(pc => pc.TranDauId == m.Id).ToList();

                var ttChinh = matchPcs.FirstOrDefault(pc => pc.VaiTro == "Trọng tài chính");
                var ttPhu1 = matchPcs.FirstOrDefault(pc => pc.VaiTro == "Trọng tài phụ 1");
                var ttPhu2 = matchPcs.FirstOrDefault(pc => pc.VaiTro == "Trọng tài phụ 2");
                var ttBan = matchPcs.FirstOrDefault(pc => pc.VaiTro == "Trọng tài bàn");
                var ttGiamSat = matchPcs.FirstOrDefault(pc => pc.VaiTro == "Giám sát trận đấu");

                return new MatchAssignmentDto
                {
                    TranDauId = m.Id,
                    SoTran = m.SoTran,
                    TenTran = m.TenTran ?? $"Trận số {m.SoTran}",
                    TenMon = tenMon,
                    TenVongDau = tenVong,
                    TenSanDau = tenSan,
                    ThoiGianDuKien = m.ThoiGianDuKien,
                    TrangThai = m.TrangThai,

                    TrongTaiChinhId = ttChinh?.TrongTaiId,
                    TenTrongTaiChinh = (ttChinh != null && refMap.TryGetValue(ttChinh.TrongTaiId, out var tc)) ? tc.HoTen : null,

                    TrongTaiPhu1Id = ttPhu1?.TrongTaiId,
                    TenTrongTaiPhu1 = (ttPhu1 != null && refMap.TryGetValue(ttPhu1.TrongTaiId, out var tp1)) ? tp1.HoTen : null,

                    TrongTaiPhu2Id = ttPhu2?.TrongTaiId,
                    TenTrongTaiPhu2 = (ttPhu2 != null && refMap.TryGetValue(ttPhu2.TrongTaiId, out var tp2)) ? tp2.HoTen : null,

                    TrongTaiBanId = ttBan?.TrongTaiId,
                    TenTrongTaiBan = (ttBan != null && refMap.TryGetValue(ttBan.TrongTaiId, out var tb)) ? tb.HoTen : null,

                    GiamSatId = ttGiamSat?.TrongTaiId,
                    TenGiamSat = (ttGiamSat != null && refMap.TryGetValue(ttGiamSat.TrongTaiId, out var gs)) ? gs.HoTen : null,

                    TotalAssigned = matchPcs.Count
                };
            }).ToList();
        }

        /// <summary>
        /// Kiểm tra xung đột thời gian và quá tải thể lực của trọng tài theo quy chuẩn CauHinhLichThiDau.
        /// Xét giới hạn số trận tối đa/ngày (SoTranToiDaMoiTrongTaiMoiNgay) và thời gian đệm nghỉ tối thiểu giữa 2 trận (NghiToiThieuTrongTaiPhut).
        /// </summary>
        /// <param name="trongTaiId">Mã định danh trọng tài dự kiến phân công</param>
        /// <param name="tranDauId">Mã định danh trận đấu</param>
        /// <returns>Đối tượng ConflictResultDto chứa trạng thái xung đột, cờ quá tải và danh sách chi tiết các vi phạm</returns>
        public async Task<ConflictResultDto> CheckConflictAsync(int trongTaiId, int tranDauId)
        {
            var match = (await _unitOfWork.TranDaus.FindAsync(t => t.Id == tranDauId && t.IsDeleted != true)).FirstOrDefault();
            if (match == null || !match.ThoiGianDuKien.HasValue)
            {
                return new ConflictResultDto { HasConflict = false };
            }

            var gdm = (await _unitOfWork.GiaiDauMonTheThaos.FindAsync(g => g.Id == match.GiaiDauMonTheThaoId && g.IsDeleted != true)).FirstOrDefault();
            int currentMonId = gdm?.MonTheThaoId ?? 0;
            var mon = (await _unitOfWork.MonTheThaos.FindAsync(m => m.Id == currentMonId && m.IsDeleted != true)).FirstOrDefault();
            var currCfg = (await _unitOfWork.CauHinhLichThiDaus.FindAsync(c => c.MonTheThaoId == currentMonId && c.IsDeleted != true)).FirstOrDefault();

            int soTranToiDaMoiNgay = currCfg?.SoTranToiDaMoiTrongTaiMoiNgay ?? 4;
            int nghiToiThieuPhut = currCfg?.NghiToiThieuTrongTaiPhut ?? 15;
            int thoiLuongTranPhut = (match.ThoiGianKetThuc.HasValue && match.ThoiGianDuKien.HasValue)
                ? (int)(match.ThoiGianKetThuc.Value - match.ThoiGianDuKien.Value).TotalMinutes
                : (currCfg?.ThoiLuongTranMacDinhPhut > 0 ? currCfg.ThoiLuongTranMacDinhPhut : 60);

            DateTime currStart = match.ThoiGianDuKien.Value;
            DateTime currEnd = match.ThoiGianKetThuc ?? currStart.AddMinutes(thoiLuongTranPhut);
            DateTime matchDate = currStart.Date;

            // Danh sách các trận khác trọng tài này đã được phân công
            var pcs = (await _unitOfWork.PhanCongTrongTais.FindAsync(pc => pc.TrongTaiId == trongTaiId && pc.TranDauId != tranDauId && pc.IsDeleted != true)).ToList();
            var otherMatchIds = pcs.Select(x => x.TranDauId).Distinct().ToList();

            var otherMatches = (await _unitOfWork.TranDaus.FindAsync(t => otherMatchIds.Contains(t.Id) && t.ThoiGianDuKien.HasValue && t.IsDeleted != true)).ToList();

            var gdms = (await _unitOfWork.GiaiDauMonTheThaos.FindAsync(gm => true)).ToDictionary(x => x.Id);
            var mons = (await _unitOfWork.MonTheThaos.FindAsync(m => true)).ToDictionary(x => x.Id);
            var allConfigs = (await _unitOfWork.CauHinhLichThiDaus.FindAsync(c => c.IsDeleted != true)).ToDictionary(c => c.MonTheThaoId);

            // 1. Kiểm tra giới hạn số trận bắt tối đa trong ngày (SoTranToiDaMoiTrongTaiMoiNgay)
            var matchesSameDay = otherMatches.Where(m => m.ThoiGianDuKien!.Value.Date == matchDate).ToList();
            int currentDayCount = matchesSameDay.Count;
            bool isOverloaded = currentDayCount >= soTranToiDaMoiNgay;
            string? overloadMsg = isOverloaded
                ? $"Trọng tài đã được phân công {currentDayCount}/{soTranToiDaMoiNgay} trận trong ngày {matchDate:dd/MM/yyyy}. Việc gán thêm sẽ vượt quá giới hạn thể lực ({soTranToiDaMoiNgay} trận/ngày) theo cấu hình môn {mon?.Ten ?? "thi đấu"}!"
                : null;

            var conflicts = new List<ConflictItemDto>();

            // 2. Kiểm tra thời gian đệm nghỉ giữa 2 trận liên tiếp (NghiToiThieuTrongTaiPhut)
            foreach (var om in otherMatches)
            {
                var gdmOm = gdms.TryGetValue(om.GiaiDauMonTheThaoId, out var g) ? g : null;
                int omMonId = gdmOm?.MonTheThaoId ?? 0;
                var omMon = mons.TryGetValue(omMonId, out var omM) ? omM : null;
                var omCfg = allConfigs.TryGetValue(omMonId, out var oc) ? oc : null;

                int omDuration = (om.ThoiGianKetThuc.HasValue && om.ThoiGianDuKien.HasValue)
                    ? (int)(om.ThoiGianKetThuc.Value - om.ThoiGianDuKien.Value).TotalMinutes
                    : (omCfg?.ThoiLuongTranMacDinhPhut > 0 ? omCfg.ThoiLuongTranMacDinhPhut : 60);

                DateTime omStart = om.ThoiGianDuKien!.Value;
                DateTime omEnd = om.ThoiGianKetThuc ?? omStart.AddMinutes(omDuration);
                int reqRest = Math.Max(nghiToiThieuPhut, omCfg?.NghiToiThieuTrongTaiPhut ?? 15);

                bool violatesRest = (currStart < omEnd.AddMinutes(reqRest)) && (omStart < currEnd.AddMinutes(reqRest));
                if (violatesRest)
                {
                    bool directOverlap = (currStart < omEnd) && (omStart < currEnd);
                    string loaiXungDot = directOverlap ? "TrungGio" : "ThieuThoiGianNghi";
                    int gapMinutes = 0;
                    string moTa = "";

                    if (directOverlap)
                    {
                        gapMinutes = 0;
                        moTa = $"Trực tiếp trùng giờ thi đấu với Trận #{om.SoTran} ({omStart:HH:mm} - {omEnd:HH:mm}) môn {omMon?.Ten ?? "này"}!";
                    }
                    else if (currStart >= omEnd)
                    {
                        gapMinutes = (int)(currStart - omEnd).TotalMinutes;
                        moTa = $"Chỉ được nghỉ {gapMinutes} phút sau Trận #{om.SoTran} (kết thúc lúc {omEnd:HH:mm}), chưa đủ thời gian đệm hồi sức tối thiểu {reqRest} phút theo cấu hình môn {mon?.Ten}!";
                    }
                    else
                    {
                        gapMinutes = (int)(omStart - currEnd).TotalMinutes;
                        moTa = $"Trận này kết thúc lúc {currEnd:HH:mm}, chỉ cách Trận #{om.SoTran} ({omStart:HH:mm}) {gapMinutes} phút, chưa đủ thời gian đệm hồi sức tối thiểu {reqRest} phút theo cấu hình môn {mon?.Ten}!";
                    }

                    var pc = pcs.FirstOrDefault(p => p.TranDauId == om.Id);

                    conflicts.Add(new ConflictItemDto
                    {
                        TranDauId = om.Id,
                        SoTran = om.SoTran,
                        TenTran = om.TenTran ?? $"Trận số {om.SoTran}",
                        ThoiGian = om.ThoiGianDuKien.Value.ToString("HH:mm dd/MM/yyyy"),
                        TenMon = omMon?.Ten ?? "Môn thi đấu",
                        VaiTro = pc?.VaiTro ?? "Trọng tài",
                        DiffMinutes = gapMinutes,
                        RequiredRestMinutes = reqRest,
                        LoaiXungDot = loaiXungDot,
                        MoTa = moTa
                    });
                }
            }

            return new ConflictResultDto
            {
                HasConflict = conflicts.Count > 0 || isOverloaded,
                IsOverloaded = isOverloaded,
                OverloadMessage = overloadMsg,
                Conflicts = conflicts
            };
        }

        /// <summary>
        /// Gán, thay đổi hoặc hủy phân công trọng tài cho trận đấu kèm cơ chế bảo vệ quá tải thể lực
        /// và thời gian đệm nghỉ hồi sức tối thiểu theo CauHinhLichThiDau của môn.
        /// </summary>
        /// <param name="tranDauId">Mã định danh trận đấu</param>
        /// <param name="vaiTro">Vai trò điều hành (Trọng tài chính, phụ 1, phụ 2, bàn, giám sát)</param>
        /// <param name="trongTaiId">Mã định danh trọng tài (null/0 để hủy gán)</param>
        /// <param name="force">Cờ cho phép bỏ qua cảnh báo khi Trưởng ban chủ động xác nhận</param>
        /// <returns>Kết quả thành công, thông báo lỗi/cảnh báo và tên trọng tài</returns>
        public async Task<(bool success, string message, string? refereeName)> AssignRefereeAsync(int tranDauId, string vaiTro, int? trongTaiId, bool force = false)
        {
            var match = (await _unitOfWork.TranDaus.FindAsync(t => t.Id == tranDauId && t.IsDeleted != true)).FirstOrDefault();
            if (match == null)
            {
                return (false, "Không tìm thấy trận đấu.", null);
            }

            var existingPcs = (await _unitOfWork.PhanCongTrongTais.FindAsync(pc => pc.TranDauId == tranDauId && pc.VaiTro == vaiTro && pc.IsDeleted != true)).ToList();

            if (!trongTaiId.HasValue || trongTaiId.Value == 0)
            {
                foreach (var pc in existingPcs)
                {
                    pc.IsDeleted = true;
                    _unitOfWork.PhanCongTrongTais.Update(pc);
                }
                await _unitOfWork.CompleteAsync();
                return (true, $"Đã hủy phân công {vaiTro} cho trận đấu.", null);
            }

            var referee = (await _unitOfWork.TrongTais.FindAsync(t => t.Id == trongTaiId.Value)).FirstOrDefault();
            if (referee == null)
            {
                return (false, "Không tìm thấy thông tin trọng tài.", null);
            }

            // Kiểm tra xung đột & quá tải thể lực theo cấu hình môn
            if (!force)
            {
                var conflictCheck = await CheckConflictAsync(trongTaiId.Value, tranDauId);
                if (conflictCheck.IsOverloaded)
                {
                    return (false, $"[CẢNH BÁO QUÁ TẢI] {conflictCheck.OverloadMessage}", referee.HoTen);
                }
                if (conflictCheck.Conflicts.Any())
                {
                    var firstConf = conflictCheck.Conflicts.First();
                    return (false, $"[CẢNH BÁO XUNG ĐỘT] Trọng tài {referee.HoTen}: {firstConf.MoTa}", referee.HoTen);
                }
            }

            if (existingPcs.Count > 0)
            {
                var pc = existingPcs.First();
                pc.TrongTaiId = trongTaiId.Value;
                pc.LastModified = DateTime.UtcNow;
                _unitOfWork.PhanCongTrongTais.Update(pc);
            }
            else
            {
                var newPc = new PhanCongTrongTai
                {
                    TranDauId = tranDauId,
                    TrongTaiId = trongTaiId.Value,
                    VaiTro = vaiTro,
                    Created = DateTime.UtcNow
                };
                await _unitOfWork.PhanCongTrongTais.AddAsync(newPc);
            }

            await _unitOfWork.CompleteAsync();

            return (true, $"Đã phân công {vaiTro}: {referee.HoTen} thành công!", referee.HoTen);
        }

        public async Task<List<RefereeScheduleGroupDto>> GetRefereeSchedulesAsync(int giaiDauId, int? trongTaiId, DateTime? date, int? monTheThaoId)
        {
            var gdmList = (await _unitOfWork.GiaiDauMonTheThaos.FindAsync(gm => gm.GiaiDauId == giaiDauId && gm.IsDeleted != true)).ToList();
            if (monTheThaoId.HasValue)
            {
                gdmList = gdmList.Where(gm => gm.MonTheThaoId == monTheThaoId.Value).ToList();
            }

            var gdmIds = gdmList.Select(gm => gm.Id).ToList();

            var allMons = (await _unitOfWork.MonTheThaos.FindAsync(m => m.IsDeleted != true)).ToDictionary(m => m.Id);
            var allReferees = (await _unitOfWork.TrongTais.FindAsync(t => t.IsDeleted != true)).OrderBy(t => t.HoTen).ToList();

            var matches = (await _unitOfWork.TranDaus.FindAsync(t => gdmIds.Contains(t.GiaiDauMonTheThaoId) && t.IsDeleted != true)).ToList();

            if (date.HasValue)
            {
                matches = matches.Where(m => m.ThoiGianDuKien.HasValue && m.ThoiGianDuKien.Value.Date == date.Value.Date).ToList();
            }

            var matchIds = matches.Select(m => m.Id).ToList();

            var pcs = (await _unitOfWork.PhanCongTrongTais.FindAsync(pc => matchIds.Contains(pc.TranDauId) && pc.IsDeleted != true)).ToList();

            if (trongTaiId.HasValue)
            {
                pcs = pcs.Where(pc => pc.TrongTaiId == trongTaiId.Value).ToList();
            }

            var refMap = allReferees.ToDictionary(t => t.Id);
            var matchMap = matches.ToDictionary(m => m.Id);
            var gdmMap = gdmList.ToDictionary(g => g.Id);

            var sanDauIds = matches.Where(m => m.SanDauId.HasValue).Select(m => m.SanDauId!.Value).Distinct().ToList();
            var sanDaus = (await _unitOfWork.SanDaus.FindAsync(s => sanDauIds.Contains(s.Id))).ToDictionary(s => s.Id);

            return pcs
                .GroupBy(pc => pc.TrongTaiId)
                .Where(grp => refMap.ContainsKey(grp.Key))
                .Select(grp =>
                {
                    var referee = refMap[grp.Key];
                    var assignedTasks = grp
                        .Where(pc => matchMap.ContainsKey(pc.TranDauId))
                        .Select(pc =>
                        {
                            var match = matchMap[pc.TranDauId];
                            var gdm = gdmMap.TryGetValue(match.GiaiDauMonTheThaoId, out var g) ? g : null;
                            var mon = (gdm != null && allMons.TryGetValue(gdm.MonTheThaoId, out var m)) ? m : null;
                            var san = (match.SanDauId.HasValue && sanDaus.TryGetValue(match.SanDauId.Value, out var s)) ? s : null;

                            return new RefereeTaskDto
                            {
                                TranDauId = match.Id,
                                SoTran = match.SoTran,
                                TenTran = match.TenTran ?? $"Trận {match.SoTran}",
                                TenMon = mon?.Ten ?? "Môn",
                                TenSan = san?.Ten ?? "Sân đấu",
                                ThoiGianDuKien = match.ThoiGianDuKien,
                                TrangThai = match.TrangThai,
                                VaiTro = pc.VaiTro ?? "Trọng tài",
                                GhiChu = pc.GhiChu
                            };
                        })
                        .OrderBy(t => t.ThoiGianDuKien ?? DateTime.MaxValue)
                        .ToList();

                    return new RefereeScheduleGroupDto
                    {
                        TrongTaiId = referee.Id,
                        MaTrongTai = referee.Ma,
                        HoTen = referee.HoTen,
                        CapBac = referee.CapBac,
                        SoDienThoai = referee.SoDienThoai,
                        TongSoTran = assignedTasks.Count,
                        Tasks = assignedTasks
                    };
                })
                .OrderByDescending(s => s.TongSoTran)
                .ThenBy(s => s.HoTen)
                .ToList();
        }

        /// <summary>
        /// Quét toàn bộ các phân công trọng tài trong giải đấu để phát hiện các cặp trận vi phạm
        /// thời gian nghỉ hồi sức tối thiểu (NghiToiThieuTrongTaiPhut), trùng giờ thi đấu,
        /// và các trường hợp trọng tài bị quá tải số trận trong ngày (SoTranToiDaMoiTrongTaiMoiNgay).
        /// </summary>
        /// <param name="giaiDauId">Mã định danh giải đấu</param>
        /// <returns>Danh sách các xung đột và quá tải được phát hiện</returns>
        public async Task<List<ScanConflictItemDto>> ScanConflictsAsync(int giaiDauId)
        {
            var gdmList = (await _unitOfWork.GiaiDauMonTheThaos.FindAsync(gm => gm.GiaiDauId == giaiDauId && gm.IsDeleted != true)).ToList();
            var gdmIds = gdmList.Select(gm => gm.Id).ToList();

            var allMons = (await _unitOfWork.MonTheThaos.FindAsync(m => m.IsDeleted != true)).ToDictionary(m => m.Id);
            var gdmMap = gdmList.ToDictionary(g => g.Id);
            var allConfigs = (await _unitOfWork.CauHinhLichThiDaus.FindAsync(c => c.IsDeleted != true)).ToDictionary(c => c.MonTheThaoId);

            var matches = (await _unitOfWork.TranDaus.FindAsync(t => gdmIds.Contains(t.GiaiDauMonTheThaoId) && t.ThoiGianDuKien.HasValue && t.IsDeleted != true)).ToList();
            var matchIds = matches.Select(m => m.Id).ToList();
            var matchMap = matches.ToDictionary(m => m.Id);

            var sanDauIds = matches.Where(m => m.SanDauId.HasValue).Select(m => m.SanDauId!.Value).Distinct().ToList();
            var sanDaus = (await _unitOfWork.SanDaus.FindAsync(s => sanDauIds.Contains(s.Id))).ToDictionary(s => s.Id);

            var pcs = (await _unitOfWork.PhanCongTrongTais.FindAsync(pc => matchIds.Contains(pc.TranDauId) && pc.IsDeleted != true)).ToList();
            var allReferees = (await _unitOfWork.TrongTais.FindAsync(t => t.IsDeleted != true)).ToDictionary(t => t.Id);

            var conflicts = new List<ScanConflictItemDto>();
            var groupedByRef = pcs.GroupBy(pc => pc.TrongTaiId);

            foreach (var grp in groupedByRef)
            {
                if (!allReferees.TryGetValue(grp.Key, out var refInfo)) continue;

                var refAssignments = grp
                    .Where(pc => matchMap.ContainsKey(pc.TranDauId))
                    .Select(pc => new { pc, Match = matchMap[pc.TranDauId] })
                    .OrderBy(x => x.Match.ThoiGianDuKien!.Value)
                    .ToList();

                // 1. Quét kiểm tra vi phạm thời gian đệm nghỉ giữa 2 trận liên tiếp (NghiToiThieuTrongTaiPhut)
                for (int i = 0; i < refAssignments.Count - 1; i++)
                {
                    for (int j = i + 1; j < refAssignments.Count; j++)
                    {
                        var a1 = refAssignments[i];
                        var a2 = refAssignments[j];

                        var gdm1 = gdmMap.TryGetValue(a1.Match.GiaiDauMonTheThaoId, out var g1) ? g1 : null;
                        var mon1 = (gdm1 != null && allMons.TryGetValue(gdm1.MonTheThaoId, out var m1)) ? m1 : null;
                        var cfg1 = (gdm1 != null && allConfigs.TryGetValue(gdm1.MonTheThaoId, out var c1)) ? c1 : null;

                        var gdm2 = gdmMap.TryGetValue(a2.Match.GiaiDauMonTheThaoId, out var g2) ? g2 : null;
                        var mon2 = (gdm2 != null && allMons.TryGetValue(gdm2.MonTheThaoId, out var m2)) ? m2 : null;
                        var cfg2 = (gdm2 != null && allConfigs.TryGetValue(gdm2.MonTheThaoId, out var c2)) ? c2 : null;

                        int dur1 = (a1.Match.ThoiGianKetThuc.HasValue && a1.Match.ThoiGianDuKien.HasValue)
                            ? (int)(a1.Match.ThoiGianKetThuc.Value - a1.Match.ThoiGianDuKien.Value).TotalMinutes
                            : (cfg1?.ThoiLuongTranMacDinhPhut > 0 ? cfg1.ThoiLuongTranMacDinhPhut : 60);

                        int dur2 = (a2.Match.ThoiGianKetThuc.HasValue && a2.Match.ThoiGianDuKien.HasValue)
                            ? (int)(a2.Match.ThoiGianKetThuc.Value - a2.Match.ThoiGianDuKien.Value).TotalMinutes
                            : (cfg2?.ThoiLuongTranMacDinhPhut > 0 ? cfg2.ThoiLuongTranMacDinhPhut : 60);

                        DateTime start1 = a1.Match.ThoiGianDuKien!.Value;
                        DateTime end1 = a1.Match.ThoiGianKetThuc ?? start1.AddMinutes(dur1);

                        DateTime start2 = a2.Match.ThoiGianDuKien!.Value;
                        DateTime end2 = a2.Match.ThoiGianKetThuc ?? start2.AddMinutes(dur2);

                        int reqRest = Math.Max(cfg1?.NghiToiThieuTrongTaiPhut ?? 15, cfg2?.NghiToiThieuTrongTaiPhut ?? 15);

                        bool violatesRest = (start1 < end2.AddMinutes(reqRest)) && (start2 < end1.AddMinutes(reqRest));
                        if (violatesRest)
                        {
                            bool directOverlap = (start1 < end2) && (start2 < end1);
                            int gapMin = directOverlap ? 0 : (int)Math.Abs((start2 - end1).TotalMinutes);
                            string loai = directOverlap ? "TrungGio" : "ThieuThoiGianNghi";
                            string moTa = directOverlap
                                ? "Trực tiếp trùng giờ thi đấu giữa 2 trận!"
                                : $"Khoảng cách nghỉ giữa 2 trận chỉ có {gapMin} phút, không đủ thời gian đệm hồi sức tối thiểu ({reqRest} phút) theo quy chuẩn môn.";

                            var san1 = (a1.Match.SanDauId.HasValue && sanDaus.TryGetValue(a1.Match.SanDauId.Value, out var s1)) ? s1 : null;
                            var san2 = (a2.Match.SanDauId.HasValue && sanDaus.TryGetValue(a2.Match.SanDauId.Value, out var s2)) ? s2 : null;

                            conflicts.Add(new ScanConflictItemDto
                            {
                                TrongTaiId = refInfo.Id,
                                MaTrongTai = refInfo.Ma,
                                HoTenTrongTai = refInfo.HoTen,
                                SoDienThoai = refInfo.SoDienThoai,
                                ChenhLechPhut = gapMin,
                                LoaiXungDot = loai,
                                MoTaLoi = moTa,

                                Tran1Id = a1.Match.Id,
                                Tran1So = a1.Match.SoTran,
                                Tran1Ten = a1.Match.TenTran ?? $"Trận {a1.Match.SoTran}",
                                Tran1Mon = mon1?.Ten ?? "Môn 1",
                                Tran1San = san1?.Ten ?? "Sân 1",
                                Tran1ThoiGian = start1,
                                Tran1VaiTro = a1.pc.VaiTro ?? "Trọng tài",

                                Tran2Id = a2.Match.Id,
                                Tran2So = a2.Match.SoTran,
                                Tran2Ten = a2.Match.TenTran ?? $"Trận {a2.Match.SoTran}",
                                Tran2Mon = mon2?.Ten ?? "Môn 2",
                                Tran2San = san2?.Ten ?? "Sân 2",
                                Tran2ThoiGian = start2,
                                Tran2VaiTro = a2.pc.VaiTro ?? "Trọng tài"
                            });
                        }
                    }
                }

                // 2. Quét kiểm tra quá tải số trận trong ngày (SoTranToiDaMoiTrongTaiMoiNgay)
                var assignmentsByDate = refAssignments.GroupBy(x => x.Match.ThoiGianDuKien!.Value.Date);
                foreach (var dayGroup in assignmentsByDate)
                {
                    var dayMatches = dayGroup.ToList();
                    int maxAllowed = dayMatches.Select(dm =>
                    {
                        var gdm = gdmMap.TryGetValue(dm.Match.GiaiDauMonTheThaoId, out var g) ? g : null;
                        int mId = gdm?.MonTheThaoId ?? 0;
                        return allConfigs.TryGetValue(mId, out var c) ? c.SoTranToiDaMoiTrongTaiMoiNgay : 4;
                    }).DefaultIfEmpty(4).Min();

                    if (dayMatches.Count > maxAllowed)
                    {
                        var firstMatch = dayMatches.First();
                        var lastMatch = dayMatches.Last();
                        var gdm1 = gdmMap.TryGetValue(firstMatch.Match.GiaiDauMonTheThaoId, out var g1) ? g1 : null;
                        var mon1 = (gdm1 != null && allMons.TryGetValue(gdm1.MonTheThaoId, out var m1)) ? m1 : null;

                        conflicts.Add(new ScanConflictItemDto
                        {
                            TrongTaiId = refInfo.Id,
                            MaTrongTai = refInfo.Ma,
                            HoTenTrongTai = refInfo.HoTen,
                            SoDienThoai = refInfo.SoDienThoai,
                            ChenhLechPhut = 0,
                            LoaiXungDot = "QuaTaiSoTran",
                            MoTaLoi = $"Quá tải thể lực: Trọng tài bị phân công {dayMatches.Count}/{maxAllowed} trận trong ngày {dayGroup.Key:dd/MM/yyyy} (Vượt ngưỡng quy định môn {mon1?.Ten})!",

                            Tran1Id = firstMatch.Match.Id,
                            Tran1So = firstMatch.Match.SoTran,
                            Tran1Ten = firstMatch.Match.TenTran ?? $"Trận {firstMatch.Match.SoTran}",
                            Tran1Mon = mon1?.Ten ?? "Môn",
                            Tran1San = "Toàn ngày",
                            Tran1ThoiGian = firstMatch.Match.ThoiGianDuKien!.Value,
                            Tran1VaiTro = firstMatch.pc.VaiTro ?? "Trọng tài",

                            Tran2Id = lastMatch.Match.Id,
                            Tran2So = lastMatch.Match.SoTran,
                            Tran2Ten = lastMatch.Match.TenTran ?? $"Trận {lastMatch.Match.SoTran}",
                            Tran2Mon = mon1?.Ten ?? "Môn",
                            Tran2San = "Toàn ngày",
                            Tran2ThoiGian = lastMatch.Match.ThoiGianDuKien!.Value,
                            Tran2VaiTro = lastMatch.pc.VaiTro ?? "Trọng tài"
                        });
                    }
                }
            }

            return conflicts;
        }

        public async Task<List<CategoryAssignmentViewModelDto>> GetCategoryAssignmentsForManagerAsync(int giaiDauId)
        {
            var gdmList = (await _unitOfWork.GiaiDauMonTheThaos.FindAsync(gm => gm.GiaiDauId == giaiDauId && gm.IsDeleted != true)).ToList();
            var monIds = gdmList.Select(x => x.MonTheThaoId).Distinct().ToList();

            var allMons = (await _unitOfWork.MonTheThaos.FindAsync(m => monIds.Contains(m.Id) && m.IsDeleted != true)).ToList();
            var dmIds = allMons.Select(m => m.DanhMucId).Distinct().ToList();

            var allDms = (await _unitOfWork.DanhMucMonTheThaos.FindAsync(dm => dmIds.Contains(dm.Id) && dm.IsDeleted != true)).ToDictionary(dm => dm.Id, dm => dm);

            var allReferees = (await _unitOfWork.TrongTais.FindAsync(t => t.IsDeleted != true)).ToDictionary(r => r.Id, r => r.HoTen);

            var categoriesAssigned = new List<CategoryAssignmentViewModelDto>();
            var dmGroup = allMons.GroupBy(m => m.DanhMucId);

            foreach (var grp in dmGroup)
            {
                if (allDms.TryGetValue(grp.Key, out var dm))
                {
                    var relatedMonIds = grp.Select(m => m.Id).ToList();
                    var relatedGdms = gdmList.Where(gm => relatedMonIds.Contains(gm.MonTheThaoId)).ToList();

                    int? coordId = relatedGdms.FirstOrDefault(x => x.NguoiDieuHanhId.HasValue)?.NguoiDieuHanhId;
                    string? coordName = null;
                    if (coordId.HasValue && allReferees.TryGetValue(coordId.Value, out var rn))
                    {
                        coordName = rn;
                    }

                    categoriesAssigned.Add(new CategoryAssignmentViewModelDto
                    {
                        DanhMucId = dm.Id,
                        TenDanhMuc = dm.Ten,
                        MaDanhMuc = dm.Ma,
                        NguoiDieuHanhId = coordId,
                        TenNguoiDieuHanh = coordName,
                        DanhSachMon = grp.Select(m => m.Ten).ToList(),
                        SoMonCon = grp.Count()
                    });
                }
            }

            return categoriesAssigned;
        }

        /// <summary>
        /// Phân công Trưởng Ban Trọng Tài cho giải đấu (Dành cho Manager/Admin)
        /// </summary>
        /// <param name="giaiDauId">ID của giải đấu</param>
        /// <param name="trongTaiId">ID trọng tài được phân công làm Trưởng ban (hoặc null nếu hủy phân công)</param>
        /// <returns>Bộ giá trị (success, message) kết quả thực hiện</returns>
        public async Task<(bool success, string message)> AssignHeadRefereeForManagerAsync(int giaiDauId, int? trongTaiId)
        {
            var entity = await _unitOfWork.GiaiDaus.GetByIdAsync(giaiDauId);
            if (entity == null || entity.IsDeleted == true)
            {
                return (false, "Không tìm thấy giải đấu.");
            }

            entity.TruongBanTrongTaiId = trongTaiId;
            entity.LastModified = DateTime.UtcNow;
            _unitOfWork.GiaiDaus.Update(entity);
            await _unitOfWork.CompleteAsync();

            return (true, "Đã phân công Trưởng ban trọng tài cho giải đấu thành công!");
        }

        /// <summary>
        /// Phân công Người Điều Hành Môn cho từng danh mục môn thể thao trong giải đấu (Dành cho Manager/Admin)
        /// </summary>
        /// <param name="giaiDauId">ID giải đấu</param>
        /// <param name="danhMucId">ID danh mục môn thể thao</param>
        /// <param name="trongTaiId">ID trọng tài được giao điều hành (hoặc null nếu hủy phân công)</param>
        /// <returns>Bộ giá trị (success, message) kết quả thực hiện</returns>
        public async Task<(bool success, string message)> AssignSportCoordinatorForManagerAsync(int giaiDauId, int danhMucId, int? trongTaiId)
        {
            var mons = (await _unitOfWork.MonTheThaos.FindAsync(m => m.DanhMucId == danhMucId && m.IsDeleted != true)).ToList();
            var monIds = mons.Select(m => m.Id).ToList();

            var gdms = (await _unitOfWork.GiaiDauMonTheThaos.FindAsync(gm => gm.GiaiDauId == giaiDauId && monIds.Contains(gm.MonTheThaoId) && gm.IsDeleted != true)).ToList();
            if (gdms.Count == 0)
            {
                return (false, "Không tìm thấy môn thi đấu tương ứng trong giải.");
            }

            foreach (var gdm in gdms)
            {
                var trackedGdm = await _unitOfWork.GiaiDauMonTheThaos.GetByIdAsync(gdm.Id) ?? gdm;
                trackedGdm.NguoiDieuHanhId = trongTaiId;
                trackedGdm.LastModified = DateTime.UtcNow;
                _unitOfWork.GiaiDauMonTheThaos.Update(trackedGdm);
            }

            await _unitOfWork.CompleteAsync();

            return (true, "Đã phân công Người điều hành môn thành công!");
        }
    }
}
