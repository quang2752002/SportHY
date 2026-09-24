using AutoMapper;
using Dms.Application.DTOs;
using Dms.Application.Interfaces;
using Dms.Domain.Entities;
using Dms.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Dms.Application.Services
{
    /// <summary>
    /// Triển khai dịch vụ nghiệp vụ dành cho Thư Ký Giải (Tournament Secretariat).
    /// Tuân thủ quy tắc Thin Controller, Soft Delete và Service Method Commenting.
    /// </summary>
    public class ThuKyGiaiService : IThuKyGiaiService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ITranDauService _tranDauService;

        public ThuKyGiaiService(IUnitOfWork unitOfWork, IMapper mapper, ITranDauService tranDauService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _tranDauService = tranDauService;
        }

        /// <summary>
        /// Lấy các giải đấu mà thư ký được phân công và chỉ trả về các bản ghi còn hoạt động.
        /// </summary>
        /// <param name="thuKyId">ID hồ sơ thư ký.</param>
        /// <returns>Danh sách giải đấu được phân công, sắp xếp theo ngày bắt đầu giảm dần.</returns>
        public async Task<List<GiaiDauDto>> GetAssignedTournamentsAsync(int thuKyId)
        {
            var secretary = await _unitOfWork.ThuKys.GetByIdAsync(thuKyId);
            if (secretary == null || secretary.IsDeleted == true || !secretary.TrangThai)
            {
                return new List<GiaiDauDto>();
            }

            var assignments = (await _unitOfWork.PhanCongThuKys.FindAsync(p =>
                p.ThuKyId == thuKyId && p.IsDeleted != true)).ToList();
            var tournamentIds = assignments.Select(p => p.GiaiDauId).Distinct().ToList();
            var tournaments = (await _unitOfWork.GiaiDaus.FindAsync(g =>
                tournamentIds.Contains(g.Id) && g.IsDeleted != true))
                .OrderByDescending(g => g.NgayBatDau)
                .ToList();

            return _mapper.Map<List<GiaiDauDto>>(tournaments);
        }

        /// <summary>
        /// Lấy danh sách thư ký hoạt động và đánh dấu các thư ký đã được phân công vào giải.
        /// </summary>
        /// <param name="giaiDauId">ID giải đấu.</param>
        /// <returns>Danh sách thư ký phục vụ màn hình phân công.</returns>
        public async Task<List<ThuKyPhanCongDto>> GetSecretaryAssignmentsForManagerAsync(int giaiDauId)
        {
            var secretaries = (await _unitOfWork.ThuKys.FindAsync(t =>
                t.IsDeleted != true && t.TrangThai)).ToList();
            var assignments = (await _unitOfWork.PhanCongThuKys.FindAsync(p =>
                p.GiaiDauId == giaiDauId && p.IsDeleted != true)).ToList();
            var assignedIds = assignments.Select(p => p.ThuKyId).ToHashSet();

            return secretaries
                .OrderBy(t => t.HoTen)
                .Select(t => new ThuKyPhanCongDto
                {
                    ThuKyId = t.Id,
                    Ma = t.Ma,
                    HoTen = t.HoTen,
                    ChucVu = t.ChucVu,
                    DonViCongTac = t.DonViCongTac,
                    SoDienThoai = t.SoDienThoai,
                    Email = t.Email,
                    DaPhanCong = assignedIds.Contains(t.Id)
                })
                .ToList();
        }

        /// <summary>
        /// Lưu danh sách thư ký của một giải đấu; các phân công bị bỏ chọn được chuyển sang trạng thái xóa mềm.
        /// </summary>
        /// <param name="giaiDauId">ID giải đấu.</param>
        /// <param name="thuKyIds">Danh sách ID thư ký được phân công.</param>
        /// <returns>Bộ đôi cho biết kết quả và thông báo nghiệp vụ.</returns>
        public async Task<(bool success, string message)> SaveSecretaryAssignmentsAsync(int giaiDauId, List<int> thuKyIds)
        {
            var tournament = await _unitOfWork.GiaiDaus.GetByIdAsync(giaiDauId);
            if (tournament == null || tournament.IsDeleted == true)
            {
                return (false, "Không tìm thấy giải đấu.");
            }

            var selectedIds = (thuKyIds ?? new List<int>()).Where(id => id > 0).Distinct().ToHashSet();
            var activeSecretaries = (await _unitOfWork.ThuKys.FindAsync(t =>
                selectedIds.Contains(t.Id) && t.IsDeleted != true && t.TrangThai)).ToList();
            if (activeSecretaries.Count != selectedIds.Count)
            {
                return (false, "Danh sách thư ký có người không tồn tại hoặc đang không hoạt động.");
            }

            var existing = (await _unitOfWork.PhanCongThuKys.FindAsync(p =>
                p.GiaiDauId == giaiDauId)).ToList();

            foreach (var assignment in existing)
            {
                var shouldBeActive = selectedIds.Contains(assignment.ThuKyId);
                assignment.IsDeleted = !shouldBeActive;
                assignment.LastModified = DateTime.UtcNow;
                _unitOfWork.PhanCongThuKys.Update(assignment);
            }

            var existingSecretaryIds = existing.Select(p => p.ThuKyId).ToHashSet();
            foreach (var thuKyId in selectedIds.Where(id => !existingSecretaryIds.Contains(id)))
            {
                await _unitOfWork.PhanCongThuKys.AddAsync(new PhanCongThuKy
                {
                    GiaiDauId = giaiDauId,
                    ThuKyId = thuKyId,
                    IsDeleted = false,
                    Created = DateTime.UtcNow
                });
            }

            await _unitOfWork.CompleteAsync();
            return (true, selectedIds.Count == 0
                ? "Đã bỏ phân công tất cả thư ký khỏi giải đấu."
                : $"Đã lưu phân công {selectedIds.Count} thư ký cho giải đấu.");
        }

        /// <summary>
        /// Kiểm tra thư ký có phân công còn hiệu lực trong giải đấu hay không.
        /// </summary>
        /// <param name="thuKyId">ID hồ sơ thư ký.</param>
        /// <param name="giaiDauId">ID giải đấu.</param>
        /// <returns>True nếu phân công tồn tại và cả thư ký, giải đấu đều hoạt động.</returns>
        public async Task<bool> IsSecretaryAssignedAsync(int thuKyId, int giaiDauId)
        {
            var secretary = await _unitOfWork.ThuKys.GetByIdAsync(thuKyId);
            var tournament = await _unitOfWork.GiaiDaus.GetByIdAsync(giaiDauId);
            if (secretary == null || secretary.IsDeleted == true || !secretary.TrangThai ||
                tournament == null || tournament.IsDeleted == true)
            {
                return false;
            }

            var assignments = await _unitOfWork.PhanCongThuKys.FindAsync(p =>
                p.ThuKyId == thuKyId && p.GiaiDauId == giaiDauId && p.IsDeleted != true);
            return assignments.Any();
        }

        /// <summary>
        /// Lấy giải đấu chứa trận đấu được chỉ định.
        /// </summary>
        /// <param name="tranDauId">ID trận đấu.</param>
        /// <returns>ID giải đấu hoặc null.</returns>
        public async Task<int?> GetTournamentIdByMatchAsync(int tranDauId)
        {
            var match = await _unitOfWork.TranDaus.GetByIdAsync(tranDauId);
            if (match == null || match.IsDeleted == true) return null;
            return await GetTournamentIdByContentAsync(match.GiaiDauMonTheThaoId);
        }

        /// <summary>
        /// Lấy giải đấu chứa nội dung thi đấu được chỉ định.
        /// </summary>
        /// <param name="giaiDauMonTheThaoId">ID môn thể thao thuộc giải.</param>
        /// <returns>ID giải đấu hoặc null.</returns>
        public async Task<int?> GetTournamentIdByContentAsync(int giaiDauMonTheThaoId)
        {
            var content = await _unitOfWork.GiaiDauMonTheThaos.GetByIdAsync(giaiDauMonTheThaoId);
            return content == null || content.IsDeleted == true ? null : content.GiaiDauId;
        }

        #region Helper Methods (JSON GhiChu parsing & tournament selection)

        private async Task<GiaiDau?> GetEffectiveTournamentAsync(int? giaiDauId)
        {
            if (giaiDauId.HasValue && giaiDauId.Value > 0)
            {
                var target = await _unitOfWork.GiaiDaus.GetByIdAsync(giaiDauId.Value);
                if (target != null && target.IsDeleted != true) return target;
            }

            return null;
        }

        private class ParsedMatchGhiChu
        {
            public int Score1 { get; set; }
            public int Score2 { get; set; }
            public string Winner { get; set; } = string.Empty;
            public string Notes { get; set; } = string.Empty;
            public List<SetScoreDto> SetScores { get; set; } = new();
            public List<MatchEventItemDto> Events { get; set; } = new();
            public Dictionary<string, SignatureInfoDto> Signatures { get; set; } = new();
            public string AuditStatus { get; set; } = "ChuaCoKetQua";
            public string? SecretaryNotes { get; set; }
            public string? ConfirmedBy { get; set; }
            public DateTime? ConfirmedAt { get; set; }
        }

        private ParsedMatchGhiChu ParseMatchGhiChu(string? ghiChu, string matchStatus)
        {
            var result = new ParsedMatchGhiChu();

            if (string.IsNullOrWhiteSpace(ghiChu) || !ghiChu.TrimStart().StartsWith("{"))
            {
                if (matchStatus == "HoanThanh" || matchStatus == "KetThuc")
                {
                    result.AuditStatus = "ChoXacNhan";
                }
                return result;
            }

            try
            {
                using var doc = JsonDocument.Parse(ghiChu);
                var root = doc.RootElement;

                if (root.TryGetProperty("score1", out var p1) && p1.TryGetInt32(out var s1)) result.Score1 = s1;
                if (root.TryGetProperty("score2", out var p2) && p2.TryGetInt32(out var s2)) result.Score2 = s2;
                if (root.TryGetProperty("winner", out var pw)) result.Winner = pw.GetString() ?? "";
                if (root.TryGetProperty("notes", out var pn)) result.Notes = pn.GetString() ?? "";

                if (root.TryGetProperty("auditStatus", out var pas)) result.AuditStatus = pas.GetString() ?? "";
                if (root.TryGetProperty("secretaryNotes", out var psn)) result.SecretaryNotes = psn.GetString();
                if (root.TryGetProperty("confirmedBy", out var pcb)) result.ConfirmedBy = pcb.GetString();
                if (root.TryGetProperty("confirmedAt", out var pca) && pca.TryGetDateTime(out var cat)) result.ConfirmedAt = cat;

                if (root.TryGetProperty("setScores", out var pSets) && pSets.ValueKind == JsonValueKind.Array)
                {
                    foreach (var el in pSets.EnumerateArray())
                    {
                        result.SetScores.Add(new SetScoreDto
                        {
                            SetNumber = el.TryGetProperty("setNumber", out var sn) && sn.TryGetInt32(out var snv) ? snv : 1,
                            Score1 = el.TryGetProperty("score1", out var set1) && set1.TryGetInt32(out var s1v) ? s1v : 0,
                            Score2 = el.TryGetProperty("score2", out var set2) && set2.TryGetInt32(out var s2v) ? s2v : 0,
                        });
                    }
                }

                if (root.TryGetProperty("events", out var pEvents) && pEvents.ValueKind == JsonValueKind.Array)
                {
                    foreach (var el in pEvents.EnumerateArray())
                    {
                        result.Events.Add(new MatchEventItemDto
                        {
                            Id = el.TryGetProperty("id", out var eid) && eid.TryGetInt32(out var idv) ? idv : 0,
                            Minute = el.TryGetProperty("minute", out var em) && em.TryGetInt32(out var mv) ? mv : 0,
                            Type = el.TryGetProperty("type", out var et) ? et.GetString() ?? "goal" : "goal",
                            Team = el.TryGetProperty("team", out var etm) && etm.TryGetInt32(out var tmv) ? tmv : 1,
                            Player = el.TryGetProperty("player", out var ep) ? ep.GetString() : null,
                            Notes = el.TryGetProperty("notes", out var en) ? en.GetString() : null,
                        });
                    }
                }

                if (root.TryGetProperty("signatures", out var pSigs) && pSigs.ValueKind == JsonValueKind.Object)
                {
                    foreach (var prop in pSigs.EnumerateObject())
                    {
                        result.Signatures[prop.Name] = new SignatureInfoDto
                        {
                            SignerName = prop.Value.TryGetProperty("signerName", out var ps) ? ps.GetString() : null,
                            Role = prop.Value.TryGetProperty("role", out var pr) ? pr.GetString() : null,
                            SignedAt = prop.Value.TryGetProperty("signedAt", out var pt) && pt.TryGetDateTime(out var dt) ? dt : null
                        };
                    }
                }

                if (string.IsNullOrEmpty(result.AuditStatus))
                {
                    if (result.ConfirmedAt.HasValue)
                    {
                        result.AuditStatus = "DaXacNhan";
                    }
                    else if (matchStatus == "HoanThanh" || matchStatus == "KetThuc")
                    {
                        result.AuditStatus = "ChoXacNhan";
                    }
                }
            }
            catch
            {
                if (matchStatus == "HoanThanh" || matchStatus == "KetThuc")
                {
                    result.AuditStatus = "ChoXacNhan";
                }
            }

            return result;
        }

        #endregion

        /// <summary>
        /// Lấy dữ liệu tổng quan cho Dashboard Ban Thư ký (KPIs, cảnh báo, tiến độ các môn, kết quả mới nhất).
        /// </summary>
        public async Task<ThuKyDashboardDto> GetDashboardAsync(int? giaiDauId)
        {
            var giaiDau = await GetEffectiveTournamentAsync(giaiDauId);
            if (giaiDau == null)
            {
                return new ThuKyDashboardDto { TenGiaiDau = "Chưa có giải đấu" };
            }

            int gId = giaiDau.Id;

            var gdmList = (await _unitOfWork.GiaiDauMonTheThaos.FindAsync(g => g.GiaiDauId == gId && g.IsDeleted != true)).ToList();
            var matches = (await _tranDauService.GetAllAsync(giaiDauId: gId))?.ToList() ?? new List<TranDauDto>();

            var tienDoCacMon = await GetTienDoCacMonAsync(gId);
            var ketQuaList = await GetDanhSachKetQuaAsync(gId);
            var bangTongSap = await GetBangTongSapHuyChuongAsync(gId);

            int totalMatches = matches.Count;
            int completedMatches = matches.Count(m => m.TrangThai == "HoanThanh" || m.TrangThai == "KetThuc");
            int liveMatches = matches.Count(m => m.TrangThai == "DangDienRa" || m.TrangThai == "DangDau");
            int upcomingMatches = matches.Count(m => m.TrangThai == "ChuaDau" || m.TrangThai == "SapDau");

            int choDuyet = ketQuaList.Count(k => k.TrangThaiXacNhan == "ChoXacNhan");
            int daDuyet = ketQuaList.Count(k => k.TrangThaiXacNhan == "DaXacNhan");
            int canKiemTra = ketQuaList.Count(k => k.TrangThaiXacNhan == "CanKiemTraLai");

            int totalBienBan = completedMatches;
            int bienBanDuChuKy = ketQuaList.Count(k => k.IsScoresheetComplete);
            int bienBanThieuChuKy = totalBienBan - bienBanDuChuKy;
            if (bienBanThieuChuKy < 0) bienBanThieuChuKy = 0;

            double tiLeHoanThanh = totalMatches > 0 ? Math.Round((double)completedMatches / totalMatches * 100, 1) : 0;

            var canhBao = ketQuaList.Where(k => k.TrangThaiXacNhan == "CanKiemTraLai" || (k.TrangThaiXacNhan == "ChoXacNhan" && !k.IsScoresheetComplete)).Take(5).ToList();
            var ketQuaMoi = ketQuaList.OrderByDescending(k => k.ThoiGianKetThuc ?? k.ThoiGianDuKien ?? DateTime.MinValue).Take(6).ToList();

            return new ThuKyDashboardDto
            {
                GiaiDauId = gId,
                TenGiaiDau = giaiDau.Ten,
                TongSoMon = gdmList.Select(g => g.MonTheThaoId).Distinct().Count(),
                TongSoNoiDung = gdmList.Count,
                TongSoTranDau = totalMatches,
                SoTranDaHoanThanh = completedMatches,
                SoTranDangDienRa = liveMatches,
                SoTranChuaDau = upcomingMatches,
                SoKetQuaChoDuyet = choDuyet,
                SoKetQuaDaDuyet = daDuyet,
                SoKetQuaCanKiemTra = canKiemTra,
                TongSoBienBan = totalBienBan,
                SoBienBanDuChuKy = bienBanDuChuKy,
                SoBienBanThieuChuKy = bienBanThieuChuKy,
                TongSoHuyChuongDaTrao = bangTongSap.TongSoHuyChuongDaTrao,
                TiLeHoanThanh = tiLeHoanThanh,
                TienDoCacMon = tienDoCacMon,
                KetQuaMoiNhat = ketQuaMoi,
                CanhBaoCanXuLy = canhBao,
                TopHuyChuong = bangTongSap.BangXepHang.Take(5).ToList()
            };
        }

        /// <summary>
        /// Lấy danh sách các môn thể thao thuộc giải đấu, kèm danh mục môn và thống kê tiến độ, có hỗ trợ lọc và tìm kiếm.
        /// </summary>
        public async Task<List<ThuKyNoiDungDto>> GetDanhSachNoiDungAsync(int? giaiDauId, int? monTheThaoId = null, string? loaiThiDau = null, string? gioiTinh = null, string? keyword = null)
        {
            var giaiDau = await GetEffectiveTournamentAsync(giaiDauId);
            if (giaiDau == null) return new List<ThuKyNoiDungDto>();

            int gId = giaiDau.Id;

            var gdmList = (await _unitOfWork.GiaiDauMonTheThaos.FindAsync(g => g.GiaiDauId == gId && g.IsDeleted != true)).ToList();
            var monList = (await _unitOfWork.MonTheThaos.FindAsync(m => m.IsDeleted != true)).ToDictionary(m => m.Id);
            var danhMucIds = monList.Values.Select(m => m.DanhMucId).Distinct().ToList();
            var danhMucList = (await _unitOfWork.DanhMucMonTheThaos.FindAsync(d =>
                danhMucIds.Contains(d.Id) && d.IsDeleted != true)).ToDictionary(d => d.Id);

            var gdmIds = gdmList.Select(g => g.Id).ToList();
            var allDangKys = (await _unitOfWork.DangKyThiDaus.FindAsync(d => gdmIds.Contains(d.GiaiDauMonTheThaoId) && d.IsDeleted != true)).ToList();
            var allMatches = (await _unitOfWork.TranDaus.FindAsync(t => gdmIds.Contains(t.GiaiDauMonTheThaoId) && t.IsDeleted != true)).ToList();

            var result = new List<ThuKyNoiDungDto>();

            foreach (var gdm in gdmList)
            {
                monList.TryGetValue(gdm.MonTheThaoId, out var mon);
                danhMucList.TryGetValue(mon?.DanhMucId ?? 0, out var danhMuc);

                var monTen = mon?.Ten ?? "Môn thi đấu";
                var monMa = mon?.Ma ?? "";
                var monGioiTinh = mon?.GioiTinh ?? "HonHop";
                var monHinhThuc = mon?.HinhThucThiDau.ToString() ?? "CaNhan";

                var dangKysCount = allDangKys.Count(d => d.GiaiDauMonTheThaoId == gdm.Id);
                var matches = allMatches.Where(t => t.GiaiDauMonTheThaoId == gdm.Id).ToList();
                var totalMatches = matches.Count;
                var completedMatches = matches.Count(m => m.TrangThai == "HoanThanh" || m.TrangThai == "KetThuc");

                string trangThai = "ChuaDau";
                if (totalMatches > 0 && completedMatches == totalMatches) trangThai = "HoanThanh";
                else if (completedMatches > 0 || matches.Any(m => m.TrangThai == "DangDienRa" || m.TrangThai == "DangDau")) trangThai = "DangDau";

                double pct = totalMatches > 0 ? Math.Round((double)completedMatches / totalMatches * 100, 1) : 0;

                var item = new ThuKyNoiDungDto
                {
                    Id = gdm.Id,
                    GiaiDauId = gId,
                    TenGiaiDau = giaiDau.Ten,
                    MonTheThaoId = gdm.MonTheThaoId,
                    TenMonTheThao = monTen,
                    MaMonTheThao = monMa,
                    DanhMucMonTheThaoId = mon?.DanhMucId ?? 0,
                    MaDanhMucMonTheThao = danhMuc?.Ma,
                    TenDanhMucMonTheThao = danhMuc?.Ten ?? "Chưa phân loại",
                    TenNoiDung = $"{monTen} ({monGioiTinh})",
                    GioiTinh = monGioiTinh,
                    LoaiThiDau = (mon != null && mon.LaMonDongDoi) ? "DongDoi" : "CaNhan",
                    HinhThucThiDau = monHinhThuc,
                    SoDangKy = dangKysCount,
                    SoTranDaXep = totalMatches,
                    SoTranDaHoanThanh = completedMatches,
                    TrangThai = trangThai,
                    PhanTramTienDo = pct
                };

                // Lọc
                if (monTheThaoId.HasValue && item.MonTheThaoId != monTheThaoId.Value) continue;
                if (!string.IsNullOrEmpty(loaiThiDau) && !string.Equals(item.LoaiThiDau, loaiThiDau, StringComparison.OrdinalIgnoreCase)) continue;
                if (!string.IsNullOrEmpty(gioiTinh) && !string.Equals(item.GioiTinh, gioiTinh, StringComparison.OrdinalIgnoreCase)) continue;
                if (!string.IsNullOrWhiteSpace(keyword))
                {
                    var kw = keyword.Trim().ToLower();
                    var matchKeyword =
                        (item.TenMonTheThao ?? "").ToLower().Contains(kw) ||
                        (item.MaMonTheThao ?? "").ToLower().Contains(kw) ||
                        (item.TenDanhMucMonTheThao ?? "").ToLower().Contains(kw) ||
                        (item.MaDanhMucMonTheThao ?? "").ToLower().Contains(kw);
                    if (!matchKeyword) continue;
                }

                result.Add(item);
            }

            return result.OrderBy(r => r.TenMonTheThao).ThenBy(r => r.GioiTinh).ToList();
        }

        /// <summary>
        /// Lấy thông tin chi tiết của một môn thể thao trong giải đấu, gồm danh mục, danh sách đăng ký,
        /// các vòng, bảng và trận đấu.
        /// </summary>
        public async Task<ThuKyNoiDungChiTietDto?> GetChiTietNoiDungAsync(int giaiDauMonTheThaoId)
        {
            var gdm = await _unitOfWork.GiaiDauMonTheThaos.GetByIdAsync(giaiDauMonTheThaoId);
            if (gdm == null || gdm.IsDeleted == true) return null;

            var giaiDau = await _unitOfWork.GiaiDaus.GetByIdAsync(gdm.GiaiDauId);
            var mon = await _unitOfWork.MonTheThaos.GetByIdAsync(gdm.MonTheThaoId);
            var danhMuc = mon == null
                ? null
                : await _unitOfWork.DanhMucMonTheThaos.GetByIdAsync(mon.DanhMucId);

            var rawMatches = (await _tranDauService.GetAllAsync(giaiDauMonTheThaoId: giaiDauMonTheThaoId))?.ToList() ?? new List<TranDauDto>();
            var dangKys = (await _unitOfWork.DangKyThiDaus.FindAsync(d => d.GiaiDauMonTheThaoId == giaiDauMonTheThaoId && d.IsDeleted != true)).ToList();
            var vongs = (await _unitOfWork.VongDaus.FindAsync(v => v.GiaiDauMonTheThaoId == giaiDauMonTheThaoId && v.IsDeleted != true)).ToList();
            var bangs = (await _unitOfWork.BangDaus.FindAsync(b => b.GiaiDauMonTheThaoId == giaiDauMonTheThaoId && b.IsDeleted != true)).ToList();

            var donVis = (await _unitOfWork.DonVis.FindAsync(d => d.IsDeleted != true)).ToDictionary(d => d.Id);
            var dois = (await _unitOfWork.Dois.FindAsync(d => d.IsDeleted != true)).ToDictionary(d => d.Id);

            // Lấy thành viên đội thay vì ChiTietDangKyThiDau
            var doiIds = dangKys.Where(d => d.DoiId.HasValue).Select(d => d.DoiId!.Value).Distinct().ToList();
            var thanhViens = (await _unitOfWork.ThanhVienDois.FindAsync(tv => doiIds.Contains(tv.DoiId) && tv.IsDeleted != true)).ToList();
            var vdvIds = thanhViens.Select(tv => tv.VanDongVienId).Distinct().ToList();
            var vdvs = (await _unitOfWork.VanDongViens.FindAsync(v => vdvIds.Contains(v.Id) && v.IsDeleted != true)).ToDictionary(v => v.Id);

            var dangKyList = new List<NoiDungDangKyItemDto>();
            foreach (var dk in dangKys)
            {
                string tenDonVi = "";
                string tenDoi = "";

                if (dk.DoiId.HasValue && dois.TryGetValue(dk.DoiId.Value, out var doi))
                {
                    tenDoi = doi.Ten;
                    if (doi.DonViId.HasValue && donVis.TryGetValue(doi.DonViId.Value, out var dvDoi))
                    {
                        tenDonVi = dvDoi.Ten;
                    }
                }

                // Lấy VĐV từ thành viên đội
                var myThanhViens = dk.DoiId.HasValue
                    ? thanhViens.Where(tv => tv.DoiId == dk.DoiId.Value).ToList()
                    : new List<ThanhVienDoi>();
                var vdvNames = new List<string>();
                foreach (var tv in myThanhViens)
                {
                    if (vdvs.TryGetValue(tv.VanDongVienId, out var vdv))
                    {
                        vdvNames.Add(vdv.HoTen);
                        if (string.IsNullOrEmpty(tenDonVi) && vdv.DonViId.HasValue && donVis.TryGetValue(vdv.DonViId.Value, out var dvVdv))
                        {
                            tenDonVi = dvVdv.Ten;
                        }
                    }
                }

                dangKyList.Add(new NoiDungDangKyItemDto
                {
                    DangKyThiDauId = dk.Id,
                    SoDangKy = dk.SoDangKy,
                    TenDangKy = dk.TenDangKy ?? (tenDoi != "" ? tenDoi : string.Join(", ", vdvNames)),
                    TenDonVi = tenDonVi,
                    TenDoi = tenDoi,
                    DanhSachVdv = vdvNames,
                    TrangThai = dk.TrangThai,
                    NgayDangKy = dk.NgayDangKy
                });
            }

            int completed = rawMatches.Count(m => m.TrangThai == "HoanThanh" || m.TrangThai == "KetThuc");
            var thongTinChung = new ThuKyNoiDungDto
            {
                Id = gdm.Id,
                GiaiDauId = gdm.GiaiDauId,
                TenGiaiDau = giaiDau?.Ten,
                MonTheThaoId = gdm.MonTheThaoId,
                TenMonTheThao = mon?.Ten,
                MaMonTheThao = mon?.Ma,
                DanhMucMonTheThaoId = mon?.DanhMucId ?? 0,
                MaDanhMucMonTheThao = danhMuc?.Ma,
                TenDanhMucMonTheThao = danhMuc?.Ten ?? "Chưa phân loại",
                TenNoiDung = $"{mon?.Ten ?? "Môn"} ({mon?.GioiTinh ?? "Hỗn hợp"})",
                GioiTinh = mon?.GioiTinh ?? "HonHop",
                LoaiThiDau = (mon != null && mon.LaMonDongDoi) ? "DongDoi" : "CaNhan",
                HinhThucThiDau = mon?.HinhThucThiDau.ToString(),
                SoDangKy = dangKys.Count,
                SoTranDaXep = rawMatches.Count,
                SoTranDaHoanThanh = completed,
                PhanTramTienDo = rawMatches.Count > 0 ? Math.Round((double)completed / rawMatches.Count * 100, 1) : 0
            };

            return new ThuKyNoiDungChiTietDto
            {
                ThongTinChung = thongTinChung,
                DanhSachDangKy = dangKyList,
                DanhSachTranDau = rawMatches,
                DanhSachVongDau = _mapper.Map<List<VongDauDto>>(vongs),
                DanhSachBangDau = _mapper.Map<List<BangDauDto>>(bangs)
            };
        }

        /// <summary>
        /// Lấy danh sách tiến độ thi đấu theo từng môn thể thao của giải đấu.
        /// </summary>
        public async Task<List<ThuKyTienDoMonDto>> GetTienDoCacMonAsync(int? giaiDauId)
        {
            var giaiDau = await GetEffectiveTournamentAsync(giaiDauId);
            if (giaiDau == null) return new List<ThuKyTienDoMonDto>();

            int gId = giaiDau.Id;

            var gdmList = (await _unitOfWork.GiaiDauMonTheThaos.FindAsync(g => g.GiaiDauId == gId && g.IsDeleted != true)).ToList();
            var monList = (await _unitOfWork.MonTheThaos.FindAsync(m => m.IsDeleted != true)).ToDictionary(m => m.Id);

            var gdmIds = gdmList.Select(g => g.Id).ToList();
            var allMatches = (await _tranDauService.GetAllAsync(giaiDauId: gId))?.ToList() ?? new List<TranDauDto>();
            var allHuyChuongs = (await _unitOfWork.HuyChuongs.FindAsync(h => h.GiaiDauId == gId && h.IsDeleted != true)).ToList();
            var allDangKys = (await _unitOfWork.DangKyThiDaus.FindAsync(d => gdmIds.Contains(d.GiaiDauMonTheThaoId) && d.IsDeleted != true)).ToList();

            var groups = gdmList.GroupBy(g => g.MonTheThaoId);
            var result = new List<ThuKyTienDoMonDto>();

            foreach (var grp in groups)
            {
                int monId = grp.Key;
                monList.TryGetValue(monId, out var mon);

                var thisGdmIds = grp.Select(g => g.Id).ToList();
                var matches = allMatches.Where(m => thisGdmIds.Contains(m.GiaiDauMonTheThaoId)).ToList();
                var huyChuongs = allHuyChuongs.Where(h => thisGdmIds.Contains(h.GiaiDauMonTheThaoId)).ToList();
                var dangKys = allDangKys.Where(d => thisGdmIds.Contains(d.GiaiDauMonTheThaoId)).ToList();

                int totalMatches = matches.Count;
                int completedMatches = matches.Count(m => m.TrangThai == "HoanThanh" || m.TrangThai == "KetThuc");
                int liveMatches = matches.Count(m => m.TrangThai == "DangDienRa" || m.TrangThai == "DangDau");
                int upcomingMatches = matches.Count(m => m.TrangThai == "ChuaDau" || m.TrangThai == "SapDau");

                int daDuyet = 0;
                int choDuyet = 0;

                foreach (var m in matches)
                {
                    var parsed = ParseMatchGhiChu(m.GhiChu, m.TrangThai);
                    if (parsed.AuditStatus == "DaXacNhan") daDuyet++;
                    else if (parsed.AuditStatus == "ChoXacNhan") choDuyet++;
                }

                double pct = totalMatches > 0 ? Math.Round((double)completedMatches / totalMatches * 100, 1) : 0;

                string trangThai = "ChuaDau";
                if (totalMatches > 0 && completedMatches == totalMatches) trangThai = "HoanThanh";
                else if (completedMatches > 0 || liveMatches > 0) trangThai = "DangDau";

                result.Add(new ThuKyTienDoMonDto
                {
                    GiaiDauMonTheThaoId = grp.First().Id,
                    MonTheThaoId = monId,
                    TenMon = mon?.Ten ?? "Môn thể thao",
                    MaMon = mon?.Ma ?? "",
                    IconMon = "fa-volleyball",
                    TongSoNoiDung = grp.Count(),
                    TongSoTran = totalMatches,
                    SoTranDaHoanThanh = completedMatches,
                    SoTranDangDienRa = liveMatches,
                    SoTranChuaDau = upcomingMatches,
                    TiLeHoanThanh = pct,
                    TongSoVdv = dangKys.Count,
                    SoHuyChuongDaTrao = huyChuongs.Count,
                    SoBienBanDaDuyet = daDuyet,
                    SoBienBanChoDuyet = choDuyet,
                    TrangThai = trangThai
                });
            }

            return result.OrderByDescending(r => r.TiLeHoanThanh).ThenBy(r => r.TenMon).ToList();
        }

        /// <summary>
        /// Lấy danh sách kết quả trận đấu phục vụ kiểm tra và rà soát của Thư ký giải.
        /// </summary>
        public async Task<List<ThuKyKetQuaDto>> GetDanhSachKetQuaAsync(int? giaiDauId, int? monTheThaoId = null, string? trangThaiXacNhan = null, string? keyword = null)
        {
            var giaiDau = await GetEffectiveTournamentAsync(giaiDauId);
            if (giaiDau == null) return new List<ThuKyKetQuaDto>();

            int gId = giaiDau.Id;

            var allMatches = (await _tranDauService.GetAllAsync(giaiDauId: gId))?.ToList() ?? new List<TranDauDto>();

            var gdmList = (await _unitOfWork.GiaiDauMonTheThaos.FindAsync(g => g.GiaiDauId == gId && g.IsDeleted != true)).ToList();
            if (monTheThaoId.HasValue)
            {
                var targetGdmIds = gdmList.Where(g => g.MonTheThaoId == monTheThaoId.Value).Select(g => g.Id).ToList();
                allMatches = allMatches.Where(m => targetGdmIds.Contains(m.GiaiDauMonTheThaoId)).ToList();
            }

            var result = new List<ThuKyKetQuaDto>();

            foreach (var m in allMatches)
            {
                var parsed = ParseMatchGhiChu(m.GhiChu, m.TrangThai);
                bool hasScoresheet = !string.IsNullOrWhiteSpace(m.GhiChu) && m.GhiChu.TrimStart().StartsWith("{");
                bool isComplete = parsed.Signatures.ContainsKey("referee") && parsed.Signatures.ContainsKey("team1") && parsed.Signatures.ContainsKey("team2");

                var item = new ThuKyKetQuaDto
                {
                    TranDauId = m.Id,
                    SoTran = m.SoTran,
                    TenTran = m.TenTran ?? $"Trận {m.SoTran}",
                    GiaiDauMonTheThaoId = m.GiaiDauMonTheThaoId,
                    TenMonTheThao = m.TenMonTheThao ?? "Môn thi",
                    TenVongDau = m.TenVongDau ?? "Vòng đấu",
                    TenBangDau = m.TenBangDau,
                    TenSanDau = m.TenSanDau,
                    ThoiGianDuKien = m.ThoiGianDuKien,
                    ThoiGianBatDau = m.ThoiGianBatDau,
                    ThoiGianKetThuc = m.ThoiGianKetThuc,
                    TrangThaiTranDau = m.TrangThai,
                    Doi1DangKyId = m.Doi1DangKyId,
                    TenDoi1 = m.TenDoi1 ?? "Đội 1",
                    DonViDoi1 = m.DonViDoi1,
                    TySoDoi1 = parsed.Score1,
                    Doi2DangKyId = m.Doi2DangKyId,
                    TenDoi2 = m.TenDoi2 ?? "Đội 2",
                    DonViDoi2 = m.DonViDoi2,
                    TySoDoi2 = parsed.Score2,
                    DoiThang = parsed.Winner,
                    SetScores = parsed.SetScores,
                    TrangThaiXacNhan = parsed.AuditStatus,
                    NguoiXacNhan = parsed.ConfirmedBy,
                    NgayXacNhan = parsed.ConfirmedAt,
                    GhiChuThuKy = parsed.SecretaryNotes,
                    HasScoresheet = hasScoresheet,
                    IsScoresheetComplete = isComplete,
                    SoChuKy = parsed.Signatures.Count
                };

                // Lọc theo trạng thái xác nhận
                if (!string.IsNullOrEmpty(trangThaiXacNhan) && !string.Equals(item.TrangThaiXacNhan, trangThaiXacNhan, StringComparison.OrdinalIgnoreCase)) continue;

                // Lọc từ khóa
                if (!string.IsNullOrWhiteSpace(keyword))
                {
                    var kw = keyword.Trim().ToLower();
                    if (!item.TenTran.ToLower().Contains(kw) &&
                        !item.TenDoi1.ToLower().Contains(kw) &&
                        !item.TenDoi2.ToLower().Contains(kw) &&
                        !(item.TenMonTheThao ?? "").ToLower().Contains(kw)) continue;
                }

                result.Add(item);
            }

            return result.OrderByDescending(r => r.ThoiGianKetThuc ?? r.ThoiGianDuKien ?? DateTime.MinValue).ToList();
        }

        /// <summary>
        /// Thư ký giải xác nhận và phê duyệt kết quả chính thức của một trận đấu.
        /// </summary>
        public async Task<(bool success, string message)> XacNhanKetQuaAsync(int tranDauId, string? ghiChu, string nguoiXacNhan)
        {
            var match = await _unitOfWork.TranDaus.GetByIdAsync(tranDauId);
            if (match == null || match.IsDeleted == true)
            {
                return (false, "Không tìm thấy thông tin trận đấu.");
            }

            var dict = new Dictionary<string, object>();
            if (!string.IsNullOrEmpty(match.GhiChu) && match.GhiChu.TrimStart().StartsWith("{"))
            {
                try
                {
                    using var doc = JsonDocument.Parse(match.GhiChu);
                    foreach (var prop in doc.RootElement.EnumerateObject())
                    {
                        dict[prop.Name] = prop.Value.Clone();
                    }
                }
                catch { }
            }

            dict["auditStatus"] = "DaXacNhan";
            dict["confirmedBy"] = nguoiXacNhan;
            dict["confirmedAt"] = DateTime.Now;
            if (!string.IsNullOrEmpty(ghiChu))
            {
                dict["secretaryNotes"] = ghiChu.Trim();
            }

            match.GhiChu = JsonSerializer.Serialize(dict);
            match.LastModified = DateTime.UtcNow;
            match.LastModifiedBy = nguoiXacNhan;

            _unitOfWork.TranDaus.Update(match);
            await _unitOfWork.CompleteAsync();

            return (true, $"Đã xác nhận & phê duyệt kết quả chính thức cho Trận {match.SoTran} thành công!");
        }

        /// <summary>
        /// Thư ký gắn cờ yêu cầu trọng tài hoặc tổ chuyên môn kiểm tra lại kết quả có dấu hiệu bất thường.
        /// </summary>
        public async Task<(bool success, string message)> YeuCauKiemTraLaiAsync(int tranDauId, string lyDo, string nguoiYeuCau)
        {
            var match = await _unitOfWork.TranDaus.GetByIdAsync(tranDauId);
            if (match == null || match.IsDeleted == true)
            {
                return (false, "Không tìm thấy thông tin trận đấu.");
            }

            var dict = new Dictionary<string, object>();
            if (!string.IsNullOrEmpty(match.GhiChu) && match.GhiChu.TrimStart().StartsWith("{"))
            {
                try
                {
                    using var doc = JsonDocument.Parse(match.GhiChu);
                    foreach (var prop in doc.RootElement.EnumerateObject())
                    {
                        dict[prop.Name] = prop.Value.Clone();
                    }
                }
                catch { }
            }

            dict["auditStatus"] = "CanKiemTraLai";
            dict["disputeReason"] = lyDo;
            dict["disputeRequestedBy"] = nguoiYeuCau;
            dict["disputeRequestedAt"] = DateTime.Now;
            dict["secretaryNotes"] = $"[Yêu cầu kiểm tra lại]: {lyDo}";

            match.GhiChu = JsonSerializer.Serialize(dict);
            match.LastModified = DateTime.UtcNow;
            match.LastModifiedBy = nguoiYeuCau;

            _unitOfWork.TranDaus.Update(match);
            await _unitOfWork.CompleteAsync();

            return (true, $"Đã gửi cờ yêu cầu kiểm tra lại cho Trận {match.SoTran}. Trọng tài sẽ được thông báo đối soát.");
        }

        /// <summary>
        /// Thư ký duyệt kết quả hàng loạt cho các trận đấu đã hoàn thành và hợp lệ.
        /// </summary>
        public async Task<(bool success, string message)> XacNhanHangLoatAsync(List<int> tranDauIds, string nguoiXacNhan)
        {
            if (tranDauIds == null || tranDauIds.Count == 0)
            {
                return (false, "Vui lòng chọn ít nhất một trận đấu để phê duyệt.");
            }

            var matches = (await _unitOfWork.TranDaus.FindAsync(t => tranDauIds.Contains(t.Id) && t.IsDeleted != true)).ToList();
            int approvedCount = 0;

            foreach (var match in matches)
            {
                var dict = new Dictionary<string, object>();
                if (!string.IsNullOrEmpty(match.GhiChu) && match.GhiChu.TrimStart().StartsWith("{"))
                {
                    try
                    {
                        using var doc = JsonDocument.Parse(match.GhiChu);
                        foreach (var prop in doc.RootElement.EnumerateObject())
                        {
                            dict[prop.Name] = prop.Value.Clone();
                        }
                    }
                    catch { }
                }

                dict["auditStatus"] = "DaXacNhan";
                dict["confirmedBy"] = nguoiXacNhan;
                dict["confirmedAt"] = DateTime.Now;

                match.GhiChu = JsonSerializer.Serialize(dict);
                match.LastModified = DateTime.UtcNow;
                match.LastModifiedBy = nguoiXacNhan;

                _unitOfWork.TranDaus.Update(match);
                approvedCount++;
            }

            await _unitOfWork.CompleteAsync();
            return (true, $"Đã duyệt thành công kết quả cho {approvedCount} trận đấu được chọn!");
        }

        /// <summary>
        /// Lấy danh sách kiểm tra biên bản thi đấu của các trận.
        /// </summary>
        public async Task<List<ThuKyBienBanDto>> GetDanhSachBienBanAsync(int? giaiDauId, int? monTheThaoId = null, string? trangThaiKy = null, string? keyword = null)
        {
            var giaiDau = await GetEffectiveTournamentAsync(giaiDauId);
            if (giaiDau == null) return new List<ThuKyBienBanDto>();

            int gId = giaiDau.Id;

            var allMatches = (await _tranDauService.GetAllAsync(giaiDauId: gId))?.ToList() ?? new List<TranDauDto>();

            var gdmList = (await _unitOfWork.GiaiDauMonTheThaos.FindAsync(g => g.GiaiDauId == gId && g.IsDeleted != true)).ToList();
            if (monTheThaoId.HasValue)
            {
                var targetGdmIds = gdmList.Where(g => g.MonTheThaoId == monTheThaoId.Value).Select(g => g.Id).ToList();
                allMatches = allMatches.Where(m => targetGdmIds.Contains(m.GiaiDauMonTheThaoId)).ToList();
            }

            var result = new List<ThuKyBienBanDto>();

            foreach (var m in allMatches)
            {
                var parsed = ParseMatchGhiChu(m.GhiChu, m.TrangThai);

                var bb = new ThuKyBienBanDto
                {
                    TranDauId = m.Id,
                    SoTran = m.SoTran,
                    TenTran = m.TenTran ?? $"Trận {m.SoTran}",
                    GiaiDauId = gId,
                    TenGiaiDau = giaiDau.Ten,
                    TenMonTheThao = m.TenMonTheThao ?? "Môn thi",
                    TenNoiDung = m.TenMonTheThao,
                    TenVongDau = m.TenVongDau,
                    TenBangDau = m.TenBangDau,
                    TenSanDau = m.TenSanDau,
                    ThoiGianBatDau = m.ThoiGianBatDau,
                    ThoiGianKetThuc = m.ThoiGianKetThuc,
                    TrangThai = m.TrangThai,
                    TenDoi1 = m.TenDoi1 ?? "Đội 1",
                    DonViDoi1 = m.DonViDoi1,
                    Score1 = parsed.Score1,
                    TenDoi2 = m.TenDoi2 ?? "Đội 2",
                    DonViDoi2 = m.DonViDoi2,
                    Score2 = parsed.Score2,
                    Winner = parsed.Winner,
                    Notes = parsed.Notes,
                    SetScores = parsed.SetScores,
                    Events = parsed.Events,
                    Signatures = parsed.Signatures,
                    DanhSachTrongTai = m.DanhSachTrongTai
                };

                // Lọc trạng thái ký
                if (!string.IsNullOrEmpty(trangThaiKy))
                {
                    if (trangThaiKy == "DuChuKy" && !bb.DaDuChuKy) continue;
                    if (trangThaiKy == "ThieuChuKy" && bb.DaDuChuKy) continue;
                    if (trangThaiKy == "DaDuyetThuKy" && !bb.ThuKyDaXacNhan) continue;
                }

                if (!string.IsNullOrWhiteSpace(keyword))
                {
                    var kw = keyword.Trim().ToLower();
                    if (!bb.TenTran.ToLower().Contains(kw) &&
                        !bb.TenDoi1.ToLower().Contains(kw) &&
                        !bb.TenDoi2.ToLower().Contains(kw) &&
                        !(bb.TenMonTheThao ?? "").ToLower().Contains(kw)) continue;
                }

                result.Add(bb);
            }

            return result.OrderByDescending(r => r.ThoiGianKetThuc ?? r.ThoiGianBatDau ?? DateTime.MinValue).ToList();
        }

        /// <summary>
        /// Lấy thông tin chi tiết của một biên bản trận đấu (danh sách VĐV, diễn biến, điểm số từng set, chữ ký 4 bên).
        /// </summary>
        public async Task<ThuKyBienBanDto?> GetChiTietBienBanAsync(int tranDauId)
        {
            var match = await _tranDauService.GetByIdAsync(tranDauId);
            if (match == null) return null;

            var gdm = await _unitOfWork.GiaiDauMonTheThaos.GetByIdAsync(match.GiaiDauMonTheThaoId);
            var giaiDau = gdm != null ? await _unitOfWork.GiaiDaus.GetByIdAsync(gdm.GiaiDauId) : null;
            var mon = gdm != null ? await _unitOfWork.MonTheThaos.GetByIdAsync(gdm.MonTheThaoId) : null;

            var parsed = ParseMatchGhiChu(match.GhiChu, match.TrangThai);

            // Đọc danh sách VĐV Đội 1 qua ThanhVienDoi (đăng ký theo đội)
            var vdv1List = new List<VanDongVienDto>();
            if (match.Doi1DangKyId.HasValue)
            {
                var dk1 = await _unitOfWork.DangKyThiDaus.GetByIdAsync(match.Doi1DangKyId.Value);
                if (dk1?.DoiId != null)
                {
                    var tvs1 = (await _unitOfWork.ThanhVienDois.FindAsync(tv => tv.DoiId == dk1.DoiId.Value && tv.IsDeleted != true)).ToList();
                    var vdv1Ids = tvs1.Select(tv => tv.VanDongVienId).ToList();
                    var rawVdvs1 = (await _unitOfWork.VanDongViens.FindAsync(v => vdv1Ids.Contains(v.Id) && v.IsDeleted != true)).ToList();
                    vdv1List = _mapper.Map<List<VanDongVienDto>>(rawVdvs1);
                }
            }

            // Đọc danh sách VĐV Đội 2 qua ThanhVienDoi (đăng ký theo đội)
            var vdv2List = new List<VanDongVienDto>();
            if (match.Doi2DangKyId.HasValue)
            {
                var dk2 = await _unitOfWork.DangKyThiDaus.GetByIdAsync(match.Doi2DangKyId.Value);
                if (dk2?.DoiId != null)
                {
                    var tvs2 = (await _unitOfWork.ThanhVienDois.FindAsync(tv => tv.DoiId == dk2.DoiId.Value && tv.IsDeleted != true)).ToList();
                    var vdv2Ids = tvs2.Select(tv => tv.VanDongVienId).ToList();
                    var rawVdvs2 = (await _unitOfWork.VanDongViens.FindAsync(v => vdv2Ids.Contains(v.Id) && v.IsDeleted != true)).ToList();
                    vdv2List = _mapper.Map<List<VanDongVienDto>>(rawVdvs2);
                }
            }

            return new ThuKyBienBanDto
            {
                TranDauId = match.Id,
                SoTran = match.SoTran,
                TenTran = match.TenTran ?? $"Trận {match.SoTran}",
                GiaiDauId = giaiDau?.Id ?? 0,
                TenGiaiDau = giaiDau?.Ten,
                TenMonTheThao = mon?.Ten ?? match.TenMonTheThao,
                TenNoiDung = $"{mon?.Ten} ({mon?.GioiTinh ?? "Hỗn hợp"})",
                TenVongDau = match.TenVongDau,
                TenBangDau = match.TenBangDau,
                TenSanDau = match.TenSanDau,
                ThoiGianBatDau = match.ThoiGianBatDau,
                ThoiGianKetThuc = match.ThoiGianKetThuc,
                TrangThai = match.TrangThai,
                TenDoi1 = match.TenDoi1 ?? "Đội 1",
                DonViDoi1 = match.DonViDoi1,
                Score1 = parsed.Score1,
                VdvDoi1 = vdv1List,
                TenDoi2 = match.TenDoi2 ?? "Đội 2",
                DonViDoi2 = match.DonViDoi2,
                Score2 = parsed.Score2,
                VdvDoi2 = vdv2List,
                Winner = parsed.Winner,
                Notes = parsed.Notes,
                SetScores = parsed.SetScores,
                Events = parsed.Events,
                DanhSachTrongTai = match.DanhSachTrongTai,
                Signatures = parsed.Signatures
            };
        }

        /// <summary>
        /// Thư ký giải ký xác nhận biên bản điện tử chính thức.
        /// </summary>
        public async Task<(bool success, string message)> KyXacNhanBienBanAsync(int tranDauId, string signerName, string username)
        {
            if (string.IsNullOrWhiteSpace(signerName))
            {
                return (false, "Vui lòng nhập họ tên người ký xác nhận.");
            }

            var match = await _unitOfWork.TranDaus.GetByIdAsync(tranDauId);
            if (match == null || match.IsDeleted == true)
            {
                return (false, "Không tìm thấy trận đấu.");
            }

            var dict = new Dictionary<string, object>();
            var sigs = new Dictionary<string, object>();

            if (!string.IsNullOrEmpty(match.GhiChu) && match.GhiChu.TrimStart().StartsWith("{"))
            {
                try
                {
                    using var doc = JsonDocument.Parse(match.GhiChu);
                    foreach (var prop in doc.RootElement.EnumerateObject())
                    {
                        if (prop.Name == "signatures")
                        {
                            foreach (var s in prop.Value.EnumerateObject())
                            {
                                sigs[s.Name] = s.Value.Clone();
                            }
                        }
                        else
                        {
                            dict[prop.Name] = prop.Value.Clone();
                        }
                    }
                }
                catch { }
            }

            sigs["secretary"] = new
            {
                signerName = signerName.Trim(),
                role = "secretary",
                signedAt = DateTime.Now
            };
            dict["signatures"] = sigs;
            dict["auditStatus"] = "DaXacNhan";
            dict["confirmedBy"] = signerName.Trim();
            dict["confirmedAt"] = DateTime.Now;

            match.GhiChu = JsonSerializer.Serialize(dict);
            match.LastModified = DateTime.UtcNow;
            match.LastModifiedBy = username;

            _unitOfWork.TranDaus.Update(match);
            await _unitOfWork.CompleteAsync();

            return (true, $"Thư ký giải [{signerName}] đã ký xác nhận biên bản trận đấu thành công!");
        }

        /// <summary>
        /// Lấy bảng tổng sắp huy chương toàn đoàn (Huy chương Vàng, Bạc, Đồng, Tổng điểm và Xếp hạng).
        /// <summary>
        /// Lấy bảng tổng sắp huy chương (Vàng, Bạc, Đồng) theo giải đấu cụ thể hoặc tổng hợp toàn bộ các giải đấu nếu giaiDauId null hoặc 0, có hỗ trợ lọc theo danh mục môn hoặc môn thể thao.
        /// </summary>
        /// <param name="giaiDauId">Mã giải đấu (tùy chọn; nếu null hoặc 0 sẽ tổng hợp toàn bộ các giải đấu trong hệ thống)</param>
        /// <param name="danhMucMonTheThaoId">Mã danh mục môn thể thao cần lọc (tùy chọn)</param>
        /// <param name="monTheThaoId">Mã môn thể thao cần lọc (tùy chọn)</param>
        /// <returns>Đối tượng DTO bảng tổng sắp huy chương toàn đoàn</returns>
        public async Task<BangTongSapHuyChuongDto> GetBangTongSapHuyChuongAsync(int? giaiDauId, int? danhMucMonTheThaoId = null, int? monTheThaoId = null)
        {
            var isAllTournaments = !giaiDauId.HasValue || giaiDauId.Value <= 0;
            GiaiDau? giaiDau = null;
            if (!isAllTournaments)
            {
                giaiDau = await _unitOfWork.GiaiDaus.GetByIdAsync(giaiDauId!.Value);
                if (giaiDau == null || giaiDau.IsDeleted == true)
                {
                    return new BangTongSapHuyChuongDto
                    {
                        GiaiDauId = 0,
                        TenGiaiDau = "Không tìm thấy giải đấu",
                        NgayXuatBaoCao = DateTime.Now
                    };
                }
            }

            var donVis = (await _unitOfWork.DonVis.FindAsync(d => d.IsDeleted != true)).ToList();
            var huyChuongs = (await _unitOfWork.HuyChuongs.FindAsync(h =>
                (isAllTournaments || h.GiaiDauId == giaiDauId!.Value) && h.IsDeleted != true
            )).ToList();
            var gdms = (await _unitOfWork.GiaiDauMonTheThaos.FindAsync(g => g.IsDeleted != true)).ToDictionary(g => g.Id);
            var mons = (await _unitOfWork.MonTheThaos.FindAsync(m => m.IsDeleted != true)).ToDictionary(m => m.Id);
            var danhMucs = (await _unitOfWork.DanhMucMonTheThaos.FindAsync(d => d.IsDeleted != true)).ToDictionary(d => d.Id);
            var loaiHcs = (await _unitOfWork.LoaiHuyChuongs.FindAsync(l => l.IsDeleted != true)).ToDictionary(l => l.Id);
            var dangKys = (await _unitOfWork.DangKyThiDaus.FindAsync(d => d.IsDeleted != true)).ToDictionary(d => d.Id);
            var dois = (await _unitOfWork.Dois.FindAsync(d => d.IsDeleted != true)).ToDictionary(d => d.Id);

            // Lấy thành viên đội thay vì ChiTietDangKyThiDau
            var doiIdsForMedal = dangKys.Values.Where(d => d.DoiId.HasValue).Select(d => d.DoiId!.Value).Distinct().ToList();
            var chiTiets = (await _unitOfWork.ThanhVienDois.FindAsync(tv => doiIdsForMedal.Contains(tv.DoiId) && tv.IsDeleted != true)).ToList();
            var vdvIds = chiTiets.Select(tv => tv.VanDongVienId).Distinct().ToList();
            var vdvs = (await _unitOfWork.VanDongViens.FindAsync(v => vdvIds.Contains(v.Id) && v.IsDeleted != true)).ToDictionary(v => v.Id);

            // Lọc huy chương theo danh mục môn hoặc môn thể thao nếu được chỉ định
            string? tenDanhMuc = null;
            if (danhMucMonTheThaoId.HasValue && danhMucMonTheThaoId.Value > 0 && danhMucs.TryGetValue(danhMucMonTheThaoId.Value, out var dm))
            {
                tenDanhMuc = dm.Ten;
            }

            string? tenMon = null;
            if (monTheThaoId.HasValue && monTheThaoId.Value > 0 && mons.TryGetValue(monTheThaoId.Value, out var m))
            {
                tenMon = m.Ten;
            }

            var filteredHuyChuongs = huyChuongs.Where(hc =>
            {
                if (!gdms.TryGetValue(hc.GiaiDauMonTheThaoId, out var gdm)) return false;
                if (monTheThaoId.HasValue && monTheThaoId.Value > 0 && gdm.MonTheThaoId != monTheThaoId.Value) return false;
                if (danhMucMonTheThaoId.HasValue && danhMucMonTheThaoId.Value > 0)
                {
                    if (!mons.TryGetValue(gdm.MonTheThaoId, out var sport) || sport.DanhMucId != danhMucMonTheThaoId.Value)
                        return false;
                }
                return true;
            }).ToList();

            var (sortedRankings, vangTotal, bacTotal, dongTotal) = TinhBangXepHangDoan(
                filteredHuyChuongs, donVis, loaiHcs, dangKys, dois, chiTiets, vdvs, chiLayDonViCoHuyChuong: false);

            return new BangTongSapHuyChuongDto
            {
                GiaiDauId = isAllTournaments ? 0 : giaiDau!.Id,
                TenGiaiDau = isAllTournaments ? "Tất cả các giải đấu (Tổng hợp toàn đoàn)" : giaiDau!.Ten,
                DanhMucMonTheThaoId = danhMucMonTheThaoId,
                TenDanhMucMonTheThao = tenDanhMuc,
                MonTheThaoId = monTheThaoId,
                TenMonTheThao = tenMon,
                TongSoHuyChuongVang = vangTotal,
                TongSoHuyChuongBac = bacTotal,
                TongSoHuyChuongDong = dongTotal,
                BangXepHang = sortedRankings,
                NgayXuatBaoCao = DateTime.Now
            };
        }

        /// <summary>
        /// Lấy danh sách bảng xếp hạng huy chương phân loại theo từng Danh mục môn thể thao.
        /// </summary>
        /// <param name="giaiDauId">ID giải đấu (tùy chọn; null hoặc 0 để tính toàn bộ các giải).</param>
        /// <returns>Danh sách các bảng xếp hạng huy chương gom nhóm theo từng Danh mục môn thể thao.</returns>
        public async Task<List<BangXepHangTheoDanhMucDto>> GetBangXepHangTheoDanhMucAsync(int? giaiDauId)
        {
            var isAllTournaments = !giaiDauId.HasValue || giaiDauId.Value <= 0;
            var donVis = (await _unitOfWork.DonVis.FindAsync(d => d.IsDeleted != true)).ToList();
            var danhMucs = (await _unitOfWork.DanhMucMonTheThaos.FindAsync(d => d.IsDeleted != true && d.TrangThai)).ToList();
            var mons = (await _unitOfWork.MonTheThaos.FindAsync(m => m.IsDeleted != true && m.TrangThai)).ToList();
            var gdms = (await _unitOfWork.GiaiDauMonTheThaos.FindAsync(g =>
                (isAllTournaments || g.GiaiDauId == giaiDauId!.Value) && g.IsDeleted != true
            )).ToList();

            var gdmIds = gdms.Select(g => g.Id).ToList();
            var huyChuongs = (await _unitOfWork.HuyChuongs.FindAsync(h =>
                (isAllTournaments || h.GiaiDauId == giaiDauId!.Value) &&
                gdmIds.Contains(h.GiaiDauMonTheThaoId) &&
                h.IsDeleted != true
            )).ToList();

            var loaiHcs = (await _unitOfWork.LoaiHuyChuongs.FindAsync(l => l.IsDeleted != true)).ToDictionary(l => l.Id);
            var dangKys = (await _unitOfWork.DangKyThiDaus.FindAsync(d => d.IsDeleted != true)).ToDictionary(d => d.Id);
            var dois = (await _unitOfWork.Dois.FindAsync(d => d.IsDeleted != true)).ToDictionary(d => d.Id);

            // Lấy thành viên đội thay vì ChiTietDangKyThiDau
            var doiIdsForDanhMuc = dangKys.Values.Where(d => d.DoiId.HasValue).Select(d => d.DoiId!.Value).Distinct().ToList();
            var chiTiets = (await _unitOfWork.ThanhVienDois.FindAsync(tv => doiIdsForDanhMuc.Contains(tv.DoiId) && tv.IsDeleted != true)).ToList();
            var vdvIds = chiTiets.Select(tv => tv.VanDongVienId).Distinct().ToList();
            var vdvs = (await _unitOfWork.VanDongViens.FindAsync(v => vdvIds.Contains(v.Id) && v.IsDeleted != true)).ToDictionary(v => v.Id);

            var gdmDict = gdms.ToDictionary(g => g.Id);
            var monDict = mons.ToDictionary(m => m.Id);

            var result = new List<BangXepHangTheoDanhMucDto>();

            foreach (var dm in danhMucs)
            {
                var monsInDm = mons.Where(m => m.DanhMucId == dm.Id).Select(m => m.Id).ToHashSet();
                var gdmsInDm = gdms.Where(g => monsInDm.Contains(g.MonTheThaoId)).Select(g => g.Id).ToHashSet();

                // Nếu có giải đấu cụ thể và danh mục này không có môn nào trong giải thì bỏ qua
                if (!isAllTournaments && gdmsInDm.Count == 0) continue;

                var medalsInDm = huyChuongs.Where(h => gdmsInDm.Contains(h.GiaiDauMonTheThaoId)).ToList();
                var (bangXepHang, vTot, bTot, dTot) = TinhBangXepHangDoan(
                    medalsInDm, donVis, loaiHcs, dangKys, dois, chiTiets, vdvs, chiLayDonViCoHuyChuong: true);

                result.Add(new BangXepHangTheoDanhMucDto
                {
                    DanhMucId = dm.Id,
                    MaDanhMuc = dm.Ma,
                    TenDanhMuc = dm.Ten,
                    TongSoMon = gdmsInDm.Count > 0 ? gdms.Count(g => gdmsInDm.Contains(g.Id)) : monsInDm.Count,
                    TongHuyChuongVang = vTot,
                    TongHuyChuongBac = bTot,
                    TongHuyChuongDong = dTot,
                    BangXepHang = bangXepHang
                });
            }

            return result.OrderByDescending(r => r.TongHuyChuong)
                         .ThenByDescending(r => r.TongHuyChuongVang)
                         .ThenBy(r => r.TenDanhMuc)
                         .ToList();
        }

        /// <summary>
        /// Lấy kết quả trao huy chương riêng cho từng MonTheThao trong từng giải đấu.
        /// </summary>
        /// <param name="giaiDauId">ID giải đấu; null hoặc 0 để lấy kết quả từng môn của mọi giải, không gộp các giải với nhau.</param>
        /// <param name="danhMucMonTheThaoId">ID danh mục môn thể thao (tùy chọn để lọc môn thuộc danh mục).</param>
        /// <returns>Danh sách bảng kết quả theo từng giải và MonTheThao, giữ riêng người hoặc đội nhận từng huy chương.</returns>
        public async Task<List<BangXepHangTheoMonDto>> GetBangXepHangTheoMonAsync(int? giaiDauId, int? danhMucMonTheThaoId = null)
        {
            var isAllTournaments = !giaiDauId.HasValue || giaiDauId.Value <= 0;
            var danhMucs = (await _unitOfWork.DanhMucMonTheThaos.FindAsync(d => d.IsDeleted != true)).ToDictionary(d => d.Id);
            var mons = (await _unitOfWork.MonTheThaos.FindAsync(m =>
                m.IsDeleted != true &&
                (!danhMucMonTheThaoId.HasValue || danhMucMonTheThaoId.Value <= 0 || m.DanhMucId == danhMucMonTheThaoId.Value)
            )).ToList();

            var gdms = (await _unitOfWork.GiaiDauMonTheThaos.FindAsync(g =>
                (isAllTournaments || g.GiaiDauId == giaiDauId!.Value) && g.IsDeleted != true
            )).ToList();

            var gdmIds = gdms.Select(g => g.Id).ToList();
            var giaiDauIds = gdms.Select(g => g.GiaiDauId).Distinct().ToList();
            var giaiDaus = (await _unitOfWork.GiaiDaus.FindAsync(g => giaiDauIds.Contains(g.Id) && g.IsDeleted != true)).ToDictionary(g => g.Id);
            var huyChuongs = (await _unitOfWork.HuyChuongs.FindAsync(h =>
                (isAllTournaments || h.GiaiDauId == giaiDauId!.Value) &&
                gdmIds.Contains(h.GiaiDauMonTheThaoId) &&
                h.IsDeleted != true
            )).ToList();

            var loaiHcs = (await _unitOfWork.LoaiHuyChuongs.FindAsync(l => l.IsDeleted != true)).ToDictionary(l => l.Id);
            var dkIds = huyChuongs.Select(h => h.DangKyThiDauId).Distinct().ToList();
            var dangKys = (await _unitOfWork.DangKyThiDaus.FindAsync(d => dkIds.Contains(d.Id) && d.IsDeleted != true)).ToDictionary(d => d.Id);
            var doiIds = dangKys.Values.Where(d => d.DoiId.HasValue).Select(d => d.DoiId!.Value).Distinct().ToList();
            var dois = (await _unitOfWork.Dois.FindAsync(d => doiIds.Contains(d.Id) && d.IsDeleted != true)).ToDictionary(d => d.Id);
            // Lấy thành viên đội thay vì ChiTietDangKyThiDau
            var doiIdsForMon = dois.Keys.ToList();
            var chiTiets = (await _unitOfWork.ThanhVienDois.FindAsync(tv => doiIdsForMon.Contains(tv.DoiId) && tv.IsDeleted != true)).ToList();
            var vdvIds = chiTiets.Select(tv => tv.VanDongVienId).Distinct().ToList();
            var vdvs = (await _unitOfWork.VanDongViens.FindAsync(v => vdvIds.Contains(v.Id) && v.IsDeleted != true)).ToDictionary(v => v.Id);
            var donViIds = dois.Values.Where(d => d.DonViId.HasValue).Select(d => d.DonViId!.Value)
                .Concat(vdvs.Values.Where(v => v.DonViId.HasValue).Select(v => v.DonViId!.Value))
                .Distinct()
                .ToList();
            var donVis = (await _unitOfWork.DonVis.FindAsync(d => donViIds.Contains(d.Id) && d.IsDeleted != true)).ToDictionary(d => d.Id);
            var monMap = mons.ToDictionary(m => m.Id);

            var result = new List<BangXepHangTheoMonDto>();

            foreach (var gdm in gdms)
            {
                if (!monMap.TryGetValue(gdm.MonTheThaoId, out var mon)) continue;

                var medalsForEvent = huyChuongs.Where(h => h.GiaiDauMonTheThaoId == gdm.Id).ToList();
                var (ketQua, vangTotal, bacTotal, dongTotal) = TinhKetQuaHuyChuongMon(
                    medalsForEvent, mon.LaMonDongDoi, loaiHcs, dangKys, dois, chiTiets, vdvs, donVis);

                danhMucs.TryGetValue(mon.DanhMucId, out var dm);
                giaiDaus.TryGetValue(gdm.GiaiDauId, out var giaiDau);

                result.Add(new BangXepHangTheoMonDto
                {
                    GiaiDauId = gdm.GiaiDauId,
                    TenGiaiDau = giaiDau?.Ten ?? "Giải đấu",
                    GiaiDauMonTheThaoId = gdm.Id,
                    MonTheThaoId = mon.Id,
                    MaMon = mon.Ma,
                    TenMon = mon.Ten,
                    DanhMucId = mon.DanhMucId,
                    TenDanhMuc = dm?.Ten ?? "Chưa phân loại",
                    TongHuyChuongVang = vangTotal,
                    TongHuyChuongBac = bacTotal,
                    TongHuyChuongDong = dongTotal,
                    LaMonDongDoi = mon.LaMonDongDoi,
                    KetQua = ketQua
                });
            }

            return result.OrderBy(r => r.TenGiaiDau)
                         .ThenBy(r => r.TenDanhMuc)
                         .ThenBy(r => r.TenMon)
                         .ToList();
        }

        /// <summary>
        /// Hàm nội bộ hỗ trợ tính toán và xếp hạng danh sách huy chương theo các đoàn/đơn vị tham gia.
        /// </summary>
        /// <param name="medals">Danh sách huy chương đầu vào.</param>
        /// <param name="donVis">Danh sách đơn vị trong hệ thống.</param>
        /// <param name="loaiHcs">Map danh mục loại huy chương.</param>
        /// <param name="dangKys">Map đăng ký thi đấu.</param>
        /// <param name="dois">Map đội tham gia.</param>
        /// <param name="chiTiets">Danh sách thành viên đội (thay thế chi tiết đăng ký thi đấu).</param>
        /// <param name="vdvs">Map vận động viên.</param>
        /// <param name="chiLayDonViCoHuyChuong">True nếu chỉ muốn lấy các đơn vị có ít nhất 1 huy chương.</param>
        /// <returns>Bộ tuple gồm danh sách xếp hạng đã sắp xếp và tổng số lượng từng loại huy chương.</returns>
        private (List<HuyChuongDoanDto> bangXepHang, int vangTotal, int bacTotal, int dongTotal) TinhBangXepHangDoan(
            List<HuyChuong> medals,
            List<DonVi> donVis,
            Dictionary<int, LoaiHuyChuong> loaiHcs,
            Dictionary<int, DangKyThiDau> dangKys,
            Dictionary<int, Doi> dois,
            List<ThanhVienDoi> chiTiets,
            Dictionary<int, VanDongVien> vdvs,
            bool chiLayDonViCoHuyChuong)
        {
            var tally = new Dictionary<int, (int vang, int bac, int dong)>();
            if (!chiLayDonViCoHuyChuong)
            {
                foreach (var dv in donVis)
                {
                    tally[dv.Id] = (0, 0, 0);
                }
            }

            int vangTotal = 0, bacTotal = 0, dongTotal = 0;

            foreach (var hc in medals)
            {
                loaiHcs.TryGetValue(hc.LoaiHuyChuongId, out var loai);
                string loaiTen = (loai?.Ten ?? "").ToLower();

                int? donViId = null;
                if (dangKys.TryGetValue(hc.DangKyThiDauId, out var dk))
                {
                    if (dk.DoiId.HasValue && dois.TryGetValue(dk.DoiId.Value, out var doi))
                    {
                        donViId = doi.DonViId;
                    }
                    if (!donViId.HasValue && dk.DoiId.HasValue)
                    {
                        // Fallback: lấy đơn vị qua thành viên đội đầu tiên
                        var tv = chiTiets.FirstOrDefault(c => c.DoiId == dk.DoiId.Value);
                        if (tv != null && vdvs.TryGetValue(tv.VanDongVienId, out var vdv))
                        {
                            donViId = vdv.DonViId;
                        }
                    }
                }

                if (!donViId.HasValue) continue;

                if (!tally.ContainsKey(donViId.Value))
                {
                    tally[donViId.Value] = (0, 0, 0);
                }

                var cur = tally[donViId.Value];
                if (loaiTen.Contains("vàng") || loaiTen.Contains("gold") || hc.XepHang == 1)
                {
                    tally[donViId.Value] = (cur.vang + 1, cur.bac, cur.dong);
                    vangTotal++;
                }
                else if (loaiTen.Contains("bạc") || loaiTen.Contains("silver") || hc.XepHang == 2)
                {
                    tally[donViId.Value] = (cur.vang, cur.bac + 1, cur.dong);
                    bacTotal++;
                }
                else
                {
                    tally[donViId.Value] = (cur.vang, cur.bac, cur.dong + 1);
                    dongTotal++;
                }
            }

            var donViMap = donVis.ToDictionary(d => d.Id);
            var list = new List<HuyChuongDoanDto>();

            foreach (var kvp in tally)
            {
                if (chiLayDonViCoHuyChuong && (kvp.Value.vang + kvp.Value.bac + kvp.Value.dong == 0))
                {
                    continue;
                }

                donViMap.TryGetValue(kvp.Key, out var dv);
                list.Add(new HuyChuongDoanDto
                {
                    DonViId = kvp.Key,
                    MaDonVi = dv?.Ma ?? "",
                    TenDonVi = dv?.Ten ?? "Đơn vị",
                    SoHuyChuongVang = kvp.Value.vang,
                    SoHuyChuongBac = kvp.Value.bac,
                    SoHuyChuongDong = kvp.Value.dong
                });
            }

            var sorted = list.OrderByDescending(x => x.SoHuyChuongVang)
                             .ThenByDescending(x => x.SoHuyChuongBac)
                             .ThenByDescending(x => x.SoHuyChuongDong)
                             .ThenByDescending(x => x.TongSoHuyChuong)
                             .ThenBy(x => x.TenDonVi)
                             .ToList();

            for (int i = 0; i < sorted.Count; i++)
            {
                sorted[i].XepHang = i + 1;
            }

            return (sorted, vangTotal, bacTotal, dongTotal);
        }

        /// <summary>
        /// Tạo danh sách kết quả huy chương cho đúng một MonTheThao, giữ nguyên từng đăng ký nhận giải thay vì cộng gộp theo đơn vị.
        /// Với nội dung đồng đội, kết quả hiển thị đội; với nội dung cá nhân, kết quả hiển thị vận động viên.
        /// </summary>
        /// <param name="medals">Các huy chương thuộc một môn trong một giải đấu.</param>
        /// <param name="laMonDongDoi">True nếu người nhận giải là đội; false nếu người nhận giải là vận động viên.</param>
        /// <param name="loaiHcs">Map loại huy chương để xác định tên và số lượng từng huy chương.</param>
        /// <param name="dangKys">Map đăng ký thi đấu nhận huy chương.</param>
        /// <param name="dois">Map đội đăng ký thi đấu còn hoạt động.</param>
        /// <param name="chiTiets">Danh sách thành viên đội (thay thế chi tiết đăng ký thi đấu).</param>
        /// <param name="vdvs">Map vận động viên còn hoạt động.</param>
        /// <param name="donVis">Map đơn vị để hiển thị đơn vị trực thuộc người hoặc đội nhận giải.</param>
        /// <returns>Các dòng kết quả theo thứ hạng cùng tổng số huy chương vàng, bạc và đồng trong môn.</returns>
        private (List<KetQuaHuyChuongMonDto> ketQua, int vangTotal, int bacTotal, int dongTotal) TinhKetQuaHuyChuongMon(
            List<HuyChuong> medals,
            bool laMonDongDoi,
            Dictionary<int, LoaiHuyChuong> loaiHcs,
            Dictionary<int, DangKyThiDau> dangKys,
            Dictionary<int, Doi> dois,
            List<ThanhVienDoi> chiTiets,
            Dictionary<int, VanDongVien> vdvs,
            Dictionary<int, DonVi> donVis)
        {
            int vangTotal = 0;
            int bacTotal = 0;
            int dongTotal = 0;
            var results = new List<KetQuaHuyChuongMonDto>();

            foreach (var medal in medals.OrderBy(m => m.XepHang > 0 ? m.XepHang : int.MaxValue).ThenBy(m => m.Id))
            {
                if (!dangKys.TryGetValue(medal.DangKyThiDauId, out var dangKy)) continue;

                // Lấy VĐV đầu tiên trong đội qua ThanhVienDoi
                VanDongVien? vdv = null;
                if (dangKy.DoiId.HasValue)
                {
                    var tvFirst = chiTiets
                        .Where(tv => tv.DoiId == dangKy.DoiId.Value)
                        .OrderBy(tv => tv.Id)
                        .FirstOrDefault();
                    if (tvFirst != null) vdvs.TryGetValue(tvFirst.VanDongVienId, out vdv);
                }

                loaiHcs.TryGetValue(medal.LoaiHuyChuongId, out var loai);
                string loaiTen = (loai?.Ten ?? "").ToLowerInvariant();
                int medalRank = loaiTen.Contains("vàng") || loaiTen.Contains("gold") || medal.XepHang == 1
                    ? 1
                    : loaiTen.Contains("bạc") || loaiTen.Contains("silver") || medal.XepHang == 2
                        ? 2
                        : 3;
                if (medalRank == 1) vangTotal++;
                else if (medalRank == 2) bacTotal++;
                else dongTotal++;

                dois.TryGetValue(dangKy.DoiId ?? 0, out var doi);
                int? donViId = laMonDongDoi ? doi?.DonViId : vdv?.DonViId;
                donVis.TryGetValue(donViId ?? 0, out var donVi);

                results.Add(new KetQuaHuyChuongMonDto
                {
                    DangKyThiDauId = dangKy.Id,
                    XepHang = medal.XepHang > 0 ? medal.XepHang : medalRank,
                    TenLoaiHuyChuong = loai?.Ten ?? (medalRank == 1 ? "Vàng" : medalRank == 2 ? "Bạc" : "Đồng"),
                    VanDongVienId = laMonDongDoi ? null : vdv?.Id,
                    MaVanDongVien = laMonDongDoi ? null : vdv?.Ma,
                    TenVanDongVien = laMonDongDoi ? null : vdv?.HoTen ?? dangKy.TenDangKy,
                    DoiId = laMonDongDoi ? doi?.Id : null,
                    MaDoi = laMonDongDoi ? doi?.Ma : null,
                    TenDoi = laMonDongDoi ? doi?.Ten ?? dangKy.TenDangKy ?? vdv?.HoTen : null,
                    DonViId = donViId,
                    MaDonVi = donVi?.Ma,
                    TenDonVi = donVi?.Ten
                });
            }

            return (results, vangTotal, bacTotal, dongTotal);
        }

        /// <summary>
        /// Tổng hợp báo cáo toàn diện giải đấu (tiến độ, số lượng VĐV, tổng kết huy chương, biên bản).
        /// </summary>
        public async Task<ThuKyBaoCaoTongHopDto> GetBaoCaoTongHopAsync(int? giaiDauId)
        {
            var giaiDau = await GetEffectiveTournamentAsync(giaiDauId);
            if (giaiDau == null) return new ThuKyBaoCaoTongHopDto();

            int gId = giaiDau.Id;

            var gdmList = (await _unitOfWork.GiaiDauMonTheThaos.FindAsync(g => g.GiaiDauId == gId && g.IsDeleted != true)).ToList();
            var gdmIds = gdmList.Select(g => g.Id).ToList();

            var matches = (await _tranDauService.GetAllAsync(giaiDauId: gId))?.ToList() ?? new List<TranDauDto>();
            var dangKys = (await _unitOfWork.DangKyThiDaus.FindAsync(d => gdmIds.Contains(d.GiaiDauMonTheThaoId) && d.IsDeleted != true)).ToList();
            var donVis = (await _unitOfWork.DonVis.FindAsync(d => d.IsDeleted != true)).ToList();

            // Lấy VĐV từ thành viên đội thay vì ChiTietDangKyThiDau
            var doiIdsForBaoCao = dangKys.Where(d => d.DoiId.HasValue).Select(d => d.DoiId!.Value).Distinct().ToList();
            var thanhViensBaoCao = (await _unitOfWork.ThanhVienDois.FindAsync(tv => doiIdsForBaoCao.Contains(tv.DoiId) && tv.IsDeleted != true)).ToList();
            var vdvIds = thanhViensBaoCao.Select(tv => tv.VanDongVienId).Distinct().ToList();

            var bangTongSap = await GetBangTongSapHuyChuongAsync(gId);
            var tienDoMonList = await GetTienDoCacMonAsync(gId);

            int totalMatches = matches.Count;
            int completed = matches.Count(m => m.TrangThai == "HoanThanh" || m.TrangThai == "KetThuc");
            int live = matches.Count(m => m.TrangThai == "DangDienRa" || m.TrangThai == "DangDau");
            int upcoming = matches.Count(m => m.TrangThai == "ChuaDau" || m.TrangThai == "SapDau");

            int hopLe = 0;
            foreach (var m in matches.Where(x => x.TrangThai == "HoanThanh" || x.TrangThai == "KetThuc"))
            {
                var p = ParseMatchGhiChu(m.GhiChu, m.TrangThai);
                if (p.Signatures.ContainsKey("referee") && p.Signatures.ContainsKey("team1") && p.Signatures.ContainsKey("team2"))
                {
                    hopLe++;
                }
            }

            return new ThuKyBaoCaoTongHopDto
            {
                GiaiDauId = gId,
                TenGiaiDau = giaiDau.Ten,
                NgayBatDau = giaiDau.NgayBatDau,
                NgayKetThuc = giaiDau.NgayKetThuc,
                DiaDiem = giaiDau.DiaDiem,
                TongSoMon = gdmList.Select(g => g.MonTheThaoId).Distinct().Count(),
                TongSoNoiDung = gdmList.Count,
                TongSoDoan = donVis.Count,
                TongSoVdv = vdvIds.Count,
                TongSoTranDau = totalMatches,
                SoTranDaDau = completed,
                SoTranDangDau = live,
                SoTranChuaDau = upcoming,
                TiLeHoanThanh = totalMatches > 0 ? Math.Round((double)completed / totalMatches * 100, 1) : 0,
                TongHuyChuongDaTrao = bangTongSap.TongSoHuyChuongDaTrao,
                TongBienBanDaKiemTra = completed,
                TongBienBanHopLe = hopLe,
                BangTongSap = bangTongSap,
                TienDoMonList = tienDoMonList
            };
        }

        /// <summary>
        /// Tra cứu thông minh đa đối tượng trong giải đấu (Vận động viên, Đơn vị/Đoàn thể thao, Trận đấu & Biên bản).
        /// </summary>
        public async Task<ThuKyTraCuuResultDto> TraCuuTongHopAsync(string keyword, int? giaiDauId = null, string? loaiDoiTuong = "All")
        {
            var result = new ThuKyTraCuuResultDto { Keyword = keyword ?? "" };
            if (string.IsNullOrWhiteSpace(keyword)) return result;

            var kw = keyword.Trim().ToLower();

            // 1. Tra cứu VĐV
            if (loaiDoiTuong == "All" || loaiDoiTuong == "Vdv")
            {
                var vdvs = (await _unitOfWork.VanDongViens.FindAsync(v => v.IsDeleted != true &&
                    (v.HoTen.ToLower().Contains(kw) || v.Ma.ToLower().Contains(kw) || (v.SoCCCD != null && v.SoCCCD.Contains(kw))))).ToList();

                var donVis = (await _unitOfWork.DonVis.FindAsync(d => d.IsDeleted != true)).ToDictionary(d => d.Id);
                var vdvIds = vdvs.Select(v => v.Id).ToList();
                // Lấy đội của VĐV qua ThanhVienDoi thay vì ChiTietDangKyThiDau
                var thanhViensTraCuu = (await _unitOfWork.ThanhVienDois.FindAsync(tv => vdvIds.Contains(tv.VanDongVienId) && tv.IsDeleted != true)).ToList();
                var doiIdsTraCuu = thanhViensTraCuu.Select(tv => tv.DoiId).Distinct().ToList();
                var dkIds = (await _unitOfWork.DangKyThiDaus.FindAsync(d => d.DoiId.HasValue && doiIdsTraCuu.Contains(d.DoiId!.Value) && d.IsDeleted != true)).Select(d => d.Id).Distinct().ToList();
                var dangKys = (await _unitOfWork.DangKyThiDaus.FindAsync(d => dkIds.Contains(d.Id) && d.IsDeleted != true)).ToDictionary(d => d.Id);
                var gdmIds = dangKys.Values.Select(d => d.GiaiDauMonTheThaoId).Distinct().ToList();
                var gdms = (await _unitOfWork.GiaiDauMonTheThaos.FindAsync(g => gdmIds.Contains(g.Id) && g.IsDeleted != true)).ToDictionary(g => g.Id);
                var monIds = gdms.Values.Select(g => g.MonTheThaoId).Distinct().ToList();
                var mons = (await _unitOfWork.MonTheThaos.FindAsync(m => monIds.Contains(m.Id) && m.IsDeleted != true)).ToDictionary(m => m.Id);

                var huyChuongs = (await _unitOfWork.HuyChuongs.FindAsync(h => dkIds.Contains(h.DangKyThiDauId) && h.IsDeleted != true)).ToList();
                var loaiHcs = (await _unitOfWork.LoaiHuyChuongs.FindAsync(l => l.IsDeleted != true)).ToDictionary(l => l.Id);

                foreach (var v in vdvs)
                {
                    donVis.TryGetValue(v.DonViId ?? 0, out var dv);
                    // Lấy các đăng ký liên quan qua đội của VĐV
                    var myDoiIds = thanhViensTraCuu.Where(tv => tv.VanDongVienId == v.Id).Select(tv => tv.DoiId).ToList();
                    var myCts = dangKys.Values.Where(d => d.DoiId.HasValue && myDoiIds.Contains(d.DoiId!.Value)).Select(d => d.Id).ToList();
                    var monNames = new HashSet<string>();
                    var medalNames = new List<string>();

                    foreach (var dkid in myCts)
                    {
                        if (dangKys.TryGetValue(dkid, out var dk) && gdms.TryGetValue(dk.GiaiDauMonTheThaoId, out var gdm) && mons.TryGetValue(gdm.MonTheThaoId, out var mon))
                        {
                            monNames.Add(mon.Ten);
                        }

                        var myHcs = huyChuongs.Where(h => h.DangKyThiDauId == dkid).ToList();
                        foreach (var hc in myHcs)
                        {
                            loaiHcs.TryGetValue(hc.LoaiHuyChuongId, out var lhc);
                            medalNames.Add(lhc?.Ten ?? $"Hạng {hc.XepHang}");
                        }
                    }

                    result.VanDongViens.Add(new TraCuuVdvItemDto
                    {
                        Id = v.Id,
                        MaVdv = v.Ma,
                        HoTen = v.HoTen,
                        GioiTinh = v.GioiTinh,
                        NgaySinh = v.NgaySinh,
                        SoCCCD = v.SoCCCD,
                        TenDonVi = dv?.Ten ?? "Tự do",
                        CacMonThiDau = monNames.ToList(),
                        ThanhTich = medalNames
                    });
                }
            }

            // 2. Tra cứu Đơn vị / Đoàn
            if (loaiDoiTuong == "All" || loaiDoiTuong == "DonVi")
            {
                var donVis = (await _unitOfWork.DonVis.FindAsync(d => d.IsDeleted != true &&
                    (d.Ten.ToLower().Contains(kw) || d.Ma.ToLower().Contains(kw)))).ToList();

                var bangTongSap = await GetBangTongSapHuyChuongAsync(giaiDauId);
                var mapXepHang = bangTongSap.BangXepHang.ToDictionary(b => b.DonViId);

                var allVdvs = (await _unitOfWork.VanDongViens.FindAsync(v => v.IsDeleted != true)).ToList();

                foreach (var dv in donVis)
                {
                    mapXepHang.TryGetValue(dv.Id, out var hc);
                    int vdvCount = allVdvs.Count(v => v.DonViId == dv.Id);

                    result.DonVis.Add(new TraCuuDonViItemDto
                    {
                        Id = dv.Id,
                        MaDonVi = dv.Ma,
                        TenDonVi = dv.Ten,
                        NguoiLienHe = dv.NguoiDaiDien,
                        SoDienThoai = dv.SoDienThoai,
                        SoVdvThamGia = vdvCount,
                        SoMonThamGia = 0,
                        SoHuyChuongVang = hc?.SoHuyChuongVang ?? 0,
                        SoHuyChuongBac = hc?.SoHuyChuongBac ?? 0,
                        SoHuyChuongDong = hc?.SoHuyChuongDong ?? 0
                    });
                }
            }

            // 3. Tra cứu Trận đấu & Biên bản
            if (loaiDoiTuong == "All" || loaiDoiTuong == "TranDau")
            {
                var ketQuas = await GetDanhSachKetQuaAsync(giaiDauId, keyword: kw);
                result.TranDaus = ketQuas.Take(25).ToList();
            }

            return result;
        }
    }
}
