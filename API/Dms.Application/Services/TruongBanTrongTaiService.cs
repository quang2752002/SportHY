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

        private static readonly IReadOnlyDictionary<string, string> RefereeRoleAliases =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["Trọng tài chính"] = "Trọng tài chính",
                ["Chính"] = "Trọng tài chính",
                ["TrongTaiChinh"] = "Trọng tài chính",
                ["Trọng tài phụ"] = "Trọng tài phụ 1",
                ["Trọng tài phụ 1"] = "Trọng tài phụ 1",
                ["TrongTaiPhu"] = "Trọng tài phụ 1",
                ["TrongTaiPhu1"] = "Trọng tài phụ 1",
                ["Trọng tài phụ 2"] = "Trọng tài phụ 2",
                ["TrongTaiPhu2"] = "Trọng tài phụ 2",
                ["Trọng tài bàn"] = "Trọng tài bàn",
                ["Bàn"] = "Trọng tài bàn",
                ["TrongTaiBan"] = "Trọng tài bàn",
                ["Giám sát trận đấu"] = "Giám sát trận đấu",
                ["GiamSat"] = "Giám sát trận đấu",
                ["GiamSatTranDau"] = "Giám sát trận đấu"
            };

        public TruongBanTrongTaiService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Chuẩn hóa các tên vai trò phân công cũ và mới về cùng một giá trị hiển thị.
        /// </summary>
        /// <param name="role">Tên vai trò cần chuẩn hóa.</param>
        /// <returns>Tên vai trò chuẩn hoặc chuỗi rỗng nếu vai trò không hợp lệ.</returns>
        private static string NormalizeRefereeRole(string? role)
        {
            if (string.IsNullOrWhiteSpace(role)) return string.Empty;
            return RefereeRoleAliases.TryGetValue(role.Trim(), out var canonical) ? canonical : string.Empty;
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

        /// <summary>
        /// Lấy các giải đấu đang có ít nhất một trận được phân công cho trọng tài.
        /// </summary>
        /// <param name="refereeId">ID hồ sơ trọng tài liên kết với tài khoản.</param>
        /// <returns>Danh sách ID giải đấu có phân công trọng tài còn hiệu lực.</returns>
        public async Task<List<int>> GetRefereeTournamentIdsAsync(int refereeId)
        {
            if (refereeId <= 0) return new List<int>();

            var assignments = (await _unitOfWork.PhanCongTrongTais.FindAsync(assignment =>
                assignment.TrongTaiId == refereeId && assignment.IsDeleted != true)).ToList();
            var matchIds = assignments.Select(assignment => assignment.TranDauId).Distinct().ToList();
            if (matchIds.Count == 0) return new List<int>();

            var matches = (await _unitOfWork.TranDaus.FindAsync(match =>
                matchIds.Contains(match.Id) && match.IsDeleted != true)).ToList();
            var gdmIds = matches.Select(match => match.GiaiDauMonTheThaoId).Distinct().ToList();
            if (gdmIds.Count == 0) return new List<int>();

            var tournamentIds = (await _unitOfWork.GiaiDauMonTheThaos.FindAsync(gdm =>
                    gdmIds.Contains(gdm.Id) && gdm.IsDeleted != true))
                .Select(gdm => gdm.GiaiDauId)
                .Distinct()
                .ToList();
            if (tournamentIds.Count == 0) return new List<int>();

            return (await _unitOfWork.GiaiDaus.FindAsync(tournament =>
                    tournamentIds.Contains(tournament.Id) && tournament.IsDeleted != true))
                .Select(tournament => tournament.Id)
                .Distinct()
                .ToList();
        }

        /// <summary>
        /// Kiểm tra trọng tài có được phân công điều hành một trận đấu cụ thể hay không.
        /// </summary>
        /// <param name="refereeId">ID hồ sơ trọng tài.</param>
        /// <param name="tranDauId">ID trận đấu cần kiểm tra.</param>
        /// <returns>True nếu trọng tài có phân công còn hiệu lực trong trận đấu thuộc giải còn hiệu lực.</returns>
        public async Task<bool> IsRefereeAssignedToMatchAsync(int refereeId, int tranDauId)
        {
            if (refereeId <= 0 || tranDauId <= 0) return false;

            var match = (await _unitOfWork.TranDaus.FindAsync(item =>
                item.Id == tranDauId && item.IsDeleted != true)).FirstOrDefault();
            if (match == null) return false;

            var gdm = (await _unitOfWork.GiaiDauMonTheThaos.FindAsync(item =>
                item.Id == match.GiaiDauMonTheThaoId && item.IsDeleted != true)).FirstOrDefault();
            if (gdm == null) return false;

            var tournament = (await _unitOfWork.GiaiDaus.FindAsync(item =>
                item.Id == gdm.GiaiDauId && item.IsDeleted != true)).FirstOrDefault();
            if (tournament == null) return false;

            return (await _unitOfWork.PhanCongTrongTais.FindAsync(assignment =>
                    assignment.TranDauId == tranDauId &&
                    assignment.TrongTaiId == refereeId &&
                    assignment.IsDeleted != true))
                .Any();
        }

        /// <summary>
        /// Kiểm tra trận đấu có thuộc giải đấu được chỉ định hay không.
        /// </summary>
        /// <param name="tranDauId">ID trận đấu cần kiểm tra.</param>
        /// <param name="giaiDauId">ID giải đấu cần đối chiếu.</param>
        /// <returns>True nếu trận đấu thuộc giải đấu và các bản ghi liên quan chưa bị xóa mềm.</returns>
        public async Task<bool> IsMatchInTournamentAsync(int tranDauId, int giaiDauId)
        {
            if (tranDauId <= 0 || giaiDauId <= 0) return false;

            var match = (await _unitOfWork.TranDaus.FindAsync(item =>
                item.Id == tranDauId && item.IsDeleted != true)).FirstOrDefault();
            if (match == null) return false;

            var gdm = (await _unitOfWork.GiaiDauMonTheThaos.FindAsync(item =>
                item.Id == match.GiaiDauMonTheThaoId &&
                item.GiaiDauId == giaiDauId &&
                item.IsDeleted != true)).FirstOrDefault();
            if (gdm == null) return false;

            return (await _unitOfWork.GiaiDaus.FindAsync(item =>
                    item.Id == giaiDauId && item.IsDeleted != true))
                .Any();
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

        /// <summary>
        /// Lấy thông tin hồ sơ và lịch sử phân công của trọng tài, có thể giới hạn lịch sử theo một giải.
        /// </summary>
        /// <param name="id">ID hồ sơ trọng tài.</param>
        /// <param name="giaiDauId">ID giải cần giới hạn lịch sử; null để lấy lịch sử của mọi giải.</param>
        /// <returns>Thông tin trọng tài và lịch sử phù hợp, hoặc null nếu không tìm thấy hồ sơ.</returns>
        public async Task<RefereeDetailsDto?> GetRefereeDetailsAsync(int id, int? giaiDauId = null)
        {
            var referee = (await _unitOfWork.TrongTais.FindAsync(t => t.Id == id && t.IsDeleted != true)).FirstOrDefault();
            if (referee == null) return null;

            var pcs = (await _unitOfWork.PhanCongTrongTais.FindAsync(pc => pc.TrongTaiId == id && pc.IsDeleted != true)).ToList();
            var matchIds = pcs.Select(x => x.TranDauId).ToList();
            var matches = (await _unitOfWork.TranDaus.FindAsync(m => matchIds.Contains(m.Id) && m.IsDeleted != true)).ToList();

            var gdmIds = matches.Select(m => m.GiaiDauMonTheThaoId).Distinct().ToList();
            var gdms = (await _unitOfWork.GiaiDauMonTheThaos.FindAsync(gm =>
                gdmIds.Contains(gm.Id) &&
                (!giaiDauId.HasValue || gm.GiaiDauId == giaiDauId.Value) &&
                gm.IsDeleted != true)).ToDictionary(x => x.Id);
            var scopedGdmIds = gdms.Keys.ToHashSet();
            matches = matches.Where(match => scopedGdmIds.Contains(match.GiaiDauMonTheThaoId)).ToList();
            var scopedMatchIds = matches.Select(match => match.Id).ToHashSet();
            pcs = pcs.Where(assignment => scopedMatchIds.Contains(assignment.TranDauId)).ToList();

            var monIds = gdms.Values.Select(x => x.MonTheThaoId).Distinct().ToList();
            var mons = (await _unitOfWork.MonTheThaos.FindAsync(m => monIds.Contains(m.Id) && m.IsDeleted != true)).ToDictionary(x => x.Id);

            var giaiDauIds = gdms.Values.Select(x => x.GiaiDauId).Distinct().ToList();
            var giaiDaus = (await _unitOfWork.GiaiDaus.FindAsync(g => giaiDauIds.Contains(g.Id) && g.IsDeleted != true)).ToDictionary(x => x.Id);

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

        public async Task<List<MatchAssignmentDto>> GetMatchAssignmentsAsync(
            int giaiDauId,
            int? monTheThaoId,
            string? status,
            DateTime? date,
            int? trongTaiId = null)
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

            if (trongTaiId.HasValue)
            {
                var assignedMatchIds = phanCongs
                    .Where(pc => pc.TrongTaiId == trongTaiId.Value)
                    .Select(pc => pc.TranDauId)
                    .ToHashSet();
                matches = matches.Where(match => assignedMatchIds.Contains(match.Id)).ToList();
            }

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

                var ttChinh = matchPcs.FirstOrDefault(pc =>
                    pc.VaiTro == "Trọng tài chính" ||
                    pc.VaiTro == "Chính" ||
                    pc.VaiTro == "TrongTaiChinh");
                var ttPhu1 = matchPcs.FirstOrDefault(pc =>
                    pc.VaiTro == "Trọng tài phụ 1" ||
                    pc.VaiTro == "Trọng tài phụ" ||
                    pc.VaiTro == "TrongTaiPhu" ||
                    pc.VaiTro == "TrongTaiPhu1");
                var ttPhu2 = matchPcs.FirstOrDefault(pc =>
                    pc.VaiTro == "Trọng tài phụ 2" ||
                    pc.VaiTro == "TrongTaiPhu2");
                var ttBan = matchPcs.FirstOrDefault(pc =>
                    pc.VaiTro == "Trọng tài bàn" ||
                    pc.VaiTro == "Bàn" ||
                    pc.VaiTro == "TrongTaiBan");
                var ttGiamSat = matchPcs.FirstOrDefault(pc =>
                    pc.VaiTro == "Giám sát trận đấu" ||
                    pc.VaiTro == "GiamSat" ||
                    pc.VaiTro == "GiamSatTranDau");

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

            // Một trọng tài không được giữ đồng thời nhiều vai trò trong cùng một trận.
            // Kiểm tra ngay tại đây để giao diện có thể cảnh báo trước khi tạo bản nháp.
            var sameMatchAssignment = (await _unitOfWork.PhanCongTrongTais.FindAsync(pc =>
                pc.TranDauId == tranDauId &&
                pc.TrongTaiId == trongTaiId &&
                pc.IsDeleted != true)).FirstOrDefault();
            if (sameMatchAssignment != null)
            {
                conflicts.Add(new ConflictItemDto
                {
                    TranDauId = match.Id,
                    SoTran = match.SoTran,
                    TenTran = match.TenTran ?? $"Trận số {match.SoTran}",
                    ThoiGian = match.ThoiGianDuKien.Value.ToString("HH:mm dd/MM/yyyy"),
                    TenMon = mon?.Ten ?? "Môn thi đấu",
                    VaiTro = sameMatchAssignment.VaiTro ?? "Trọng tài",
                    DiffMinutes = 0,
                    RequiredRestMinutes = nghiToiThieuPhut,
                    LoaiXungDot = "TrungTran",
                    MoTa = $"Trọng tài đã được phân công ở vai trò {sameMatchAssignment.VaiTro ?? "khác"} trong chính trận này."
                });
            }

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

            var canonicalRole = NormalizeRefereeRole(vaiTro);
            if (string.IsNullOrWhiteSpace(canonicalRole))
            {
                return (false, "Vai trò phân công không hợp lệ.", null);
            }

            var gdm = (await _unitOfWork.GiaiDauMonTheThaos.FindAsync(item =>
                item.Id == match.GiaiDauMonTheThaoId && item.IsDeleted != true)).FirstOrDefault();
            var tournament = gdm == null
                ? null
                : (await _unitOfWork.GiaiDaus.FindAsync(item => item.Id == gdm.GiaiDauId && item.IsDeleted != true)).FirstOrDefault();

            if (tournament == null)
            {
                return (false, "Không tìm thấy giải đấu của trận đấu.", null);
            }

            if (trongTaiId.HasValue && tournament.TruongBanTrongTaiId == trongTaiId.Value)
            {
                return (false, "Trưởng ban trọng tài không được phép phân công điều hành trận đấu.", null);
            }

            var existingPcs = (await _unitOfWork.PhanCongTrongTais.FindAsync(pc =>
                pc.TranDauId == tranDauId && pc.IsDeleted != true))
                .Where(pc => NormalizeRefereeRole(pc.VaiTro) == canonicalRole)
                .ToList();

            if (!trongTaiId.HasValue || trongTaiId.Value == 0)
            {
                foreach (var pc in existingPcs)
                {
                    pc.IsDeleted = true;
                    _unitOfWork.PhanCongTrongTais.Update(pc);
                }
                await _unitOfWork.CompleteAsync();
                return (true, $"Đã hủy phân công {canonicalRole} cho trận đấu.", null);
            }

            var referee = (await _unitOfWork.TrongTais.FindAsync(t =>
                t.Id == trongTaiId.Value && t.IsDeleted != true && t.TrangThai)).FirstOrDefault();
            if (referee == null)
            {
                return (false, "Không tìm thấy trọng tài đang hoạt động.", null);
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
                pc.VaiTro = canonicalRole;
                pc.LastModified = DateTime.UtcNow;
                _unitOfWork.PhanCongTrongTais.Update(pc);
            }
            else
            {
                var newPc = new PhanCongTrongTai
                {
                    TranDauId = tranDauId,
                    TrongTaiId = trongTaiId.Value,
                    VaiTro = canonicalRole,
                    Created = DateTime.UtcNow
                };
                await _unitOfWork.PhanCongTrongTais.AddAsync(newPc);
            }

            await _unitOfWork.CompleteAsync();

            return (true, $"Đã phân công {canonicalRole}: {referee.HoTen} thành công!", referee.HoTen);
        }

        /// <summary>
        /// Lập bản nháp tự động phân công các vị trí trọng tài cho những trận đấu đã được xếp lịch.
        /// Chức năng độc lập với thuật toán tự động chia lịch đấu: chỉ đọc các trận hiện có,
        /// kiểm tra lịch toàn giải, ưu tiên cân bằng số trận giữa các trọng tài và trả về đề xuất
        /// mà không ghi dữ liệu vào cơ sở dữ liệu.
        /// </summary>
        /// <param name="request">Phạm vi, trọng tài được chọn và các thay đổi nháp hiện có.</param>
        /// <returns>Kết quả nháp kèm danh sách vị trí đề xuất và vị trí chưa thể gán.</returns>
        public async Task<AutoAssignRefereesResultDto> AutoAssignRefereesAsync(AutoAssignRefereesRequestDto request)
        {
            var result = new AutoAssignRefereesResultDto();

            if (request == null || request.GiaiDauId <= 0)
            {
                result.Message = "Giải đấu không hợp lệ.";
                return result;
            }

            var tournament = (await _unitOfWork.GiaiDaus.FindAsync(item =>
                item.Id == request.GiaiDauId && item.IsDeleted != true)).FirstOrDefault();
            if (tournament == null)
            {
                result.Message = "Không tìm thấy giải đấu.";
                return result;
            }

            var headRefereeId = tournament.TruongBanTrongTaiId;

            var canonicalRoles = new[]
            {
                "Trọng tài chính",
                "Trọng tài phụ 1",
                "Trọng tài phụ 2",
                "Trọng tài bàn",
                "Giám sát trận đấu"
            };

            var requestedRoles = (request.VaiTros ?? new List<string>())
                .Select(NormalizeRefereeRole)
                .Where(role => !string.IsNullOrWhiteSpace(role))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (requestedRoles.Count == 0)
            {
                requestedRoles.AddRange(canonicalRoles);
            }

            var allGdm = (await _unitOfWork.GiaiDauMonTheThaos.FindAsync(gdm =>
                gdm.GiaiDauId == request.GiaiDauId &&
                gdm.IsDeleted != true &&
                gdm.TrangThai)).ToList();

            if (allGdm.Count == 0)
            {
                result.Message = "Giải đấu chưa có môn thi đấu đang hoạt động.";
                return result;
            }

            var selectedGdm = request.MonTheThaoId.HasValue
                ? allGdm.Where(gdm => gdm.MonTheThaoId == request.MonTheThaoId.Value).ToList()
                : allGdm;
            var selectedGdmIds = selectedGdm.Select(gdm => gdm.Id).ToHashSet();

            if (selectedGdmIds.Count == 0)
            {
                result.Message = "Không tìm thấy môn thi đấu thuộc giải đã chọn.";
                return result;
            }

            var allMatches = (await _unitOfWork.TranDaus.FindAsync(match =>
                selectedGdmIds.Contains(match.GiaiDauMonTheThaoId) &&
                match.IsDeleted != true &&
                match.ThoiGianDuKien.HasValue)).ToList();

            var targetMatches = allMatches.AsEnumerable();
            if (request.ChiPhanCongTranChuaDau)
            {
                targetMatches = targetMatches.Where(match => match.TrangThai == "ChuaDau");
            }

            if (request.Date.HasValue)
            {
                targetMatches = targetMatches.Where(match => match.ThoiGianDuKien!.Value.Date == request.Date.Value.Date);
            }

            var targetMatchList = targetMatches
                .OrderBy(match => match.ThoiGianDuKien!.Value)
                .ThenBy(match => match.SoTran)
                .ToList();

            result.MatchesConsidered = targetMatchList.Count;
            if (targetMatchList.Count == 0)
            {
                result.Success = true;
                result.Message = "Không có trận đấu phù hợp để tự động phân công.";
                return result;
            }

            // Nạp toàn bộ trận có lịch trong giải để kiểm tra xung đột xuyên môn.
            var allTournamentGdmIds = allGdm.Select(gdm => gdm.Id).ToHashSet();
            var tournamentMatches = (await _unitOfWork.TranDaus.FindAsync(match =>
                allTournamentGdmIds.Contains(match.GiaiDauMonTheThaoId) &&
                match.IsDeleted != true &&
                match.ThoiGianDuKien.HasValue)).ToList();
            var matchMap = tournamentMatches.ToDictionary(match => match.Id);
            var gdmMap = allGdm.ToDictionary(gdm => gdm.Id);

            var matchIds = tournamentMatches.Select(match => match.Id).ToHashSet();
            var persistedAssignments = (await _unitOfWork.PhanCongTrongTais.FindAsync(pc =>
                matchIds.Contains(pc.TranDauId) && pc.IsDeleted != true)).ToList();

            // Tạo ảnh chụp dữ liệu để mô phỏng nháp. Trưởng ban không được giữ vị trí điều hành trận đấu.
            var existingAssignments = persistedAssignments
                .Where(assignment => !headRefereeId.HasValue || assignment.TrongTaiId != headRefereeId.Value)
                .Select(assignment => new PhanCongTrongTai
                {
                    Id = assignment.Id,
                    TranDauId = assignment.TranDauId,
                    TrongTaiId = assignment.TrongTaiId,
                    VaiTro = NormalizeRefereeRole(assignment.VaiTro)
                })
                .ToList();

            var draftChanges = (request.DraftChanges ?? new List<RefereeAssignmentDraftItemDto>())
                .Select(change => new
                {
                    Change = change,
                    VaiTro = NormalizeRefereeRole(change.VaiTro)
                })
                .Where(item => item.Change.TranDauId > 0 && !string.IsNullOrWhiteSpace(item.VaiTro) && matchMap.ContainsKey(item.Change.TranDauId))
                .GroupBy(item => new { item.Change.TranDauId, item.VaiTro })
                .Select(group => group.Last())
                .ToList();

            var draftAssignmentId = -1;
            foreach (var draft in draftChanges)
            {
                existingAssignments.RemoveAll(assignment =>
                    assignment.TranDauId == draft.Change.TranDauId &&
                    NormalizeRefereeRole(assignment.VaiTro) == draft.VaiTro);

                if (draft.Change.TrongTaiId.HasValue && draft.Change.TrongTaiId.Value > 0 &&
                    (!headRefereeId.HasValue || draft.Change.TrongTaiId.Value != headRefereeId.Value))
                {
                    existingAssignments.Add(new PhanCongTrongTai
                    {
                        Id = draftAssignmentId--,
                        TranDauId = draft.Change.TranDauId,
                        TrongTaiId = draft.Change.TrongTaiId.Value,
                        VaiTro = draft.VaiTro
                    });
                }
            }

            var activeReferees = (await _unitOfWork.TrongTais.FindAsync(referee =>
                referee.IsDeleted != true &&
                referee.TrangThai &&
                (!headRefereeId.HasValue || referee.Id != headRefereeId.Value)))
                .OrderBy(referee => referee.HoTen)
                .ToList();

            var selectedRefereeIds = (request.TrongTaiIds ?? new List<int>())
                .Where(id => id > 0)
                .Distinct()
                .ToHashSet();
            if (selectedRefereeIds.Count > 0)
            {
                activeReferees = activeReferees
                    .Where(referee => selectedRefereeIds.Contains(referee.Id))
                    .ToList();
            }

            if (activeReferees.Count == 0)
            {
                result.Message = "Không có trọng tài hợp lệ trong danh sách đã chọn để phân công.";
                return result;
            }

            var monIds = allGdm.Select(gdm => gdm.MonTheThaoId).Distinct().ToHashSet();
            var configurations = (await _unitOfWork.CauHinhLichThiDaus.FindAsync(config =>
                config.IsDeleted != true && monIds.Contains(config.MonTheThaoId)))
                .ToDictionary(config => config.MonTheThaoId);

            int GetSportId(TranDau match)
            {
                return gdmMap.TryGetValue(match.GiaiDauMonTheThaoId, out var gdm) ? gdm.MonTheThaoId : 0;
            }

            DateTime GetMatchEnd(TranDau match)
            {
                var start = match.ThoiGianDuKien!.Value;
                if (match.ThoiGianKetThuc.HasValue && match.ThoiGianKetThuc.Value > start)
                {
                    return match.ThoiGianKetThuc.Value;
                }

                var sportId = GetSportId(match);
                var duration = configurations.TryGetValue(sportId, out var config) && config.ThoiLuongTranMacDinhPhut > 0
                    ? config.ThoiLuongTranMacDinhPhut
                    : 60;
                return start.AddMinutes(duration);
            }

            int GetMaxMatchesPerDay(TranDau match)
            {
                var sportId = GetSportId(match);
                return configurations.TryGetValue(sportId, out var config) && config.SoTranToiDaMoiTrongTaiMoiNgay > 0
                    ? config.SoTranToiDaMoiTrongTaiMoiNgay
                    : 4;
            }

            int GetRequiredRestMinutes(TranDau match)
            {
                var sportId = GetSportId(match);
                return configurations.TryGetValue(sportId, out var config) && config.NghiToiThieuTrongTaiPhut > 0
                    ? config.NghiToiThieuTrongTaiPhut
                    : 15;
            }

            var refereeIntervals = new Dictionary<int, List<(int TranDauId, DateTime Start, DateTime End, string VaiTro)>>();
            foreach (var referee in activeReferees)
            {
                refereeIntervals[referee.Id] = new List<(int, DateTime, DateTime, string)>();
            }

            foreach (var assignment in existingAssignments)
            {
                if (!refereeIntervals.ContainsKey(assignment.TrongTaiId) || !matchMap.TryGetValue(assignment.TranDauId, out var assignedMatch))
                {
                    continue;
                }

                refereeIntervals[assignment.TrongTaiId].Add((
                    assignedMatch.Id,
                    assignedMatch.ThoiGianDuKien!.Value,
                    GetMatchEnd(assignedMatch),
                    NormalizeRefereeRole(assignment.VaiTro)));
            }

            var plans = new List<(TranDau Match, string VaiTro, TrongTai Referee)>();
            var plannedRefsByMatch = new Dictionary<int, HashSet<int>>();
            var releasedAssignmentIds = new HashSet<int>();

            foreach (var match in targetMatchList)
            {
                plannedRefsByMatch[match.Id] = existingAssignments
                    .Where(assignment => assignment.TranDauId == match.Id)
                    .Select(assignment => assignment.TrongTaiId)
                    .ToHashSet();
            }

            void RebuildMatchReferees(int tranDauId)
            {
                var refs = existingAssignments
                    .Where(assignment => assignment.TranDauId == tranDauId && !releasedAssignmentIds.Contains(assignment.Id))
                    .Select(assignment => assignment.TrongTaiId)
                    .ToHashSet();

                foreach (var plan in plans.Where(plan => plan.Match.Id == tranDauId))
                {
                    refs.Add(plan.Referee.Id);
                }

                plannedRefsByMatch[tranDauId] = refs;
            }

            string? GetConflictReason(int refereeId, TranDau targetMatch)
            {
                if (plannedRefsByMatch.TryGetValue(targetMatch.Id, out var assignedRefs) && assignedRefs.Contains(refereeId))
                {
                    return "Trọng tài này đã có vị trí khác trong cùng trận.";
                }

                var intervals = refereeIntervals.GetValueOrDefault(refereeId, new List<(int TranDauId, DateTime Start, DateTime End, string VaiTro)>())
                    .Where(interval => interval.TranDauId != targetMatch.Id)
                    .GroupBy(interval => interval.TranDauId)
                    .Select(group => group.First())
                    .ToList();

                var targetStart = targetMatch.ThoiGianDuKien!.Value;
                var targetEnd = GetMatchEnd(targetMatch);
                var sameDayMatches = intervals.Count(interval => interval.Start.Date == targetStart.Date);
                var maxPerDay = GetMaxMatchesPerDay(targetMatch);
                if (sameDayMatches >= maxPerDay)
                {
                    return $"Đã đủ {sameDayMatches}/{maxPerDay} trận trong ngày {targetStart:dd/MM/yyyy}.";
                }

                foreach (var interval in intervals)
                {
                    if (!matchMap.TryGetValue(interval.TranDauId, out var otherMatch))
                    {
                        continue;
                    }

                    var requiredRest = Math.Max(GetRequiredRestMinutes(targetMatch), GetRequiredRestMinutes(otherMatch));
                    var violatesRest = targetStart < interval.End.AddMinutes(requiredRest) &&
                                       interval.Start < targetEnd.AddMinutes(requiredRest);
                    if (!violatesRest)
                    {
                        continue;
                    }

                    var directOverlap = targetStart < interval.End && interval.Start < targetEnd;
                    return directOverlap
                        ? $"Trùng giờ với trận #{otherMatch.SoTran}."
                        : $"Chưa đủ thời gian nghỉ tối thiểu {requiredRest} phút sau trận #{otherMatch.SoTran}.";
                }

                return null;
            }

            foreach (var match in targetMatchList)
            {
                foreach (var role in requestedRoles)
                {
                    var currentAssignments = existingAssignments
                        .Where(assignment => assignment.TranDauId == match.Id &&
                                             !releasedAssignmentIds.Contains(assignment.Id) &&
                                             NormalizeRefereeRole(assignment.VaiTro).Equals(role, StringComparison.OrdinalIgnoreCase))
                        .ToList();

                    if (currentAssignments.Count > 0 && request.ChiLapViTriTrong)
                    {
                        continue;
                    }

                    var temporarilyReleasedIntervals = new List<(int RefereeId, (int TranDauId, DateTime Start, DateTime End, string VaiTro) Interval)>();
                    if (currentAssignments.Count > 0)
                    {
                        foreach (var currentAssignment in currentAssignments)
                        {
                            releasedAssignmentIds.Add(currentAssignment.Id);
                            if (refereeIntervals.TryGetValue(currentAssignment.TrongTaiId, out var intervals))
                            {
                                var removed = intervals
                                    .Where(interval => interval.TranDauId == match.Id && interval.VaiTro.Equals(role, StringComparison.OrdinalIgnoreCase))
                                    .ToList();
                                intervals.RemoveAll(interval => interval.TranDauId == match.Id && interval.VaiTro.Equals(role, StringComparison.OrdinalIgnoreCase));
                                temporarilyReleasedIntervals.AddRange(removed.Select(interval => (currentAssignment.TrongTaiId, interval)));
                            }
                        }

                        RebuildMatchReferees(match.Id);
                    }

                    var candidates = activeReferees
                        .Where(referee => GetConflictReason(referee.Id, match) == null)
                        .Select(referee => new
                        {
                            Referee = referee,
                            TotalMatches = refereeIntervals[referee.Id].Select(interval => interval.TranDauId).Distinct().Count(),
                            SameDayMatches = refereeIntervals[referee.Id]
                                .Where(interval => interval.Start.Date == match.ThoiGianDuKien!.Value.Date)
                                .Select(interval => interval.TranDauId)
                                .Distinct()
                                .Count()
                        })
                        .OrderBy(candidate => candidate.TotalMatches)
                        .ThenBy(candidate => candidate.SameDayMatches)
                        .ThenBy(candidate => candidate.Referee.HoTen)
                        .ToList();

                    // Cân bằng tải là điều kiện bắt buộc: chỉ chọn trọng tài đang có ít
                    // trận nhất trong toàn bộ nhóm đã chọn. Nếu nhóm này đều xung đột lịch,
                    // để trống vị trí thay vì giao thêm cho người đang có nhiều trận hơn.
                    var minimumTotalMatches = activeReferees
                        .Select(referee => refereeIntervals[referee.Id]
                            .Select(interval => interval.TranDauId)
                            .Distinct()
                            .Count())
                        .DefaultIfEmpty(0)
                        .Min();
                    var balancedCandidates = candidates
                        .Where(candidate => candidate.TotalMatches == minimumTotalMatches)
                        .ToList();

                    // Nếu nhiều người cùng ít trận, ưu tiên người ít trận trong ngày hơn,
                    // sau đó random để không luôn chọn theo thứ tự tên.
                    TrongTai? selected = null;
                    if (balancedCandidates.Count > 0)
                    {
                        var minimumSameDayMatches = balancedCandidates
                            .Min(candidate => candidate.SameDayMatches);
                        var leastLoaded = balancedCandidates
                            .Where(candidate => candidate.SameDayMatches == minimumSameDayMatches)
                            .ToList();
                        selected = leastLoaded[Random.Shared.Next(leastLoaded.Count)].Referee;
                    }
                    if (selected == null)
                    {
                        // Không tìm được phương án thay thế thì khôi phục phân công cũ.
                        foreach (var currentAssignment in currentAssignments)
                        {
                            releasedAssignmentIds.Remove(currentAssignment.Id);
                        }

                        foreach (var released in temporarilyReleasedIntervals)
                        {
                            refereeIntervals[released.RefereeId].Add(released.Interval);
                        }

                        RebuildMatchReferees(match.Id);
                        result.Unassigned.Add(new AutoAssignRefereeUnassignedDto
                        {
                            TranDauId = match.Id,
                            SoTran = match.SoTran,
                            TenTran = match.TenTran ?? $"Trận {match.SoTran}",
                            VaiTro = role,
                            LyDo = balancedCandidates.Count == 0
                                ? "Không thể giữ cân bằng số trận: các trọng tài đang ít trận nhất đều trùng lịch, thiếu thời gian nghỉ hoặc đã đạt giới hạn trong ngày."
                                : "Không còn trọng tài phù hợp do trùng lịch, thiếu thời gian nghỉ hoặc vượt giới hạn số trận trong ngày."
                        });
                        continue;
                    }

                    var selectedInterval = (match.Id, match.ThoiGianDuKien!.Value, GetMatchEnd(match), role);
                    refereeIntervals[selected.Id].Add(selectedInterval);
                    plans.Add((match, role, selected));
                    RebuildMatchReferees(match.Id);
                    result.Assignments.Add(new AutoAssignRefereeItemDto
                    {
                        TranDauId = match.Id,
                        SoTran = match.SoTran,
                        TenTran = match.TenTran ?? $"Trận {match.SoTran}",
                        VaiTro = role,
                        TrongTaiId = selected.Id,
                        HoTenTrongTai = selected.HoTen
                    });
                }
            }

            result.Success = true;
            result.PositionsAssigned = result.Assignments.Count;
            result.PositionsUnassigned = result.Unassigned.Count;
            result.MatchesAssigned = result.Assignments.Select(assignment => assignment.TranDauId).Distinct().Count();
            result.Message = result.PositionsAssigned == 0
                ? "Không có vị trí trống nào có thể đưa vào bản nháp phân công."
                : $"Đã tạo nháp {result.PositionsAssigned} vị trí cho {result.MatchesAssigned} trận. " +
                  (result.PositionsUnassigned > 0
                      ? $"Còn {result.PositionsUnassigned} vị trí chưa thể phân công do không đủ điều kiện."
                      : "Tất cả vị trí yêu cầu đã được đưa vào nháp. Hãy bấm Lưu để xác nhận.");

            return result;
        }

        /// <summary>
        /// Xác nhận và lưu bản nháp phân công trọng tài cho một giải đấu.
        /// Mọi phân công bị thay đổi hoặc hủy đều được xóa mềm trước khi thêm bản ghi mới;
        /// Trưởng ban trọng tài bị loại khỏi toàn bộ kết quả lưu theo quy định nghiệp vụ.
        /// </summary>
        /// <param name="request">Giải đấu và các thay đổi nháp theo từng trận, từng vai trò.</param>
        /// <param name="savedBy">Tài khoản thực hiện để lưu lịch sử thay đổi.</param>
        /// <returns>Kết quả lưu gồm số vị trí xử lý, số bản ghi tạo mới và số bản ghi xóa mềm.</returns>
        public async Task<SaveRefereeAssignmentDraftResultDto> SaveRefereeAssignmentDraftAsync(
            SaveRefereeAssignmentDraftRequestDto request,
            string? savedBy = null)
        {
            var result = new SaveRefereeAssignmentDraftResultDto();
            if (request == null || request.GiaiDauId <= 0)
            {
                result.Message = "Giải đấu không hợp lệ.";
                return result;
            }

            var tournament = (await _unitOfWork.GiaiDaus.FindAsync(item =>
                item.Id == request.GiaiDauId && item.IsDeleted != true)).FirstOrDefault();
            if (tournament == null)
            {
                result.Message = "Không tìm thấy giải đấu.";
                return result;
            }

            var gdmIds = (await _unitOfWork.GiaiDauMonTheThaos.FindAsync(item =>
                item.GiaiDauId == request.GiaiDauId && item.IsDeleted != true))
                .Select(item => item.Id)
                .ToHashSet();
            var matches = (await _unitOfWork.TranDaus.FindAsync(item =>
                gdmIds.Contains(item.GiaiDauMonTheThaoId) && item.IsDeleted != true)).ToList();
            var matchIds = matches.Select(match => match.Id).ToHashSet();

            var draftBySlot = new Dictionary<string, RefereeAssignmentDraftItemDto>(StringComparer.OrdinalIgnoreCase);
            foreach (var change in request.Changes ?? new List<RefereeAssignmentDraftItemDto>())
            {
                var canonicalRole = NormalizeRefereeRole(change.VaiTro);
                if (change.TranDauId <= 0 || !matchIds.Contains(change.TranDauId) || string.IsNullOrWhiteSpace(canonicalRole))
                {
                    result.Message = "Bản nháp chứa trận đấu hoặc vai trò không hợp lệ.";
                    return result;
                }

                draftBySlot[$"{change.TranDauId}:{canonicalRole}"] = new RefereeAssignmentDraftItemDto
                {
                    TranDauId = change.TranDauId,
                    VaiTro = canonicalRole,
                    TrongTaiId = change.TrongTaiId,
                    Force = change.Force
                };
            }

            var existingAssignments = (await _unitOfWork.PhanCongTrongTais.FindAsync(assignment =>
                matchIds.Contains(assignment.TranDauId) && assignment.IsDeleted != true)).ToList();

            // Dọn các phân công cũ của Trưởng ban trong lần lưu kế tiếp để đảm bảo quy định được áp dụng cả với dữ liệu cũ.
            if (tournament.TruongBanTrongTaiId.HasValue)
            {
                foreach (var assignment in existingAssignments.Where(assignment =>
                             assignment.TrongTaiId == tournament.TruongBanTrongTaiId.Value))
                {
                    var canonicalRole = NormalizeRefereeRole(assignment.VaiTro);
                    if (!string.IsNullOrWhiteSpace(canonicalRole))
                    {
                        var slotKey = $"{assignment.TranDauId}:{canonicalRole}";
                        if (!draftBySlot.ContainsKey(slotKey))
                        {
                            draftBySlot[slotKey] = new RefereeAssignmentDraftItemDto
                            {
                                TranDauId = assignment.TranDauId,
                                VaiTro = canonicalRole,
                                TrongTaiId = null
                            };
                        }
                    }
                }
            }

            if (draftBySlot.Count == 0)
            {
                result.Success = true;
                result.Message = "Bản nháp không có thay đổi để lưu.";
                return result;
            }

            var activeReferees = (await _unitOfWork.TrongTais.FindAsync(referee =>
                referee.IsDeleted != true && referee.TrangThai)).ToDictionary(referee => referee.Id);

            foreach (var change in draftBySlot.Values.Where(change => change.TrongTaiId.HasValue && change.TrongTaiId.Value > 0))
            {
                if (tournament.TruongBanTrongTaiId == change.TrongTaiId)
                {
                    result.Message = "Trưởng ban trọng tài không được phép phân công điều hành trận đấu.";
                    return result;
                }

                if (!activeReferees.ContainsKey(change.TrongTaiId!.Value))
                {
                    result.Message = "Bản nháp có trọng tài không tồn tại hoặc không còn hoạt động.";
                    return result;
                }
            }

            var finalAssignments = existingAssignments
                .Select(assignment => new RefereeAssignmentDraftItemDto
                {
                    TranDauId = assignment.TranDauId,
                    VaiTro = NormalizeRefereeRole(assignment.VaiTro),
                    TrongTaiId = assignment.TrongTaiId
                })
                .Where(assignment => !string.IsNullOrWhiteSpace(assignment.VaiTro))
                .ToList();

            foreach (var change in draftBySlot.Values)
            {
                finalAssignments.RemoveAll(assignment =>
                    assignment.TranDauId == change.TranDauId && assignment.VaiTro == change.VaiTro);

                if (change.TrongTaiId.HasValue && change.TrongTaiId.Value > 0)
                {
                    finalAssignments.Add(change);
                }
            }

            var duplicateRefereeInMatch = finalAssignments
                .Where(assignment => assignment.TrongTaiId.HasValue)
                .GroupBy(assignment => new { assignment.TranDauId, RefereeId = assignment.TrongTaiId!.Value })
                .FirstOrDefault(group => group.Count() > 1);
            if (duplicateRefereeInMatch != null)
            {
                result.Message = $"Một trọng tài không thể giữ nhiều vị trí trong trận #{matches.First(match => match.Id == duplicateRefereeInMatch.Key.TranDauId).SoTran}.";
                return result;
            }

            var assignmentsRemoved = 0;
            var assignmentsCreated = 0;
            foreach (var change in draftBySlot.Values)
            {
                var assignmentsAtSlot = existingAssignments.Where(assignment =>
                    assignment.TranDauId == change.TranDauId &&
                    NormalizeRefereeRole(assignment.VaiTro) == change.VaiTro).ToList();

                foreach (var assignment in assignmentsAtSlot)
                {
                    assignment.IsDeleted = true;
                    assignment.LastModified = DateTime.UtcNow;
                    assignment.LastModifiedBy = savedBy;
                    _unitOfWork.PhanCongTrongTais.Update(assignment);
                    assignmentsRemoved++;
                }

                if (!change.TrongTaiId.HasValue || change.TrongTaiId.Value <= 0)
                {
                    continue;
                }

                await _unitOfWork.PhanCongTrongTais.AddAsync(new PhanCongTrongTai
                {
                    TranDauId = change.TranDauId,
                    TrongTaiId = change.TrongTaiId.Value,
                    VaiTro = change.VaiTro,
                    Created = DateTime.UtcNow,
                    CreatedBy = savedBy,
                    IsDeleted = false
                });
                assignmentsCreated++;
            }

            await _unitOfWork.CompleteAsync();

            result.Success = true;
            result.ChangesSaved = draftBySlot.Count;
            result.AssignmentsCreated = assignmentsCreated;
            result.AssignmentsRemoved = assignmentsRemoved;
            result.Message = $"Đã lưu {result.ChangesSaved} thay đổi nháp: tạo {assignmentsCreated} phân công mới, xóa mềm {assignmentsRemoved} phân công cũ.";
            return result;
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
