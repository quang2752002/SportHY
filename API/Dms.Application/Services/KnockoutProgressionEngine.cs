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
    /// Triển khai động cơ quản lý tiến trình nhánh đấu Knockout: tự động đưa đội thắng vào vòng tiếp theo,
    /// đưa đội thua bán kết vào trận tranh hạng 3, và tự động trao huy chương khi giải kết thúc.
    /// </summary>
    public class KnockoutProgressionEngine : IKnockoutProgressionEngine
    {
        private readonly IUnitOfWork _unitOfWork;

        public KnockoutProgressionEngine(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Xử lý tiến trình sau khi một trận đấu loại trực tiếp (Knockout) kết thúc:
        /// 1. Tự động đưa đội thắng vào vị trí tương ứng của trận đấu kế tiếp.
        /// 2. Tự động đưa đội thua bán kết vào trận tranh hạng 3 (nếu có).
        /// 3. Cập nhật trạng thái trận kế tiếp thành "ChuaDau" (Sẵn sàng) nếu đã đủ 2 đội.
        /// 4. Tự động tạo bản ghi trao Huy chương Vàng, Bạc, Đồng nếu là trận Chung kết hoặc Tranh hạng 3.
        /// </summary>
        /// <param name="tranDauId">Mã định danh trận đấu vừa kết thúc</param>
        /// <param name="winnerDangKyId">Mã đăng ký thi đấu của đội/VĐV chiến thắng</param>
        /// <param name="loserDangKyId">Mã đăng ký thi đấu của đội/VĐV thua cuộc</param>
        /// <param name="username">Tên người thực hiện cập nhật</param>
        /// <returns>Đối tượng KnockoutProgressionResult chứa chi tiết tiến trình đã cập nhật</returns>
        public async Task<KnockoutProgressionResult> ProcessKnockoutProgressionAsync(
            int tranDauId,
            int winnerDangKyId,
            int loserDangKyId,
            string? username = null)
        {
            var result = new KnockoutProgressionResult();

            var currentMatch = await _unitOfWork.TranDaus.GetByIdAsync(tranDauId);
            if (currentMatch == null)
            {
                result.Success = false;
                return result;
            }

            // Ghi nhận đội thắng / đội thua vào trận hiện tại
            currentMatch.DoiThangDangKyId = winnerDangKyId;
            currentMatch.DoiThuaDangKyId = loserDangKyId;
            currentMatch.LastModified = DateTime.UtcNow;
            currentMatch.LastModifiedBy = username;
            _unitOfWork.TranDaus.Update(currentMatch);

            var winnerDk = await _unitOfWork.DangKyThiDaus.GetByIdAsync(winnerDangKyId);
            string winnerName = winnerDk?.TenDangKy ?? $"Đội #{winnerDangKyId}";

            var loserDk = await _unitOfWork.DangKyThiDaus.GetByIdAsync(loserDangKyId);
            string loserName = loserDk?.TenDangKy ?? $"Đội #{loserDangKyId}";

            // 1. TIẾN TRÌNH ĐỘI THẮNG (WINNER PROGRESSION)
            if (currentMatch.NextTranDauId.HasValue && currentMatch.NextTranDauId.Value > 0)
            {
                int nextMatchId = currentMatch.NextTranDauId.Value;
                int targetSlot = currentMatch.NextTranDauViTri ?? 1;

                var nextMatch = await _unitOfWork.TranDaus.GetByIdAsync(nextMatchId);
                if (nextMatch != null)
                {
                    // Lấy hoặc tạo mới ThanhPhanTranDau tại slot này
                    var existingTps = (await _unitOfWork.ThanhPhanTranDaus.FindAsync(
                        tp => tp.TranDauId == nextMatchId && tp.ViTri == targetSlot && tp.IsDeleted != true
                    )).ToList();

                    if (existingTps.Any())
                    {
                        var tp = existingTps.First();
                        tp.DangKyThiDauId = winnerDangKyId;
                        tp.TrangThai = "ThamGia";
                        tp.LastModified = DateTime.UtcNow;
                        tp.LastModifiedBy = username;
                        _unitOfWork.ThanhPhanTranDaus.Update(tp);
                    }
                    else
                    {
                        await _unitOfWork.ThanhPhanTranDaus.AddAsync(new ThanhPhanTranDau
                        {
                            TranDauId = nextMatchId,
                            DangKyThiDauId = winnerDangKyId,
                            ViTri = targetSlot,
                            TrangThai = "ThamGia",
                            Created = DateTime.UtcNow,
                            CreatedBy = username,
                            IsDeleted = false
                        });
                    }

                    // Kiểm tra xem trận kế tiếp đã đủ cả 2 đội chưa
                    var allNextTps = (await _unitOfWork.ThanhPhanTranDaus.FindAsync(
                        tp => tp.TranDauId == nextMatchId && tp.IsDeleted != true
                    )).ToList();

                    bool hasSlot1 = allNextTps.Any(tp => tp.ViTri == 1) || targetSlot == 1;
                    bool hasSlot2 = allNextTps.Any(tp => tp.ViTri == 2) || targetSlot == 2;

                    if (hasSlot1 && hasSlot2 && nextMatch.TrangThai == "ChuaDau")
                    {
                        nextMatch.TrangThai = "ChuaDau"; // Sẵn sàng thi đấu
                        _unitOfWork.TranDaus.Update(nextMatch);
                    }

                    result.NextMatchNotice = $"Đã tự động đưa '{winnerName}' vào Vị trí {targetSlot} của trận #{nextMatch.SoTran} ({nextMatch.TenTran ?? "Vòng tiếp theo"}).";
                }
            }

            // 2. TIẾN TRÌNH ĐỘI THUA (LOSER PROGRESSION - TRANH HẠNG 3)
            if (currentMatch.LoserNextTranDauId.HasValue && currentMatch.LoserNextTranDauId.Value > 0)
            {
                int bronzeMatchId = currentMatch.LoserNextTranDauId.Value;
                int loserSlot = currentMatch.LoserNextTranDauViTri ?? 1;

                var bronzeMatch = await _unitOfWork.TranDaus.GetByIdAsync(bronzeMatchId);
                if (bronzeMatch != null)
                {
                    var existingBronzeTps = (await _unitOfWork.ThanhPhanTranDaus.FindAsync(
                        tp => tp.TranDauId == bronzeMatchId && tp.ViTri == loserSlot && tp.IsDeleted != true
                    )).ToList();

                    if (existingBronzeTps.Any())
                    {
                        var tp = existingBronzeTps.First();
                        tp.DangKyThiDauId = loserDangKyId;
                        tp.TrangThai = "ThamGia";
                        tp.LastModified = DateTime.UtcNow;
                        tp.LastModifiedBy = username;
                        _unitOfWork.ThanhPhanTranDaus.Update(tp);
                    }
                    else
                    {
                        await _unitOfWork.ThanhPhanTranDaus.AddAsync(new ThanhPhanTranDau
                        {
                            TranDauId = bronzeMatchId,
                            DangKyThiDauId = loserDangKyId,
                            ViTri = loserSlot,
                            TrangThai = "ThamGia",
                            Created = DateTime.UtcNow,
                            CreatedBy = username,
                            IsDeleted = false
                        });
                    }

                    result.BronzeMatchNotice = $"Đã tự động đưa '{loserName}' vào Vị trí {loserSlot} của trận Tranh hạng 3 #{bronzeMatch.SoTran}.";
                }
            }

            // 3. TỰ ĐỘNG TRAO HUY CHƯƠNG KHI LÀ TRẬN CHUNG KẾT HOẶC TRANH HẠNG 3
            var gdm = await _unitOfWork.GiaiDauMonTheThaos.GetByIdAsync(currentMatch.GiaiDauMonTheThaoId);
            var vong = await _unitOfWork.VongDaus.GetByIdAsync(currentMatch.VongDauId);

            string vongTen = (vong?.Ten ?? "").ToLower();
            string loaiVong = (vong?.LoaiVong ?? "").ToLower();
            string maBracket = currentMatch.MaTranBracket ?? "";

            bool isFinal = maBracket == "FINAL" || vongTen.Contains("chung kết") || loaiVong.Contains("chungket");
            bool isBronze = maBracket == "BRONZE" || vongTen.Contains("tranh hạng 3") || vongTen.Contains("hạng ba");

            if (gdm != null && (isFinal || isBronze))
            {
                var loaiHcs = (await _unitOfWork.LoaiHuyChuongs.FindAsync(l => l.IsDeleted != true))
                    .GroupBy(l => l.Ma.ToUpper())
                    .ToDictionary(g => g.Key, g => g.First());
                loaiHcs.TryGetValue("VANG", out var lhcVang);
                loaiHcs.TryGetValue("BAC", out var lhcBac);
                loaiHcs.TryGetValue("DONG", out var lhcDong);

                if (isFinal)
                {
                    // 1. Trao Huy chương Vàng cho Đội Thắng
                    if (lhcVang != null)
                    {
                        await AwardMedalIfNotExistsAsync(gdm.GiaiDauId, gdm.Id, winnerDangKyId, lhcVang.Id, 1, "Vô địch - Thắng trận Chung kết", username);
                        result.MedalsAwarded.Add($"🥇 Huy chương Vàng trao cho '{winnerName}' (Hạng 1)");
                    }

                    // 2. Trao Huy chương Bạc cho Đội Thua
                    if (lhcBac != null)
                    {
                        await AwardMedalIfNotExistsAsync(gdm.GiaiDauId, gdm.Id, loserDangKyId, lhcBac.Id, 2, "Á quân - Thua trận Chung kết", username);
                        result.MedalsAwarded.Add($"🥈 Huy chương Bạc trao cho '{loserName}' (Hạng 2)");
                    }
                }
                else if (isBronze)
                {
                    // 3. Trao Huy chương Đồng cho Đội Thắng trận Tranh Hạng 3
                    if (lhcDong != null)
                    {
                        await AwardMedalIfNotExistsAsync(gdm.GiaiDauId, gdm.Id, winnerDangKyId, lhcDong.Id, 3, "Hạng Ba - Thắng trận Tranh hạng 3", username);
                        result.MedalsAwarded.Add($"🥉 Huy chương Đồng trao cho '{winnerName}' (Hạng 3)");
                    }
                }
            }

            await _unitOfWork.CompleteAsync();
            return result;
        }

        private async Task AwardMedalIfNotExistsAsync(
            int giaiDauId,
            int giaiDauMonTheThaoId,
            int dangKyId,
            int loaiHuyChuongId,
            int xepHang,
            string ghiChu,
            string? username)
        {
            var existing = await _unitOfWork.HuyChuongs.FindAsync(h =>
                h.GiaiDauMonTheThaoId == giaiDauMonTheThaoId &&
                h.DangKyThiDauId == dangKyId &&
                h.IsDeleted != true
            );

            if (existing.Any())
            {
                var h = existing.First();
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
                await _unitOfWork.HuyChuongs.AddAsync(new HuyChuong
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
                });
            }
        }
    }
}
