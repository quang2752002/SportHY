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
    /// <summary>
    /// Service quản lý ghi nhận, cập nhật, hiển thị và thu hồi huy chương trao cho các đơn vị/VĐV
    /// </summary>
    public class HuyChuongService : IHuyChuongService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IGroupStandingsEngine _groupStandingsEngine;
        private readonly ICauHinhTheThucService _theThucService;

        /// <summary>
        /// Khởi tạo Service quản lý huy chương với các phụ thuộc cần thiết
        /// </summary>
        /// <param name="unitOfWork">Đơn vị công tác truy cập dữ liệu</param>
        /// <param name="groupStandingsEngine">Động cơ tính toán bảng xếp hạng vòng bảng</param>
        /// <param name="theThucService">Service cấu hình thể thức thi đấu</param>
        public HuyChuongService(
            IUnitOfWork unitOfWork,
            IGroupStandingsEngine groupStandingsEngine,
            ICauHinhTheThucService theThucService)
        {
            _unitOfWork = unitOfWork;
            _groupStandingsEngine = groupStandingsEngine;
            _theThucService = theThucService;
        }

        /// <summary>
        /// Lấy toàn bộ danh sách huy chương đã trao, có thể lọc theo giải đấu và môn thi đấu
        /// </summary>
        /// <param name="giaiDauId">Mã giải đấu (tùy chọn)</param>
        /// <param name="monTheThaoId">Mã môn thể thao (tùy chọn)</param>
        /// <returns>Danh sách chi tiết các huy chương đã trao</returns>
        public async Task<IEnumerable<HuyChuongDto>> GetAllAsync(int? giaiDauId = null, int? monTheThaoId = null)
        {
            var huyChuongs = (await _unitOfWork.HuyChuongs.FindAsync(h =>
                h.IsDeleted != true &&
                (!giaiDauId.HasValue || giaiDauId.Value == 0 || h.GiaiDauId == giaiDauId.Value)
            )).ToList();

            if (!huyChuongs.Any()) return Enumerable.Empty<HuyChuongDto>();

            var gIds = huyChuongs.Select(h => h.GiaiDauId).Distinct().ToList();
            var gdmIds = huyChuongs.Select(h => h.GiaiDauMonTheThaoId).Distinct().ToList();
            var dkIds = huyChuongs.Select(h => h.DangKyThiDauId).Distinct().ToList();
            var lhcIds = huyChuongs.Select(h => h.LoaiHuyChuongId).Distinct().ToList();

            var giaiDaus = (await _unitOfWork.GiaiDaus.FindAsync(g => gIds.Contains(g.Id))).ToDictionary(g => g.Id);
            var gdms = (await _unitOfWork.GiaiDauMonTheThaos.FindAsync(g => gdmIds.Contains(g.Id))).ToDictionary(g => g.Id);
            var monIds = gdms.Values.Select(m => m.MonTheThaoId).Distinct().ToList();
            var mons = (await _unitOfWork.MonTheThaos.FindAsync(m => monIds.Contains(m.Id))).ToDictionary(m => m.Id);

            var dks = (await _unitOfWork.DangKyThiDaus.FindAsync(d => dkIds.Contains(d.Id))).ToDictionary(d => d.Id);
            var lhcs = (await _unitOfWork.LoaiHuyChuongs.FindAsync(l => lhcIds.Contains(l.Id))).ToDictionary(l => l.Id);

            var dois = (await _unitOfWork.Dois.FindAsync(d => d.IsDeleted != true)).ToDictionary(d => d.Id);
            var donVis = (await _unitOfWork.DonVis.FindAsync(d => d.IsDeleted != true)).ToDictionary(d => d.Id);

            // Lấy thành viên đội theo DoiId thay vì ChiTietDangKyThiDau
            var doiIds = dks.Values.Where(d => d.DoiId.HasValue).Select(d => d.DoiId!.Value).Distinct().ToList();
            var thanhViens = (await _unitOfWork.ThanhVienDois.FindAsync(tv => doiIds.Contains(tv.DoiId) && tv.IsDeleted != true)).ToList();
            var vdvIds = thanhViens.Select(tv => tv.VanDongVienId).Distinct().ToList();
            var vdvs = (await _unitOfWork.VanDongViens.FindAsync(v => vdvIds.Contains(v.Id))).ToDictionary(v => v.Id);

            var result = new List<HuyChuongDto>();
            foreach (var hc in huyChuongs)
            {
                gdms.TryGetValue(hc.GiaiDauMonTheThaoId, out var gdm);
                if (monTheThaoId.HasValue && monTheThaoId.Value > 0)
                {
                    if (gdm == null || gdm.MonTheThaoId != monTheThaoId.Value) continue;
                }

                giaiDaus.TryGetValue(hc.GiaiDauId, out var gd);
                MonTheThao? mon = null;
                if (gdm != null) mons.TryGetValue(gdm.MonTheThaoId, out mon);

                dks.TryGetValue(hc.DangKyThiDauId, out var dk);
                lhcs.TryGetValue(hc.LoaiHuyChuongId, out var lhc);

                string? tenDonVi = null;
                string? tenVdv = null;
                string? tenDoi = null;

                if (dk != null)
                {
                    if (dk.DoiId.HasValue && dois.TryGetValue(dk.DoiId.Value, out var doi))
                    {
                        tenDoi = doi.Ten;
                        if (doi.DonViId.HasValue && donVis.TryGetValue(doi.DonViId.Value, out var dvDoi))
                        {
                            tenDonVi = dvDoi.Ten;
                        }
                    }

                    // Lấy tên VĐV và đơn vị từ thành viên đội
                    if (dk.DoiId.HasValue)
                    {
                        var tvList = thanhViens.Where(tv => tv.DoiId == dk.DoiId.Value).ToList();
                        if (tvList.Any())
                        {
                            var vdvNames = tvList
                                .Select(tv => vdvs.TryGetValue(tv.VanDongVienId, out var v) ? v.HoTen : "")
                                .Where(s => !string.IsNullOrEmpty(s))
                                .ToList();
                            tenVdv = string.Join(", ", vdvNames);

                            if (string.IsNullOrEmpty(tenDonVi))
                            {
                                var firstVdvId = tvList.First().VanDongVienId;
                                if (vdvs.TryGetValue(firstVdvId, out var fVdv) && fVdv.DonViId.HasValue && donVis.TryGetValue(fVdv.DonViId.Value, out var dvVdv))
                                {
                                    tenDonVi = dvVdv.Ten;
                                }
                            }
                        }
                    }
                }

                result.Add(new HuyChuongDto
                {
                    Id = hc.Id,
                    GiaiDauId = hc.GiaiDauId,
                    TenGiaiDau = gd?.Ten,
                    GiaiDauMonTheThaoId = hc.GiaiDauMonTheThaoId,
                    TenMonTheThao = mon?.Ten,
                    DangKyThiDauId = hc.DangKyThiDauId,
                    TenDangKy = dk?.TenDangKy,
                    TenDonVi = tenDonVi,
                    TenVanDongVien = tenVdv,
                    TenDoi = tenDoi,
                    LoaiHuyChuongId = hc.LoaiHuyChuongId,
                    TenLoaiHuyChuong = lhc?.Ten,
                    XepHang = hc.XepHang,
                    NgayTrao = hc.NgayTrao,
                    GhiChu = hc.GhiChu,
                    Created = hc.Created,
                    LastModified = hc.LastModified
                });
            }

            return result;
        }

        /// <summary>
        /// Lấy thông tin chi tiết một huy chương theo định danh Id
        /// </summary>
        /// <param name="id">Mã huy chương</param>
        /// <returns>Thông tin DTO của huy chương hoặc null nếu không tồn tại</returns>
        public async Task<HuyChuongDto?> GetByIdAsync(int id)
        {
            var hc = await _unitOfWork.HuyChuongs.GetByIdAsync(id);
            if (hc == null || hc.IsDeleted == true) return null;

            var list = await GetAllAsync(hc.GiaiDauId);
            return list.FirstOrDefault(x => x.Id == id);
        }

        /// <summary>
        /// Ghi nhận trao mới một huy chương cho giải đấu
        /// </summary>
        /// <param name="dto">Dữ liệu tạo huy chương</param>
        /// <param name="createdBy">Tài khoản người thực hiện</param>
        /// <returns>Thông tin huy chương đã được tạo</returns>
        public async Task<HuyChuongDto> CreateAsync(CreateUpdateHuyChuongDto dto, string? createdBy = null)
        {
            var entity = new HuyChuong
            {
                GiaiDauId = dto.GiaiDauId,
                GiaiDauMonTheThaoId = dto.GiaiDauMonTheThaoId,
                DangKyThiDauId = dto.DangKyThiDauId,
                LoaiHuyChuongId = dto.LoaiHuyChuongId,
                XepHang = dto.XepHang,
                NgayTrao = dto.NgayTrao ?? DateTime.Now,
                GhiChu = dto.GhiChu,
                CreatedBy = createdBy,
                Created = DateTime.UtcNow,
                IsDeleted = false
            };

            await _unitOfWork.HuyChuongs.AddAsync(entity);
            await _unitOfWork.CompleteAsync();

            return (await GetByIdAsync(entity.Id))!;
        }

        /// <summary>
        /// Cập nhật thông tin huy chương đã trao
        /// </summary>
        /// <param name="id">Mã huy chương cần sửa</param>
        /// <param name="dto">Dữ liệu cập nhật</param>
        /// <param name="updatedBy">Tài khoản người cập nhật</param>
        /// <returns>Thông tin huy chương sau cập nhật hoặc null nếu không tìm thấy</returns>
        public async Task<HuyChuongDto?> UpdateAsync(int id, CreateUpdateHuyChuongDto dto, string? updatedBy = null)
        {
            var entity = await _unitOfWork.HuyChuongs.GetByIdAsync(id);
            if (entity == null || entity.IsDeleted == true) return null;

            entity.GiaiDauId = dto.GiaiDauId;
            entity.GiaiDauMonTheThaoId = dto.GiaiDauMonTheThaoId;
            entity.DangKyThiDauId = dto.DangKyThiDauId;
            entity.LoaiHuyChuongId = dto.LoaiHuyChuongId;
            entity.XepHang = dto.XepHang;
            entity.NgayTrao = dto.NgayTrao;
            entity.GhiChu = dto.GhiChu;
            entity.LastModifiedBy = updatedBy;
            entity.LastModified = DateTime.UtcNow;

            _unitOfWork.HuyChuongs.Update(entity);
            await _unitOfWork.CompleteAsync();

            return await GetByIdAsync(id);
        }

        /// <summary>
        /// Thu hồi hoặc xóa mềm huy chương khỏi hệ thống (IsDeleted = true)
        /// </summary>
        /// <param name="id">Mã định danh huy chương cần thu hồi</param>
        /// <returns>True nếu thu hồi thành công, False nếu không tìm thấy bản ghi</returns>
        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _unitOfWork.HuyChuongs.GetByIdAsync(id);
            if (entity == null || entity.IsDeleted == true) return false;

            entity.IsDeleted = true;
            entity.LastModified = DateTime.UtcNow;

            _unitOfWork.HuyChuongs.Update(entity);
            await _unitOfWork.CompleteAsync();

            return true;
        }

        /// <summary>
        /// Tự động quét và đồng bộ huy chương (Vàng, Bạc, Đồng) từ kết quả các trận đấu và bảng đấu
        /// cho các thể thức Vòng Tròn (Round Robin), Loại Trực Tiếp (Knockout) và Điền Kinh/Bơi Lội.
        /// </summary>
        /// <param name="giaiDauId">Mã giải đấu (tùy chọn)</param>
        /// <param name="monTheThaoId">Mã môn thể thao (tùy chọn)</param>
        /// <param name="username">Người thực hiện</param>
        /// <returns>Bộ ba kết quả: thành công, thông báo và danh sách các huy chương được trao/cập nhật</returns>
        public async Task<(bool success, string message, List<string> awarded)> SyncMedalsFromResultsAsync(
            int? giaiDauId = null,
            int? monTheThaoId = null,
            string? username = null)
        {
            var awarded = new List<string>();

            // 1. Đảm bảo các loại huy chương chuẩn (VANG, BAC, DONG) luôn sẵn sàng trong hệ thống
            var loaiHcs = (await _unitOfWork.LoaiHuyChuongs.FindAsync(l => l.IsDeleted != true)).ToList();
            var lhcVang = loaiHcs.FirstOrDefault(l => (l.Ma ?? "").ToUpper() == "VANG" || l.ThuTu == 1);
            var lhcBac = loaiHcs.FirstOrDefault(l => (l.Ma ?? "").ToUpper() == "BAC" || l.ThuTu == 2);
            var lhcDong = loaiHcs.FirstOrDefault(l => (l.Ma ?? "").ToUpper() == "DONG" || l.ThuTu == 3);

            bool needSaveLoai = false;
            if (lhcVang == null)
            {
                lhcVang = new LoaiHuyChuong { Ma = "VANG", Ten = "Huy chương Vàng", ThuTu = 1, Created = DateTime.UtcNow, IsDeleted = false };
                await _unitOfWork.LoaiHuyChuongs.AddAsync(lhcVang);
                needSaveLoai = true;
            }
            if (lhcBac == null)
            {
                lhcBac = new LoaiHuyChuong { Ma = "BAC", Ten = "Huy chương Bạc", ThuTu = 2, Created = DateTime.UtcNow, IsDeleted = false };
                await _unitOfWork.LoaiHuyChuongs.AddAsync(lhcBac);
                needSaveLoai = true;
            }
            if (lhcDong == null)
            {
                lhcDong = new LoaiHuyChuong { Ma = "DONG", Ten = "Huy chương Đồng", ThuTu = 3, Created = DateTime.UtcNow, IsDeleted = false };
                await _unitOfWork.LoaiHuyChuongs.AddAsync(lhcDong);
                needSaveLoai = true;
            }
            if (needSaveLoai)
            {
                await _unitOfWork.CompleteAsync();
            }

            // 2. Lấy danh sách các liên kết môn trong giải cần đồng bộ
            var gdms = (await _unitOfWork.GiaiDauMonTheThaos.FindAsync(g =>
                g.IsDeleted != true &&
                (!giaiDauId.HasValue || giaiDauId.Value == 0 || g.GiaiDauId == giaiDauId.Value) &&
                (!monTheThaoId.HasValue || monTheThaoId.Value == 0 || g.MonTheThaoId == monTheThaoId.Value)
            )).ToList();

            if (!gdms.Any())
            {
                return (false, "Không tìm thấy môn thi đấu nào phù hợp với bộ lọc.", awarded);
            }

            var gdmIds = gdms.Select(g => g.Id).ToList();
            var monIds = gdms.Select(g => g.MonTheThaoId).Distinct().ToList();
            var mons = (await _unitOfWork.MonTheThaos.FindAsync(m => monIds.Contains(m.Id) && m.IsDeleted != true)).ToDictionary(m => m.Id);

            var allBangDaus = (await _unitOfWork.BangDaus.FindAsync(b => gdmIds.Contains(b.GiaiDauMonTheThaoId) && b.IsDeleted != true)).ToList();
            var allVongDaus = (await _unitOfWork.VongDaus.FindAsync(v => gdmIds.Contains(v.GiaiDauMonTheThaoId) && v.IsDeleted != true)).ToList();
            var allDangKys = (await _unitOfWork.DangKyThiDaus.FindAsync(d => gdmIds.Contains(d.GiaiDauMonTheThaoId) && d.IsDeleted != true)).ToDictionary(d => d.Id);

            var allMatches = (await _unitOfWork.TranDaus.GetPagedAsync(
                1, 10000,
                t => gdmIds.Contains(t.GiaiDauMonTheThaoId) && t.IsDeleted != true,
                null,
                t => t.ThanhPhanTranDaus
            )).Items.ToList();

            var existingMedals = (await _unitOfWork.HuyChuongs.FindAsync(h => gdmIds.Contains(h.GiaiDauMonTheThaoId) && h.IsDeleted != true)).ToList();

            foreach (var gdm in gdms)
            {
                if (!mons.TryGetValue(gdm.MonTheThaoId, out var mon)) continue;
                var hinhThuc = mon.HinhThucThiDau;
                var gdmMatches = allMatches.Where(m => m.GiaiDauMonTheThaoId == gdm.Id).ToList();
                var gdmBangs = allBangDaus.Where(b => b.GiaiDauMonTheThaoId == gdm.Id).OrderBy(b => b.ThuTu).ToList();
                var gdmVongs = allVongDaus.Where(v => v.GiaiDauMonTheThaoId == gdm.Id).ToDictionary(v => v.Id);

                // --- A. THỂ THỨC VÒNG TRÒN (Round Robin / VongBang) ---
                bool hasKnockoutMatches = gdmMatches.Any(m => !m.BangDauId.HasValue);
                bool isPureRoundRobin = hinhThuc == Dms.Domain.Enums.HinhThucThiDau.VongBang || (!hasKnockoutMatches && gdmBangs.Any());

                if (isPureRoundRobin && gdmBangs.Any())
                {
                    var config = await _theThucService.GetEffectiveConfigAsync(gdm.MonTheThaoId, gdm.Id);

                    foreach (var b in gdmBangs)
                    {
                        var groupMatches = gdmMatches.Where(m => m.BangDauId == b.Id).ToList();
                        if (!groupMatches.Any()) continue;

                        int completed = groupMatches.Count(m => m.TrangThai == "KetThuc" || m.TrangThai == "DaDau");
                        if (completed == 0) continue; // Chưa có trận nào kết thúc

                        // Tính lại bảng xếp hạng
                        if (config != null)
                        {
                            await _groupStandingsEngine.RecalculateGroupStandingsAsync(b.Id, config, username);
                        }

                        var members = (await _unitOfWork.ThanhVienBangs.FindAsync(m => m.BangDauId == b.Id && m.IsDeleted != true))
                            .OrderBy(m => m.XepHang ?? 999).ThenByDescending(m => m.Diem).ToList();

                        if (members.Count == 0) continue;

                        var top1 = members.FirstOrDefault(m => m.XepHang == 1) ?? members[0];
                        var top2 = members.FirstOrDefault(m => m.XepHang == 2) ?? (members.Count > 1 ? members[1] : null);
                        var top3 = members.FirstOrDefault(m => m.XepHang == 3) ?? (members.Count > 2 ? members[2] : null);

                        var topDangKyIds = new HashSet<int>();

                        if (top1 != null && lhcVang != null)
                        {
                            topDangKyIds.Add(top1.DangKyThiDauId);
                            string name = allDangKys.TryGetValue(top1.DangKyThiDauId, out var dk1) ? dk1.TenDangKy : $"Đội {top1.DangKyThiDauId}";
                            await UpsertMedalAsync(gdm.GiaiDauId, gdm.Id, top1.DangKyThiDauId, lhcVang.Id, 1, $"🥇 Hạng Nhất (HCV) - {b.Ten} ({mon.Ten})", username, existingMedals);
                            awarded.Add($"🥇 HCV: '{name}' - Hạng 1 {b.Ten} ({mon.Ten})");
                        }

                        if (top2 != null && lhcBac != null)
                        {
                            topDangKyIds.Add(top2.DangKyThiDauId);
                            string name = allDangKys.TryGetValue(top2.DangKyThiDauId, out var dk2) ? dk2.TenDangKy : $"Đội {top2.DangKyThiDauId}";
                            await UpsertMedalAsync(gdm.GiaiDauId, gdm.Id, top2.DangKyThiDauId, lhcBac.Id, 2, $"🥈 Hạng Nhì (HCB) - {b.Ten} ({mon.Ten})", username, existingMedals);
                            awarded.Add($"🥈 HCB: '{name}' - Hạng 2 {b.Ten} ({mon.Ten})");
                        }

                        if (top3 != null && lhcDong != null)
                        {
                            topDangKyIds.Add(top3.DangKyThiDauId);
                            string name = allDangKys.TryGetValue(top3.DangKyThiDauId, out var dk3) ? dk3.TenDangKy : $"Đội {top3.DangKyThiDauId}";
                            await UpsertMedalAsync(gdm.GiaiDauId, gdm.Id, top3.DangKyThiDauId, lhcDong.Id, 3, $"🥉 Hạng Ba (HCĐ) - {b.Ten} ({mon.Ten})", username, existingMedals);
                            awarded.Add($"🥉 HCĐ: '{name}' - Hạng 3 {b.Ten} ({mon.Ten})");
                        }

                        // Thu hồi huy chương của những đội không còn trong Top 3 của bảng này
                        var obsoleteMedals = existingMedals.Where(h =>
                            h.GiaiDauMonTheThaoId == gdm.Id &&
                            !topDangKyIds.Contains(h.DangKyThiDauId) &&
                            members.Any(m => m.DangKyThiDauId == h.DangKyThiDauId)
                        ).ToList();

                        foreach (var obs in obsoleteMedals)
                        {
                            obs.IsDeleted = true;
                            obs.LastModified = DateTime.UtcNow;
                            obs.LastModifiedBy = username;
                            _unitOfWork.HuyChuongs.Update(obs);
                        }
                    }
                }

                // --- B. THỂ THỨC KNOCKOUT & HYBRID (Loại trực tiếp & Vòng bảng + Knockout) ---
                bool isLeaderboardSport = hinhThuc == Dms.Domain.Enums.HinhThucThiDau.TinhDiemXepHang
                    || hinhThuc == Dms.Domain.Enums.HinhThucThiDau.DuaThoiGian
                    || hinhThuc == Dms.Domain.Enums.HinhThucThiDau.DoLuotThi
                    || hinhThuc == Dms.Domain.Enums.HinhThucThiDau.BieuDienChamDiem;

                if (hasKnockoutMatches && !isLeaderboardSport)
                {
                    // 1. Trận Chung kết
                    var finalMatch = gdmMatches.FirstOrDefault(m =>
                    {
                        if (m.BangDauId.HasValue) return false;
                        if (m.MaTranBracket == "FINAL") return true;
                        if (m.VongDauId > 0 && gdmVongs.TryGetValue(m.VongDauId, out var v) && v != null)
                        {
                            string vt = (v.Ten ?? "").ToLower();
                            string lv = (v.LoaiVong ?? "").ToLower();
                            return vt.Contains("chung kết") || lv.Contains("chungket");
                        }
                        return false;
                    });

                    if (finalMatch != null && (finalMatch.TrangThai == "KetThuc" || finalMatch.TrangThai == "DaDau"))
                    {
                        var (wId, lId) = GetWinnerLoser(finalMatch);
                        if (wId > 0 && lhcVang != null)
                        {
                            string wName = allDangKys.TryGetValue(wId, out var dkW) ? dkW.TenDangKy : $"Đội {wId}";
                            await UpsertMedalAsync(gdm.GiaiDauId, gdm.Id, wId, lhcVang.Id, 1, $"🥇 Vô địch (HCV) - Chung kết ({mon.Ten})", username, existingMedals);
                            awarded.Add($"🥇 HCV: '{wName}' - Vô địch ({mon.Ten})");
                        }
                        if (lId > 0 && lhcBac != null)
                        {
                            string lName = allDangKys.TryGetValue(lId, out var dkL) ? dkL.TenDangKy : $"Đội {lId}";
                            await UpsertMedalAsync(gdm.GiaiDauId, gdm.Id, lId, lhcBac.Id, 2, $"🥈 Á quân (HCB) - Chung kết ({mon.Ten})", username, existingMedals);
                            awarded.Add($"🥈 HCB: '{lName}' - Á quân ({mon.Ten})");
                        }
                    }

                    // 2. Trận Tranh Hạng 3 (Bronze match)
                    var bronzeMatch = gdmMatches.FirstOrDefault(m =>
                    {
                        if (m.BangDauId.HasValue) return false;
                        if (m.MaTranBracket == "BRONZE") return true;
                        if (m.VongDauId > 0 && gdmVongs.TryGetValue(m.VongDauId, out var v) && v != null)
                        {
                            string vt = (v.Ten ?? "").ToLower();
                            return vt.Contains("tranh hạng 3") || vt.Contains("hạng ba");
                        }
                        return false;
                    });

                    if (bronzeMatch != null && (bronzeMatch.TrangThai == "KetThuc" || bronzeMatch.TrangThai == "DaDau"))
                    {
                        var (wId, _) = GetWinnerLoser(bronzeMatch);
                        if (wId > 0 && lhcDong != null)
                        {
                            string wName = allDangKys.TryGetValue(wId, out var dkW) ? dkW.TenDangKy : $"Đội {wId}";
                            await UpsertMedalAsync(gdm.GiaiDauId, gdm.Id, wId, lhcDong.Id, 3, $"🥉 Hạng Ba (HCĐ) - Tranh hạng 3 ({mon.Ten})", username, existingMedals);
                            awarded.Add($"🥉 HCĐ: '{wName}' - Hạng 3 ({mon.Ten})");
                        }
                    }
                    else
                    {
                        // Nếu KHÔNG CÓ trận tranh hạng 3, trao HCĐ đồng hạng cho 2 đội thua Bán kết (Semi-Final)
                        var semiMatches = gdmMatches.Where(m =>
                        {
                            if (m.BangDauId.HasValue) return false;
                            if (m.MaTranBracket == "SEMI_1" || m.MaTranBracket == "SEMI_2") return true;
                            if (m.VongDauId > 0 && gdmVongs.TryGetValue(m.VongDauId, out var v) && v != null)
                            {
                                string vt = (v.Ten ?? "").ToLower();
                                string lv = (v.LoaiVong ?? "").ToLower();
                                return vt.Contains("bán kết") || lv.Contains("banket");
                            }
                            return false;
                        }).ToList();

                        if (semiMatches.Any() && semiMatches.All(m => m.TrangThai == "KetThuc" || m.TrangThai == "DaDau") && lhcDong != null)
                        {
                            foreach (var sm in semiMatches)
                            {
                                var (_, loserId) = GetWinnerLoser(sm);
                                if (loserId > 0)
                                {
                                    string lName = allDangKys.TryGetValue(loserId, out var dkL) ? dkL.TenDangKy : $"Đội {loserId}";
                                    await UpsertMedalAsync(gdm.GiaiDauId, gdm.Id, loserId, lhcDong.Id, 3, $"🥉 Đồng Hạng Ba (HCĐ) - Thua Bán kết ({mon.Ten})", username, existingMedals);
                                    awarded.Add($"🥉 HCĐ: '{lName}' - Đồng hạng 3 ({mon.Ten})");
                                }
                            }
                        }
                    }
                }

                // --- C. THỂ THỨC ĐO THÀNH TÍCH / TÍNH GIỜ / LẦN THỬ / BIỂU DIỄN ---
                if (isLeaderboardSport)
                {
                    var finalMatches = gdmMatches.Where(m =>
                    {
                        if (m.MaTranBracket == "FINAL") return true;
                        if (m.VongDauId > 0 && gdmVongs.TryGetValue(m.VongDauId, out var v) && v != null)
                        {
                            string vt = (v.Ten ?? "").ToLower();
                            string lv = (v.LoaiVong ?? "").ToLower();
                            return vt.Contains("chung kết") || lv.Contains("chungket");
                        }
                        return false;
                    }).ToList();

                    foreach (var fm in finalMatches)
                    {
                        if (fm.TrangThai != "KetThuc" && fm.TrangThai != "DaDau") continue;

                        var tps = (await _unitOfWork.ThanhPhanTranDaus.FindAsync(tp => tp.TranDauId == fm.Id && tp.IsDeleted != true)).ToList();
                        var tpIds = tps.Select(t => t.Id).ToList();
                        var kqs = (await _unitOfWork.KetQuaTranDaus.FindAsync(k => tpIds.Contains(k.ThanhPhanTranDauId) && k.IsDeleted != true)).ToList();

                        var rankedList = (from tp in tps
                                          join kq in kqs on tp.Id equals kq.ThanhPhanTranDauId
                                          where kq.XepHang.HasValue && kq.XepHang.Value > 0
                                          orderby kq.XepHang.Value
                                          select new { tp.DangKyThiDauId, XepHang = kq.XepHang.Value, kq.KetQuaText }).ToList();

                        foreach (var r in rankedList.Where(x => x.XepHang == 1))
                        {
                            if (lhcVang != null)
                            {
                                string name = allDangKys.TryGetValue(r.DangKyThiDauId, out var dk) ? dk.TenDangKy : $"VĐV {r.DangKyThiDauId}";
                                await UpsertMedalAsync(gdm.GiaiDauId, gdm.Id, r.DangKyThiDauId, lhcVang.Id, 1, $"🥇 Vô địch (HCV) - Chung kết {mon.Ten} ({r.KetQuaText})", username, existingMedals);
                                awarded.Add($"🥇 HCV: '{name}' - Hạng 1 Chung kết {mon.Ten} ({r.KetQuaText})");
                            }
                        }
                        foreach (var r in rankedList.Where(x => x.XepHang == 2))
                        {
                            if (lhcBac != null)
                            {
                                string name = allDangKys.TryGetValue(r.DangKyThiDauId, out var dk) ? dk.TenDangKy : $"VĐV {r.DangKyThiDauId}";
                                await UpsertMedalAsync(gdm.GiaiDauId, gdm.Id, r.DangKyThiDauId, lhcBac.Id, 2, $"🥈 Hạng Nhì (HCB) - Chung kết {mon.Ten} ({r.KetQuaText})", username, existingMedals);
                                awarded.Add($"🥈 HCB: '{name}' - Hạng 2 Chung kết {mon.Ten} ({r.KetQuaText})");
                            }
                        }
                        foreach (var r in rankedList.Where(x => x.XepHang == 3))
                        {
                            if (lhcDong != null)
                            {
                                string name = allDangKys.TryGetValue(r.DangKyThiDauId, out var dk) ? dk.TenDangKy : $"VĐV {r.DangKyThiDauId}";
                                await UpsertMedalAsync(gdm.GiaiDauId, gdm.Id, r.DangKyThiDauId, lhcDong.Id, 3, $"🥉 Hạng Ba (HCĐ) - Chung kết {mon.Ten} ({r.KetQuaText})", username, existingMedals);
                                awarded.Add($"🥉 HCĐ: '{name}' - Hạng 3 Chung kết {mon.Ten} ({r.KetQuaText})");
                            }
                        }
                    }
                }
            }

            await _unitOfWork.CompleteAsync();

            string summaryMsg = awarded.Count > 0
                ? $"Đã đồng bộ và trao thành công {awarded.Count} huy chương từ kết quả thi đấu!"
                : "Không có trận đấu hoặc bảng đấu hoàn thành mới nào cần đồng bộ thêm.";

            return (true, summaryMsg, awarded);
        }

        private async Task UpsertMedalAsync(
            int giaiDauId,
            int giaiDauMonTheThaoId,
            int dangKyId,
            int loaiHuyChuongId,
            int xepHang,
            string ghiChu,
            string? username,
            List<HuyChuong> existingMedals)
        {
            var h = existingMedals.FirstOrDefault(m =>
                m.GiaiDauMonTheThaoId == giaiDauMonTheThaoId &&
                m.DangKyThiDauId == dangKyId &&
                m.IsDeleted != true
            );

            if (h != null)
            {
                h.LoaiHuyChuongId = loaiHuyChuongId;
                h.XepHang = xepHang;
                h.GhiChu = ghiChu;
                h.NgayTrao = DateTime.UtcNow;
                h.LastModified = DateTime.UtcNow;
                h.LastModifiedBy = username;
                _unitOfWork.HuyChuongs.Update(h);
            }
            else
            {
                var newMedal = new HuyChuong
                {
                    GiaiDauId = giaiDauId,
                    GiaiDauMonTheThaoId = giaiDauMonTheThaoId,
                    DangKyThiDauId = dangKyId,
                    LoaiHuyChuongId = loaiHuyChuongId,
                    XepHang = xepHang,
                    NgayTrao = DateTime.UtcNow,
                    GhiChu = ghiChu,
                    Created = DateTime.UtcNow,
                    CreatedBy = username,
                    IsDeleted = false
                };
                await _unitOfWork.HuyChuongs.AddAsync(newMedal);
                existingMedals.Add(newMedal);
            }
        }

        private (int winnerId, int loserId) GetWinnerLoser(TranDau match)
        {
            int w = match.DoiThangDangKyId ?? 0;
            int l = match.DoiThuaDangKyId ?? 0;

            if (w == 0 || l == 0)
            {
                var tps = match.ThanhPhanTranDaus.Where(tp => tp.IsDeleted != true).OrderBy(tp => tp.ViTri ?? 1).ToList();
                if (tps.Count >= 2)
                {
                    int t1 = tps[0].DangKyThiDauId;
                    int t2 = tps[1].DangKyThiDauId;
                    int s1 = match.TySoDoi1 ?? 0;
                    int s2 = match.TySoDoi2 ?? 0;
                    int p1 = match.DiemPenaltyDoi1 ?? 0;
                    int p2 = match.DiemPenaltyDoi2 ?? 0;

                    if (s1 > s2 || (s1 == s2 && p1 > p2))
                    {
                        w = t1;
                        l = t2;
                    }
                    else if (s2 > s1 || (s1 == s2 && p2 > p1))
                    {
                        w = t2;
                        l = t1;
                    }
                }
            }

            return (w, l);
        }
    }
}
