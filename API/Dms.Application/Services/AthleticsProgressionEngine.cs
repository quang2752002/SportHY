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
            var qualificationError = await ValidateQualificationTieAsync(currentMatch, heatResults, config, isAscending);
            if (qualificationError != null)
            {
                result.Success = false;
                result.Message = qualificationError;
                return result;
            }

            // Sắp xếp thành tích chính; nếu không cho đồng hạng thì dùng chỉ số phụ đã cấu hình.
            var validParticipants = heatResults.Where(r => r.TrangThai == "ThamGia" && r.GiaTri.HasValue).ToList();
            var invalidParticipants = heatResults.Where(r => r.TrangThai != "ThamGia" || !r.GiaTri.HasValue).ToList();

            var tiedPrimaryGroups = validParticipants
                .GroupBy(participant => participant.GiaTri!.Value)
                .Where(group => group.Count() > 1)
                .ToList();
            if (!config.ChoPhepDongHangThanhTich && tiedPrimaryGroups.Any(group =>
                    group.Any(participant => !participant.GiaTriPhu.HasValue) ||
                    group.Select(participant => participant.GiaTriPhu!.Value).Distinct().Count() != group.Count()))
            {
                result.Success = false;
                result.Message = "Có VĐV bằng thành tích chính. Hãy nhập chỉ số phụ khác nhau cho các VĐV đồng thành tích hoặc bật tùy chọn cho phép đồng hạng.";
                return result;
            }

            if (isAscending && config.ChoPhepDongHangThanhTich)
                validParticipants = validParticipants.OrderBy(r => r.GiaTri!.Value).ToList();
            else if (!isAscending && config.ChoPhepDongHangThanhTich)
                validParticipants = validParticipants.OrderByDescending(r => r.GiaTri!.Value).ToList();
            else if (isAscending && config.TieuChiPhuCangNhoCangTot)
                validParticipants = validParticipants.OrderBy(r => r.GiaTri!.Value).ThenBy(r => r.GiaTriPhu).ToList();
            else if (isAscending)
                validParticipants = validParticipants.OrderBy(r => r.GiaTri!.Value).ThenByDescending(r => r.GiaTriPhu).ToList();
            else if (config.TieuChiPhuCangNhoCangTot)
                validParticipants = validParticipants.OrderByDescending(r => r.GiaTri!.Value).ThenBy(r => r.GiaTriPhu).ToList();
            else
                validParticipants = validParticipants.OrderByDescending(r => r.GiaTri!.Value).ThenByDescending(r => r.GiaTriPhu).ToList();

            // Gán đồng hạng theo thành tích chính nếu được cấu hình; chỉ số phụ phân định khi tắt đồng hạng.
            for (var index = 0; index < validParticipants.Count; index++)
            {
                var participant = validParticipants[index];
                var previous = index > 0 ? validParticipants[index - 1] : null;
                var samePlace = previous != null && previous.GiaTri == participant.GiaTri &&
                    (config.ChoPhepDongHangThanhTich || previous.GiaTriPhu == participant.GiaTriPhu);
                participant.XepHang = samePlace ? previous!.XepHang : index + 1;
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
                            Diem = item.GiaTriPhu,
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
                        existingKq.Diem = item.GiaTriPhu;
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

                    // Tất cả VĐV đồng hạng trên bục nhận cùng loại huy chương.
                    if (lhcVang != null)
                    {
                        foreach (var participant in validParticipants.Where(item => item.XepHang == 1))
                        {
                            var registration = await _unitOfWork.DangKyThiDaus.GetByIdAsync(participant.DangKyThiDauId);
                            await AwardMedalAsync(gdm.GiaiDauId, gdm.Id, participant.DangKyThiDauId, lhcVang.Id, 1, "Vô địch Lượt Chung kết", username);
                            result.MedalsAwarded.Add($"🥇 Huy chương Vàng: {registration?.TenDangKy ?? "VĐV Hạng 1"} ({participant.KetQuaText})");
                        }
                    }

                    if (lhcBac != null)
                    {
                        foreach (var participant in validParticipants.Where(item => item.XepHang == 2))
                        {
                            var registration = await _unitOfWork.DangKyThiDaus.GetByIdAsync(participant.DangKyThiDauId);
                            await AwardMedalAsync(gdm.GiaiDauId, gdm.Id, participant.DangKyThiDauId, lhcBac.Id, 2, "Hạng Nhì Lượt Chung kết", username);
                            result.MedalsAwarded.Add($"🥈 Huy chương Bạc: {registration?.TenDangKy ?? "VĐV Hạng 2"} ({participant.KetQuaText})");
                        }
                    }

                    if (lhcDong != null)
                    {
                        foreach (var participant in validParticipants.Where(item => item.XepHang == 3))
                        {
                            var registration = await _unitOfWork.DangKyThiDaus.GetByIdAsync(participant.DangKyThiDauId);
                            await AwardMedalAsync(gdm.GiaiDauId, gdm.Id, participant.DangKyThiDauId, lhcDong.Id, 3, "Hạng Ba Lượt Chung kết", username);
                            result.MedalsAwarded.Add($"🥉 Huy chương Đồng: {registration?.TenDangKy ?? "VĐV Hạng 3"} ({participant.KetQuaText})");
                        }
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
                                        select new { tp.DangKyThiDauId, kq.GiaTri, kq.Diem, kq.KetQuaText })
                            .GroupBy(item => item.DangKyThiDauId)
                            .Select(group => group.First())
                            .ToList();

                        var sortedAll = isAscending
                            ? combined.OrderBy(item => item.GiaTri!.Value)
                            : combined.OrderByDescending(item => item.GiaTri!.Value);
                        if (!config.ChoPhepDongHangThanhTich)
                        {
                            sortedAll = config.TieuChiPhuCangNhoCangTot
                                ? sortedAll.ThenBy(item => item.Diem)
                                : sortedAll.ThenByDescending(item => item.Diem);
                        }
                        var rankedAll = sortedAll.ToList();

                        // Lấy Top N vào Chung kết (ví dụ Top 8)
                        int finalSize = Math.Max(1, config.SoVdvVaoChungKet ?? 8);
                        var topAdvancing = rankedAll.Take(finalSize).ToList();

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

        /// <summary>
        /// Kiểm tra trước khi chốt lượt cuối của vòng loại rằng hòa thành tích tại ranh giới vào chung kết
        /// đã được phân định bằng chỉ số phụ hoặc có thể đưa toàn bộ nhóm đồng hạng vào trong sức chứa.
        /// </summary>
        /// <param name="currentMatch">Lượt thi đang được hoàn tất</param>
        /// <param name="heatResults">Kết quả VĐV vừa gửi từ màn trọng tài</param>
        /// <param name="config">Cấu hình xếp hạng và số VĐV vào chung kết</param>
        /// <param name="isAscending">Cho biết thành tích chính có tiêu chí nhỏ hơn là tốt hơn hay không</param>
        /// <returns>Thông báo lỗi nếu có hòa chưa phân định; ngược lại trả về null</returns>
        private async Task<string?> ValidateQualificationTieAsync(
            TranDau currentMatch,
            List<HeatParticipantResultDto> heatResults,
            CauHinhTheThucDto config,
            bool isAscending)
        {
            if (!currentMatch.NextTranDauId.HasValue || currentMatch.NextTranDauId.Value <= 0)
            {
                return null;
            }

            var heats = (await _unitOfWork.TranDaus.FindAsync(
                match => match.VongDauId == currentMatch.VongDauId && match.IsDeleted != true)).ToList();
            if (heats.Where(match => match.Id != currentMatch.Id).Any(match => match.TrangThai != "KetThuc"))
            {
                return null;
            }

            var otherHeatIds = heats.Where(match => match.Id != currentMatch.Id).Select(match => match.Id).ToList();
            var otherParticipants = (await _unitOfWork.ThanhPhanTranDaus.FindAsync(
                participant => otherHeatIds.Contains(participant.TranDauId) && participant.IsDeleted != true)).ToList();
            var otherParticipantIds = otherParticipants.Select(participant => participant.Id).ToList();
            var otherResults = (await _unitOfWork.KetQuaTranDaus.FindAsync(
                result => otherParticipantIds.Contains(result.ThanhPhanTranDauId) && result.IsDeleted != true && result.GiaTri.HasValue)).ToList();

            var entries = (from participant in otherParticipants
                           join result in otherResults on participant.Id equals result.ThanhPhanTranDauId
                           where participant.TrangThai == "ThamGia"
                           select (Primary: result.GiaTri!.Value, Secondary: result.Diem))
                .ToList();
            entries.AddRange(heatResults
                .Where(item => item.TrangThai == "ThamGia" && item.GiaTri.HasValue)
                .Select(item => (Primary: item.GiaTri!.Value, Secondary: item.GiaTriPhu)));

            if (!config.ChoPhepDongHangThanhTich && entries
                .GroupBy(item => item.Primary)
                .Any(group => group.Count() > 1 &&
                    (group.Any(item => !item.Secondary.HasValue) ||
                     group.Select(item => item.Secondary!.Value).Distinct().Count() != group.Count())))
            {
                return "Có VĐV bằng thành tích chính giữa các lượt. Hãy nhập chỉ số phụ khác nhau trước khi chốt lượt cuối vòng loại.";
            }

            IOrderedEnumerable<(decimal Primary, decimal? Secondary)> ordered = isAscending
                ? entries.OrderBy(item => item.Primary)
                : entries.OrderByDescending(item => item.Primary);
            if (!config.ChoPhepDongHangThanhTich)
            {
                ordered = config.TieuChiPhuCangNhoCangTot
                    ? ordered.ThenBy(item => item.Secondary)
                    : ordered.ThenByDescending(item => item.Secondary);
            }

            var ranked = ordered.ToList();
            var finalSize = Math.Max(1, config.SoVdvVaoChungKet ?? 8);
            if (config.ChoPhepDongHangThanhTich && ranked.Count > finalSize &&
                ranked[finalSize - 1].Primary == ranked[finalSize].Primary)
            {
                return $"Có đồng hạng tại vị trí cuối được vào chung kết (Top {finalSize}). Hãy tăng số VĐV vào chung kết hoặc cấu hình tiêu chí phụ trước khi chốt lượt.";
            }

            return null;
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
