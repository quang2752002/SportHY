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
    /// Triển khai động cơ quản lý lượt thi và bảng xếp hạng thành tích cho môn Điền kinh và Bơi lội:
    /// lưu kết quả từng làn, xử lý DNS/DNF/DQ, tự động gom Top thành tích vào Lượt Chung kết,
    /// xếp làn ưu tiên hạt giống, và tự động trao huy chương.
    /// </summary>
    public class AthleticsProgressionEngine : IAthleticsProgressionEngine
    {
        private readonly IUnitOfWork _unitOfWork;

        public AthleticsProgressionEngine(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Ghi nhận kết quả lượt thi (Heat) bơi lội / điền kinh, cập nhật thứ hạng theo làn,
        /// kiểm tra phá kỷ lục giải, và tự động đưa Top VĐV vào Lượt Chung kết hoặc trao huy chương.
        /// </summary>
        /// <param name="tranDauId">Mã định danh lượt thi vừa hoàn thành</param>
        /// <param name="heatResults">Danh sách thành tích của các VĐV theo làn</param>
        /// <param name="config">Cấu hình thể thức đo thành tích của môn</param>
        /// <param name="username">Tên người thực hiện thao tác</param>
        /// <returns>Đối tượng AthleticsProcessResult chứa thông tin chi tiết tiến trình</returns>
        public async Task<AthleticsProcessResult> ProcessHeatResultAsync(
            int tranDauId,
            List<HeatParticipantResultDto> heatResults,
            CauHinhTheThucDto config,
            string? username = null)
        {
            var result = new AthleticsProcessResult();

            var currentMatch = await _unitOfWork.TranDaus.GetByIdAsync(tranDauId);
            if (currentMatch == null)
            {
                result.Success = false;
                result.Message = "Không tìm thấy lượt thi đấu.";
                return result;
            }

            bool isAscending = (config.TieuChiXepHangThanhTich ?? "CangNhoCangTot") == "CangNhoCangTot";

            // Sắp xếp thứ hạng trong lượt thi:
            // Các VĐV hoàn thành (ThamGia và có GiaTri) sắp xếp theo GiaTri (ít giây hơn hoặc nhiều mét hơn)
            // Các VĐV DNS, DNF, DQ xếp cuối
            var validParticipants = heatResults.Where(r => r.TrangThai == "ThamGia" && r.GiaTri.HasValue).ToList();
            var invalidParticipants = heatResults.Where(r => r.TrangThai != "ThamGia" || !r.GiaTri.HasValue).ToList();

            if (isAscending)
            {
                validParticipants = validParticipants.OrderBy(r => r.GiaTri!.Value).ToList();
            }
            else
            {
                validParticipants = validParticipants.OrderByDescending(r => r.GiaTri!.Value).ToList();
            }

            // Gán rank 1, 2, 3... cho các VĐV hợp lệ
            int rank = 1;
            foreach (var p in validParticipants)
            {
                p.XepHang = rank++;
            }
            foreach (var p in invalidParticipants)
            {
                p.XepHang = null;
            }

            // Cập nhật vào cơ sở dữ liệu (ThanhPhanTranDau & KetQuaTranDau)
            foreach (var item in heatResults)
            {
                var tp = await _unitOfWork.ThanhPhanTranDaus.GetByIdAsync(item.ThanhPhanTranDauId);
                if (tp != null)
                {
                    tp.TrangThai = item.TrangThai;
                    tp.SoLane = item.SoLane;
                    tp.LastModified = DateTime.UtcNow;
                    tp.LastModifiedBy = username;
                    _unitOfWork.ThanhPhanTranDaus.Update(tp);

                    // Lưu KetQuaTranDau
                    var existingKq = (await _unitOfWork.KetQuaTranDaus.FindAsync(
                        k => k.ThanhPhanTranDauId == tp.Id && k.IsDeleted != true
                    )).FirstOrDefault();

                    bool isRecord = false;
                    if (item.GiaTri.HasValue && config.KyLucHienTai.HasValue)
                    {
                        if (isAscending && item.GiaTri.Value < config.KyLucHienTai.Value) isRecord = true;
                        else if (!isAscending && item.GiaTri.Value > config.KyLucHienTai.Value) isRecord = true;
                    }

                    if (isRecord)
                    {
                        result.IsRecordBroken = true;
                        var dk = await _unitOfWork.DangKyThiDaus.GetByIdAsync(item.DangKyThiDauId);
                        result.RecordBreakerNotice = $"🌟 PHÁ KỶ LỤC GIẢI: VĐV {dk?.TenDangKy ?? ""} đạt {item.KetQuaText ?? item.GiaTri.ToString()} (Kỷ lục cũ: {config.KyLucHienTaiText ?? config.KyLucHienTai.ToString()})!";
                    }

                    if (existingKq == null)
                    {
                        await _unitOfWork.KetQuaTranDaus.AddAsync(new KetQuaTranDau
                        {
                            ThanhPhanTranDauId = tp.Id,
                            LoaiKetQua = config.LoaiDoThanhTich ?? "ThoiGian",
                            GiaTri = item.GiaTri,
                            DonVi = config.DonViThanhTich ?? "giay",
                            XepHang = item.XepHang,
                            KyLuc = isRecord,
                            KetQuaText = item.KetQuaText ?? (item.GiaTri.HasValue ? $"{item.GiaTri.Value:0.00}{config.DonViThanhTich}" : item.TrangThai),
                            Created = DateTime.UtcNow,
                            CreatedBy = username,
                            IsDeleted = false
                        });
                    }
                    else
                    {
                        existingKq.LoaiKetQua = config.LoaiDoThanhTich ?? "ThoiGian";
                        existingKq.GiaTri = item.GiaTri;
                        existingKq.DonVi = config.DonViThanhTich ?? "giay";
                        existingKq.XepHang = item.XepHang;
                        existingKq.KyLuc = isRecord;
                        existingKq.KetQuaText = item.KetQuaText ?? (item.GiaTri.HasValue ? $"{item.GiaTri.Value:0.00}{config.DonViThanhTich}" : item.TrangThai);
                        existingKq.LastModified = DateTime.UtcNow;
                        existingKq.LastModifiedBy = username;
                        _unitOfWork.KetQuaTranDaus.Update(existingKq);
                    }
                }
            }

            if (currentMatch.TrangThai == "ChuaDau" || !currentMatch.ThoiGianBatDau.HasValue)
            {
                currentMatch.ThoiGianBatDau = DateTime.UtcNow;
            }
            currentMatch.TrangThai = "KetThuc";
            currentMatch.ThoiGianKetThuc = DateTime.UtcNow;
            currentMatch.LastModified = DateTime.UtcNow;
            currentMatch.LastModifiedBy = username;
            _unitOfWork.TranDaus.Update(currentMatch);
            await _unitOfWork.CompleteAsync();

            // Kiểm tra xem lượt này là Vòng loại hay Chung kết
            var vong = await _unitOfWork.VongDaus.GetByIdAsync(currentMatch.VongDauId);
            string vongTen = (vong?.Ten ?? "").ToLower();
            bool isFinal = vongTen.Contains("chung kết") || currentMatch.MaTranBracket == "FINAL";

            var gdm = await _unitOfWork.GiaiDauMonTheThaos.GetByIdAsync(currentMatch.GiaiDauMonTheThaoId);

            if (isFinal)
            {
                // Trao huy chương trực tiếp cho Top 1, 2, 3 của Lượt Chung Kết
                if (validParticipants.Count > 0 && gdm != null)
                {
                    var loaiHcs = (await _unitOfWork.LoaiHuyChuongs.FindAsync(l => l.IsDeleted != true))
                        .GroupBy(l => l.Ma.ToUpper())
                        .ToDictionary(g => g.Key, g => g.First());
                    loaiHcs.TryGetValue("VANG", out var lhcVang);
                    loaiHcs.TryGetValue("BAC", out var lhcBac);
                    loaiHcs.TryGetValue("DONG", out var lhcDong);

                    // Top 1 -> Vàng
                    if (lhcVang != null && validParticipants.Count >= 1)
                    {
                        var dk1 = await _unitOfWork.DangKyThiDaus.GetByIdAsync(validParticipants[0].DangKyThiDauId);
                        await AwardMedalAsync(gdm.GiaiDauId, gdm.Id, validParticipants[0].DangKyThiDauId, lhcVang.Id, 1, "Vô địch Lượt Chung kết", username);
                        result.MedalsAwarded.Add($"🥇 Huy chương Vàng: {dk1?.TenDangKy ?? "VĐV Hạng 1"} ({validParticipants[0].KetQuaText})");
                    }

                    // Top 2 -> Bạc
                    if (lhcBac != null && validParticipants.Count >= 2)
                    {
                        var dk2 = await _unitOfWork.DangKyThiDaus.GetByIdAsync(validParticipants[1].DangKyThiDauId);
                        await AwardMedalAsync(gdm.GiaiDauId, gdm.Id, validParticipants[1].DangKyThiDauId, lhcBac.Id, 2, "Hạng Nhì Lượt Chung kết", username);
                        result.MedalsAwarded.Add($"🥈 Huy chương Bạc: {dk2?.TenDangKy ?? "VĐV Hạng 2"} ({validParticipants[1].KetQuaText})");
                    }

                    // Top 3 -> Đồng
                    if (lhcDong != null && validParticipants.Count >= 3)
                    {
                        var dk3 = await _unitOfWork.DangKyThiDaus.GetByIdAsync(validParticipants[2].DangKyThiDauId);
                        await AwardMedalAsync(gdm.GiaiDauId, gdm.Id, validParticipants[2].DangKyThiDauId, lhcDong.Id, 3, "Hạng Ba Lượt Chung kết", username);
                        result.MedalsAwarded.Add($"🥉 Huy chương Đồng: {dk3?.TenDangKy ?? "VĐV Hạng 3"} ({validParticipants[2].KetQuaText})");
                    }

                    await _unitOfWork.CompleteAsync();
                }
            }
            else
            {
                // Vòng loại (Heats): Kiểm tra nếu tất cả các lượt vòng loại đã xong và có Lượt Chung kết
                var allHeatsInRound = (await _unitOfWork.TranDaus.FindAsync(
                    t => t.VongDauId == currentMatch.VongDauId && t.IsDeleted != true
                )).ToList();

                bool allFinished = allHeatsInRound.All(h => h.TrangThai == "KetThuc" || h.Id == currentMatch.Id);
                if (allFinished && currentMatch.NextTranDauId.HasValue && currentMatch.NextTranDauId.Value > 0)
                {
                    int finalMatchId = currentMatch.NextTranDauId.Value;
                    var finalMatch = await _unitOfWork.TranDaus.GetByIdAsync(finalMatchId);

                    if (finalMatch != null)
                    {
                        // Quét tất cả thành tích của toàn bộ các lượt vòng loại
                        var heatMatchIds = allHeatsInRound.Select(h => h.Id).ToList();
                        var allRoundTps = (await _unitOfWork.ThanhPhanTranDaus.FindAsync(
                            tp => heatMatchIds.Contains(tp.TranDauId) && tp.IsDeleted != true
                        )).ToList();

                        var allTpIds = allRoundTps.Select(t => t.Id).ToList();
                        var allKqs = (await _unitOfWork.KetQuaTranDaus.FindAsync(
                            k => allTpIds.Contains(k.ThanhPhanTranDauId) && k.IsDeleted != true && k.GiaTri.HasValue
                        )).ToList();

                        var combined = (from tp in allRoundTps
                                        join kq in allKqs on tp.Id equals kq.ThanhPhanTranDauId
                                        where tp.TrangThai == "ThamGia"
                                        select new { tp.DangKyThiDauId, kq.GiaTri, kq.KetQuaText }).ToList();

                        var sortedAll = isAscending
                            ? combined.OrderBy(x => x.GiaTri!.Value).ToList()
                            : combined.OrderByDescending(x => x.GiaTri!.Value).ToList();

                        // Lấy Top N vào Chung kết (ví dụ Top 8)
                        int finalSize = config.SoVdvVaoChungKet ?? 8;
                        var topAdvancing = sortedAll.Take(finalSize).ToList();

                        // Phân bổ làn bơi / làn chạy hạt giống chuẩn quốc tế
                        // Thứ tự ưu tiên làn: 4, 5, 3, 6, 2, 7, 1, 8
                        int[] lanePriority = { 4, 5, 3, 6, 2, 7, 1, 8 };

                        // Xóa thành phần cũ của lượt chung kết nếu có
                        var oldFinalTps = (await _unitOfWork.ThanhPhanTranDaus.FindAsync(
                            tp => tp.TranDauId == finalMatchId
                        )).ToList();
                        foreach (var oft in oldFinalTps) _unitOfWork.ThanhPhanTranDaus.Delete(oft);

                        for (int i = 0; i < topAdvancing.Count; i++)
                        {
                            int lane = i < lanePriority.Length ? lanePriority[i] : (i + 1);
                            await _unitOfWork.ThanhPhanTranDaus.AddAsync(new ThanhPhanTranDau
                            {
                                TranDauId = finalMatchId,
                                DangKyThiDauId = topAdvancing[i].DangKyThiDauId,
                                SoLane = lane,
                                ViTri = lane,
                                TrangThai = "ThamGia",
                                Created = DateTime.UtcNow,
                                CreatedBy = username,
                                IsDeleted = false
                            });
                        }

                        finalMatch.TrangThai = "ChuaDau"; // Sẵn sàng thi đấu
                        _unitOfWork.TranDaus.Update(finalMatch);
                        await _unitOfWork.CompleteAsync();

                        result.FinalHeatAdvancementNotice = $"Đã tự động lọc Top {topAdvancing.Count} VĐV có thành tích tốt nhất toàn bộ vòng loại và xếp vào Lượt Chung kết (với làn hạt giống trung tâm 4, 5).";
                    }
                }
            }

            result.Success = true;
            result.Message = "Đã lưu kết quả lượt thi thành công!";
            return result;
        }

        private async Task AwardMedalAsync(
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
