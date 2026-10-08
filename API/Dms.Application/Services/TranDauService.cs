using Dms.Application.DTOs;
using Dms.Application.Interfaces;
using Dms.Domain.Common;
using Dms.Domain.Entities;
using Dms.Domain.Enums;
using Dms.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace Dms.Application.Services
{
    public class TranDauService : ITranDauService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICauHinhTheThucService _theThucService;
        private readonly IMatchScoringEngine _scoringEngine;
        private readonly IGroupStandingsEngine _groupStandingsEngine;
        private readonly IKnockoutProgressionEngine _knockoutProgressionEngine;
        private readonly IAthleticsProgressionEngine _athleticsProgressionEngine;

        public TranDauService(
            IUnitOfWork unitOfWork,
            ICauHinhTheThucService theThucService,
            IMatchScoringEngine scoringEngine,
            IGroupStandingsEngine groupStandingsEngine,
            IKnockoutProgressionEngine knockoutProgressionEngine,
            IAthleticsProgressionEngine athleticsProgressionEngine)
        {
            _unitOfWork = unitOfWork;
            _theThucService = theThucService;
            _scoringEngine = scoringEngine;
            _groupStandingsEngine = groupStandingsEngine;
            _knockoutProgressionEngine = knockoutProgressionEngine;
            _athleticsProgressionEngine = athleticsProgressionEngine;
        }

        /// <summary>
        /// Lấy danh sách trận đấu phân trang theo các tiêu chí tìm kiếm và bộ lọc (giải đấu, danh mục môn, môn thi đấu, vòng, bảng, sân, ngày, trạng thái).
        /// </summary>
        public async Task<PagedResult<TranDauDto>> GetPagedAsync(
            int pageIndex,
            int pageSize,
            string? keyword = null,
            int? giaiDauId = null,
            int? giaiDauMonTheThaoId = null,
            int? vongDauId = null,
            int? bangDauId = null,
            int? sanDauId = null,
            DateTime? ngay = null,
            string? trangThai = null,
            int? danhMucMonTheThaoId = null)
        {
            var all = await GetAllAsync(giaiDauId, giaiDauMonTheThaoId, vongDauId, bangDauId, sanDauId, ngay, danhMucMonTheThaoId);

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var kw = keyword.Trim().ToLower();
                all = all.Where(t =>
                    (t.TenTran != null && t.TenTran.ToLower().Contains(kw)) ||
                    (t.TenDoi1 != null && t.TenDoi1.ToLower().Contains(kw)) ||
                    (t.TenDoi2 != null && t.TenDoi2.ToLower().Contains(kw)) ||
                    (t.TenSanDau != null && t.TenSanDau.ToLower().Contains(kw))
                );
            }

            if (!string.IsNullOrWhiteSpace(trangThai))
            {
                all = all.Where(t => t.TrangThai == trangThai);
            }

            var totalCount = all.Count();
            var items = all
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return new PagedResult<TranDauDto>(items, totalCount, pageIndex, pageSize);
        }

        /// <summary>
        /// Lấy tất cả danh sách trận đấu thỏa mãn các điều kiện lọc (giải đấu, danh mục môn, môn thi đấu, vòng, bảng, sân, ngày).
        /// </summary>
        public async Task<IEnumerable<TranDauDto>> GetAllAsync(
            int? giaiDauId = null,
            int? giaiDauMonTheThaoId = null,
            int? vongDauId = null,
            int? bangDauId = null,
            int? sanDauId = null,
            DateTime? ngay = null,
            int? danhMucMonTheThaoId = null)
        {
            var paged = await _unitOfWork.TranDaus.GetPagedAsync(
                pageIndex: 1,
                pageSize: 2000,
                predicate: t => t.IsDeleted != true &&
                                (!giaiDauMonTheThaoId.HasValue || t.GiaiDauMonTheThaoId == giaiDauMonTheThaoId.Value) &&
                                (!vongDauId.HasValue || t.VongDauId == vongDauId.Value) &&
                                (!bangDauId.HasValue || t.BangDauId == bangDauId.Value) &&
                                (!sanDauId.HasValue || t.SanDauId == sanDauId.Value) &&
                                (!ngay.HasValue || (t.ThoiGianDuKien.HasValue && t.ThoiGianDuKien.Value.Date == ngay.Value.Date)),
                orderBy: q => q.OrderBy(t => t.ThoiGianDuKien).ThenBy(t => t.SoTran),
                t => t.GiaiDauMonTheThao,
                t => t.GiaiDauMonTheThao.GiaiDau,
                t => t.GiaiDauMonTheThao.MonTheThao,
                t => t.GiaiDauMonTheThao.MonTheThao.DanhMuc,
                t => t.VongDau,
                t => t.BangDau!,
                t => t.SanDau!,
                t => t.SanDau!.CumSan,
                t => t.ThanhPhanTranDaus,
                t => t.PhanCongTrongTais
            );

            var items = paged.Items.AsEnumerable();

            if (giaiDauId.HasValue)
            {
                items = items.Where(t => t.GiaiDauMonTheThao?.GiaiDauId == giaiDauId.Value);
            }

            if (danhMucMonTheThaoId.HasValue)
            {
                items = items.Where(t => t.GiaiDauMonTheThao?.MonTheThao?.DanhMucId == danhMucMonTheThaoId.Value);
            }

            var tranList = items.ToList();
            if (!tranList.Any()) return new List<TranDauDto>();

            // Lấy thêm thông tin DangKyThiDau và TrongTai để điền tên
            var allDkIds = tranList.SelectMany(t => t.ThanhPhanTranDaus.Where(tp => tp.IsDeleted != true).Select(tp => tp.DangKyThiDauId)).Distinct().ToList();
            var dangKyList = (await _unitOfWork.DangKyThiDaus.GetPagedAsync(
                1, 2000,
                predicate: d => allDkIds.Contains(d.Id),
                orderBy: null,
                d => d.Doi!,
                d => d.Doi!.DonVi!
            )).Items.ToList();
            var dangKyMap = dangKyList.ToDictionary(d => d.Id);

            // Lấy VDV từ ThanhVienDoi thay vì ChiTietDangKyThiDau
            var allDoiIds = dangKyList.Where(d => d.DoiId.HasValue).Select(d => d.DoiId!.Value).Distinct().ToList();
            var allThanhViens = allDoiIds.Any()
                ? (await _unitOfWork.ThanhVienDois.FindAsync(tv => allDoiIds.Contains(tv.DoiId) && tv.IsDeleted != true)).ToList()
                : new List<ThanhVienDoi>();
            var allVdvIds = allThanhViens.Select(tv => tv.VanDongVienId).Distinct().ToList();
            var vdvMap = (await _unitOfWork.VanDongViens.FindAsync(v => allVdvIds.Contains(v.Id))).ToDictionary(v => v.Id, v => v.HoTen);

            var allTtIds = tranList.SelectMany(t => t.PhanCongTrongTais.Where(pc => pc.IsDeleted != true).Select(pc => pc.TrongTaiId)).Distinct().ToList();
            var trongTaiMap = (await _unitOfWork.TrongTais.FindAsync(tt => allTtIds.Contains(tt.Id))).ToDictionary(tt => tt.Id);

            var result = new List<TranDauDto>();
            foreach (var t in tranList)
            {
                var tpList = t.ThanhPhanTranDaus.Where(tp => tp.IsDeleted != true).OrderBy(tp => tp.ViTri ?? 1).ToList();
                var pcList = t.PhanCongTrongTais.Where(pc => pc.IsDeleted != true).ToList();
                bool laMonDongDoi = t.GiaiDauMonTheThao?.MonTheThao?.LaMonDongDoi ?? false;

                var thanhPhanDtos = new List<ThanhPhanTranDauItemDto>();
                foreach (var tp in tpList)
                {
                    dangKyMap.TryGetValue(tp.DangKyThiDauId, out var dk);
                    string displayName;
                    if (laMonDongDoi)
                    {
                        displayName = (!string.IsNullOrWhiteSpace(dk?.Doi?.Ten) && !dk.Doi.Ten.StartsWith("Tham gia -", StringComparison.OrdinalIgnoreCase))
                            ? dk.Doi.Ten
                            : (!string.IsNullOrWhiteSpace(dk?.TenDangKy) && !dk.TenDangKy.StartsWith("Tham gia -", StringComparison.OrdinalIgnoreCase) ? dk.TenDangKy : $"Đội #{tp.DangKyThiDauId}");
                    }
                    else
                    {
                        // Lấy VĐV đầu tiên từ thành viên đội
                        var firstVdvId = dk?.DoiId.HasValue == true
                            ? allThanhViens.Where(tv => tv.DoiId == dk!.DoiId!.Value && tv.IsDeleted != true).Select(tv => (int?)tv.VanDongVienId).FirstOrDefault()
                            : null;
                        string? vdvName = null;
                        if (firstVdvId.HasValue) vdvMap.TryGetValue(firstVdvId.Value, out vdvName);
                        if (!string.IsNullOrEmpty(vdvName))
                        {
                            displayName = vdvName;
                        }
                        else if (!string.IsNullOrWhiteSpace(dk?.Doi?.Ten) && !dk.Doi.Ten.StartsWith("Tham gia -", StringComparison.OrdinalIgnoreCase))
                        {
                            displayName = dk.Doi.Ten;
                        }
                        else if (!string.IsNullOrWhiteSpace(dk?.TenDangKy) && !dk.TenDangKy.StartsWith("Tham gia -", StringComparison.OrdinalIgnoreCase))
                        {
                            displayName = dk.TenDangKy;
                        }
                        else
                        {
                            displayName = $"VĐV #{tp.DangKyThiDauId}";
                        }
                    }

                    thanhPhanDtos.Add(new ThanhPhanTranDauItemDto
                    {
                        Id = tp.Id,
                        TranDauId = tp.TranDauId,
                        DangKyThiDauId = tp.DangKyThiDauId,
                        TenDangKy = displayName,
                        TenDoi = displayName,
                        TenDonVi = dk?.Doi?.DonVi?.Ten,
                        SoLane = tp.SoLane,
                        SoDeoBIB = tp.SoDeoBIB,
                        ThuTuThiDau = tp.ThuTuThiDau,
                        ViTri = tp.ViTri,
                        TrangThai = tp.TrangThai,
                        GhiChu = tp.GhiChu
                    });
                }

                var trongTaiDtos = new List<PhanCongTrongTaiItemDto>();
                foreach (var pc in pcList)
                {
                    trongTaiMap.TryGetValue(pc.TrongTaiId, out var tt);
                    trongTaiDtos.Add(new PhanCongTrongTaiItemDto
                    {
                        Id = pc.Id,
                        TranDauId = pc.TranDauId,
                        TrongTaiId = pc.TrongTaiId,
                        TenTrongTai = tt?.HoTen,
                        SoDienThoai = tt?.SoDienThoai,
                        CapBac = tt?.CapBac,
                        VaiTro = pc.VaiTro,
                        GhiChu = pc.GhiChu
                    });
                }

                var doi1 = thanhPhanDtos.FirstOrDefault(x => x.ViTri == 1) ?? (thanhPhanDtos.All(x => x.ViTri == null) ? thanhPhanDtos.ElementAtOrDefault(0) : null);
                var doi2 = thanhPhanDtos.FirstOrDefault(x => x.ViTri == 2) ?? (thanhPhanDtos.All(x => x.ViTri == null) ? thanhPhanDtos.ElementAtOrDefault(1) : null);
                if (doi1 != null && doi2 != null && doi1.DangKyThiDauId == doi2.DangKyThiDauId) doi2 = null;

                result.Add(new TranDauDto
                {
                    Id = t.Id,
                    GiaiDauMonTheThaoId = t.GiaiDauMonTheThaoId,
                    GiaiDauId = t.GiaiDauMonTheThao?.GiaiDauId,
                    TenGiaiDau = t.GiaiDauMonTheThao?.GiaiDau?.Ten,
                    MonTheThaoId = t.GiaiDauMonTheThao?.MonTheThaoId,
                    TenMonTheThao = t.GiaiDauMonTheThao?.MonTheThao?.Ten,
                    DanhMucMonTheThaoId = t.GiaiDauMonTheThao?.MonTheThao?.DanhMucId,
                    TenDanhMucMonTheThao = t.GiaiDauMonTheThao?.MonTheThao?.DanhMuc?.Ten,
                    VongDauId = t.VongDauId,
                    TenVongDau = t.VongDau?.Ten,
                    BangDauId = t.BangDauId,
                    TenBangDau = t.BangDau?.Ten,
                    SanDauId = t.SanDauId,
                    TenSanDau = t.SanDau?.Ten,
                    TenCumSan = t.SanDau?.CumSan?.Ten,
                    SoTran = t.SoTran,
                    TenTran = t.TenTran,
                    ThoiGianDuKien = t.ThoiGianDuKien,
                    ThoiGianBatDau = t.ThoiGianBatDau,
                    ThoiGianKetThuc = t.ThoiGianKetThuc,
                    TrangThai = t.TrangThai,
                    GhiChu = t.GhiChu,
                    Doi1DangKyId = doi1?.DangKyThiDauId,
                    DonViDoi1 = doi1?.TenDonVi,
                    Doi2DangKyId = doi2?.DangKyThiDauId,
                    DonViDoi2 = doi2?.TenDonVi,

                    TySoDoi1 = t.TySoDoi1,
                    TySoDoi2 = t.TySoDoi2,
                    DiemPenaltyDoi1 = t.DiemPenaltyDoi1,
                    DiemPenaltyDoi2 = t.DiemPenaltyDoi2,
                    IsHoa = t.IsHoa,
                    DoiThangDangKyId = t.DoiThangDangKyId,
                    DoiThuaDangKyId = t.DoiThuaDangKyId,
                    NextTranDauId = t.NextTranDauId,
                    NextTranDauViTri = t.NextTranDauViTri,
                    LoserNextTranDauId = t.LoserNextTranDauId,
                    LoserNextTranDauViTri = t.LoserNextTranDauViTri,
                    MaTranBracket = t.MaTranBracket,
                    MaTranHienThi = t.MaTranHienThi,
                    IsLichCoDinh = t.IsLichCoDinh,

                    // Trích xuất placeholder nếu đội chưa xác định (để hiển thị nhánh đấu như Nhất Bảng A, Thắng Tứ kết 1...)
                    TenDoi1 = doi1?.TenDoi ?? doi1?.TenDangKy ?? GetPlaceholderTeam(t.GhiChu, t.TenTran, 1),
                    TenDoi2 = doi2?.TenDoi ?? doi2?.TenDangKy ?? GetPlaceholderTeam(t.GhiChu, t.TenTran, 2),
                    ThanhPhanTranDaus = thanhPhanDtos,
                    DanhSachTrongTai = trongTaiDtos,
                    Created = t.Created,
                    LastModified = t.LastModified
                });
            }

            return result;
        }

        /// <summary>
        /// Returns only matches assigned to the specified referee unless the caller has tournament-wide access.
        /// </summary>
        /// <param name="giaiDauId">Optional tournament ID used to filter the match list.</param>
        /// <param name="trongTaiId">The referee ID whose active assignments define the accessible matches.</param>
        /// <param name="allowAllMatches">Whether the caller may see every match in the tournament.</param>
        /// <returns>Matches within the caller's authorized scope.</returns>
        public async Task<IEnumerable<TranDauDto>> GetAccessibleMatchesAsync(int? giaiDauId, int? trongTaiId, bool allowAllMatches)
        {
            var matches = await GetAllAsync(giaiDauId: giaiDauId);
            if (allowAllMatches) return matches;
            if (!trongTaiId.HasValue) return Enumerable.Empty<TranDauDto>();

            return matches.Where(match => match.DanhSachTrongTai.Any(assignment => assignment.TrongTaiId == trongTaiId.Value)).ToList();
        }

        /// <summary>
        /// Lọc và sắp xếp trận trên màn nhiệm vụ trọng tài: trận đang diễn ra trước, trận sắp diễn ra kế tiếp
        /// theo thời gian gần nhất, và trận đã kết thúc ở cuối danh sách.
        /// </summary>
        /// <param name="matches">Các trận đã được giới hạn theo phạm vi mà người dùng được phép xem.</param>
        /// <param name="monTheThaoId">Mã môn thể thao cần lọc; null nếu không lọc.</param>
        /// <param name="danhMucMonTheThaoId">Mã danh mục môn cần lọc; null nếu không lọc.</param>
        /// <returns>Danh sách trận sau khi lọc và sắp xếp theo mức độ ưu tiên điều hành.</returns>
        public Task<IEnumerable<TranDauDto>> GetOrderedRefereeTaskMatchesAsync(
            IEnumerable<TranDauDto> matches,
            int? monTheThaoId = null,
            int? danhMucMonTheThaoId = null)
        {
            var result = (matches ?? Enumerable.Empty<TranDauDto>())
                .Where(match => !monTheThaoId.HasValue || match.MonTheThaoId == monTheThaoId.Value)
                .Where(match => !danhMucMonTheThaoId.HasValue || match.DanhMucMonTheThaoId == danhMucMonTheThaoId.Value)
                .OrderBy(match => match.TrangThai switch
                {
                    "DangDau" or "DangDienRa" => 0,
                    "KetThuc" or "DaKetThuc" or "DaDau" => 2,
                    _ => 1
                })
                .ThenBy(match => match.TrangThai == "KetThuc" || match.TrangThai == "DaKetThuc" || match.TrangThai == "DaDau"
                    ? -(match.ThoiGianKetThuc ?? match.ThoiGianDuKien ?? DateTime.MinValue).Ticks
                    : (match.ThoiGianDuKien ?? DateTime.MaxValue).Ticks)
                .ThenBy(match => match.SoTran)
                .ToList();

            return Task.FromResult<IEnumerable<TranDauDto>>(result);
        }

        /// <summary>
        /// Verifies match access using the user's elevated tournament permission or an explicit referee assignment.
        /// </summary>
        /// <param name="tranDauId">The match ID to check.</param>
        /// <param name="trongTaiId">The signed-in referee ID, if the account is linked to one.</param>
        /// <param name="allowAllMatches">Whether the caller may access all tournament matches.</param>
        /// <returns>True when access is allowed; otherwise false.</returns>
        public async Task<bool> CanAccessMatchAsync(int tranDauId, int? trongTaiId, bool allowAllMatches)
        {
            if (allowAllMatches) return true;
            if (!trongTaiId.HasValue) return false;

            var match = await GetByIdAsync(tranDauId);
            return match?.DanhSachTrongTai.Any(assignment => assignment.TrongTaiId == trongTaiId.Value) == true;
        }

        /// <summary>
        /// Updates a match's score and progress fields without rewriting its teams or referee assignments.
        /// When provided, persists the score for each period to HiepDau and KetQuaHiepDau; optionally
        /// requires the match to already be in progress. Athletics heats must use their heat-result workflow.
        /// </summary>
        /// <param name="id">The match ID to update.</param>
        /// <param name="dto">The score, status, notes, and outcome to persist.</param>
        /// <param name="updatedBy">The account performing the update.</param>
        /// <param name="requireInProgress">Whether the match must already be in progress before saving.</param>
        /// <returns>True when the match was updated; false when it does not exist.</returns>
        public async Task<bool> UpdateMatchProgressAsync(int id, UpdateMatchProgressDto dto, string? updatedBy = null, bool requireInProgress = false)
        {
            var entity = await _unitOfWork.TranDaus.GetByIdAsync(id);
            if (entity == null || entity.IsDeleted == true) return false;
            if (requireInProgress &&
                (!string.Equals(entity.TrangThai, "DangDau", StringComparison.OrdinalIgnoreCase) ||
                 !string.Equals(dto.TrangThai, "DangDau", StringComparison.OrdinalIgnoreCase)))
            {
                throw new InvalidOperationException("Chỉ được cập nhật điểm khi trận đấu đang diễn ra. Hãy bắt đầu trận trước.");
            }
            if (string.Equals(dto.TrangThai, "DangDau", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(entity.TrangThai, "DangDau", StringComparison.OrdinalIgnoreCase))
            {
                var startBlockReason = await GetRoundStartBlockReasonAsync(id);
                if (startBlockReason != null) throw new InvalidOperationException(startBlockReason);
            }
            if (dto.Score1 < 0 || dto.Score2 < 0 || !new[] { "ChuaDau", "DangDau", "KetThuc" }.Contains(dto.TrangThai))
            {
                throw new ArgumentException("Tỷ số hoặc trạng thái trận đấu không hợp lệ.");
            }

            var config = await _theThucService.GetConfigByTranDauIdAsync(id);
            if (config == null)
            {
                throw new InvalidOperationException("Chưa tìm thấy cấu hình tính điểm của môn thi đấu.");
            }
            if (config.IsPerformanceSport)
            {
                throw new InvalidOperationException("Môn đo thành tích phải lưu kết quả theo từng vận động viên, không dùng tỷ số trận.");
            }

            var setScores = dto.SetScores ?? new List<SetScoreDto>();
            ValidatePeriodScores(config, setScores, dto.TrangThai == "KetThuc");
            var projectedScore = MatchScoreCalculator.CalculateCurrentScore(config, setScores);
            dto.Score1 = projectedScore.Score1;
            dto.Score2 = projectedScore.Score2;
            dto.GhiChu = RewriteSnapshotScores(dto.GhiChu, dto.Score1, dto.Score2);

            if (dto.TrangThai == "KetThuc")
            {
                if (dto.Score1 == dto.Score2)
                {
                    var tieEvaluation = _scoringEngine.EvaluateMatchResult(
                        new CompleteMatchRequestDto
                        {
                            TranDauId = id,
                            Score1 = dto.Score1,
                            Score2 = dto.Score2,
                            PenaltyScore1 = dto.PenaltyScore1,
                            PenaltyScore2 = dto.PenaltyScore2,
                            ExtraTimeScore1 = dto.ExtraTimeScore1,
                            ExtraTimeScore2 = dto.ExtraTimeScore2,
                            SetScores = setScores
                        },
                        config,
                        !entity.BangDauId.HasValue);
                    if (!tieEvaluation.IsValid)
                    {
                        throw new InvalidOperationException(tieEvaluation.ErrorMessage ?? "Chưa phân định được kết quả hòa theo cấu hình môn.");
                    }
                }
            }

            var nowUtc = DateTime.UtcNow;
            var wasInProgress = string.Equals(entity.TrangThai, "DangDau", StringComparison.OrdinalIgnoreCase);
            if ((dto.TrangThai == "DangDau" && !wasInProgress) ||
                (dto.TrangThai == "KetThuc" && !wasInProgress && entity.TrangThai != "KetThuc"))
            {
                entity.ThoiGianBatDau = nowUtc;
            }
            if (dto.TrangThai == "KetThuc")
            {
                entity.ThoiGianKetThuc ??= nowUtc;
            }
            else
            {
                entity.ThoiGianKetThuc = null;
            }

            entity.TySoDoi1 = dto.Score1;
            entity.TySoDoi2 = dto.Score2;
            entity.DiemPenaltyDoi1 = dto.PenaltyScore1;
            entity.DiemPenaltyDoi2 = dto.PenaltyScore2;
            entity.TrangThai = dto.TrangThai;
            entity.GhiChu = dto.GhiChu;
            entity.IsHoa = dto.TrangThai == "KetThuc" && dto.IsHoa;
            entity.DoiThangDangKyId = dto.TrangThai == "KetThuc" ? dto.DoiThangDangKyId : null;
            entity.DoiThuaDangKyId = dto.TrangThai == "KetThuc" ? dto.DoiThuaDangKyId : null;
            entity.TrangThaiDuyetKetQua = "ChoDuyet";
            entity.ThoiGianDuyetKetQua = null;
            entity.NguoiDuyetKetQua = null;
            entity.GhiChuDuyetKetQua = null;
            entity.LastModified = nowUtc;
            entity.LastModifiedBy = updatedBy;

            await SaveMatchSetScoresAsync(entity, setScores, updatedBy, nowUtc, dto.TrangThai == "KetThuc");

            _unitOfWork.TranDaus.Update(entity);
            await _unitOfWork.CompleteAsync();
            return true;
        }

        /// <summary>
        /// Đồng bộ điểm từng hiệp của một trận đang diễn ra vào các bản ghi HiepDau và KetQuaHiepDau.
        /// Các hiệp không còn có trong dữ liệu mới được xóa mềm cùng kết quả của chúng để giữ lịch sử hệ thống.
        /// </summary>
        /// <param name="match">Trận đấu đang được cập nhật.</param>
        /// <param name="setScores">Danh sách thứ tự hiệp và điểm của hai bên.</param>
        /// <param name="updatedBy">Tài khoản thực hiện cập nhật.</param>
        /// <param name="nowUtc">Thời điểm UTC dùng chung cho các bản ghi cập nhật.</param>
        /// <param name="markAllComplete">Đánh dấu mọi hiệp/set đã kết thúc khi chốt trận.</param>
        /// <returns>Task hoàn tất khi dữ liệu từng hiệp đã được đồng bộ vào Unit of Work.</returns>
        private async Task SaveMatchSetScoresAsync(TranDau match, List<SetScoreDto> setScores, string? updatedBy, DateTime nowUtc, bool markAllComplete = false)
        {
            var teamParticipants = (await _unitOfWork.ThanhPhanTranDaus.FindAsync(
                participant => participant.TranDauId == match.Id && participant.IsDeleted != true)).ToList();
            var team1 = teamParticipants.FirstOrDefault(participant => participant.ViTri == 1)
                ?? (teamParticipants.All(p => p.ViTri == null) ? teamParticipants.ElementAtOrDefault(0) : null);
            var team2 = teamParticipants.FirstOrDefault(participant => participant.ViTri == 2)
                ?? (teamParticipants.All(p => p.ViTri == null) ? teamParticipants.ElementAtOrDefault(1) : null);
            if (team1 != null && team2 != null && team1.Id == team2.Id) team2 = null;
            var periods = (await _unitOfWork.HiepDaus.FindAsync(
                period => period.TranDauId == match.Id && period.LoaiHiep == "HiepChinh" && period.IsDeleted != true)).ToList();
            var requestedNumbers = setScores.Select(set => set.SetNumber).ToHashSet();
            var duplicatePeriods = periods
                .GroupBy(period => period.SoHiep)
                .SelectMany(group => group.Skip(1));

            foreach (var obsoletePeriod in periods
                .Where(period => !requestedNumbers.Contains(period.SoHiep))
                .Concat(duplicatePeriods))
            {
                var obsoleteResults = await _unitOfWork.KetQuaHiepDaus.FindAsync(result =>
                    result.HiepDauId == obsoletePeriod.Id && result.IsDeleted != true);
                foreach (var obsoleteResult in obsoleteResults)
                {
                    _unitOfWork.KetQuaHiepDaus.Delete(obsoleteResult);
                }
                _unitOfWork.HiepDaus.Delete(obsoletePeriod);
            }

            var config = await _theThucService.GetConfigByTranDauIdAsync(match.Id);
            var periodName = config?.LoaiTheThuc == "SetDiem" ? "Set" : "Hiệp";
            var periodByNumber = periods
                .Where(period => requestedNumbers.Contains(period.SoHiep))
                .GroupBy(period => period.SoHiep)
                .ToDictionary(group => group.Key, group => group.First());
            var addedPeriod = false;

            foreach (var set in setScores.OrderBy(item => item.SetNumber))
            {
                var isNewPeriod = false;
                if (!periodByNumber.TryGetValue(set.SetNumber, out var period))
                {
                    period = new HiepDau
                    {
                        TranDauId = match.Id,
                        SoHiep = set.SetNumber,
                        LoaiHiep = "HiepChinh",
                        Created = nowUtc,
                        CreatedBy = updatedBy,
                        IsDeleted = false
                    };
                    await _unitOfWork.HiepDaus.AddAsync(period);
                    periodByNumber[set.SetNumber] = period;
                    addedPeriod = true;
                    isNewPeriod = true;
                }

                period.TenHiep = $"{periodName} {set.SetNumber}";
                period.TrangThai = !markAllComplete && set.SetNumber == setScores.Max(item => item.SetNumber) ? "DangDau" : "KetThuc";
                period.ThoiGianBatDau ??= nowUtc;
                period.ThoiGianKetThuc = period.TrangThai == "KetThuc" ? period.ThoiGianKetThuc ?? nowUtc : null;
                period.LastModified = nowUtc;
                period.LastModifiedBy = updatedBy;
                if (!isNewPeriod)
                {
                    _unitOfWork.HiepDaus.Update(period);
                }
            }

            if (addedPeriod)
            {
                await _unitOfWork.CompleteAsync();
            }

            foreach (var set in setScores)
            {
                var period = periodByNumber[set.SetNumber];
                await UpsertSetScoreAsync(period.Id, team1?.Id, set.Score1, updatedBy, nowUtc);
                await UpsertSetScoreAsync(period.Id, team2?.Id, set.Score2, updatedBy, nowUtc);
            }
        }

        /// <summary>
        /// Tạo mới hoặc cập nhật điểm của một thành phần thi đấu trong một hiệp.
        /// </summary>
        /// <param name="periodId">ID hiệp chứa kết quả.</param>
        /// <param name="participantId">ID đội/VĐV trong trận; nếu không có thì bỏ qua kết quả đó.</param>
        /// <param name="score">Điểm được ghi nhận cho thành phần.</param>
        /// <param name="updatedBy">Tài khoản thực hiện cập nhật.</param>
        /// <param name="nowUtc">Thời điểm UTC dùng cho audit.</param>
        /// <returns>Task hoàn tất sau khi bản ghi đã được thêm hoặc đưa vào trạng thái cập nhật.</returns>
        private async Task UpsertSetScoreAsync(int periodId, int? participantId, int score, string? updatedBy, DateTime nowUtc)
        {
            if (!participantId.HasValue) return;

            var existingScores = await _unitOfWork.KetQuaHiepDaus.FindAsync(result =>
                result.HiepDauId == periodId && result.ThanhPhanTranDauId == participantId.Value && result.IsDeleted != true);
            var result = existingScores.FirstOrDefault();
            if (result == null)
            {
                await _unitOfWork.KetQuaHiepDaus.AddAsync(new KetQuaHiepDau
                {
                    HiepDauId = periodId,
                    ThanhPhanTranDauId = participantId.Value,
                    Diem = score,
                    Created = nowUtc,
                    CreatedBy = updatedBy,
                    IsDeleted = false
                });
                return;
            }

            result.Diem = score;
            result.LastModified = nowUtc;
            result.LastModifiedBy = updatedBy;
            _unitOfWork.KetQuaHiepDaus.Update(result);
        }

        /// <summary>
        /// Kiểm tra cấu trúc điểm từng hiệp/set trước khi lưu hoặc chốt trận.
        /// </summary>
        /// <param name="config">Cấu hình tính điểm của môn.</param>
        /// <param name="periodScores">Danh sách điểm từng hiệp/set gửi lên.</param>
        /// <param name="requireComplete">Yêu cầu đủ số hiệp chính hoặc đã xác định đội thắng khi chốt trận.</param>
        /// <returns>Không trả dữ liệu; ném lỗi nghiệp vụ nếu danh sách không hợp lệ.</returns>
        private static void ValidatePeriodScores(CauHinhTheThucDto config, List<SetScoreDto> periodScores, bool requireComplete)
        {
            var maxPeriods = config.SoHiepToiDa;
            if (maxPeriods < 1 || periodScores.Count == 0 || periodScores.Count > maxPeriods)
            {
                throw new InvalidOperationException("Số hiệp/set đã nhập không phù hợp với cấu hình môn thi đấu.");
            }

            var orderedPeriods = periodScores.OrderBy(period => period.SetNumber).ToList();
            if (orderedPeriods.Where((period, index) => period.SetNumber != index + 1 || period.Score1 < 0 || period.Score2 < 0).Any())
            {
                throw new InvalidOperationException("Số thứ tự hiệp/set phải liên tục và điểm không được âm.");
            }

            if (string.Equals(config.LoaiTheThuc, "ThoiGianHiep", StringComparison.OrdinalIgnoreCase))
            {
                if (requireComplete && orderedPeriods.Count != maxPeriods)
                {
                    throw new InvalidOperationException($"Cần nhập đủ {maxPeriods} hiệp chính trước khi hoàn tất trận.");
                }
                return;
            }

            if (!string.Equals(config.LoaiTheThuc, "SetDiem", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Cơ chế tính điểm của môn thi đấu không được hỗ trợ.");
            }

            var targetSets = config.SoHiepThangDeThangTran.GetValueOrDefault(1);
            if (targetSets < 1 || targetSets > maxPeriods)
            {
                throw new InvalidOperationException("Cấu hình số set thắng để thắng trận không hợp lệ.");
            }

            var won1 = 0;
            var won2 = 0;
            for (var index = 0; index < orderedPeriods.Count; index++)
            {
                var period = orderedPeriods[index];
                var winner = MatchScoreCalculator.GetSetWinner(config, period);
                var isLastPeriod = index == orderedPeriods.Count - 1;
                if (winner == 0 && (!isLastPeriod || requireComplete))
                {
                    throw new InvalidOperationException($"Set {period.SetNumber} chưa kết thúc theo luật điểm của môn.");
                }

                if (winner == 1) won1++;
                else if (winner == 2) won2++;

                if (!isLastPeriod && (won1 >= targetSets || won2 >= targetSets))
                {
                    throw new InvalidOperationException("Không thể nhập thêm set sau khi một đội đã đủ số set thắng.");
                }
            }

            if (requireComplete && won1 < targetSets && won2 < targetSets)
            {
                throw new InvalidOperationException($"Chưa đội nào thắng đủ {targetSets} set để kết thúc trận.");
            }
        }

        /// <summary>
        /// Đồng bộ hai tỉ số do máy chủ tính vào snapshot JSON để dữ liệu cũ và mới không bị lệch.
        /// </summary>
        /// <param name="snapshot">Snapshot kết quả JSON hiện tại.</param>
        /// <param name="score1">Tỉ số chuẩn của đội 1.</param>
        /// <param name="score2">Tỉ số chuẩn của đội 2.</param>
        /// <returns>Snapshot đã cập nhật tỉ số, hoặc giá trị ban đầu nếu nội dung không phải JSON snapshot.</returns>
        private static string? RewriteSnapshotScores(string? snapshot, int score1, int score2)
        {
            if (string.IsNullOrWhiteSpace(snapshot) || !snapshot.TrimStart().StartsWith("{", StringComparison.Ordinal))
            {
                return snapshot;
            }

            try
            {
                if (JsonNode.Parse(snapshot) is not JsonObject json) return snapshot;
                json["score1"] = score1;
                json["score2"] = score2;
                return json.ToJsonString();
            }
            catch
            {
                return snapshot;
            }
        }

        /// <summary>
        /// Starts a scheduled match and records its actual start time without changing scores or match notes.
        /// Repeated start requests are idempotent while the match is already in progress.
        /// </summary>
        /// <param name="id">The ID of the match to start.</param>
        /// <param name="updatedBy">The referee account performing the operation.</param>
        /// <returns>True when the match exists and is started or already in progress; false when it does not exist.</returns>
        public async Task<bool> StartMatchAsync(int id, string? updatedBy = null)
        {
            var entity = await _unitOfWork.TranDaus.GetByIdAsync(id);
            if (entity == null || entity.IsDeleted == true) return false;
            if (string.Equals(entity.TrangThai, "DangDau", StringComparison.OrdinalIgnoreCase)) return true;
            if (!string.Equals(entity.TrangThai, "ChuaDau", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Chỉ có thể bắt đầu trận đang ở trạng thái chưa đấu.");
            }

            var startBlockReason = await GetRoundStartBlockReasonAsync(id);
            if (startBlockReason != null) throw new InvalidOperationException(startBlockReason);

            var nowUtc = DateTime.UtcNow;
            entity.TrangThai = "DangDau";
            entity.ThoiGianBatDau = nowUtc;
            entity.ThoiGianKetThuc = null;
            entity.LastModified = nowUtc;
            entity.LastModifiedBy = updatedBy;
            _unitOfWork.TranDaus.Update(entity);
            await _unitOfWork.CompleteAsync();
            return true;
        }

        /// <summary>
        /// Kiểm tra mọi trận thuộc các vòng có thứ tự thấp hơn trong cùng môn đã kết thúc trước khi bắt đầu trận.
        /// </summary>
        /// <param name="tranDauId">ID trận đấu dự kiến bắt đầu.</param>
        /// <returns>Thông báo chặn nếu còn trận ở vòng trước chưa kết thúc; null nếu đủ điều kiện bắt đầu.</returns>
        public async Task<string?> GetRoundStartBlockReasonAsync(int tranDauId)
        {
            var match = (await _unitOfWork.TranDaus.FindAsync(item =>
                item.Id == tranDauId && item.IsDeleted != true)).FirstOrDefault();
            if (match == null) return "Không tìm thấy trận đấu.";

            var currentRound = (await _unitOfWork.VongDaus.FindAsync(round =>
                round.Id == match.VongDauId && round.IsDeleted != true)).FirstOrDefault();
            if (currentRound == null) return "Không xác định được vòng đấu của trận này nên chưa thể bắt đầu.";

            var previousRounds = (await _unitOfWork.VongDaus.FindAsync(round =>
                round.GiaiDauMonTheThaoId == match.GiaiDauMonTheThaoId &&
                round.ThuTu < currentRound.ThuTu &&
                round.IsDeleted != true))
                .OrderBy(round => round.ThuTu)
                .ToList();
            if (previousRounds.Count == 0) return null;

            var previousRoundIds = previousRounds.Select(round => round.Id).ToHashSet();
            var unfinishedMatches = (await _unitOfWork.TranDaus.FindAsync(previousMatch =>
                    previousMatch.GiaiDauMonTheThaoId == match.GiaiDauMonTheThaoId &&
                    previousRoundIds.Contains(previousMatch.VongDauId) &&
                    previousMatch.Id != match.Id &&
                    previousMatch.IsDeleted != true))
                .Where(previousMatch => !new[] { "KetThuc", "DaDau", "DaKetThuc", "HoanThanh" }
                    .Contains(previousMatch.TrangThai, StringComparer.OrdinalIgnoreCase))
                .ToList();
            if (unfinishedMatches.Count == 0) return null;

            var unfinishedRoundIds = unfinishedMatches.Select(item => item.VongDauId).ToHashSet();
            var firstUnfinishedRound = previousRounds.First(round => unfinishedRoundIds.Contains(round.Id));
            return $"Chưa thể bắt đầu trận ở vòng \"{currentRound.Ten}\": vòng trước \"{firstUnfinishedRound.Ten}\" còn {unfinishedMatches.Count} trận chưa kết thúc. Vui lòng hoàn tất các trận vòng trước.";
        }

        /// <summary>
        /// Saves report content without changing match timing, result values, teams, or referee assignments.
        /// </summary>
        /// <param name="id">The match ID whose report should be updated.</param>
        /// <param name="ghiChu">The serialized report content.</param>
        /// <param name="updatedBy">The account performing the update.</param>
        /// <returns>True when the report was updated; false when the match does not exist.</returns>
        public async Task<bool> UpdateMatchReportAsync(int id, string ghiChu, string? updatedBy = null)
        {
            var entity = await _unitOfWork.TranDaus.GetByIdAsync(id);
            if (entity == null || entity.IsDeleted == true) return false;
            if (!IsCompletedMatchStatus(entity.TrangThai))
            {
                throw new InvalidOperationException("Chỉ có thể cập nhật biên bản sau khi trận đấu đã kết thúc.");
            }

            entity.GhiChu = ghiChu;
            entity.LastModified = DateTime.UtcNow;
            entity.LastModifiedBy = updatedBy;
            _unitOfWork.TranDaus.Update(entity);
            await _unitOfWork.CompleteAsync();
            return true;
        }

        /// <summary>
        /// Kiểm tra trạng thái hoàn tất của một trận đấu còn hiệu lực.
        /// </summary>
        /// <param name="tranDauId">ID trận đấu cần kiểm tra.</param>
        /// <returns>True nếu trận đấu tồn tại, chưa bị xóa mềm và đã kết thúc; ngược lại false.</returns>
        public async Task<bool> IsMatchCompletedAsync(int tranDauId)
        {
            var match = await _unitOfWork.TranDaus.GetByIdAsync(tranDauId);
            return match != null && match.IsDeleted != true && IsCompletedMatchStatus(match.TrangThai);
        }

        private static bool IsCompletedMatchStatus(string? status)
        {
            return string.Equals(status, "KetThuc", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(status, "DaKetThuc", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(status, "DaDau", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Lấy thành tích đã lưu theo từng làn trong một lượt thi để trọng tài có thể xem lại và sửa chỉ số phụ.
        /// </summary>
        /// <param name="tranDauId">ID lượt thi cần đọc.</param>
        /// <returns>Danh sách kết quả theo thành phần trận đấu, rỗng nếu lượt chưa có thành phần.</returns>
        public async Task<List<HeatParticipantResultDto>> GetHeatResultsByMatchIdAsync(int tranDauId)
        {
            var participants = (await _unitOfWork.ThanhPhanTranDaus.FindAsync(
                participant => participant.TranDauId == tranDauId && participant.IsDeleted != true))
                .OrderBy(participant => participant.SoLane ?? participant.ViTri ?? int.MaxValue)
                .ThenBy(participant => participant.Id)
                .ToList();
            var participantIds = participants.Select(participant => participant.Id).ToList();
            var results = (await _unitOfWork.KetQuaTranDaus.FindAsync(
                result => participantIds.Contains(result.ThanhPhanTranDauId) && result.IsDeleted != true))
                .GroupBy(result => result.ThanhPhanTranDauId)
                .ToDictionary(group => group.Key, group => group.OrderByDescending(result => result.LastModified ?? result.Created).First());

            return participants.Select(participant =>
            {
                results.TryGetValue(participant.Id, out var result);
                return new HeatParticipantResultDto
                {
                    ThanhPhanTranDauId = participant.Id,
                    DangKyThiDauId = participant.DangKyThiDauId,
                    SoLane = participant.SoLane ?? participant.ViTri ?? 0,
                    GiaTri = result?.GiaTri,
                    GiaTriPhu = result?.Diem,
                    KetQuaText = result?.KetQuaText,
                    TrangThai = participant.TrangThai,
                    XepHang = result?.XepHang,
                    ChiTietKetQuaJson = result?.ChiTietKetQuaJson,
                    SoDeoBIB = participant.SoDeoBIB,
                    ThuTuThiDau = participant.ThuTuThiDau
                };
            }).ToList();
        }

        /// <summary>
        /// Đọc điểm từng hiệp/set từ các bảng kết quả chuẩn để khôi phục chính xác màn trọng tài sau khi tải lại.
        /// </summary>
        /// <param name="tranDauId">ID trận đấu cần đọc điểm.</param>
        /// <returns>Danh sách điểm hai bên theo thứ tự hiệp/set; danh sách rỗng nếu trận chưa có điểm.</returns>
        public async Task<List<SetScoreDto>> GetMatchPeriodScoresAsync(int tranDauId)
        {
            var periods = (await _unitOfWork.HiepDaus.FindAsync(period =>
                    period.TranDauId == tranDauId && period.LoaiHiep == "HiepChinh" && period.IsDeleted != true))
                .OrderBy(period => period.SoHiep)
                .ToList();
            if (periods.Count == 0) return new List<SetScoreDto>();

            var participants = (await _unitOfWork.ThanhPhanTranDaus.FindAsync(participant =>
                    participant.TranDauId == tranDauId && participant.IsDeleted != true))
                .ToList();
            var team1 = participants.FirstOrDefault(participant => participant.ViTri == 1)
                ?? (participants.All(p => p.ViTri == null) ? participants.ElementAtOrDefault(0) : null);
            var team2 = participants.FirstOrDefault(participant => participant.ViTri == 2)
                ?? (participants.All(p => p.ViTri == null) ? participants.ElementAtOrDefault(1) : null);
            if (team1 != null && team2 != null && team1.Id == team2.Id) team2 = null;
            var periodIds = periods.Select(period => period.Id).ToList();
            var results = (await _unitOfWork.KetQuaHiepDaus.FindAsync(result =>
                    periodIds.Contains(result.HiepDauId) && result.IsDeleted != true))
                .ToList();

            return periods.Select(period => new SetScoreDto
            {
                SetNumber = period.SoHiep,
                Score1 = team1 == null ? 0 : (int)(results.FirstOrDefault(result =>
                    result.HiepDauId == period.Id && result.ThanhPhanTranDauId == team1.Id)?.Diem ?? 0),
                Score2 = team2 == null ? 0 : (int)(results.FirstOrDefault(result =>
                    result.HiepDauId == period.Id && result.ThanhPhanTranDauId == team2.Id)?.Diem ?? 0)
            }).ToList();
        }

        public async Task<TranDauDto?> GetByIdAsync(int id)
        {
            var paged = await _unitOfWork.TranDaus.GetPagedAsync(
                pageIndex: 1,
                pageSize: 1,
                predicate: t => t.Id == id && t.IsDeleted != true,
                orderBy: null
            );

            var t = paged.Items.FirstOrDefault();
            if (t == null) return null;

            var list = await GetAllAsync(giaiDauMonTheThaoId: t.GiaiDauMonTheThaoId);
            return list.FirstOrDefault(x => x.Id == id);
        }

        public async Task<TranDauDto> CreateAsync(CreateUpdateTranDauDto dto, string? createdBy = null)
        {
            var entity = new TranDau
            {
                GiaiDauMonTheThaoId = dto.GiaiDauMonTheThaoId,
                VongDauId = dto.VongDauId,
                BangDauId = dto.BangDauId,
                SanDauId = dto.SanDauId,
                SoTran = dto.SoTran,
                TenTran = dto.TenTran?.Trim(),
                ThoiGianDuKien = dto.ThoiGianDuKien,
                ThoiGianBatDau = dto.ThoiGianBatDau ?? dto.ThoiGianDuKien,
                ThoiGianKetThuc = dto.ThoiGianKetThuc,
                TrangThai = string.IsNullOrWhiteSpace(dto.TrangThai) ? "ChuaDau" : dto.TrangThai,
                GhiChu = dto.GhiChu,
                TySoDoi1 = dto.TySoDoi1,
                TySoDoi2 = dto.TySoDoi2,
                DiemPenaltyDoi1 = dto.DiemPenaltyDoi1,
                DiemPenaltyDoi2 = dto.DiemPenaltyDoi2,
                IsHoa = dto.IsHoa,
                DoiThangDangKyId = dto.DoiThangDangKyId,
                DoiThuaDangKyId = dto.DoiThuaDangKyId,
                NextTranDauId = dto.NextTranDauId,
                NextTranDauViTri = dto.NextTranDauViTri,
                LoserNextTranDauId = dto.LoserNextTranDauId,
                LoserNextTranDauViTri = dto.LoserNextTranDauViTri,
                MaTranBracket = dto.MaTranBracket,
                MaTranHienThi = dto.MaTranHienThi,
                IsLichCoDinh = dto.IsLichCoDinh,
                Created = DateTime.UtcNow,
                CreatedBy = createdBy,
                IsDeleted = false
            };

            await _unitOfWork.TranDaus.AddAsync(entity);
            await _unitOfWork.CompleteAsync();

            // Thêm Đội 1
            if (dto.Doi1DangKyId.HasValue && dto.Doi1DangKyId.Value > 0)
            {
                await _unitOfWork.ThanhPhanTranDaus.AddAsync(new ThanhPhanTranDau
                {
                    TranDauId = entity.Id,
                    DangKyThiDauId = dto.Doi1DangKyId.Value,
                    ViTri = 1,
                    TrangThai = "ThamGia",
                    Created = DateTime.UtcNow,
                    CreatedBy = createdBy,
                    IsDeleted = false
                });
            }

            // Thêm Đội 2
            if (dto.Doi2DangKyId.HasValue && dto.Doi2DangKyId.Value > 0)
            {
                await _unitOfWork.ThanhPhanTranDaus.AddAsync(new ThanhPhanTranDau
                {
                    TranDauId = entity.Id,
                    DangKyThiDauId = dto.Doi2DangKyId.Value,
                    ViTri = 2,
                    TrangThai = "ThamGia",
                    Created = DateTime.UtcNow,
                    CreatedBy = createdBy,
                    IsDeleted = false
                });
            }

            // Thêm Trọng tài
            if (dto.DanhSachTrongTai != null && dto.DanhSachTrongTai.Any())
            {
                foreach (var tt in dto.DanhSachTrongTai)
                {
                    await _unitOfWork.PhanCongTrongTais.AddAsync(new PhanCongTrongTai
                    {
                        TranDauId = entity.Id,
                        TrongTaiId = tt.TrongTaiId,
                        VaiTro = string.IsNullOrWhiteSpace(tt.VaiTro) ? "TrongTaiChinh" : tt.VaiTro,
                        GhiChu = tt.GhiChu,
                        Created = DateTime.UtcNow,
                        CreatedBy = createdBy,
                        IsDeleted = false
                    });
                }
            }

            await _unitOfWork.CompleteAsync();
            return (await GetByIdAsync(entity.Id))!;
        }

        public async Task<TranDauDto?> UpdateAsync(int id, CreateUpdateTranDauDto dto, string? updatedBy = null)
        {
            var paged = await _unitOfWork.TranDaus.GetPagedAsync(
                pageIndex: 1,
                pageSize: 1,
                predicate: t => t.Id == id && t.IsDeleted != true
            );

            var entity = paged.Items.FirstOrDefault();
            if (entity == null) return null;

            if (string.Equals(dto.TrangThai, "DangDau", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(entity.TrangThai, "DangDau", StringComparison.OrdinalIgnoreCase))
            {
                var startBlockReason = await GetRoundStartBlockReasonAsync(id);
                if (startBlockReason != null) throw new InvalidOperationException(startBlockReason);
            }

            entity.VongDauId = dto.VongDauId;
            entity.BangDauId = dto.BangDauId;
            entity.SanDauId = dto.SanDauId;
            entity.SoTran = dto.SoTran;
            entity.TenTran = dto.TenTran?.Trim();
            entity.ThoiGianDuKien = dto.ThoiGianDuKien;
            entity.ThoiGianBatDau = dto.ThoiGianBatDau ?? dto.ThoiGianDuKien;
            entity.ThoiGianKetThuc = dto.ThoiGianKetThuc;
            if (!string.IsNullOrWhiteSpace(dto.TrangThai)) entity.TrangThai = dto.TrangThai;
            entity.GhiChu = dto.GhiChu;
            entity.TySoDoi1 = dto.TySoDoi1;
            entity.TySoDoi2 = dto.TySoDoi2;
            entity.DiemPenaltyDoi1 = dto.DiemPenaltyDoi1;
            entity.DiemPenaltyDoi2 = dto.DiemPenaltyDoi2;
            entity.IsHoa = dto.IsHoa;
            entity.DoiThangDangKyId = dto.DoiThangDangKyId;
            entity.DoiThuaDangKyId = dto.DoiThuaDangKyId;
            entity.NextTranDauId = dto.NextTranDauId ?? entity.NextTranDauId;
            entity.NextTranDauViTri = dto.NextTranDauViTri ?? entity.NextTranDauViTri;
            entity.LoserNextTranDauId = dto.LoserNextTranDauId ?? entity.LoserNextTranDauId;
            entity.LoserNextTranDauViTri = dto.LoserNextTranDauViTri ?? entity.LoserNextTranDauViTri;
            entity.MaTranBracket = dto.MaTranBracket ?? entity.MaTranBracket;
            entity.MaTranHienThi = dto.MaTranHienThi ?? entity.MaTranHienThi;
            entity.IsLichCoDinh = dto.IsLichCoDinh;
            entity.LastModified = DateTime.UtcNow;
            entity.LastModifiedBy = updatedBy;

            _unitOfWork.TranDaus.Update(entity);

            // Cập nhật lại ThanhPhanTranDau (chỉ xóa và cập nhật mới nếu caller có truyền đội)
            if ((dto.Doi1DangKyId.HasValue && dto.Doi1DangKyId.Value > 0) || (dto.Doi2DangKyId.HasValue && dto.Doi2DangKyId.Value > 0))
            {
                var oldTp = (await _unitOfWork.ThanhPhanTranDaus.FindAsync(tp => tp.TranDauId == id && tp.IsDeleted != true)).ToList();
                var oldDoi1 = oldTp.FirstOrDefault(tp => tp.ViTri == 1);
                var oldDoi2 = oldTp.FirstOrDefault(tp => tp.ViTri == 2);

                int? targetDoi1 = (dto.Doi1DangKyId.HasValue && dto.Doi1DangKyId.Value > 0) ? dto.Doi1DangKyId.Value : oldDoi1?.DangKyThiDauId;
                int? targetDoi2 = (dto.Doi2DangKyId.HasValue && dto.Doi2DangKyId.Value > 0) ? dto.Doi2DangKyId.Value : oldDoi2?.DangKyThiDauId;

                foreach (var tp in oldTp)
                {
                    _unitOfWork.ThanhPhanTranDaus.Delete(tp);
                }

                if (targetDoi1.HasValue && targetDoi1.Value > 0)
                {
                    await _unitOfWork.ThanhPhanTranDaus.AddAsync(new ThanhPhanTranDau
                    {
                        TranDauId = id,
                        DangKyThiDauId = targetDoi1.Value,
                        ViTri = 1,
                        TrangThai = "ThamGia",
                        Created = DateTime.UtcNow,
                        CreatedBy = updatedBy,
                        IsDeleted = false
                    });
                }

                if (targetDoi2.HasValue && targetDoi2.Value > 0)
                {
                    await _unitOfWork.ThanhPhanTranDaus.AddAsync(new ThanhPhanTranDau
                    {
                        TranDauId = id,
                        DangKyThiDauId = targetDoi2.Value,
                        ViTri = 2,
                        TrangThai = "ThamGia",
                        Created = DateTime.UtcNow,
                        CreatedBy = updatedBy,
                        IsDeleted = false
                    });
                }
            }

            // Cập nhật lại PhanCongTrongTai (chỉ cập nhật nếu caller truyền danh sách trọng tài)
            if (dto.DanhSachTrongTai != null)
            {
                var oldPc = (await _unitOfWork.PhanCongTrongTais.FindAsync(pc => pc.TranDauId == id)).ToList();
                foreach (var pc in oldPc)
                {
                    _unitOfWork.PhanCongTrongTais.Delete(pc);
                }

                if (dto.DanhSachTrongTai.Any())
                {
                    foreach (var tt in dto.DanhSachTrongTai)
                    {
                        await _unitOfWork.PhanCongTrongTais.AddAsync(new PhanCongTrongTai
                        {
                            TranDauId = id,
                            TrongTaiId = tt.TrongTaiId,
                            VaiTro = string.IsNullOrWhiteSpace(tt.VaiTro) ? "TrongTaiChinh" : tt.VaiTro,
                            GhiChu = tt.GhiChu,
                            Created = DateTime.UtcNow,
                            CreatedBy = updatedBy,
                            IsDeleted = false
                        });
                    }
                }
            }

            await _unitOfWork.CompleteAsync();

            // 1. Nếu là trận VÒNG BẢNG: Tự động cập nhật bảng xếp hạng và kiểm tra đẩy đội vào Knockout
            if (entity.TrangThai == "KetThuc" && entity.BangDauId.HasValue)
            {
                var config = await _theThucService.GetConfigByTranDauIdAsync(entity.Id);
                if (config != null)
                {
                    await _groupStandingsEngine.RecalculateGroupStandingsAsync(entity.BangDauId.Value, config, updatedBy);
                }

                try
                {
                    await AdvanceGroupStageWinnersAsync(entity.GiaiDauMonTheThaoId, forceAdvance: false, username: updatedBy);
                }
                catch { }

                try
                {
                    await CheckAndAwardRoundRobinMedalsAsync(entity.GiaiDauMonTheThaoId, entity.BangDauId.Value, updatedBy);
                }
                catch { }
            }
            // 2. Nếu là trận VÒNG KNOCKOUT: Tự động đưa đội thắng/thua đi tiếp vào Bán kết/Chung kết/Tranh 3-4 và trao huy chương
            else if (entity.TrangThai == "KetThuc" && !entity.BangDauId.HasValue)
            {
                try
                {
                    var tps = (await _unitOfWork.ThanhPhanTranDaus.FindAsync(tp => tp.TranDauId == id && tp.IsDeleted != true)).OrderBy(tp => tp.ViTri).ToList();
                    int? t1Id = tps.FirstOrDefault(tp => tp.ViTri == 1)?.DangKyThiDauId;
                    int? t2Id = tps.FirstOrDefault(tp => tp.ViTri == 2)?.DangKyThiDauId;

                    if (t1Id.HasValue && t2Id.HasValue)
                    {
                        int winnerId = entity.DoiThangDangKyId ?? 0;
                        int loserId = entity.DoiThuaDangKyId ?? 0;

                        if (winnerId == 0)
                        {
                            int s1 = entity.TySoDoi1 ?? 0;
                            int s2 = entity.TySoDoi2 ?? 0;
                            int p1 = entity.DiemPenaltyDoi1 ?? 0;
                            int p2 = entity.DiemPenaltyDoi2 ?? 0;

                            if (s1 > s2 || (s1 == s2 && p1 > p2))
                            {
                                winnerId = t1Id.Value;
                                loserId = t2Id.Value;
                            }
                            else if (s2 > s1 || (s1 == s2 && p2 > p1))
                            {
                                winnerId = t2Id.Value;
                                loserId = t1Id.Value;
                            }
                        }

                        if (winnerId > 0)
                        {
                            if (loserId == 0) loserId = (winnerId == t1Id.Value) ? t2Id.Value : t1Id.Value;
                            await _knockoutProgressionEngine.ProcessKnockoutProgressionAsync(entity.Id, winnerId, loserId, updatedBy);
                        }
                    }
                }
                catch { }
            }

            return await GetByIdAsync(id);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var paged = await _unitOfWork.TranDaus.GetPagedAsync(
                pageIndex: 1,
                pageSize: 1,
                predicate: t => t.Id == id && t.IsDeleted != true
            );

            var entity = paged.Items.FirstOrDefault();
            if (entity == null) return false;

            entity.IsDeleted = true;
            entity.LastModified = DateTime.UtcNow;
            _unitOfWork.TranDaus.Update(entity);

            // Xóa mềm các thành phần trận đấu & phân công trọng tài & hiệp đấu
            var tps = await _unitOfWork.ThanhPhanTranDaus.FindAsync(x => x.TranDauId == id);
            foreach (var tp in tps)
            {
                tp.IsDeleted = true;
                _unitOfWork.ThanhPhanTranDaus.Update(tp);
            }

            var pcs = await _unitOfWork.PhanCongTrongTais.FindAsync(x => x.TranDauId == id);
            foreach (var pc in pcs)
            {
                pc.IsDeleted = true;
                _unitOfWork.PhanCongTrongTais.Update(pc);
            }

            var hds = await _unitOfWork.HiepDaus.FindAsync(x => x.TranDauId == id);
            foreach (var hd in hds)
            {
                hd.IsDeleted = true;
                _unitOfWork.HiepDaus.Update(hd);
            }

            await _unitOfWork.CompleteAsync();
            return true;
        }

        public async Task<bool> ClearByGiaiDauMonTheThaoAsync(int giaiDauMonTheThaoId)
        {
            var trans = (await _unitOfWork.TranDaus.FindAsync(t => t.GiaiDauMonTheThaoId == giaiDauMonTheThaoId)).ToList();
            var preservedVongIds = new HashSet<int>();

            foreach (var t in trans)
            {
                // Bảo vệ các trận đã đấu xong, đang diễn ra hoặc Manager đã khóa lịch cố định
                if (t.IsLichCoDinh || t.TrangThai == "DaKetThuc" || t.TrangThai == "DangDau")
                {
                    preservedVongIds.Add(t.VongDauId);
                    continue;
                }

                // 1. Xóa KetQuaHiepDau & HiepDau (tránh lỗi FK_HiepDau_TranDau_TranDauId)
                var hds = (await _unitOfWork.HiepDaus.FindAsync(h => h.TranDauId == t.Id)).ToList();
                foreach (var hd in hds)
                {
                    var kqHds = (await _unitOfWork.KetQuaHiepDaus.FindAsync(kq => kq.HiepDauId == hd.Id)).ToList();
                    foreach (var kq in kqHds) _unitOfWork.KetQuaHiepDaus.Delete(kq);
                    _unitOfWork.HiepDaus.Delete(hd);
                }

                // 2. Xóa KetQuaTranDau & ThanhPhanTranDau
                var tps = (await _unitOfWork.ThanhPhanTranDaus.FindAsync(tp => tp.TranDauId == t.Id)).ToList();
                foreach (var tp in tps)
                {
                    var kqTrans = (await _unitOfWork.KetQuaTranDaus.FindAsync(kq => kq.ThanhPhanTranDauId == tp.Id)).ToList();
                    foreach (var kq in kqTrans) _unitOfWork.KetQuaTranDaus.Delete(kq);

                    var kqHds = (await _unitOfWork.KetQuaHiepDaus.FindAsync(kq => kq.ThanhPhanTranDauId == tp.Id)).ToList();
                    foreach (var kq in kqHds) _unitOfWork.KetQuaHiepDaus.Delete(kq);

                    _unitOfWork.ThanhPhanTranDaus.Delete(tp);
                }

                // 3. Xóa phân công trọng tài
                var pcs = (await _unitOfWork.PhanCongTrongTais.FindAsync(pc => pc.TranDauId == t.Id)).ToList();
                foreach (var pc in pcs) _unitOfWork.PhanCongTrongTais.Delete(pc);

                // 4. Xóa trận đấu
                _unitOfWork.TranDaus.Delete(t);
            }

            await _unitOfWork.CompleteAsync();

            // 5. Xóa các vòng đấu cũ nếu vòng đó không còn chứa trận đấu nào được giữ lại
            var vongs = (await _unitOfWork.VongDaus.FindAsync(v => v.GiaiDauMonTheThaoId == giaiDauMonTheThaoId)).ToList();
            foreach (var v in vongs)
            {
                if (!preservedVongIds.Contains(v.Id))
                {
                    _unitOfWork.VongDaus.Delete(v);
                }
            }
            await _unitOfWork.CompleteAsync();

            return true;
        }

        public async Task<ConflictCheckResultDto> CheckConflictAsync(ConflictCheckRequestDto request)
        {
            var result = new ConflictCheckResultDto();
            var start = request.ThoiGianBatDau;
            var end = request.ThoiGianKetThuc;

            if (start >= end)
            {
                result.HasConflict = true;
                result.Conflicts.Add("Thời gian kết thúc phải lớn hơn thời gian bắt đầu.");
                return result;
            }

            // 1. Kiểm tra Sân đấu
            if (request.SanDauId.HasValue && request.SanDauId.Value > 0)
            {
                var conflictingMatches = (await _unitOfWork.TranDaus.FindAsync(t =>
                    t.IsDeleted != true &&
                    (!request.TranDauId.HasValue || t.Id != request.TranDauId.Value) &&
                    t.SanDauId == request.SanDauId.Value &&
                    t.ThoiGianBatDau.HasValue && t.ThoiGianKetThuc.HasValue &&
                    t.ThoiGianBatDau.Value < end && t.ThoiGianKetThuc.Value > start
                )).ToList();

                if (conflictingMatches.Any())
                {
                    result.HasConflict = true;
                    var matchNames = string.Join(", ", conflictingMatches.Select(m => m.TenTran ?? $"Trận #{m.SoTran}"));
                    var msg = $"Sân đấu đã có trận ({matchNames}) diễn ra trong khung giờ này ({start:HH:mm} - {end:HH:mm}).";
                    result.Conflicts.Add(msg);

                    foreach (var cm in conflictingMatches)
                    {
                        result.ChiTietXungDot.Add(new ConflictDetailDto
                        {
                            LoaiXungDot = "SanDau",
                            ThongBao = $"Sân đấu đang có trận '{cm.TenTran ?? $"Trận #{cm.SoTran}"}' ({cm.ThoiGianBatDau:HH:mm} - {cm.ThoiGianKetThuc:HH:mm})",
                            TranDauBiTrungId = cm.Id,
                            TenTranBiTrung = cm.TenTran ?? $"Trận #{cm.SoTran}",
                            ThoiGianBatDau = cm.ThoiGianBatDau,
                            ThoiGianKetThuc = cm.ThoiGianKetThuc
                        });
                    }
                }
            }

            // 2. Kiểm tra Trọng tài
            if (request.TrongTaiIds != null && request.TrongTaiIds.Any())
            {
                var allPcInTime = (await _unitOfWork.PhanCongTrongTais.FindAsync(pc =>
                    pc.IsDeleted != true &&
                    (!request.TranDauId.HasValue || pc.TranDauId != request.TranDauId.Value) &&
                    request.TrongTaiIds.Contains(pc.TrongTaiId) &&
                    pc.TranDau.IsDeleted != true &&
                    pc.TranDau.ThoiGianBatDau.HasValue && pc.TranDau.ThoiGianKetThuc.HasValue &&
                    pc.TranDau.ThoiGianBatDau.Value < end && pc.TranDau.ThoiGianKetThuc.Value > start
                )).ToList();

                if (allPcInTime.Any())
                {
                    result.HasConflict = true;
                    var ttIds = allPcInTime.Select(x => x.TrongTaiId).Distinct().ToList();
                    var ttDict = (await _unitOfWork.TrongTais.FindAsync(x => ttIds.Contains(x.Id))).ToDictionary(x => x.Id);
                    var ttNames = ttDict.Values.Select(x => x.HoTen);
                    var msg = $"Trọng tài ({string.Join(", ", ttNames)}) đã có lịch điều hành trận khác trong khung giờ này.";
                    result.Conflicts.Add(msg);

                    foreach (var pc in allPcInTime)
                    {
                        ttDict.TryGetValue(pc.TrongTaiId, out var tt);
                        result.ChiTietXungDot.Add(new ConflictDetailDto
                        {
                            LoaiXungDot = "TrongTai",
                            ThongBao = $"Trọng tài {tt?.HoTen ?? "N/A"} đang làm nhiệm vụ tại trận #{pc.TranDauId}",
                            TranDauBiTrungId = pc.TranDauId,
                            TenTranBiTrung = pc.TranDau?.TenTran,
                            ThoiGianBatDau = pc.TranDau?.ThoiGianBatDau,
                            ThoiGianKetThuc = pc.TranDau?.ThoiGianKetThuc
                        });
                    }
                }
            }

            // 3. Kiểm tra Đội thi đấu & Vận động viên
            if (request.DangKyThiDauIds != null && request.DangKyThiDauIds.Any())
            {
                // 3.1 Kiểm tra trùng lặp bản ghi đăng ký chính xác
                var allTpInTime = (await _unitOfWork.ThanhPhanTranDaus.FindAsync(tp =>
                    tp.IsDeleted != true &&
                    (!request.TranDauId.HasValue || tp.TranDauId != request.TranDauId.Value) &&
                    request.DangKyThiDauIds.Contains(tp.DangKyThiDauId) &&
                    tp.TranDau.IsDeleted != true &&
                    tp.TranDau.ThoiGianBatDau.HasValue && tp.TranDau.ThoiGianKetThuc.HasValue &&
                    tp.TranDau.ThoiGianBatDau.Value < end && tp.TranDau.ThoiGianKetThuc.Value > start
                )).ToList();

                if (allTpInTime.Any())
                {
                    result.HasConflict = true;
                    result.Conflicts.Add("Một trong các đội thi đấu đã có trận khác diễn ra trong cùng khung giờ này.");
                    foreach (var tp in allTpInTime)
                    {
                        result.ChiTietXungDot.Add(new ConflictDetailDto
                        {
                            LoaiXungDot = "Doi",
                            ThongBao = $"Đội thi đấu (Mã ĐK #{tp.DangKyThiDauId}) đã có lịch thi đấu trận #{tp.TranDauId}",
                            TranDauBiTrungId = tp.TranDauId,
                            TenTranBiTrung = tp.TranDau?.TenTran,
                            ThoiGianBatDau = tp.TranDau?.ThoiGianBatDau,
                            ThoiGianKetThuc = tp.TranDau?.ThoiGianKetThuc
                        });
                    }
                }

                // 3.2 Kiểm tra Vận động viên thi đấu trùng giờ (VĐV thi đấu nhiều môn / nhiều nội dung)
                var targetVdvMap = await GetVdvsForDangKyListAsync(request.DangKyThiDauIds);
                var targetVdvs = targetVdvMap.Values.SelectMany(v => v).ToList();

                if (targetVdvs.Any())
                {
                    var targetVdvKeys = targetVdvs.Select(v => v.IdentityKey).Distinct().ToHashSet();

                    // Tìm các trận đấu khác trong hệ thống có khung thời gian giao thoa
                    var overlappingMatches = (await _unitOfWork.TranDaus.GetPagedAsync(
                        1, 1000,
                        predicate: t => t.IsDeleted != true &&
                                        (!request.TranDauId.HasValue || t.Id != request.TranDauId.Value) &&
                                        t.ThoiGianBatDau.HasValue && t.ThoiGianKetThuc.HasValue &&
                                        t.ThoiGianBatDau.Value < end && t.ThoiGianKetThuc.Value > start,
                        orderBy: null,
                        t => t.GiaiDauMonTheThao,
                        t => t.GiaiDauMonTheThao.MonTheThao,
                        t => t.SanDau!,
                        t => t.ThanhPhanTranDaus
                    )).Items.ToList();

                    if (overlappingMatches.Any())
                    {
                        var otherDkIds = overlappingMatches
                            .SelectMany(m => m.ThanhPhanTranDaus.Where(tp => tp.IsDeleted != true).Select(tp => tp.DangKyThiDauId))
                            .Distinct()
                            .ToList();

                        var otherVdvMap = await GetVdvsForDangKyListAsync(otherDkIds);
                        var reportedVdvKeys = new HashSet<string>();

                        foreach (var m in overlappingMatches)
                        {
                            var mDkIds = m.ThanhPhanTranDaus.Where(tp => tp.IsDeleted != true).Select(tp => tp.DangKyThiDauId).ToList();
                            var mVdvs = mDkIds.SelectMany(dkId => otherVdvMap.GetValueOrDefault(dkId) ?? new List<VdvParticipantInfo>()).ToList();

                            foreach (var mVdv in mVdvs)
                            {
                                if (targetVdvKeys.Contains(mVdv.IdentityKey))
                                {
                                    var currentVdvInfo = targetVdvs.First(v => v.IdentityKey == mVdv.IdentityKey);
                                    var key = $"{mVdv.IdentityKey}_{m.Id}";
                                    if (reportedVdvKeys.Add(key))
                                    {
                                        result.HasConflict = true;

                                        string monTen = m.GiaiDauMonTheThao?.MonTheThao?.Ten ?? "Thể thao";
                                        string noiDungTen = m.GiaiDauMonTheThao?.MonTheThao?.Ten ?? "";
                                        string sanTen = m.SanDau?.Ten ?? "Chưa xếp sân";
                                        string timeStr = $"{m.ThoiGianBatDau:HH:mm} - {m.ThoiGianKetThuc:HH:mm}";
                                        string matchName = m.TenTran ?? $"Trận #{m.SoTran}";
                                        string identityDesc = !string.IsNullOrWhiteSpace(currentVdvInfo.SoCCCD) ? $"CCCD: {currentVdvInfo.SoCCCD}" : $"Mã: {currentVdvInfo.Ma ?? currentVdvInfo.Id.ToString()}";

                                        var msg = $"VĐV '{currentVdvInfo.HoTen}' ({identityDesc}, thuộc {currentVdvInfo.TenDoi}) bị TRÙNG LỊCH với trận '{matchName}' môn '{monTen}' (Nội dung: {noiDungTen}) lúc {timeStr} tại sân {sanTen}.";
                                        result.Conflicts.Add(msg);

                                        result.ChiTietXungDot.Add(new ConflictDetailDto
                                        {
                                            LoaiXungDot = "VanDongVien",
                                            ThongBao = msg,
                                            VanDongVienId = currentVdvInfo.Id,
                                            TenVanDongVien = currentVdvInfo.HoTen,
                                            MaVanDongVien = currentVdvInfo.Ma,
                                            TenDoiHienTai = currentVdvInfo.TenDoi,
                                            TranDauBiTrungId = m.Id,
                                            TenTranBiTrung = matchName,
                                             TenMonTheThao = monTen,
                                             TenNoiDung = noiDungTen,
                                            TenSanDau = sanTen,
                                            ThoiGianBatDau = m.ThoiGianBatDau,
                                            ThoiGianKetThuc = m.ThoiGianKetThuc
                                        });
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Thuật toán CSP Engine 2 tầng (Macro-Scheduler & Micro Slot Scanner) tự động xếp lịch thi đấu,
        /// chia bảng, bốc thăm hạt giống, phân bổ sân bãi, ca kíp và chỉ định trọng tài.
        /// Tuân thủ nghiêm ngặt các tham số CauHinhLichThiDau theo từng môn để chống quá tải cho VĐV.
        /// </summary>
        /// <param name="request">Các thông số yêu cầu xếp lịch (hoặc override cấu hình môn)</param>
        /// <param name="createdBy">Tên tài khoản người thực hiện thao tác</param>
        /// <returns>Kết quả xếp lịch chi tiết bao gồm số trận đã tạo và thông báo lỗi/xung đột nếu có</returns>
        public async Task<AutoScheduleResultDto> AutoScheduleAsync(AutoScheduleRequestDto request, string? createdBy = null)
        {
            var result = new AutoScheduleResultDto();

            // 0. Nếu người dùng chọn chế độ chỉ phân bổ lại khung giờ linh hoạt cho các trận hiện có (không bốc thăm lại)
            if (request.ChiChiaLaiKhungGio)
            {
                var distReq = new DistributeMatchTimesRequestDto
                {
                    GiaiDauMonTheThaoId = request.GiaiDauMonTheThaoId,
                    ApDungCaSang = request.ApDungCaSang,
                    GioBatDauCaSang = request.GioBatDauCaSang,
                    GioKetThucCaSang = request.GioKetThucCaSang,
                    ApDungCaChieu = request.ApDungCaChieu,
                    GioBatDauCaChieu = request.GioBatDauCaChieu,
                    GioKetThucCaChieu = request.GioKetThucCaChieu,
                    ApDungCaToi = request.ApDungCaToi,
                    GioBatDauCaToi = request.GioBatDauCaToi,
                    GioKetThucCaToi = request.GioKetThucCaToi,
                    ThoiLuongTranPhut = request.ThoiLuongTranPhut,
                    NghiGiuaTranPhut = request.NghiGiuaTranPhut,
                    CheDoPhanBo = !string.IsNullOrWhiteSpace(request.CheDoPhanBoKhungGio) ? request.CheDoPhanBoKhungGio : "LuanPhienCa",
                    ChiTranChuaDau = true
                };
                return await DistributeMatchTimesAsync(distReq, createdBy);
            }

            // 1. Lấy thông tin GiaiDauMonTheThao
            var noiDung = (await _unitOfWork.GiaiDauMonTheThaos.GetPagedAsync(
                1, 1,
                predicate: n => n.Id == request.GiaiDauMonTheThaoId && n.IsDeleted != true,
                orderBy: null,
                n => n.MonTheThao,
                n => n.GiaiDau
            )).Items.FirstOrDefault();

            if (noiDung == null)
            {
                // Fallback: Trong trường hợp client gửi MonTheThaoId thay vì GiaiDauMonTheThaoId
                noiDung = (await _unitOfWork.GiaiDauMonTheThaos.GetPagedAsync(
                    1, 1,
                    predicate: n => n.MonTheThaoId == request.GiaiDauMonTheThaoId && n.IsDeleted != true,
                    orderBy: null,
                    n => n.MonTheThao,
                    n => n.GiaiDau
                )).Items.FirstOrDefault();

                if (noiDung != null)
                {
                    request.GiaiDauMonTheThaoId = noiDung.Id;
                }
            }

            if (noiDung == null)
            {
                result.Success = false;
                result.Message = "Không tìm thấy thông tin môn thi đấu trong giải.";
                return result;
            }

            // 2. Lấy danh sách đăng ký đã duyệt
            var dangKyList = (await _unitOfWork.DangKyThiDaus.FindAsync(
                d => d.GiaiDauMonTheThaoId == request.GiaiDauMonTheThaoId && d.IsDeleted != true &&
                     (d.TrangThai == "DaDuyet" || d.TrangThai == "Approved")
            )).ToList();

            if (dangKyList.Count < 2)
            {
                // Fallback lấy các đội chưa bị từ chối nếu chưa bấm duyệt
                dangKyList = (await _unitOfWork.DangKyThiDaus.FindAsync(
                    d => d.GiaiDauMonTheThaoId == request.GiaiDauMonTheThaoId && d.IsDeleted != true &&
                         d.TrangThai != "TuChoi" && d.TrangThai != "Rejected"
                )).ToList();
            }

            if (dangKyList.Count < 2)
            {
                result.Success = false;
                result.Message = "Giải đấu - Môn thi đấu cần ít nhất 2 đội/vận động viên hợp lệ để xếp lịch.";
                return result;
            }

            // 2.5 Lấy cấu hình thể thức & luật thi đấu của môn để tự động áp dụng (thời lượng, nghỉ, chia bảng, lượt thi)
            var theThuc = await _theThucService.GetEffectiveConfigAsync(noiDung.MonTheThaoId, request.GiaiDauMonTheThaoId);
            if (theThuc != null)
            {
                if (theThuc.SoBang > 0) request.SoBang = theThuc.SoBang;
                if (theThuc.SoDoiMoiBang > 1) request.SoDoiMoiBang = theThuc.SoDoiMoiBang;
                if (theThuc.SoDoiMoiBangVaoVongTrong > 0) request.SoDoiMoiBangVaoVongTrong = theThuc.SoDoiMoiBangVaoVongTrong;
                if (theThuc.SoVdvMoiLuotThi > 0) request.SoVdvMoiLuotThi = theThuc.SoVdvMoiLuotThi;
                if (theThuc.SoVongThi > 0) request.SoVongThi = theThuc.SoVongThi;
                if (!string.IsNullOrWhiteSpace(theThuc.PhuongThucPhanNhom)) request.PhuongThucPhanNhom = theThuc.PhuongThucPhanNhom;
                if (theThuc.ThoiLuongTranPhut > 0) request.ThoiLuongTranPhut = theThuc.ThoiLuongTranPhut;
                if (theThuc.NghiGiuaTranPhut >= 0) request.NghiGiuaTranPhut = theThuc.NghiGiuaTranPhut;
            }

            // 3. Nếu yêu cầu xóa lịch cũ
            if (request.XoaLichCu)
            {
                await ClearByGiaiDauMonTheThaoAsync(request.GiaiDauMonTheThaoId);
            }

            // 4. Lấy danh sách Sân đấu
            var sanDauList = new List<SanDau>();
            if (request.SanDauIds != null && request.SanDauIds.Any())
            {
                sanDauList = (await _unitOfWork.SanDaus.FindAsync(s => request.SanDauIds.Contains(s.Id) && s.TrangThai && s.IsDeleted != true)).ToList();
            }
            if (!sanDauList.Any())
            {
                // Mặc định lấy các sân phục vụ môn này
                int? monId = noiDung.MonTheThaoId;
                sanDauList = (await _unitOfWork.SanDaus.FindAsync(s => (!monId.HasValue || s.MonTheThaoId == monId.Value) && s.TrangThai && s.IsDeleted != true)).ToList();
            }

            if (!sanDauList.Any())
            {
                // Fallback lấy bất kỳ sân nào đang hoạt động
                sanDauList = (await _unitOfWork.SanDaus.FindAsync(s => s.TrangThai && s.IsDeleted != true)).Take(4).ToList();
            }

            // 5. Lấy danh sách Trọng tài
            var trongTaiList = new List<TrongTai>();
            if (request.TrongTaiIds != null && request.TrongTaiIds.Any())
            {
                trongTaiList = (await _unitOfWork.TrongTais.FindAsync(tt => request.TrongTaiIds.Contains(tt.Id) && tt.TrangThai && tt.IsDeleted != true)).ToList();
            }
            else
            {
                trongTaiList = (await _unitOfWork.TrongTais.FindAsync(tt => tt.TrangThai && tt.IsDeleted != true)).ToList();
            }

            // 6. Xác định hình thức thi đấu từ MonTheThao
            var effectiveHinhThuc = noiDung.MonTheThao?.HinhThucThiDau ?? HinhThucThiDau.LoaiTrucTiep;

            bool laLoaiTrucTiep = effectiveHinhThuc == HinhThucThiDau.LoaiTrucTiep ||
                                   effectiveHinhThuc == HinhThucThiDau.NhanhThangNhanhThua;
            bool coVongBang = effectiveHinhThuc == HinhThucThiDau.VongBang ||
                              effectiveHinhThuc == HinhThucThiDau.KetHopVongBangVaLoaiTrucTiep;

            // Nếu là thể thức Loại trực tiếp: đảm bảo KHÔNG có bảng đấu (xóa sạch bảng đấu cũ nếu có)
            if (laLoaiTrucTiep)
            {
                var oldBangs = (await _unitOfWork.BangDaus.FindAsync(b => b.GiaiDauMonTheThaoId == request.GiaiDauMonTheThaoId)).ToList();
                if (oldBangs.Any())
                {
                    foreach (var b in oldBangs)
                    {
                        var tvbs = (await _unitOfWork.ThanhVienBangs.FindAsync(tv => tv.BangDauId == b.Id)).ToList();
                        foreach (var tv in tvbs) _unitOfWork.ThanhVienBangs.Delete(tv);
                        _unitOfWork.BangDaus.Delete(b);
                    }
                    await _unitOfWork.CompleteAsync();
                }
            }
            /*
             Xếp cặp lấy đội thắng -> vòng trong (chưa cần xác định bán kết hay tứ kết) - Khi nào còn 4 đội - Gọi là bán kết (2 cặp) -> trung kết (tranh huy chương vàng/ tranh huy chương đồng)
             
             */
            // Xây dựng danh sách các cặp đấu (Fixture item)
            // Mỗi fixture: { VongDauTen, BangDauId, Doi1Id, Doi2Id, TenTran }
            var fixtures = new List<MatchFixture>();
            int fixtureSeq = 1;
            // Danh sách Heat (Lượt thi) cho thể thức TinhDiemXepHang
            var heatFixtures = new List<HeatFixture>();
            var teamMap = dangKyList.ToDictionary(d => d.Id, d => d.TenDangKy ?? d.SoDangKy ?? $"Đội {d.Id}");

            if (coVongBang)
            {
                // Kiểm tra xem đã có Bảng đấu chưa
                var bangDaus = (await _unitOfWork.BangDaus.GetPagedAsync(
                    1, 100,
                    predicate: b => b.GiaiDauMonTheThaoId == request.GiaiDauMonTheThaoId && b.IsDeleted != true,
                    orderBy: q => q.OrderBy(b => b.ThuTu),
                    b => b.ThanhVienBangs
                )).Items.ToList();

                // Nếu chưa có bảng đấu hoặc bảng đấu trống:
                // Tự tạo nếu request.TaoBangDauNeuChuaCo = true HOẶC nếu là thể thức VongBang thuần túy
                bool canAutoCreateGroups = request.TaoBangDauNeuChuaCo || effectiveHinhThuc == HinhThucThiDau.VongBang;
                if ((!bangDaus.Any() || !bangDaus.Any(b => b.ThanhVienBangs.Any(m => m.IsDeleted != true))) && canAutoCreateGroups)
                {
                    int soBang;
                    if (effectiveHinhThuc == HinhThucThiDau.VongBang && request.CheDoVongBang == "single_group")
                    {
                        soBang = 1;
                    }
                    else if (request.SoBang > 0)
                    {
                        soBang = request.SoBang;
                    }
                    else
                    {
                        int soDoiMoiBang = request.SoDoiMoiBang > 1 ? request.SoDoiMoiBang : 4;
                        soBang = Math.Max(1, (int)Math.Ceiling((double)dangKyList.Count / soDoiMoiBang));
                    }

                    var bangDauService = new BangDauService(_unitOfWork);
                    await bangDauService.AutoDistributeAsync(new AutoDistributeBangDto
                    {
                        GiaiDauMonTheThaoId = request.GiaiDauMonTheThaoId,
                        SoBang = soBang,
                        TienToBang = (soBang == 1 && effectiveHinhThuc == HinhThucThiDau.VongBang) ? "Bảng đấu vòng tròn " : "Bảng "
                    }, createdBy);

                    bangDaus = (await _unitOfWork.BangDaus.GetPagedAsync(
                        1, 100,
                        predicate: b => b.GiaiDauMonTheThaoId == request.GiaiDauMonTheThaoId && b.IsDeleted != true,
                        orderBy: q => q.OrderBy(b => b.ThuTu),
                        b => b.ThanhVienBangs
                    )).Items.ToList();
                }

                // Với từng bảng đấu, áp dụng thuật toán Round-Robin để sinh cặp đấu
                int maxGroupRound = 0;
                int soLuotDau = (effectiveHinhThuc == HinhThucThiDau.VongBang && request.SoLuotDau > 1) ? request.SoLuotDau : 1;

                foreach (var b in bangDaus)
                {
                    var teamIds = b.ThanhVienBangs.Where(m => m.IsDeleted != true).Select(m => m.DangKyThiDauId).ToList();
                    if (teamIds.Count < 2) continue;

                    var groupFixtures = GenerateRoundRobin(teamIds);
                    int totalRoundsInGroup = groupFixtures.Count * soLuotDau;
                    if (totalRoundsInGroup > maxGroupRound) maxGroupRound = totalRoundsInGroup;

                    // Lượt 1 (Lượt đi)
                    for (int r = 0; r < groupFixtures.Count; r++)
                    {
                        int roundNum = r + 1;
                        string prefix = bangDaus.Count > 1 ? $"{b.Ten} - " : "";
                        string vongTen = soLuotDau > 1
                            ? $"{prefix}Lượt đi Vòng {roundNum}"
                            : (bangDaus.Count > 1 ? $"{b.Ten} - Lượt {roundNum}" : $"Vòng {roundNum}");

                        foreach (var pair in groupFixtures[r])
                        {
                            var d1Name = teamMap.GetValueOrDefault(pair.Item1, $"Đội {pair.Item1}");
                            var d2Name = teamMap.GetValueOrDefault(pair.Item2, $"Đội {pair.Item2}");
                            fixtures.Add(new MatchFixture
                            {
                                FixtureId = fixtureSeq++,
                                VongTen = vongTen,
                                VongThuTu = roundNum,
                                BangDauId = b.Id,
                                Doi1DangKyId = pair.Item1,
                                Doi2DangKyId = pair.Item2,
                                TenTran = soLuotDau > 1
                                    ? $"{prefix}Lượt đi Vòng {roundNum}: {d1Name} vs {d2Name}"
                                    : $"{prefix}Lượt {roundNum}: {d1Name} vs {d2Name}"
                            });
                        }
                    }

                    // Lượt 2 (Lượt về - nếu soLuotDau >= 2)
                    if (soLuotDau >= 2)
                    {
                        for (int r = 0; r < groupFixtures.Count; r++)
                        {
                            int roundNum = groupFixtures.Count + r + 1;
                            string prefix = bangDaus.Count > 1 ? $"{b.Ten} - " : "";
                            string vongTen = $"{prefix}Lượt về Vòng {r + 1}";

                            foreach (var pair in groupFixtures[r])
                            {
                                // Đảo vai trò đội 1 / đội 2 (home/away)
                                var d1Name = teamMap.GetValueOrDefault(pair.Item2, $"Đội {pair.Item2}");
                                var d2Name = teamMap.GetValueOrDefault(pair.Item1, $"Đội {pair.Item1}");
                                fixtures.Add(new MatchFixture
                                {
                                    FixtureId = fixtureSeq++,
                                    VongTen = vongTen,
                                    VongThuTu = roundNum,
                                    BangDauId = b.Id,
                                    Doi1DangKyId = pair.Item2,
                                    Doi2DangKyId = pair.Item1,
                                    TenTran = $"{prefix}Lượt về Vòng {r + 1}: {d1Name} vs {d2Name}"
                                });
                            }
                        }
                    }
                }

                // Nếu là thể thức Kết Hợp Vòng Bảng & Loại Trực Tiếp: Tự động xếp lịch tiếp các vòng sau (Tứ kết, Bán kết, Tranh 3-4, Chung kết)
                if (effectiveHinhThuc == HinhThucThiDau.KetHopVongBangVaLoaiTrucTiep && bangDaus.Any())
                {
                    int numGroups = bangDaus.Count;
                    string GetGName(int idx) => idx < bangDaus.Count ? (bangDaus[idx].Ten ?? $"Bảng {(char)('A' + idx)}") : $"Bảng {(char)('A' + idx)}";

                    int nextRoundThuTu = maxGroupRound + 1;
                    int soDoiMoiBang = request.SoDoiMoiBangVaoVongTrong > 0 ? request.SoDoiMoiBangVaoVongTrong : 2;
                    int soDoiThu3 = (soDoiMoiBang >= 2 && request.SoDoiThu3TotNhat > 0) ? request.SoDoiThu3TotNhat : 0;
                    int totalAdvancing = (numGroups * soDoiMoiBang) + soDoiThu3;

                    if (totalAdvancing <= 2 || (numGroups == 1 && dangKyList.Count < 4))
                    {
                        // 2 đội vào thẳng trận Chung kết (Ví dụ: 2 bảng chỉ lấy Nhất bảng, hoặc 1 bảng lấy Top 2)
                        string t1Name = numGroups >= 2 ? $"Nhất {GetGName(0)}" : $"Nhất {GetGName(0)}";
                        string t2Name = numGroups >= 2 ? (soDoiMoiBang == 1 ? $"Nhất {GetGName(1)}" : $"Nhì {GetGName(0)}") : $"Nhì {GetGName(0)}";
                        fixtures.Add(new MatchFixture
                        {
                            FixtureId = fixtureSeq++,
                            VongTen = "Chung kết",
                            VongThuTu = nextRoundThuTu,
                            BangDauId = null,
                            Doi1DangKyId = 0,
                            Doi2DangKyId = 0,
                            TenTran = $"Chung kết: {t1Name} vs {t2Name}",
                            TenDoi1Placeholder = t1Name,
                            TenDoi2Placeholder = t2Name,
                            GhiChu = $"TBD: {t1Name} vs {t2Name}",
                            MaTranBracket = "CK"
                        });
                    }
                    else if (totalAdvancing == 4 || (numGroups <= 3 && soDoiMoiBang >= 2 && soDoiThu3 == 0) || (numGroups == 4 && soDoiMoiBang == 1))
                    {
                        // 4 đội vào Bán kết -> Tranh 3-4 & Chung kết
                        string roundBanKet = "Bán kết";
                        string bk1_d1, bk1_d2, bk2_d1, bk2_d2;

                        if (numGroups >= 4 && soDoiMoiBang == 1)
                        {
                            // 4 bảng chỉ lấy Nhất bảng
                            bk1_d1 = $"Nhất {GetGName(0)}";
                            bk1_d2 = $"Nhất {GetGName(1)}";
                            bk2_d1 = $"Nhất {GetGName(2)}";
                            bk2_d2 = $"Nhất {GetGName(3)}";
                        }
                        else if (numGroups == 2)
                        {
                            // 2 bảng lấy Nhất & Nhì
                            bk1_d1 = $"Nhất {GetGName(0)}";
                            bk1_d2 = $"Nhì {GetGName(1)}";
                            bk2_d1 = $"Nhất {GetGName(1)}";
                            bk2_d2 = $"Nhì {GetGName(0)}";
                        }
                        else if (numGroups == 3)
                        {
                            // 3 bảng: 3 Nhất + 1 Nhì tốt nhất
                            bk1_d1 = $"Nhất {GetGName(0)}";
                            bk1_d2 = "Nhì có thành tích tốt nhất";
                            bk2_d1 = $"Nhất {GetGName(1)}";
                            bk2_d2 = $"Nhất {GetGName(2)}";
                        }
                        else
                        {
                            // 1 bảng (Top 4)
                            bk1_d1 = $"Nhất {GetGName(0)}";
                            bk1_d2 = $"Hạng 4 {GetGName(0)}";
                            bk2_d1 = $"Nhì {GetGName(0)}";
                            bk2_d2 = $"Hạng 3 {GetGName(0)}";
                        }

                        var bk1 = new MatchFixture
                        {
                            FixtureId = fixtureSeq++,
                            VongTen = roundBanKet,
                            VongThuTu = nextRoundThuTu,
                            BangDauId = null,
                            Doi1DangKyId = 0,
                            Doi2DangKyId = 0,
                            TenTran = $"Bán kết 1: {bk1_d1} vs {bk1_d2}",
                            TenDoi1Placeholder = bk1_d1,
                            TenDoi2Placeholder = bk1_d2,
                            GhiChu = $"TBD: {bk1_d1} vs {bk1_d2}",
                            MaTranBracket = "BK1"
                        };

                        var bk2 = new MatchFixture
                        {
                            FixtureId = fixtureSeq++,
                            VongTen = roundBanKet,
                            VongThuTu = nextRoundThuTu,
                            BangDauId = null,
                            Doi1DangKyId = 0,
                            Doi2DangKyId = 0,
                            TenTran = $"Bán kết 2: {bk2_d1} vs {bk2_d2}",
                            TenDoi1Placeholder = bk2_d1,
                            TenDoi2Placeholder = bk2_d2,
                            GhiChu = $"TBD: {bk2_d1} vs {bk2_d2}",
                            MaTranBracket = "BK2"
                        };

                        nextRoundThuTu++;

                        var tranh3 = new MatchFixture
                        {
                            FixtureId = fixtureSeq++,
                            VongTen = "Tranh hạng 3 - 4",
                            VongThuTu = nextRoundThuTu,
                            BangDauId = null,
                            Doi1DangKyId = 0,
                            Doi2DangKyId = 0,
                            TenTran = "Trận tranh hạng 3 - 4: Thua Bán kết 1 vs Thua Bán kết 2",
                            TenDoi1Placeholder = "Thua Bán kết 1",
                            TenDoi2Placeholder = "Thua Bán kết 2",
                            GhiChu = "TBD: Thua Bán kết 1 vs Thua Bán kết 2",
                            MaTranBracket = "T34"
                        };

                        var ck = new MatchFixture
                        {
                            FixtureId = fixtureSeq++,
                            VongTen = "Chung kết",
                            VongThuTu = nextRoundThuTu,
                            BangDauId = null,
                            Doi1DangKyId = 0,
                            Doi2DangKyId = 0,
                            TenTran = "Chung kết: Thắng Bán kết 1 vs Thắng Bán kết 2",
                            TenDoi1Placeholder = "Thắng Bán kết 1",
                            TenDoi2Placeholder = "Thắng Bán kết 2",
                            GhiChu = "TBD: Thắng Bán kết 1 vs Thắng Bán kết 2",
                            MaTranBracket = "CK"
                        };

                        bk1.NextFixtureId = ck.FixtureId; bk1.NextSlot = 1;
                        bk1.LoserNextFixtureId = tranh3.FixtureId; bk1.LoserNextSlot = 1;

                        bk2.NextFixtureId = ck.FixtureId; bk2.NextSlot = 2;
                        bk2.LoserNextFixtureId = tranh3.FixtureId; bk2.LoserNextSlot = 2;

                        fixtures.Add(bk1);
                        fixtures.Add(bk2);
                        fixtures.Add(tranh3);
                        fixtures.Add(ck);
                    }
                    else if (totalAdvancing >= 14 || (numGroups >= 6 && soDoiThu3 >= 4) || (numGroups >= 8 && soDoiMoiBang >= 2))
                    {
                        // 16 đội: Vòng 1/8 (8 trận) -> Tứ kết (4 trận) -> Bán kết (2 trận) -> Tranh 3-4 & Chung kết
                        string roundVong18 = "Vòng 1/8";
                        var r1Matches = new List<MatchFixture>();

                        if (numGroups == 6 && soDoiThu3 == 4)
                        {
                            // Mô hình EURO (6 bảng x 2 = 12 + 4 đội hạng 3 tốt nhất = 16 đội)
                            r1Matches.Add(new MatchFixture { FixtureId = fixtureSeq++, VongTen = roundVong18, VongThuTu = nextRoundThuTu, BangDauId = null, Doi1DangKyId = 0, Doi2DangKyId = 0, TenTran = $"Trận 1 (1/8): Nhất {GetGName(0)} vs Đội thứ 3 tốt nhất (1)", TenDoi1Placeholder = $"Nhất {GetGName(0)}", TenDoi2Placeholder = "Đội thứ 3 tốt nhất (1)", GhiChu = $"TBD: Nhất {GetGName(0)} vs Đội thứ 3 tốt nhất (1)", MaTranBracket = "V18_1" });
                            r1Matches.Add(new MatchFixture { FixtureId = fixtureSeq++, VongTen = roundVong18, VongThuTu = nextRoundThuTu, BangDauId = null, Doi1DangKyId = 0, Doi2DangKyId = 0, TenTran = $"Trận 2 (1/8): Nhất {GetGName(1)} vs Đội thứ 3 tốt nhất (2)", TenDoi1Placeholder = $"Nhất {GetGName(1)}", TenDoi2Placeholder = "Đội thứ 3 tốt nhất (2)", GhiChu = $"TBD: Nhất {GetGName(1)} vs Đội thứ 3 tốt nhất (2)", MaTranBracket = "V18_2" });
                            r1Matches.Add(new MatchFixture { FixtureId = fixtureSeq++, VongTen = roundVong18, VongThuTu = nextRoundThuTu, BangDauId = null, Doi1DangKyId = 0, Doi2DangKyId = 0, TenTran = $"Trận 3 (1/8): Nhất {GetGName(2)} vs Nhì {GetGName(3)}", TenDoi1Placeholder = $"Nhất {GetGName(2)}", TenDoi2Placeholder = $"Nhì {GetGName(3)}", GhiChu = $"TBD: Nhất {GetGName(2)} vs Nhì {GetGName(3)}", MaTranBracket = "V18_3" });
                            r1Matches.Add(new MatchFixture { FixtureId = fixtureSeq++, VongTen = roundVong18, VongThuTu = nextRoundThuTu, BangDauId = null, Doi1DangKyId = 0, Doi2DangKyId = 0, TenTran = $"Trận 4 (1/8): Nhất {GetGName(3)} vs Đội thứ 3 tốt nhất (3)", TenDoi1Placeholder = $"Nhất {GetGName(3)}", TenDoi2Placeholder = "Đội thứ 3 tốt nhất (3)", GhiChu = $"TBD: Nhất {GetGName(3)} vs Đội thứ 3 tốt nhất (3)", MaTranBracket = "V18_4" });
                            r1Matches.Add(new MatchFixture { FixtureId = fixtureSeq++, VongTen = roundVong18, VongThuTu = nextRoundThuTu, BangDauId = null, Doi1DangKyId = 0, Doi2DangKyId = 0, TenTran = $"Trận 5 (1/8): Nhất {GetGName(4)} vs Đội thứ 3 tốt nhất (4)", TenDoi1Placeholder = $"Nhất {GetGName(4)}", TenDoi2Placeholder = "Đội thứ 3 tốt nhất (4)", GhiChu = $"TBD: Nhất {GetGName(4)} vs Đội thứ 3 tốt nhất (4)", MaTranBracket = "V18_5" });
                            r1Matches.Add(new MatchFixture { FixtureId = fixtureSeq++, VongTen = roundVong18, VongThuTu = nextRoundThuTu, BangDauId = null, Doi1DangKyId = 0, Doi2DangKyId = 0, TenTran = $"Trận 6 (1/8): Nhất {GetGName(5)} vs Nhì {GetGName(4)}", TenDoi1Placeholder = $"Nhất {GetGName(5)}", TenDoi2Placeholder = $"Nhì {GetGName(4)}", GhiChu = $"TBD: Nhất {GetGName(5)} vs Nhì {GetGName(4)}", MaTranBracket = "V18_6" });
                            r1Matches.Add(new MatchFixture { FixtureId = fixtureSeq++, VongTen = roundVong18, VongThuTu = nextRoundThuTu, BangDauId = null, Doi1DangKyId = 0, Doi2DangKyId = 0, TenTran = $"Trận 7 (1/8): Nhì {GetGName(0)} vs Nhì {GetGName(2)}", TenDoi1Placeholder = $"Nhì {GetGName(0)}", TenDoi2Placeholder = $"Nhì {GetGName(2)}", GhiChu = $"TBD: Nhì {GetGName(0)} vs Nhì {GetGName(2)}", MaTranBracket = "V18_7" });
                            r1Matches.Add(new MatchFixture { FixtureId = fixtureSeq++, VongTen = roundVong18, VongThuTu = nextRoundThuTu, BangDauId = null, Doi1DangKyId = 0, Doi2DangKyId = 0, TenTran = $"Trận 8 (1/8): Nhì {GetGName(1)} vs Nhì {GetGName(5)}", TenDoi1Placeholder = $"Nhì {GetGName(1)}", TenDoi2Placeholder = $"Nhì {GetGName(5)}", GhiChu = $"TBD: Nhì {GetGName(1)} vs Nhì {GetGName(5)}", MaTranBracket = "V18_8" });
                        }
                        else
                        {
                            // 8 bảng Nhất & Nhì (16 đội)
                            for (int i = 0; i < 4; i++)
                            {
                                int gA = 2 * i;
                                int gB = 2 * i + 1;
                                r1Matches.Add(new MatchFixture { FixtureId = fixtureSeq++, VongTen = roundVong18, VongThuTu = nextRoundThuTu, BangDauId = null, Doi1DangKyId = 0, Doi2DangKyId = 0, TenTran = $"Trận {2 * i + 1} (1/8): Nhất {GetGName(gA)} vs Nhì {GetGName(gB)}", TenDoi1Placeholder = $"Nhất {GetGName(gA)}", TenDoi2Placeholder = $"Nhì {GetGName(gB)}", GhiChu = $"TBD: Nhất {GetGName(gA)} vs Nhì {GetGName(gB)}", MaTranBracket = $"V18_{2 * i + 1}" });
                                r1Matches.Add(new MatchFixture { FixtureId = fixtureSeq++, VongTen = roundVong18, VongThuTu = nextRoundThuTu, BangDauId = null, Doi1DangKyId = 0, Doi2DangKyId = 0, TenTran = $"Trận {2 * i + 2} (1/8): Nhất {GetGName(gB)} vs Nhì {GetGName(gA)}", TenDoi1Placeholder = $"Nhất {GetGName(gB)}", TenDoi2Placeholder = $"Nhì {GetGName(gA)}", GhiChu = $"TBD: Nhất {GetGName(gB)} vs Nhì {GetGName(gA)}", MaTranBracket = $"V18_{2 * i + 2}" });
                            }
                        }

                        nextRoundThuTu++;

                        string roundTuKet = "Tứ kết";
                        var tk1 = new MatchFixture { FixtureId = fixtureSeq++, VongTen = roundTuKet, VongThuTu = nextRoundThuTu, BangDauId = null, Doi1DangKyId = 0, Doi2DangKyId = 0, TenTran = "Tứ kết 1: Thắng Trận 1 (1/8) vs Thắng Trận 2 (1/8)", TenDoi1Placeholder = "Thắng Trận 1 (1/8)", TenDoi2Placeholder = "Thắng Trận 2 (1/8)", GhiChu = "TBD: Thắng Trận 1 (1/8) vs Thắng Trận 2 (1/8)", MaTranBracket = "TK1" };
                        var tk2 = new MatchFixture { FixtureId = fixtureSeq++, VongTen = roundTuKet, VongThuTu = nextRoundThuTu, BangDauId = null, Doi1DangKyId = 0, Doi2DangKyId = 0, TenTran = "Tứ kết 2: Thắng Trận 3 (1/8) vs Thắng Trận 4 (1/8)", TenDoi1Placeholder = "Thắng Trận 3 (1/8)", TenDoi2Placeholder = "Thắng Trận 4 (1/8)", GhiChu = "TBD: Thắng Trận 3 (1/8) vs Thắng Trận 4 (1/8)", MaTranBracket = "TK2" };
                        var tk3 = new MatchFixture { FixtureId = fixtureSeq++, VongTen = roundTuKet, VongThuTu = nextRoundThuTu, BangDauId = null, Doi1DangKyId = 0, Doi2DangKyId = 0, TenTran = "Tứ kết 3: Thắng Trận 5 (1/8) vs Thắng Trận 6 (1/8)", TenDoi1Placeholder = "Thắng Trận 5 (1/8)", TenDoi2Placeholder = "Thắng Trận 6 (1/8)", GhiChu = "TBD: Thắng Trận 5 (1/8) vs Thắng Trận 6 (1/8)", MaTranBracket = "TK3" };
                        var tk4 = new MatchFixture { FixtureId = fixtureSeq++, VongTen = roundTuKet, VongThuTu = nextRoundThuTu, BangDauId = null, Doi1DangKyId = 0, Doi2DangKyId = 0, TenTran = "Tứ kết 4: Thắng Trận 7 (1/8) vs Thắng Trận 8 (1/8)", TenDoi1Placeholder = "Thắng Trận 7 (1/8)", TenDoi2Placeholder = "Thắng Trận 8 (1/8)", GhiChu = "TBD: Thắng Trận 7 (1/8) vs Thắng Trận 8 (1/8)", MaTranBracket = "TK4" };

                        // Nối Vòng 1/8 -> Tứ kết
                        r1Matches[0].NextFixtureId = tk1.FixtureId; r1Matches[0].NextSlot = 1;
                        r1Matches[1].NextFixtureId = tk1.FixtureId; r1Matches[1].NextSlot = 2;
                        r1Matches[2].NextFixtureId = tk2.FixtureId; r1Matches[2].NextSlot = 1;
                        r1Matches[3].NextFixtureId = tk2.FixtureId; r1Matches[3].NextSlot = 2;
                        r1Matches[4].NextFixtureId = tk3.FixtureId; r1Matches[4].NextSlot = 1;
                        r1Matches[5].NextFixtureId = tk3.FixtureId; r1Matches[5].NextSlot = 2;
                        r1Matches[6].NextFixtureId = tk4.FixtureId; r1Matches[6].NextSlot = 1;
                        r1Matches[7].NextFixtureId = tk4.FixtureId; r1Matches[7].NextSlot = 2;

                        nextRoundThuTu++;

                        string roundBanKet = "Bán kết";
                        var bk1 = new MatchFixture { FixtureId = fixtureSeq++, VongTen = roundBanKet, VongThuTu = nextRoundThuTu, BangDauId = null, Doi1DangKyId = 0, Doi2DangKyId = 0, TenTran = "Bán kết 1: Thắng Tứ kết 1 vs Thắng Tứ kết 2", TenDoi1Placeholder = "Thắng Tứ kết 1", TenDoi2Placeholder = "Thắng Tứ kết 2", GhiChu = "TBD: Thắng Tứ kết 1 vs Thắng Tứ kết 2", MaTranBracket = "BK1" };
                        var bk2 = new MatchFixture { FixtureId = fixtureSeq++, VongTen = roundBanKet, VongThuTu = nextRoundThuTu, BangDauId = null, Doi1DangKyId = 0, Doi2DangKyId = 0, TenTran = "Bán kết 2: Thắng Tứ kết 3 vs Thắng Tứ kết 4", TenDoi1Placeholder = "Thắng Tứ kết 3", TenDoi2Placeholder = "Thắng Tứ kết 4", GhiChu = "TBD: Thắng Tứ kết 3 vs Thắng Tứ kết 4", MaTranBracket = "BK2" };

                        tk1.NextFixtureId = bk1.FixtureId; tk1.NextSlot = 1;
                        tk2.NextFixtureId = bk1.FixtureId; tk2.NextSlot = 2;
                        tk3.NextFixtureId = bk2.FixtureId; tk3.NextSlot = 1;
                        tk4.NextFixtureId = bk2.FixtureId; tk4.NextSlot = 2;

                        nextRoundThuTu++;

                        var tranh3 = new MatchFixture { FixtureId = fixtureSeq++, VongTen = "Tranh hạng 3 - 4", VongThuTu = nextRoundThuTu, BangDauId = null, Doi1DangKyId = 0, Doi2DangKyId = 0, TenTran = "Trận tranh hạng 3 - 4: Thua Bán kết 1 vs Thua Bán kết 2", TenDoi1Placeholder = "Thua Bán kết 1", TenDoi2Placeholder = "Thua Bán kết 2", GhiChu = "TBD: Thua Bán kết 1 vs Thua Bán kết 2", MaTranBracket = "T34" };
                        var ck = new MatchFixture { FixtureId = fixtureSeq++, VongTen = "Chung kết", VongThuTu = nextRoundThuTu, BangDauId = null, Doi1DangKyId = 0, Doi2DangKyId = 0, TenTran = "Chung kết: Thắng Bán kết 1 vs Thắng Bán kết 2", TenDoi1Placeholder = "Thắng Bán kết 1", TenDoi2Placeholder = "Thắng Bán kết 2", GhiChu = "TBD: Thắng Bán kết 1 vs Thắng Bán kết 2", MaTranBracket = "CK" };

                        bk1.NextFixtureId = ck.FixtureId; bk1.NextSlot = 1;
                        bk1.LoserNextFixtureId = tranh3.FixtureId; bk1.LoserNextSlot = 1;
                        bk2.NextFixtureId = ck.FixtureId; bk2.NextSlot = 2;
                        bk2.LoserNextFixtureId = tranh3.FixtureId; bk2.LoserNextSlot = 2;

                        fixtures.AddRange(r1Matches);
                        fixtures.Add(tk1); fixtures.Add(tk2); fixtures.Add(tk3); fixtures.Add(tk4);
                        fixtures.Add(bk1); fixtures.Add(bk2);
                        fixtures.Add(tranh3); fixtures.Add(ck);
                    }
                    else
                    {
                        // 8 đội vào Tứ kết (4 bảng x 2 = 8 đội, hoặc 8 bảng chỉ lấy Nhất = 8 đội)
                        string roundTuKet = "Tứ kết";
                        string tk1_d1, tk1_d2, tk2_d1, tk2_d2, tk3_d1, tk3_d2, tk4_d1, tk4_d2;

                        if (numGroups >= 8 && soDoiMoiBang == 1)
                        {
                            tk1_d1 = $"Nhất {GetGName(0)}"; tk1_d2 = $"Nhất {GetGName(1)}";
                            tk2_d1 = $"Nhất {GetGName(2)}"; tk2_d2 = $"Nhất {GetGName(3)}";
                            tk3_d1 = $"Nhất {GetGName(4)}"; tk3_d2 = $"Nhất {GetGName(5)}";
                            tk4_d1 = $"Nhất {GetGName(6)}"; tk4_d2 = $"Nhất {GetGName(7)}";
                        }
                        else if (numGroups >= 4)
                        {
                            tk1_d1 = $"Nhất {GetGName(0)}"; tk1_d2 = $"Nhì {GetGName(1)}";
                            tk2_d1 = $"Nhất {GetGName(2)}"; tk2_d2 = $"Nhì {GetGName(3)}";
                            tk3_d1 = $"Nhất {GetGName(1)}"; tk3_d2 = $"Nhì {GetGName(0)}";
                            tk4_d1 = $"Nhất {GetGName(3)}"; tk4_d2 = $"Nhì {GetGName(2)}";
                        }
                        else
                        {
                            tk1_d1 = $"Nhất {GetGName(0)}"; tk1_d2 = $"Nhì {GetGName(1)}";
                            tk2_d1 = $"Nhất {GetGName(1)}"; tk2_d2 = "Đội thứ 3 tốt nhất (1)";
                            tk3_d1 = numGroups >= 3 ? $"Nhất {GetGName(2)}" : "Vé vớt 1"; tk3_d2 = "Đội thứ 3 tốt nhất (2)";
                            tk4_d1 = numGroups >= 3 ? $"Nhì {GetGName(2)}" : "Vé vớt 2"; tk4_d2 = $"Nhì {GetGName(0)}";
                        }

                        var tk1 = new MatchFixture { FixtureId = fixtureSeq++, VongTen = roundTuKet, VongThuTu = nextRoundThuTu, BangDauId = null, Doi1DangKyId = 0, Doi2DangKyId = 0, TenTran = $"Tứ kết 1: {tk1_d1} vs {tk1_d2}", TenDoi1Placeholder = tk1_d1, TenDoi2Placeholder = tk1_d2, GhiChu = $"TBD: {tk1_d1} vs {tk1_d2}", MaTranBracket = "TK1" };
                        var tk2 = new MatchFixture { FixtureId = fixtureSeq++, VongTen = roundTuKet, VongThuTu = nextRoundThuTu, BangDauId = null, Doi1DangKyId = 0, Doi2DangKyId = 0, TenTran = $"Tứ kết 2: {tk2_d1} vs {tk2_d2}", TenDoi1Placeholder = tk2_d1, TenDoi2Placeholder = tk2_d2, GhiChu = $"TBD: {tk2_d1} vs {tk2_d2}", MaTranBracket = "TK2" };
                        var tk3 = new MatchFixture { FixtureId = fixtureSeq++, VongTen = roundTuKet, VongThuTu = nextRoundThuTu, BangDauId = null, Doi1DangKyId = 0, Doi2DangKyId = 0, TenTran = $"Tứ kết 3: {tk3_d1} vs {tk3_d2}", TenDoi1Placeholder = tk3_d1, TenDoi2Placeholder = tk3_d2, GhiChu = $"TBD: {tk3_d1} vs {tk3_d2}", MaTranBracket = "TK3" };
                        var tk4 = new MatchFixture { FixtureId = fixtureSeq++, VongTen = roundTuKet, VongThuTu = nextRoundThuTu, BangDauId = null, Doi1DangKyId = 0, Doi2DangKyId = 0, TenTran = $"Tứ kết 4: {tk4_d1} vs {tk4_d2}", TenDoi1Placeholder = tk4_d1, TenDoi2Placeholder = tk4_d2, GhiChu = $"TBD: {tk4_d1} vs {tk4_d2}", MaTranBracket = "TK4" };

                        nextRoundThuTu++;

                        string roundBanKet = "Bán kết";
                        var bk1 = new MatchFixture { FixtureId = fixtureSeq++, VongTen = roundBanKet, VongThuTu = nextRoundThuTu, BangDauId = null, Doi1DangKyId = 0, Doi2DangKyId = 0, TenTran = "Bán kết 1: Thắng Tứ kết 1 vs Thắng Tứ kết 2", TenDoi1Placeholder = "Thắng Tứ kết 1", TenDoi2Placeholder = "Thắng Tứ kết 2", GhiChu = "TBD: Thắng Tứ kết 1 vs Thắng Tứ kết 2", MaTranBracket = "BK1" };
                        var bk2 = new MatchFixture { FixtureId = fixtureSeq++, VongTen = roundBanKet, VongThuTu = nextRoundThuTu, BangDauId = null, Doi1DangKyId = 0, Doi2DangKyId = 0, TenTran = "Bán kết 2: Thắng Tứ kết 3 vs Thắng Tứ kết 4", TenDoi1Placeholder = "Thắng Tứ kết 3", TenDoi2Placeholder = "Thắng Tứ kết 4", GhiChu = "TBD: Thắng Tứ kết 3 vs Thắng Tứ kết 4", MaTranBracket = "BK2" };

                        tk1.NextFixtureId = bk1.FixtureId; tk1.NextSlot = 1;
                        tk2.NextFixtureId = bk1.FixtureId; tk2.NextSlot = 2;
                        tk3.NextFixtureId = bk2.FixtureId; tk3.NextSlot = 1;
                        tk4.NextFixtureId = bk2.FixtureId; tk4.NextSlot = 2;

                        nextRoundThuTu++;

                        var tranh3 = new MatchFixture { FixtureId = fixtureSeq++, VongTen = "Tranh hạng 3 - 4", VongThuTu = nextRoundThuTu, BangDauId = null, Doi1DangKyId = 0, Doi2DangKyId = 0, TenTran = "Trận tranh hạng 3 - 4: Thua Bán kết 1 vs Thua Bán kết 2", TenDoi1Placeholder = "Thua Bán kết 1", TenDoi2Placeholder = "Thua Bán kết 2", GhiChu = "TBD: Thua Bán kết 1 vs Thua Bán kết 2", MaTranBracket = "T34" };
                        var ck = new MatchFixture { FixtureId = fixtureSeq++, VongTen = "Chung kết", VongThuTu = nextRoundThuTu, BangDauId = null, Doi1DangKyId = 0, Doi2DangKyId = 0, TenTran = "Chung kết: Thắng Bán kết 1 vs Thắng Bán kết 2", TenDoi1Placeholder = "Thắng Bán kết 1", TenDoi2Placeholder = "Thắng Bán kết 2", GhiChu = "TBD: Thắng Bán kết 1 vs Thắng Bán kết 2", MaTranBracket = "CK" };

                        bk1.NextFixtureId = ck.FixtureId; bk1.NextSlot = 1;
                        bk1.LoserNextFixtureId = tranh3.FixtureId; bk1.LoserNextSlot = 1;
                        bk2.NextFixtureId = ck.FixtureId; bk2.NextSlot = 2;
                        bk2.LoserNextFixtureId = tranh3.FixtureId; bk2.LoserNextSlot = 2;

                        fixtures.Add(tk1); fixtures.Add(tk2); fixtures.Add(tk3); fixtures.Add(tk4);
                        fixtures.Add(bk1); fixtures.Add(bk2);
                        fixtures.Add(tranh3); fixtures.Add(ck);
                    }
                }
            }
            else if (laLoaiTrucTiep)
            {
                // === Thể thức Loại trực tiếp (Single Elimination Bracket) - Bắt cặp theo cây thi đấu chuẩn ===
                var teamIds = dangKyList.Select(d => d.Id).ToList();

                // --- XÁO TRỘN NGẪU NHIÊN (Fisher-Yates Shuffle) ---
                // Mỗi lần chạy xếp lịch, thứ tự ghép cặp sẽ được random hoàn toàn.
                var rng = new Random();
                for (int i = teamIds.Count - 1; i > 0; i--)
                {
                    int j = rng.Next(i + 1);
                    (teamIds[i], teamIds[j]) = (teamIds[j], teamIds[i]);
                }

                int n = teamIds.Count;

                // --- QUY TẮC SỐ ĐỘI LẺ (PLAY-OFF / VÒNG SƠ LOẠI) ---
                if (n % 2 != 0 && n >= 3)
                {
                    int playOffTeam1 = teamIds[n - 2];
                    int playOffTeam2 = teamIds[n - 1];

                    fixtures.Add(new MatchFixture
                    {
                        FixtureId = fixtureSeq++,
                        VongTen = "Vòng sơ loại (Play-off)",
                        VongThuTu = 0, // Vòng 0 thi đấu trước Vòng 1
                        BangDauId = null,
                        Doi1DangKyId = playOffTeam1,
                        Doi2DangKyId = playOffTeam2,
                        TenTran = "Trận 1 - Vòng sơ loại (Play-off)",
                        MaTranBracket = "PO_1"
                    });

                    // Sau trận Play-off, dùng placeholder 0 (TBD) để đại diện cho đội thắng chưa xác định.
                    var round1TeamIds = teamIds.Take(n - 2).ToList();
                    round1TeamIds.Add(0); // 0 = "Chờ xác định" (đội thắng Play-off)
                    teamIds = round1TeamIds;
                    n = teamIds.Count; // n lúc này = N-1 (số chẵn)
                }

                // Bracket size là lũy thừa 2 nhỏ nhất >= n (ví dụ: 6 đội -> bracket 8; 4 đội -> bracket 4)
                int bracketSize = 1;
                while (bracketSize < n) bracketSize *= 2;
                int byes = bracketSize - n;

                var seedOrder = GetBracketSeedOrder(bracketSize);

                // Tạo các node tại vòng khởi đầu của cây bracket (Round 0)
                var currentLevel = new List<BracketNode>();
                for (int m = 0; m < bracketSize / 2; m++)
                {
                    int s1 = seedOrder[2 * m];
                    int s2 = seedOrder[2 * m + 1];
                    int t1 = s1 <= n ? teamIds[s1 - 1] : 0;
                    int t2 = s2 <= n ? teamIds[s2 - 1] : 0;

                    var node = new BracketNode();
                    if (t1 > 0 && t2 > 0)
                    {
                        node.Team1Id = t1;
                        node.Team2Id = t2;
                        node.IsBye = false;
                    }
                    else if (t1 > 0 && t2 == 0)
                    {
                        node.IsBye = true;
                        node.WinnerTeamId = t1;
                    }
                    else if (t2 > 0 && t1 == 0)
                    {
                        node.IsBye = true;
                        node.WinnerTeamId = t2;
                    }
                    currentLevel.Add(node);
                }

                // Xây dựng cây thi đấu lên các vòng tiếp theo (Bán kết, Chung kết...)
                var allLevels = new List<List<BracketNode>> { currentLevel };
                while (currentLevel.Count > 1)
                {
                    var nextLevel = new List<BracketNode>();
                    for (int j = 0; j < currentLevel.Count / 2; j++)
                    {
                        var c1 = currentLevel[2 * j];
                        var c2 = currentLevel[2 * j + 1];
                        var parent = new BracketNode
                        {
                            Child1 = c1,
                            Child2 = c2,
                            Team1Id = c1.IsBye ? c1.WinnerTeamId : 0,
                            Team2Id = c2.IsBye ? c2.WinnerTeamId : 0,
                            IsBye = false
                        };
                        c1.Parent = parent;
                        c1.ParentSlot = 1;
                        c2.Parent = parent;
                        c2.ParentSlot = 2;
                        nextLevel.Add(parent);
                    }
                    allLevels.Add(nextLevel);
                    currentLevel = nextLevel;
                }

                // Lọc các level có trận đấu thực tế diễn ra (bỏ qua các cặp đấu được bye hoàn toàn)
                var playedLevels = new List<(int levelIdx, List<BracketNode> matches)>();
                for (int lvl = 0; lvl < allLevels.Count; lvl++)
                {
                    var matches = allLevels[lvl].Where(node => !node.IsBye).ToList();
                    if (matches.Any())
                    {
                        playedLevels.Add((lvl, matches));
                    }
                }

                int totalPlayedRounds = playedLevels.Count;
                string GetRoundName(int roundOrder) // 1-indexed: 1 là vòng đầu tiên, totalPlayedRounds là Chung kết
                {
                    int fromFinal = totalPlayedRounds - roundOrder;
                    return fromFinal switch
                    {
                        0 => "Chung kết",
                        1 => "Bán kết",
                        2 => (byes > 0 && roundOrder == 1) ? "Vòng loại" : "Tứ kết",
                        3 => "Vòng 1/8",
                        4 => "Vòng 1/16",
                        _ => $"Vòng loại {roundOrder}"
                    };
                }

                MatchFixture? bronzeFixture = null;
                if (totalPlayedRounds >= 2)
                {
                    // Thêm trận Tranh hạng 3 - 4 giữa 2 đội thua Bán kết
                    bronzeFixture = new MatchFixture
                    {
                        FixtureId = fixtureSeq++,
                        VongTen = "Tranh hạng 3 - 4",
                        VongThuTu = totalPlayedRounds,
                        BangDauId = null,
                        Doi1DangKyId = 0,
                        Doi2DangKyId = 0,
                        TenTran = "Trận tranh hạng 3 - 4: Thua Bán kết 1 vs Thua Bán kết 2",
                        TenDoi1Placeholder = "Thua Bán kết 1",
                        TenDoi2Placeholder = "Thua Bán kết 2",
                        GhiChu = "TBD: Thua Bán kết 1 vs Thua Bán kết 2",
                        MaTranBracket = "T34"
                    };
                }

                for (int r = 0; r < totalPlayedRounds; r++)
                {
                    int roundOrder = r + 1;
                    string roundName = GetRoundName(roundOrder);
                    var matchesInRound = playedLevels[r].matches;

                    for (int m = 0; m < matchesInRound.Count; m++)
                    {
                        var node = matchesInRound[m];
                        string matchTitle = matchesInRound.Count == 1 ? roundName : $"{roundName} {m + 1}";
                        if (roundName == "Vòng loại")
                        {
                            matchTitle = $"Trận {m + 1} - Vòng loại";
                        }
                        else if (roundName == "Chung kết" && totalPlayedRounds >= 2 && node.Team1Id == 0 && node.Team2Id == 0)
                        {
                            matchTitle = "Chung kết: Thắng Bán kết 1 vs Thắng Bán kết 2";
                        }

                        var f = new MatchFixture
                        {
                            FixtureId = fixtureSeq++,
                            VongTen = roundName,
                            VongThuTu = roundOrder,
                            BangDauId = null,
                            Doi1DangKyId = node.Team1Id,
                            Doi2DangKyId = node.Team2Id,
                            TenTran = matchTitle,
                            TenDoi1Placeholder = (roundName == "Chung kết" && node.Team1Id == 0) ? "Thắng Bán kết 1" : null,
                            TenDoi2Placeholder = (roundName == "Chung kết" && node.Team2Id == 0) ? "Thắng Bán kết 2" : null,
                            GhiChu = (roundName == "Chung kết" && node.Team1Id == 0 && node.Team2Id == 0) ? "TBD: Thắng Bán kết 1 vs Thắng Bán kết 2" : null,
                            MaTranBracket = $"{roundName}_{(m + 1)}"
                        };
                        node.Fixture = f;
                        fixtures.Add(f);
                    }
                }

                // Kết nối liên kết DAG các vòng Knockout (Đội thắng -> Vòng sau, Đội thua Bán kết -> Tranh hạng 3)
                for (int r = 0; r < totalPlayedRounds; r++)
                {
                    foreach (var node in playedLevels[r].matches)
                    {
                        if (node.Fixture == null) continue;
                        var ancestor = node.Parent;
                        int slot = node.ParentSlot;
                        while (ancestor != null && ancestor.IsBye)
                        {
                            slot = ancestor.ParentSlot;
                            ancestor = ancestor.Parent;
                        }

                        if (ancestor != null && ancestor.Fixture != null)
                        {
                            node.Fixture.NextFixtureId = ancestor.Fixture.FixtureId;
                            node.Fixture.NextSlot = slot;

                            // Nếu ancestor là trận Chung kết (Root node), thì node hiện tại chính là Bán kết!
                            if (ancestor.Parent == null && bronzeFixture != null)
                            {
                                node.Fixture.LoserNextFixtureId = bronzeFixture.FixtureId;
                                node.Fixture.LoserNextSlot = slot;
                            }
                        }
                    }
                }

                if (bronzeFixture != null)
                {
                    fixtures.Add(bronzeFixture);
                }
            }
            else if (effectiveHinhThuc == HinhThucThiDau.TinhDiemXepHang ||
                     effectiveHinhThuc == HinhThucThiDau.DuaThoiGian ||
                     effectiveHinhThuc == HinhThucThiDau.DoLuotThi ||
                     effectiveHinhThuc == HinhThucThiDau.BieuDienChamDiem)
            {
                // ====================================================================
                // THỂ THỨC ĐO THÀNH TÍCH / TÍNH GIỜ / LẦN THỬ / BIỂU DIỄN
                // Phân VĐV thành các Lượt thi (Heat/Session/Flight) tương ứng.
                // ====================================================================
                int heatSize = request.SoVdvMoiLuotThi > 0 ? request.SoVdvMoiLuotThi : (theThuc?.SoVdvMoiLuotThi > 0 ? theThuc.SoVdvMoiLuotThi : 8);
                int soVong = request.SoVongThi > 0 ? request.SoVongThi : (theThuc?.SoVongThi > 0 ? theThuc.SoVongThi : 1);
                string phuongThuc = request.PhuongThucPhanNhom ?? theThuc?.PhuongThucPhanNhom ?? "random";

                var allVdvIds = dangKyList.Select(d => d.Id).ToList();

                // Phân nhóm / bốc thăm VDV theo phương thức
                switch (phuongThuc)
                {
                    case "performance_seed":
                        allVdvIds = allVdvIds.OrderBy(id => id).ToList();
                        break;
                    case "registration_order":
                        break;
                    default: // "random"
                        var rng = new Random();
                        for (int i = allVdvIds.Count - 1; i > 0; i--)
                        {
                            int j = rng.Next(i + 1);
                            (allVdvIds[i], allVdvIds[j]) = (allVdvIds[j], allVdvIds[i]);
                        }
                        break;
                }

                bool isMassStart = (effectiveHinhThuc == HinhThucThiDau.DuaThoiGian && theThuc?.HinhThucXuatPhat == "DongLoat");
                bool isTimeTrialInterval = (effectiveHinhThuc == HinhThucThiDau.DuaThoiGian && theThuc?.HinhThucXuatPhat == "SoLe");
                bool isTrialSport = (effectiveHinhThuc == HinhThucThiDau.DoLuotThi);
                bool isArtisticSport = (effectiveHinhThuc == HinhThucThiDau.BieuDienChamDiem);

                if (isMassStart)
                {
                    // Chạy bền / Marathon / Xe đạp đường trường: Tất cả VĐV vào ĐÚNG 1 lượt Chung kết duy nhất
                    heatFixtures.Add(new HeatFixture
                    {
                        FixtureId = fixtureSeq++,
                        VongTen = "Chung kết",
                        VongThuTu = 1,
                        TenTran = "Chung kết - Xuất phát đồng loạt (Mass Start)",
                        DanhSachVdvIds = allVdvIds
                    });
                }
                else if (isTimeTrialInterval)
                {
                    // Xe đạp / trượt tính giờ cá nhân xuất phát so le: 1 lượt thi chung kết
                    heatFixtures.Add(new HeatFixture
                    {
                        FixtureId = fixtureSeq++,
                        VongTen = "Chung kết",
                        VongThuTu = 1,
                        TenTran = "Chung kết - Tính giờ cá nhân (Time Trial)",
                        DanhSachVdvIds = allVdvIds
                    });
                }
                else if (isTrialSport)
                {
                    // Môn lần thực hiện (Cử tạ, Nhảy xa, Ném tạ):
                    // Nếu đông hơn 16 VĐV thì chia 2 nhóm (Nhóm B & Nhóm A), nếu <= 16 xếp vào 1 buổi thi chung kết
                    if (allVdvIds.Count > 16)
                    {
                        int half = (int)Math.Ceiling(allVdvIds.Count / 2.0);
                        heatFixtures.Add(new HeatFixture
                        {
                            FixtureId = fixtureSeq++,
                            VongTen = "Chung kết",
                            VongThuTu = 1,
                            TenTran = "Chung kết - Nhóm B",
                            DanhSachVdvIds = allVdvIds.Skip(half).ToList()
                        });
                        heatFixtures.Add(new HeatFixture
                        {
                            FixtureId = fixtureSeq++,
                            VongTen = "Chung kết",
                            VongThuTu = 1,
                            TenTran = "Chung kết - Nhóm A",
                            DanhSachVdvIds = allVdvIds.Take(half).ToList()
                        });
                    }
                    else
                    {
                        heatFixtures.Add(new HeatFixture
                        {
                            FixtureId = fixtureSeq++,
                            VongTen = "Chung kết",
                            VongThuTu = 1,
                            TenTran = "Chung kết - Lượt thi đấu",
                            DanhSachVdvIds = allVdvIds
                        });
                    }
                }
                else if (isArtisticSport)
                {
                    // Môn biểu diễn chấm điểm (Võ quyền, Thể dục dụng cụ): 1 buổi thi chung kết theo thứ tự bốc thăm
                    heatFixtures.Add(new HeatFixture
                    {
                        FixtureId = fixtureSeq++,
                        VongTen = "Chung kết",
                        VongThuTu = 1,
                        TenTran = "Chung kết - Lượt thi biểu diễn xếp hạng",
                        DanhSachVdvIds = allVdvIds
                    });
                }
                else
                {
                    // Đua chia làn (Bơi lội, Điền kinh chạy ngắn): Chia theo làn và có thể có nhiều vòng
                    HeatFixture? finalHeat = null;
                    if (soVong > 1)
                    {
                        finalHeat = new HeatFixture
                        {
                            FixtureId = fixtureSeq++,
                            VongTen = "Chung kết",
                            VongThuTu = soVong,
                            TenTran = "Chung kết - Lượt thi quyết định (Top hạt giống)",
                            DanhSachVdvIds = new List<int>() // Chờ các VĐV đạt chuẩn từ các lượt vòng loại
                        };
                    }

                    for (int vong = 1; vong <= (soVong > 1 ? soVong - 1 : 1); vong++)
                    {
                        string vongTen = soVong == 1 ? "Chung kết" : (soVong == 2 ? "Vòng loại" : $"Vòng Sơ loại {vong}");
                        int luot = 1;
                        for (int i = 0; i < allVdvIds.Count; i += heatSize)
                        {
                            var batch = allVdvIds.Skip(i).Take(heatSize).ToList();
                            heatFixtures.Add(new HeatFixture
                            {
                                FixtureId = fixtureSeq++,
                                VongTen = vongTen,
                                VongThuTu = vong,
                                TenTran = $"{vongTen} - Lượt {luot++}",
                                DanhSachVdvIds = batch,
                                NextFixtureId = finalHeat?.FixtureId
                            });
                        }
                    }

                    if (finalHeat != null)
                    {
                        heatFixtures.Add(finalHeat);
                    }
                }
            }
            else
            {
                // Dự phòng: các thể thức khác (HeThuySi...) → vòng tròn không chia bảng
                var teamIds = dangKyList.Select(d => d.Id).ToList();
                var allRounds = GenerateRoundRobin(teamIds);
                for (int r = 0; r < allRounds.Count; r++)
                {
                    string vongTen = $"Lượt {r + 1}";
                    foreach (var pair in allRounds[r])
                    {
                        fixtures.Add(new MatchFixture
                        {
                            FixtureId = fixtureSeq++,
                            VongTen = vongTen,
                            VongThuTu = r + 1,
                            BangDauId = null,
                            Doi1DangKyId = pair.Item1,
                            Doi2DangKyId = pair.Item2,
                            TenTran = $"Trận {fixtures.Count + 1}: {vongTen}"
                        });
                    }
                }
            }

            // ===== Xử LÝ LEADERBOARD HEAT FIXTURES trước khi kiểm tra fixtures.Any() =====
            if (heatFixtures.Any())
            {
                foreach (var hf in heatFixtures)
                {
                    fixtures.Add(new MatchFixture
                    {
                        FixtureId = hf.FixtureId,
                        VongTen = hf.VongTen,
                        VongThuTu = hf.VongThuTu,
                        BangDauId = null,
                        Doi1DangKyId = hf.DanhSachVdvIds.FirstOrDefault(),
                        Doi2DangKyId = 0,
                        TenTran = hf.TenTran,
                        GhiChu = hf.DanhSachVdvIds.Count > 0 ? $"Heat: {hf.DanhSachVdvIds.Count} VĐV thi đấu" : "Chung kết: Chờ Top VĐV xuất sắc từ vòng loại",
                        IsHeat = true,
                        HeatVdvIds = hf.DanhSachVdvIds,
                        NextFixtureId = hf.NextFixtureId
                    });
                }
            }

            if (!fixtures.Any())
            {
                result.Success = false;
                result.Message = "Không có cặp đấu nào được sinh ra. Vui lòng kiểm tra lại số lượng đội hoặc bảng đấu.";
                return result;
            }

            // 7. Tạo hoặc lấy các VongDau tương ứng
            var distinctVongs = fixtures.Select(f => new { f.VongTen, f.VongThuTu }).Distinct().ToList();
            var vongDauMap = new Dictionary<string, VongDau>();

            foreach (var v in distinctVongs)
            {
                var existingVong = (await _unitOfWork.VongDaus.FindAsync(
                    x => x.GiaiDauMonTheThaoId == request.GiaiDauMonTheThaoId && x.Ten == v.VongTen && x.IsDeleted != true
                )).FirstOrDefault();

                if (existingVong == null)
                {
                    string loaiVong = "LoaiTrucTiep";
                    if (v.VongTen.Contains("Vòng bảng")) loaiVong = "VongBang";
                    else if (v.VongTen.Contains("Vòng loại") || v.VongTen.Contains("Sơ loại")) loaiVong = "VongBang";
                    else if (effectiveHinhThuc == HinhThucThiDau.VongBang) loaiVong = "VongBang";
                    else if (v.VongTen.Contains("Lượt") || v.VongTen.Contains("Vòng tròn") || v.VongTen.StartsWith("Vòng ") || v.VongTen.StartsWith("Bảng ")) loaiVong = "VongBang";
                    else if (v.VongTen == "Tứ kết") loaiVong = "TuKet";
                    else if (v.VongTen == "Bán kết") loaiVong = "BanKet";
                    else if (v.VongTen == "Chung kết") loaiVong = "ChungKet";
                    else if (v.VongTen.Contains("Tranh hạng")) loaiVong = "TranhHangBa";

                    existingVong = new VongDau
                    {
                        GiaiDauMonTheThaoId = request.GiaiDauMonTheThaoId,
                        Ten = v.VongTen,
                        ThuTu = v.VongThuTu,
                        LoaiVong = loaiVong,
                        Created = DateTime.UtcNow,
                        CreatedBy = createdBy,
                        IsDeleted = false
                    };
                    await _unitOfWork.VongDaus.AddAsync(existingVong);
                    await _unitOfWork.CompleteAsync();
                }

                vongDauMap[v.VongTen] = existingVong;
            }

            // 8. Thuật toán CSP Engine 2 Tầng: Macro-Scheduler & Micro Slot Scanner
            int monTheThaoId = noiDung.MonTheThaoId;
            CauHinhLichThiDau? cauHinh = null;
            if (monTheThaoId > 0)
            {
                var chItems = await _unitOfWork.CauHinhLichThiDaus.FindAsync(c => c.MonTheThaoId == monTheThaoId && c.IsDeleted != true);
                cauHinh = chItems.FirstOrDefault();
            }

            // Merge tham số override từ Modal request với cấu hình mặc định DB của Môn thể thao (CauHinhLichThiDau)
            int effSoTranToiDaMoiDoiMoiNgay = request.SoTranToiDaMoiDoiMoiNgay.HasValue && request.SoTranToiDaMoiDoiMoiNgay.Value > 0 
                ? request.SoTranToiDaMoiDoiMoiNgay.Value 
                : (cauHinh?.SoTranToiDaMoiDoiMoiNgay ?? 1);
            bool effMoiVongMotNgay = request.MoiVongMotNgay ?? (cauHinh?.MoiVongMotNgay ?? true);
            bool effUuTienChungKetNgayCuoi = request.UuTienChungKetNgayCuoi ?? (cauHinh?.UuTienChungKetNgayCuoi ?? true);
            bool effChiaCaThiDau = request.ChiaCaThiDau ?? (cauHinh?.ChiaCaThiDau ?? true);
            int effThoiGianDemDiChuyenPhut = request.ThoiGianDemDiChuyenPhut.HasValue && request.ThoiGianDemDiChuyenPhut.Value >= 0 
                ? request.ThoiGianDemDiChuyenPhut.Value 
                : (cauHinh?.ThoiGianDemDiChuyenPhut ?? 30);
            int effMaxTTPerDay = request.SoTranToiDaMoiTrongTaiMoiNgay.HasValue && request.SoTranToiDaMoiTrongTaiMoiNgay.Value > 0 
                ? request.SoTranToiDaMoiTrongTaiMoiNgay.Value 
                : (cauHinh?.SoTranToiDaMoiTrongTaiMoiNgay ?? 4);
            int effThoiGianDemDonSanPhut = request.ThoiGianDemDonSanPhut.HasValue && request.ThoiGianDemDonSanPhut.Value >= 0 
                ? request.ThoiGianDemDonSanPhut.Value 
                : (cauHinh?.ThoiGianDemDonSanPhut ?? 15);
            int effNghiToiThieuTrongTaiPhut = cauHinh?.NghiToiThieuTrongTaiPhut ?? 15;

            // Helper an toàn để parse chuỗi thời gian "hh:mm"
            static TimeSpan ParseTime(string? str, string defaultVal)
            {
                if (string.IsNullOrWhiteSpace(str)) str = defaultVal;
                if (TimeSpan.TryParseExact(str, new[] { "h\\:mm", "hh\\:mm", "H\\:mm", "HH\\:mm" }, CultureInfo.InvariantCulture, out var ts))
                    return ts;
                if (TimeSpan.TryParse(str, out var ts2))
                    return ts2;
                return TimeSpan.Parse(defaultVal, CultureInfo.InvariantCulture);
            }

            var activeShifts = new List<(string Name, TimeSpan Start, TimeSpan End)>();

            // 1. Ca Sáng
            if (request.ApDungCaSang)
            {
                string sStartStr = !string.IsNullOrWhiteSpace(request.GioBatDauCaSang) 
                    ? request.GioBatDauCaSang 
                    : (!string.IsNullOrWhiteSpace(request.GioBatDauMoiNgay) ? request.GioBatDauMoiNgay : (cauHinh?.CaSangBatDau ?? "08:00"));
                string sEndStr = !string.IsNullOrWhiteSpace(request.GioKetThucCaSang) 
                    ? request.GioKetThucCaSang 
                    : (cauHinh?.CaSangKetThuc ?? "11:30");
                var sStart = ParseTime(sStartStr, "08:00");
                var sEnd = ParseTime(sEndStr, "11:30");
                if (sEnd > sStart)
                {
                    activeShifts.Add(("Ca Sáng", sStart, sEnd));
                }
            }

            // 2. Ca Chiều
            if (request.ApDungCaChieu)
            {
                string cStartStr = !string.IsNullOrWhiteSpace(request.GioBatDauCaChieu) 
                    ? request.GioBatDauCaChieu 
                    : (cauHinh?.CaChieuBatDau ?? "14:00");
                string cEndStr = !string.IsNullOrWhiteSpace(request.GioKetThucCaChieu) 
                    ? request.GioKetThucCaChieu 
                    : (!string.IsNullOrWhiteSpace(request.GioKetThucMoiNgay) ? request.GioKetThucMoiNgay : (cauHinh?.CaChieuKetThuc ?? "17:30"));
                var cStart = ParseTime(cStartStr, "14:00");
                var cEnd = ParseTime(cEndStr, "17:30");
                if (cEnd > cStart)
                {
                    activeShifts.Add(("Ca Chiều", cStart, cEnd));
                }
            }

            // 3. Ca Tối
            if (request.ApDungCaToi)
            {
                string tStartStr = !string.IsNullOrWhiteSpace(request.GioBatDauCaToi) 
                    ? request.GioBatDauCaToi 
                    : (cauHinh?.CaToBatDau ?? "18:00");
                string tEndStr = !string.IsNullOrWhiteSpace(request.GioKetThucCaToi) 
                    ? request.GioKetThucCaToi 
                    : (cauHinh?.CaToKetThuc ?? "21:30");
                var tStart = ParseTime(tStartStr, "18:00");
                var tEnd = ParseTime(tEndStr, "21:30");
                if (tEnd > tStart)
                {
                    activeShifts.Add(("Ca Tối", tStart, tEnd));
                }
            }

            // Fallback nếu không có ca nào được chọn (đảm bảo thuật toán luôn hoạt động an toàn)
            if (!activeShifts.Any())
            {
                activeShifts.Add(("Ca Sáng", ParseTime(request.GioBatDauMoiNgay, "08:00"), TimeSpan.FromHours(11.5)));
                activeShifts.Add(("Ca Chiều", TimeSpan.FromHours(14), ParseTime(request.GioKetThucMoiNgay, "17:30")));
            }

            activeShifts = activeShifts.OrderBy(s => s.Start).ToList();
            TimeSpan firstShiftStart = activeShifts.First().Start;

            int matchMinutes = request.ThoiLuongTranPhut > 0 ? request.ThoiLuongTranPhut : (cauHinh?.ThoiLuongTranMacDinhPhut > 0 ? cauHinh.ThoiLuongTranMacDinhPhut : 60);
            int breakMinutes = request.NghiGiuaTranPhut >= 0 ? request.NghiGiuaTranPhut : 15;
            TimeSpan slotDuration = TimeSpan.FromMinutes(matchMinutes + effThoiGianDemDonSanPhut);

            // Khoảng cách giữa các vòng thi đấu (giờ)
            int effKhoangCachVongPhut = (request.KhoangCachGiuaCacVongGio.HasValue && request.KhoangCachGiuaCacVongGio.Value >= 0 
                ? request.KhoangCachGiuaCacVongGio.Value 
                : (cauHinh?.KhoangCachGiuaCacVongGio ?? 12)) * 60;
            int roundGapMinutes = effKhoangCachVongPhut > 0 ? effKhoangCachVongPhut : breakMinutes;

            // Thời gian nghỉ hồi phục thể lực tối thiểu của VĐV giữa 2 trận liên tiếp (phút)
            int minRestMinutes = request.ThoiGianNghiToiThieuVdvPhut.HasValue && request.ThoiGianNghiToiThieuVdvPhut.Value > 0
                ? request.ThoiGianNghiToiThieuVdvPhut.Value
                : (cauHinh?.NghiToiThieuGiua2TranPhut > 0 ? cauHinh.NghiToiThieuGiua2TranPhut : 120);

            // --- MACRO-SCHEDULER: Phân bổ Vòng đấu vào Ngày thi đấu cụ thể ---
            var giaiDau = noiDung.GiaiDau;
            DateTime tournamentStart = request.NgayBatDau.Date;
            DateTime tournamentEnd = request.NgayKetThuc.HasValue 
                ? request.NgayKetThuc.Value.Date 
                : (giaiDau != null ? giaiDau.NgayKetThuc.Date : tournamentStart);
            if (tournamentEnd < tournamentStart) tournamentEnd = tournamentStart;

            int totalDaysAvailable = (int)(tournamentEnd - tournamentStart).TotalDays + 1;
            var distinctRoundOrders = fixtures.Select(f => f.VongThuTu).Distinct().OrderBy(x => x).ToList();

            // Tính định ngạch số trận tối đa mỗi ngày cho môn nếu bật chế độ phân bổ đều xuyên suốt
            int maxMatchesPerDayForSport = int.MaxValue;
            if (request.PhanBoDeuXuyenSuot)
            {
                if (request.SoTranToiDaMoiNgayCuaMon.HasValue && request.SoTranToiDaMoiNgayCuaMon.Value > 0)
                {
                    maxMatchesPerDayForSport = request.SoTranToiDaMoiNgayCuaMon.Value;
                }
                else if (totalDaysAvailable > 1 && fixtures.Count > 0)
                {
                    int minRecommendedPerDay = Math.Max(activeShifts.Count, Math.Min(fixtures.Count, activeShifts.Count * (sanDauList.Count > 0 ? sanDauList.Count : 1)));
                    maxMatchesPerDayForSport = Math.Max(minRecommendedPerDay, (int)Math.Ceiling((double)fixtures.Count / totalDaysAvailable));
                }
            }

            var roundTargetDate = new Dictionary<int, DateTime>();
            bool shouldSpreadRounds = (effMoiVongMotNgay || request.PhanBoDeuXuyenSuot) && totalDaysAvailable > 1 && distinctRoundOrders.Count > 1;
            if (shouldSpreadRounds)
            {
                for (int idx = 0; idx < distinctRoundOrders.Count; idx++)
                {
                    int rOrder = distinctRoundOrders[idx];
                    if (effUuTienChungKetNgayCuoi && idx == distinctRoundOrders.Count - 1)
                    {
                        roundTargetDate[rOrder] = tournamentEnd;
                    }
                    else
                    {
                        double fraction = (double)idx / Math.Max(1, distinctRoundOrders.Count - 1);
                        int dayOffset = (int)Math.Floor(fraction * (totalDaysAvailable - 1));
                        roundTargetDate[rOrder] = tournamentStart.AddDays(dayOffset);
                    }
                }
            }

            // Nạp danh sách VĐV của các fixture
            var allFixturesDkIds = fixtures.SelectMany(f => new[] { f.Doi1DangKyId, f.Doi2DangKyId }).Where(id => id > 0).Distinct().ToList();
            var fixtureVdvMap = await GetVdvsForDangKyListAsync(allFixturesDkIds);

            // Theo dõi lịch bận của Sân, VĐV (theo IdentityKey) và Trọng tài
            var courtBusy = new Dictionary<int, List<(DateTime start, DateTime end)>>();
            var vdvBusy = new Dictionary<string, List<(DateTime start, DateTime end)>>();
            var refereeBusy = new Dictionary<int, List<(DateTime start, DateTime end)>>();

            // Thống kê tải
            var courtMatchCount = new Dictionary<int, int>();
            var refereeMatchCount = new Dictionary<int, int>();
            var refereeMatchCountPerDay = new Dictionary<int, Dictionary<DateTime, int>>();
            var doiMatchCountPerDay = new Dictionary<int, Dictionary<DateTime, int>>();
            var monMatchCountPerDay = new Dictionary<DateTime, int>();

            foreach (var s in sanDauList)
            {
                courtBusy[s.Id] = new List<(DateTime start, DateTime end)>();
                courtMatchCount[s.Id] = 0;
            }
            foreach (var tt in trongTaiList)
            {
                refereeBusy[tt.Id] = new List<(DateTime start, DateTime end)>();
                refereeMatchCount[tt.Id] = 0;
                refereeMatchCountPerDay[tt.Id] = new Dictionary<DateTime, int>();
            }

            // Nạp các trận đấu đang có trong giải đấu để tránh trùng với lịch môn khác
            int? currentGiaiDauId = noiDung.GiaiDauId;
            var existingMatches = (await _unitOfWork.TranDaus.GetPagedAsync(
                1, 4000,
                predicate: t => t.IsDeleted != true &&
                                t.ThoiGianBatDau.HasValue && t.ThoiGianKetThuc.HasValue &&
                                t.ThoiGianBatDau.Value.Date >= tournamentStart.Date &&
                                (!currentGiaiDauId.HasValue || t.GiaiDauMonTheThao.GiaiDauId == currentGiaiDauId.Value),
                orderBy: null,
                t => t.ThanhPhanTranDaus,
                t => t.PhanCongTrongTais
            )).Items.ToList();

            if (existingMatches.Any())
            {
                var existingDkIds = existingMatches.SelectMany(m => m.ThanhPhanTranDaus.Where(tp => tp.IsDeleted != true).Select(tp => tp.DangKyThiDauId)).Distinct().ToList();
                var existingVdvMap = await GetVdvsForDangKyListAsync(existingDkIds);

                foreach (var em in existingMatches)
                {
                    var emStart = em.ThoiGianBatDau!.Value;
                    var emEnd = em.ThoiGianKetThuc!.Value;

                    if (em.SanDauId.HasValue)
                    {
                        if (!courtBusy.ContainsKey(em.SanDauId.Value)) courtBusy[em.SanDauId.Value] = new List<(DateTime, DateTime)>();
                        courtBusy[em.SanDauId.Value].Add((emStart, emEnd));
                        courtMatchCount[em.SanDauId.Value] = courtMatchCount.GetValueOrDefault(em.SanDauId.Value, 0) + 1;
                    }

                    foreach (var pc in em.PhanCongTrongTais.Where(pc => pc.IsDeleted != true))
                    {
                        if (!refereeBusy.ContainsKey(pc.TrongTaiId)) refereeBusy[pc.TrongTaiId] = new List<(DateTime, DateTime)>();
                        refereeBusy[pc.TrongTaiId].Add((emStart, emEnd));
                        refereeMatchCount[pc.TrongTaiId] = refereeMatchCount.GetValueOrDefault(pc.TrongTaiId, 0) + 1;
                    }

                    var emDks = em.ThanhPhanTranDaus.Where(tp => tp.IsDeleted != true).Select(tp => tp.DangKyThiDauId);
                    foreach (var dkId in emDks)
                    {
                        string teamKey = $"team_{dkId}";
                        if (!vdvBusy.ContainsKey(teamKey)) vdvBusy[teamKey] = new List<(DateTime, DateTime)>();
                        vdvBusy[teamKey].Add((emStart, emEnd));

                        if (existingVdvMap.TryGetValue(dkId, out var vdvs))
                        {
                            foreach (var v in vdvs)
                            {
                                if (!vdvBusy.ContainsKey(v.IdentityKey)) vdvBusy[v.IdentityKey] = new List<(DateTime, DateTime)>();
                                vdvBusy[v.IdentityKey].Add((emStart, emEnd));
                            }
                        }
                    }
                }
            }

            // Sắp xếp fixtures theo thứ tự vòng đấu tăng dần
            fixtures = fixtures.OrderBy(f => f.VongThuTu).ToList();

            var currentMaxSoTran = (await _unitOfWork.TranDaus.FindAsync(t => t.GiaiDauMonTheThaoId == request.GiaiDauMonTheThaoId && t.IsDeleted != true))
                .Select(t => t.SoTran)
                .DefaultIfEmpty(0)
                .Max();
            int matchCounter = currentMaxSoTran + 1;
            var createdTranList = new List<TranDau>();
            var fixtureToTranDau = new Dictionary<int, TranDau>();

            var roundMaxEndTime = new Dictionary<int, DateTime>();
            var vdvLastEndTime = new Dictionary<string, DateTime>();
            var shiftMatchCountPerDay = new Dictionary<(DateTime date, int shiftIdx), int>();

            foreach (var f in fixtures)
            {
                var fVdvs = (fixtureVdvMap.GetValueOrDefault(f.Doi1DangKyId) ?? new List<VdvParticipantInfo>())
                    .Union(fixtureVdvMap.GetValueOrDefault(f.Doi2DangKyId) ?? new List<VdvParticipantInfo>())
                    .ToList();
                var fVdvKeys = fVdvs.Select(v => v.IdentityKey).Distinct().ToList();
                if (f.Doi1DangKyId > 0) fVdvKeys.Add($"team_{f.Doi1DangKyId}");
                if (f.Doi2DangKyId > 0) fVdvKeys.Add($"team_{f.Doi2DangKyId}");

                // --- Xác định earliestAllowed dựa trên Macro Target Date & Luân phiên ca thi đấu ---
                DateTime targetDay = roundTargetDate.TryGetValue(f.VongThuTu, out var targetDate) 
                    ? targetDate.Date 
                    : tournamentStart.Date;

                TimeSpan chosenShiftStart = firstShiftStart;

                // Luân phiên các ca thi đấu trong ngày (Sáng, Chiều, Tối) để các trận không bị dồn tất cả vào 8h sáng
                if (request.CheDoPhanBoKhungGio != "LienTiepTheoSan" && activeShifts.Count > 1)
                {
                    int bestShiftIdx = 0;
                    int minShiftMatches = int.MaxValue;
                    for (int sIdx = 0; sIdx < activeShifts.Count; sIdx++)
                    {
                        int cnt = shiftMatchCountPerDay.GetValueOrDefault((targetDay, sIdx), 0);
                        if (cnt < minShiftMatches)
                        {
                            minShiftMatches = cnt;
                            bestShiftIdx = sIdx;
                        }
                    }
                    chosenShiftStart = activeShifts[bestShiftIdx].Start;
                }

                DateTime earliestAllowed = targetDay.Add(chosenShiftStart);

                // Ràng buộc thứ tự vòng đấu (Round Precedence)
                if (f.VongThuTu > 0)
                {
                    var prevMaxEnd = roundMaxEndTime
                        .Where(kv => kv.Key < f.VongThuTu)
                        .Select(kv => kv.Value)
                        .DefaultIfEmpty(DateTime.MinValue)
                        .Max();
                    if (prevMaxEnd > DateTime.MinValue)
                    {
                        int restBuffer = roundGapMinutes;
                        var minStartAfterPrevRound = prevMaxEnd.AddMinutes(restBuffer);
                        if (minStartAfterPrevRound > earliestAllowed)
                        {
                            earliestAllowed = minStartAfterPrevRound;
                        }
                    }
                }

                // Nếu là vòng Knockout đầu tiên sau vòng bảng, áp dụng cấu hình số ngày nghỉ sau vòng bảng
                int daysRestAfterGroup = cauHinh?.SoNgayNghiSauVongBang ?? 1;
                bool isFirstKnockoutAfterGroup = f.BangDauId == null && fixtures.Any(x => x.BangDauId != null && x.VongThuTu < f.VongThuTu);
                if (isFirstKnockoutAfterGroup && daysRestAfterGroup > 0)
                {
                    var groupStageMaxEnd = roundMaxEndTime
                        .Where(kv => fixtures.Any(x => x.VongThuTu == kv.Key && x.BangDauId != null))
                        .Select(kv => kv.Value)
                        .DefaultIfEmpty(DateTime.MinValue)
                        .Max();
                    if (groupStageMaxEnd > DateTime.MinValue)
                    {
                        var minAfterGroupRest = groupStageMaxEnd.Date.AddDays(daysRestAfterGroup + 1).Add(firstShiftStart);
                        if (minAfterGroupRest > earliestAllowed)
                        {
                            earliestAllowed = minAfterGroupRest;
                        }
                    }
                }

                // Thời gian nghỉ của VĐV
                if (request.TranhTrungLichVdv && minRestMinutes > 0 && fVdvKeys.Any())
                {
                    var athleteRestEnd = fVdvKeys
                        .Where(k => vdvLastEndTime.ContainsKey(k))
                        .Select(k => vdvLastEndTime[k].AddMinutes(minRestMinutes))
                        .DefaultIfEmpty(DateTime.MinValue)
                        .Max();
                    if (athleteRestEnd > earliestAllowed)
                    {
                        earliestAllowed = athleteRestEnd;
                    }
                }

                // Thời lượng trận đấu thực tế (đối với Knockout cộng thêm thời gian đệm hiệp phụ / luân lưu để giữ sân an toàn)
                int effMatchMinutes = matchMinutes;
                bool isKnockout = f.BangDauId == null || f.VongTen.Contains("Knockout", StringComparison.OrdinalIgnoreCase)
                    || f.VongTen.Contains("Tứ kết", StringComparison.OrdinalIgnoreCase)
                    || f.VongTen.Contains("Bán kết", StringComparison.OrdinalIgnoreCase)
                    || f.VongTen.Contains("Chung kết", StringComparison.OrdinalIgnoreCase)
                    || f.VongTen.Contains("Vòng 1/", StringComparison.OrdinalIgnoreCase)
                    || f.VongTen.Contains("Tranh hạng", StringComparison.OrdinalIgnoreCase);

                if (isKnockout && (cauHinh?.ThoiGianDemHiepPhuLuonLuuPhut ?? 30) > 0)
                {
                    effMatchMinutes += (cauHinh?.ThoiGianDemHiepPhuLuonLuuPhut ?? 30);
                }

                DateTime searchSlot = earliestAllowed;
                bool scheduled = false;
                int attempts = 0;
                DateTime maxLookaheadDate = tournamentEnd.AddDays(14);

                while (!scheduled && searchSlot.Date <= maxLookaheadDate && attempts < 2000)
                {
                    attempts++;

                    // --- CHIA CA THI ĐẤU (THEO CÁC CA ĐÃ ĐƯỢC CHỌN: SÁNG / CHIỀU / TỐI) ---
                    var tod = searchSlot.TimeOfDay;
                    bool inValidShift = false;

                    for (int i = 0; i < activeShifts.Count; i++)
                    {
                        var shift = activeShifts[i];

                        // Nếu tod trước ca này, nhảy vào đầu ca
                        if (tod < shift.Start)
                        {
                            searchSlot = searchSlot.Date.Add(shift.Start);
                            tod = shift.Start;
                        }

                        // Kiểm tra xem trận đấu có thể vừa vặn trong ca này không
                        if (tod >= shift.Start && tod.Add(TimeSpan.FromMinutes(effMatchMinutes)) <= shift.End)
                        {
                            inValidShift = true;
                            break;
                        }

                        // Nếu tod vượt quá ca này hoặc không vừa trận đấu, vòng lặp tiếp tục xét ca kế tiếp trong ngày
                    }

                    if (!inValidShift)
                    {
                        // Nếu trong ngày không còn ca nào vừa cho trận đấu -> Nhảy sang ca đầu tiên của ngày hôm sau!
                        searchSlot = searchSlot.Date.AddDays(1).Add(firstShiftStart);
                        continue;
                    }

                    // --- RÀNG BUỘC PHÂN BỔ ĐỀU XUYÊN SUỐT CÁC NGÀY CHO MÔN ---
                    if (request.PhanBoDeuXuyenSuot && maxMatchesPerDayForSport < int.MaxValue && searchSlot.Date < tournamentEnd)
                    {
                        int currentDayMatches = monMatchCountPerDay.GetValueOrDefault(searchSlot.Date, 0);
                        if (currentDayMatches >= maxMatchesPerDayForSport)
                        {
                            // Ngày hiện tại đã đạt định ngạch số trận của môn, chuyển sang ngày tiếp theo để dàn đều
                            searchSlot = searchSlot.Date.AddDays(1).Add(firstShiftStart);
                            continue;
                        }
                    }

                    var matchStart = searchSlot;
                    var matchEnd = searchSlot.AddMinutes(effMatchMinutes);

                    // --- RÀNG BUỘC SỐ TRẬN TỐI ĐA MỖI ĐỘI MỖI NGÀY ---
                    bool teamExceededMaxPerDay = false;
                    foreach (var teamId in new[] { f.Doi1DangKyId, f.Doi2DangKyId })
                    {
                        if (teamId > 0)
                        {
                            if (!doiMatchCountPerDay.ContainsKey(teamId)) doiMatchCountPerDay[teamId] = new Dictionary<DateTime, int>();
                            int cnt = doiMatchCountPerDay[teamId].GetValueOrDefault(matchStart.Date, 0);
                            if (cnt >= effSoTranToiDaMoiDoiMoiNgay)
                            {
                                teamExceededMaxPerDay = true;
                                break;
                            }
                        }
                    }

                    if (teamExceededMaxPerDay)
                    {
                        // Đội đã đá đủ số trận trong ngày -> nhảy sang ngày hôm sau
                        searchSlot = searchSlot.Date.AddDays(1).Add(firstShiftStart);
                        continue;
                    }

                    // --- RÀNG BUỘC VĐV BẬN & BUFFER DI CHUYỂN MÔN KHÁC ---
                    bool hasVdvConflict = false;
                    if (request.TranhTrungLichVdv && fVdvKeys.Any())
                    {
                        foreach (var vKey in fVdvKeys)
                        {
                            if (vdvBusy.TryGetValue(vKey, out var vBusyList))
                            {
                                double bufferMin = vKey.StartsWith("team_") ? 0 : effThoiGianDemDiChuyenPhut;
                                var forbiddenStart = matchStart.AddMinutes(-bufferMin);
                                var forbiddenEnd = matchEnd.AddMinutes(bufferMin);

                                if (vBusyList.Any(iv => iv.start < forbiddenEnd && iv.end > forbiddenStart))
                                {
                                    hasVdvConflict = true;
                                    break;
                                }

                                if (minRestMinutes > 0)
                                {
                                    if (vBusyList.Any(iv => iv.start.Date == matchStart.Date &&
                                                            matchStart < iv.end.AddMinutes(minRestMinutes) &&
                                                            iv.start < matchEnd.AddMinutes(minRestMinutes)))
                                    {
                                        hasVdvConflict = true;
                                        break;
                                    }
                                }
                            }
                        }
                    }

                    if (hasVdvConflict)
                    {
                        searchSlot = searchSlot.Add(slotDuration);
                        continue;
                    }

                    // --- RÀNG BUỘC NĂNG LỰC TRỌNG TÀI TẠI KHUNG GIỜ ---
                    // Đảm bảo số trận đồng thời không vượt quá khả năng phân công của tổ trọng tài đã chọn
                    if (trongTaiList.Any() && request.SoTrongTaiMoiTran > 0)
                    {
                        int maxConcurrentByReferees = Math.Max(1, trongTaiList.Count / request.SoTrongTaiMoiTran);
                        int currentConcurrentMatches = courtBusy.Values.Count(list => list.Any(iv => iv.start < matchEnd && iv.end > matchStart));
                        if (currentConcurrentMatches >= maxConcurrentByReferees)
                        {
                            searchSlot = searchSlot.Add(slotDuration);
                            continue;
                        }
                    }

                    // --- RÀNG BUỘC SÂN ĐẤU & GOM TRẬN THEO SÂN (COURT CLUSTERING) ---
                    var availableCourts = new List<SanDau>();
                    if (sanDauList.Any())
                    {
                        foreach (var s in sanDauList)
                        {
                            if (!courtBusy.TryGetValue(s.Id, out var cBusyList) ||
                                !cBusyList.Any(iv => iv.start < matchEnd.AddMinutes(effThoiGianDemDonSanPhut) && iv.end > matchStart))
                            {
                                availableCourts.Add(s);
                            }
                        }

                        if (!availableCourts.Any())
                        {
                            searchSlot = searchSlot.Add(slotDuration);
                            continue;
                        }
                    }

                    SanDau? candSan = null;
                    if (availableCourts.Any())
                    {
                        if (request.CanBangTaiSanDau)
                        {
                            candSan = availableCourts.OrderBy(s => courtMatchCount.GetValueOrDefault(s.Id, 0)).First();
                        }
                        else
                        {
                            // Gom trận theo sân: ưu tiên sân đã có trận vừa kết thúc gần nhất trong cùng ngày để tổ trọng tài cắm chốt
                            candSan = availableCourts
                                .OrderByDescending(s =>
                                {
                                    if (courtBusy.TryGetValue(s.Id, out var bList) && bList.Any(iv => iv.end.Date == matchStart.Date && iv.end <= matchStart))
                                    {
                                        return bList.Where(iv => iv.end.Date == matchStart.Date && iv.end <= matchStart).Max(iv => iv.end);
                                    }
                                    return DateTime.MinValue;
                                })
                                .ThenBy(s => courtMatchCount.GetValueOrDefault(s.Id, 0))
                                .First();
                        }
                    }

                    // === TẠO TRẬN ĐẤU (BỞI QUẢN LÝ GIẢI) ===
                    var tran = new TranDau
                    {
                        GiaiDauMonTheThaoId = request.GiaiDauMonTheThaoId,
                        VongDauId = vongDauMap[f.VongTen].Id,
                        BangDauId = f.BangDauId,
                        SanDauId = candSan?.Id,
                        SoTran = matchCounter,
                        MaTranHienThi = $"M{matchCounter:D2}",
                        IsLichCoDinh = false,
                        TenTran = !string.IsNullOrWhiteSpace(f.TenTran) ? f.TenTran : $"Trận {matchCounter} - {f.VongTen}",
                        ThoiGianDuKien = matchStart,
                        ThoiGianBatDau = matchStart,
                        ThoiGianKetThuc = matchStart.AddMinutes(matchMinutes),
                        TrangThai = "ChuaDau",
                        GhiChu = !string.IsNullOrWhiteSpace(f.GhiChu)
                            ? f.GhiChu
                            : ((f.Doi1DangKyId == 0 || f.Doi2DangKyId == 0) ? "Chờ xác định đội thi đấu (TBD)" : null),
                        Created = DateTime.UtcNow,
                        CreatedBy = createdBy,
                        IsDeleted = false
                    };

                    await _unitOfWork.TranDaus.AddAsync(tran);
                    await _unitOfWork.CompleteAsync();

                    // Gán các đội / VĐV vào trận (ThanhPhanTranDau)
                    if (f.IsHeat && f.HeatVdvIds != null && f.HeatVdvIds.Count > 0)
                    {
                        bool isMassStartHeat = (theThuc?.HinhThucXuatPhat == "DongLoat");
                        for (int lane = 0; lane < f.HeatVdvIds.Count; lane++)
                        {
                            await _unitOfWork.ThanhPhanTranDaus.AddAsync(new ThanhPhanTranDau
                            {
                                TranDauId = tran.Id,
                                DangKyThiDauId = f.HeatVdvIds[lane],
                                SoLane = isMassStartHeat ? null : (lane + 1), // Nếu xuất phát đồng loạt thì không gán làn
                                ThuTuThiDau = lane + 1,
                                SoDeoBIB = (lane + 1).ToString("D3"),
                                ViTri = lane + 1,
                                TrangThai = "ThamGia",
                                Created = DateTime.UtcNow,
                                CreatedBy = createdBy,
                                IsDeleted = false
                            });
                        }

                        // Cập nhật tracking busy cho tất cả VĐV trong Heat
                        foreach (var hVdvId in f.HeatVdvIds)
                        {
                            string teamKey = $"team_{hVdvId}";
                            if (!vdvBusy.ContainsKey(teamKey)) vdvBusy[teamKey] = new List<(DateTime, DateTime)>();
                            vdvBusy[teamKey].Add((matchStart, matchEnd));
                            vdvLastEndTime[teamKey] = matchEnd;

                            if (!doiMatchCountPerDay.ContainsKey(hVdvId)) doiMatchCountPerDay[hVdvId] = new Dictionary<DateTime, int>();
                            doiMatchCountPerDay[hVdvId][matchStart.Date] = doiMatchCountPerDay[hVdvId].GetValueOrDefault(matchStart.Date, 0) + 1;
                        }
                    }
                    else
                    {
                        // ==== MATCH THÔNG THƯỜNG: 2 đội ====
                        if (f.Doi1DangKyId > 0)
                        {
                            await _unitOfWork.ThanhPhanTranDaus.AddAsync(new ThanhPhanTranDau
                            {
                                TranDauId = tran.Id,
                                DangKyThiDauId = f.Doi1DangKyId,
                                ViTri = 1,
                                TrangThai = "ThamGia",
                                Created = DateTime.UtcNow,
                                CreatedBy = createdBy,
                                IsDeleted = false
                            });
                        }

                        if (f.Doi2DangKyId > 0)
                        {
                            await _unitOfWork.ThanhPhanTranDaus.AddAsync(new ThanhPhanTranDau
                            {
                                TranDauId = tran.Id,
                                DangKyThiDauId = f.Doi2DangKyId,
                                ViTri = 2,
                                TrangThai = "ThamGia",
                                Created = DateTime.UtcNow,
                                CreatedBy = createdBy,
                                IsDeleted = false
                            });
                        }
                    }

                    // Tự động tạo HiepDau nếu được cấu hình
                    int effSoHiepDau = request.SoHiepDau.HasValue && request.SoHiepDau.Value > 0
                        ? request.SoHiepDau.Value
                        : (cauHinh?.SoHiepDauMacDinh ?? 0);

                    if (effSoHiepDau > 0)
                    {
                        int configuredHiepPhut = request.ThoiGianMoiHiepPhut.HasValue && request.ThoiGianMoiHiepPhut.Value > 0
                            ? request.ThoiGianMoiHiepPhut.Value
                            : (cauHinh?.ThoiGianMoiHiepPhut ?? 0);

                        int hiepPhut = configuredHiepPhut > 0 ? configuredHiepPhut : (matchMinutes / effSoHiepDau);
                        for (int h = 1; h <= effSoHiepDau; h++)
                        {
                            var hiepStart = matchStart.AddMinutes((h - 1) * hiepPhut);
                            var hiepEnd = matchStart.AddMinutes(h * hiepPhut);
                            await _unitOfWork.HiepDaus.AddAsync(new HiepDau
                            {
                                TranDauId = tran.Id,
                                SoHiep = h,
                                ThoiGianBatDau = hiepStart,
                                ThoiGianKetThuc = hiepEnd,
                                TrangThai = "ChuaDau",
                                Created = DateTime.UtcNow,
                                CreatedBy = createdBy,
                                IsDeleted = false
                            });
                        }
                    }

                    await _unitOfWork.CompleteAsync();

                    // Cập nhật timeline bận sân đấu (bao gồm đệm hiệp phụ nếu knockout)
                    int candSanId = candSan?.Id ?? 0;
                    if (candSanId > 0)
                    {
                        if (!courtBusy.ContainsKey(candSanId)) courtBusy[candSanId] = new List<(DateTime, DateTime)>();
                        courtBusy[candSanId].Add((matchStart, matchEnd));
                        courtMatchCount[candSanId] = courtMatchCount.GetValueOrDefault(candSanId, 0) + 1;
                    }

                    // Cập nhật tracking đội / VĐV
                    if (!f.IsHeat)
                    {
                        foreach (var teamId in new[] { f.Doi1DangKyId, f.Doi2DangKyId })
                        {
                            if (teamId > 0)
                            {
                                if (!doiMatchCountPerDay.ContainsKey(teamId)) doiMatchCountPerDay[teamId] = new Dictionary<DateTime, int>();
                                doiMatchCountPerDay[teamId][matchStart.Date] = doiMatchCountPerDay[teamId].GetValueOrDefault(matchStart.Date, 0) + 1;
                            }
                        }

                        foreach (var vKey in fVdvKeys)
                        {
                            if (!vdvBusy.ContainsKey(vKey)) vdvBusy[vKey] = new List<(DateTime, DateTime)>();
                            vdvBusy[vKey].Add((matchStart, matchEnd));
                            vdvLastEndTime[vKey] = matchEnd;
                        }
                    }

                    if (!roundMaxEndTime.ContainsKey(f.VongThuTu) || matchEnd > roundMaxEndTime[f.VongThuTu])
                    {
                        roundMaxEndTime[f.VongThuTu] = matchEnd;
                    }

                    createdTranList.Add(tran);
                    monMatchCountPerDay[matchStart.Date] = monMatchCountPerDay.GetValueOrDefault(matchStart.Date, 0) + 1;
                    for (int sIdx = 0; sIdx < activeShifts.Count; sIdx++)
                    {
                        var sh = activeShifts[sIdx];
                        if (matchStart.TimeOfDay >= sh.Start && matchStart.TimeOfDay < sh.End)
                        {
                            shiftMatchCountPerDay[(matchStart.Date, sIdx)] = shiftMatchCountPerDay.GetValueOrDefault((matchStart.Date, sIdx), 0) + 1;
                            break;
                        }
                    }
                    if (f.FixtureId > 0) fixtureToTranDau[f.FixtureId] = tran;
                    matchCounter++;
                    scheduled = true;
                    break;
                }

                if (!scheduled)
                {
                    result.Warnings.Add($"Không thể tìm được khung giờ trống phù hợp cho cặp đấu: {f.TenTran} do xung đột lịch sân, trọng tài hoặc VĐV.");
                }
            }

            // Cập nhật liên kết DAG nhánh thi đấu Knockout / Heat giữa các TranDau trong CSDL
            bool hasBracketUpdates = false;
            foreach (var f in fixtures.Where(x => x.FixtureId > 0))
            {
                if (fixtureToTranDau.TryGetValue(f.FixtureId, out var currentTran))
                {
                    bool matchModified = false;
                    if (f.NextFixtureId.HasValue && fixtureToTranDau.TryGetValue(f.NextFixtureId.Value, out var nextTran))
                    {
                        currentTran.NextTranDauId = nextTran.Id;
                        currentTran.NextTranDauViTri = f.NextSlot;
                        matchModified = true;
                    }
                    if (f.LoserNextFixtureId.HasValue && fixtureToTranDau.TryGetValue(f.LoserNextFixtureId.Value, out var loserNextTran))
                    {
                        currentTran.LoserNextTranDauId = loserNextTran.Id;
                        currentTran.LoserNextTranDauViTri = f.LoserNextSlot;
                        matchModified = true;
                    }
                    if (!string.IsNullOrEmpty(f.MaTranBracket))
                    {
                        currentTran.MaTranBracket = f.MaTranBracket;
                        matchModified = true;
                    }
                    if (matchModified)
                    {
                        _unitOfWork.TranDaus.Update(currentTran);
                        hasBracketUpdates = true;
                    }
                }
            }
            if (hasBracketUpdates)
            {
                await _unitOfWork.CompleteAsync();
            }

            // Thống kê phân bổ tải sân đấu & trọng tài
            result.ThongKeSanDau = sanDauList.ToDictionary(
                s => s.Ten ?? $"Sân {s.Id}",
                s => courtMatchCount.GetValueOrDefault(s.Id, 0)
            );
            result.ThongKeTrongTai = trongTaiList.ToDictionary(
                tt => tt.HoTen,
                tt => refereeMatchCount.GetValueOrDefault(tt.Id, 0)
            );
            result.SoNgayThiDau = createdTranList.Where(t => t.ThoiGianBatDau.HasValue)
                .Select(t => t.ThoiGianBatDau!.Value.Date)
                .Distinct()
                .Count();
            if (result.SoNgayThiDau == 0) result.SoNgayThiDau = 1;

            var allMatches = (await GetAllAsync(giaiDauMonTheThaoId: request.GiaiDauMonTheThaoId)).ToList();
            result.Success = true;
            result.TotalMatchesCreated = createdTranList.Count;
            result.Message = $"Đã tự động xếp thành công {createdTranList.Count} trận đấu qua thuật toán phân bổ thông minh!";
            result.Matches = allMatches;

            return result;
        }

        /// <summary>
        /// Phân bổ lại khung giờ thi đấu linh hoạt cho các trận hiện có (Sáng, Chiều, Tối: 08:00, 09:30, 14:00, 15:30...)
        /// mà không làm mất hoặc thay đổi các cặp đấu đã bốc thăm.
        /// </summary>
        /// <param name="request">Thông tin cấu hình ca, thời lượng trận, thời gian nghỉ và chế độ phân bổ</param>
        /// <param name="updatedBy">Tài khoản quản lý thực hiện thao tác</param>
        /// <returns>Kết quả phân bổ lại thời gian các trận đấu</returns>
        public async Task<AutoScheduleResultDto> DistributeMatchTimesAsync(DistributeMatchTimesRequestDto request, string? updatedBy = null)
        {
            var result = new AutoScheduleResultDto();

            // 1. Lấy thông tin GiaiDauMonTheThao
            var noiDung = (await _unitOfWork.GiaiDauMonTheThaos.GetPagedAsync(
                1, 1,
                predicate: n => n.Id == request.GiaiDauMonTheThaoId && n.IsDeleted != true,
                orderBy: null,
                n => n.MonTheThao,
                n => n.GiaiDau
            )).Items.FirstOrDefault();

            if (noiDung == null)
            {
                result.Success = false;
                result.Message = "Không tìm thấy thông tin môn thi đấu trong giải.";
                return result;
            }

            // 2. Lấy danh sách trận đấu hiện có
            var matches = (await _unitOfWork.TranDaus.GetPagedAsync(
                1, 2000,
                predicate: t => t.GiaiDauMonTheThaoId == request.GiaiDauMonTheThaoId && t.IsDeleted != true &&
                                (!request.ChiTranChuaDau || t.TrangThai == "ChuaDau"),
                orderBy: q => q.OrderBy(t => t.VongDau.ThuTu).ThenBy(t => t.BangDauId).ThenBy(t => t.SoTran),
                t => t.VongDau,
                t => t.BangDau,
                t => t.SanDau,
                t => t.ThanhPhanTranDaus
            )).Items.ToList();

            if (!matches.Any())
            {
                result.Success = false;
                result.Message = "Không có trận đấu nào cần phân bổ lại khung giờ.";
                return result;
            }

            // 3. Lấy danh sách sân đấu khả dụng cho môn
            int? monId = noiDung.MonTheThaoId;
            var sanDauList = (await _unitOfWork.SanDaus.FindAsync(s => (!monId.HasValue || s.MonTheThaoId == monId.Value) && s.TrangThai && s.IsDeleted != true)).ToList();
            if (!sanDauList.Any())
            {
                sanDauList = (await _unitOfWork.SanDaus.FindAsync(s => s.TrangThai && s.IsDeleted != true)).Take(4).ToList();
            }

            // 4. Cấu hình các ca thi đấu (activeShifts)
            static TimeSpan ParseTime(string? str, string defaultVal)
            {
                if (string.IsNullOrWhiteSpace(str)) str = defaultVal;
                if (TimeSpan.TryParseExact(str, new[] { "h\\:mm", "hh\\:mm", "H\\:mm", "HH\\:mm" }, CultureInfo.InvariantCulture, out var ts))
                    return ts;
                if (TimeSpan.TryParse(str, out var ts2))
                    return ts2;
                return TimeSpan.Parse(defaultVal, CultureInfo.InvariantCulture);
            }

            var activeShifts = new List<(string Name, TimeSpan Start, TimeSpan End)>();
            if (request.ApDungCaSang)
            {
                var sStart = ParseTime(request.GioBatDauCaSang, "08:00");
                var sEnd = ParseTime(request.GioKetThucCaSang, "11:30");
                if (sEnd > sStart) activeShifts.Add(("Ca Sáng", sStart, sEnd));
            }
            if (request.ApDungCaChieu)
            {
                var cStart = ParseTime(request.GioBatDauCaChieu, "14:00");
                var cEnd = ParseTime(request.GioKetThucCaChieu, "17:30");
                if (cEnd > cStart) activeShifts.Add(("Ca Chiều", cStart, cEnd));
            }
            if (request.ApDungCaToi)
            {
                var tStart = ParseTime(request.GioBatDauCaToi, "18:00");
                var tEnd = ParseTime(request.GioKetThucCaToi, "21:30");
                if (tEnd > tStart) activeShifts.Add(("Ca Tối", tStart, tEnd));
            }

            if (!activeShifts.Any())
            {
                activeShifts.Add(("Ca Sáng", TimeSpan.FromHours(8), TimeSpan.FromHours(11.5)));
                activeShifts.Add(("Ca Chiều", TimeSpan.FromHours(14), TimeSpan.FromHours(17.5)));
            }
            activeShifts = activeShifts.OrderBy(s => s.Start).ToList();

            int matchMinutes = request.ThoiLuongTranPhut > 0 ? request.ThoiLuongTranPhut : 60;
            int breakMinutes = request.NghiGiuaTranPhut >= 0 ? request.NghiGiuaTranPhut : 15;
            TimeSpan slotStep = TimeSpan.FromMinutes(matchMinutes + breakMinutes);

            // 5. Xác định khoảng ngày thi đấu
            var distinctDates = matches.Where(m => m.ThoiGianBatDau.HasValue)
                .Select(m => m.ThoiGianBatDau!.Value.Date)
                .Distinct()
                .OrderBy(d => d)
                .ToList();

            DateTime baseStartDate = distinctDates.Any() ? distinctDates.First() : (noiDung.GiaiDau?.NgayBatDau.Date ?? DateTime.Today);
            DateTime baseEndDate = distinctDates.Any() ? distinctDates.Last() : (noiDung.GiaiDau?.NgayKetThuc.Date ?? baseStartDate);
            if (baseEndDate < baseStartDate) baseEndDate = baseStartDate;

            int totalDays = (int)(baseEndDate - baseStartDate).TotalDays + 1;
            var availableDates = Enumerable.Range(0, totalDays).Select(i => baseStartDate.AddDays(i)).ToList();

            // Sinh danh sách các khung giờ hợp lệ trong 1 ngày theo các ca
            var dailySlots = new List<TimeSpan>();
            foreach (var shift in activeShifts)
            {
                var cur = shift.Start;
                while (cur.Add(TimeSpan.FromMinutes(matchMinutes)) <= shift.End)
                {
                    dailySlots.Add(cur);
                    cur = cur.Add(slotStep);
                }
            }
            if (!dailySlots.Any())
            {
                dailySlots.Add(activeShifts.First().Start);
            }

            // 6. Phân bổ các trận vào khung giờ và sân đấu
            int updatedCount = 0;
            int slotIdx = 0;
            int courtIdx = 0;

            // Nhóm trận theo vòng để giữ tính tuần tự thể thao
            var matchesByRound = matches.GroupBy(m => m.VongDau?.ThuTu ?? 1).OrderBy(g => g.Key).ToList();
            int daysPerRound = Math.Max(1, availableDates.Count / Math.Max(1, matchesByRound.Count));

            int currentRoundDayBase = 0;

            foreach (var roundGroup in matchesByRound)
            {
                var roundMatches = roundGroup.ToList();
                int dayOffset = 0;

                for (int mIdx = 0; mIdx < roundMatches.Count; mIdx++)
                {
                    var m = roundMatches[mIdx];
                    int dayIndex = Math.Min(availableDates.Count - 1, currentRoundDayBase + dayOffset);
                    var curDate = availableDates[dayIndex];

                    // Chọn sân đấu
                    SanDau? assignedCourt = sanDauList.Any() ? sanDauList[courtIdx % sanDauList.Count] : null;

                    // Chọn khung giờ trong ngày
                    var chosenTod = dailySlots[slotIdx % dailySlots.Count];
                    var newStart = curDate.Add(chosenTod);
                    var newEnd = newStart.AddMinutes(matchMinutes);

                    m.ThoiGianDuKien = newStart;
                    m.ThoiGianBatDau = newStart;
                    m.ThoiGianKetThuc = newEnd;
                    if (assignedCourt != null)
                    {
                        m.SanDauId = assignedCourt.Id;
                    }
                    m.LastModified = DateTime.UtcNow;
                    m.LastModifiedBy = updatedBy;

                    updatedCount++;

                    // Luân chuyển slot và sân
                    courtIdx++;
                    if (request.CheDoPhanBo == "LuanPhienCa")
                    {
                        // Luân chuyển khung giờ liên tục: 8h, 9h30, 14h, 15h30...
                        slotIdx++;
                        if (slotIdx >= dailySlots.Count)
                        {
                            slotIdx = 0;
                            dayOffset = (dayOffset + 1) % Math.Max(1, daysPerRound);
                        }
                    }
                    else
                    {
                        // Xếp nối tiếp theo từng sân
                        if (courtIdx % Math.Max(1, sanDauList.Count) == 0)
                        {
                            slotIdx++;
                            if (slotIdx >= dailySlots.Count)
                            {
                                slotIdx = 0;
                                dayOffset = (dayOffset + 1) % Math.Max(1, daysPerRound);
                            }
                        }
                    }
                }

                currentRoundDayBase = Math.Min(availableDates.Count - 1, currentRoundDayBase + daysPerRound);
            }

            await _unitOfWork.CompleteAsync();

            result.Success = true;
            result.TotalMatchesCreated = updatedCount;
            result.Message = $"Đã phân bổ lại khung giờ thành công cho {updatedCount} trận đấu theo các ca Sáng, Chiều, Tối linh hoạt ({string.Join(", ", dailySlots.Select(ts => ts.ToString(@"hh\:mm")))}).";
            return result;
        }

        public async Task<TournamentConflictReportDto> CheckAllConflictsAsync(int giaiDauId)
        {
            var report = new TournamentConflictReportDto
            {
                GiaiDauId = giaiDauId
            };

            var giaiDau = await _unitOfWork.GiaiDaus.GetByIdAsync(giaiDauId);
            report.TenGiaiDau = giaiDau?.Ten;

            var matches = (await _unitOfWork.TranDaus.GetPagedAsync(
                1, 4000,
                predicate: t => t.IsDeleted != true &&
                                t.GiaiDauMonTheThao.GiaiDauId == giaiDauId &&
                                t.ThoiGianBatDau.HasValue && t.ThoiGianKetThuc.HasValue,
                orderBy: q => q.OrderBy(t => t.ThoiGianBatDau),
                t => t.GiaiDauMonTheThao,
                t => t.GiaiDauMonTheThao.MonTheThao,
                t => t.SanDau!,
                t => t.ThanhPhanTranDaus,
                t => t.PhanCongTrongTais
            )).Items.ToList();

            report.TotalMatchesChecked = matches.Count;
            if (matches.Count < 2) return report;

            var allDkIds = matches.SelectMany(m => m.ThanhPhanTranDaus.Where(tp => tp.IsDeleted != true).Select(tp => tp.DangKyThiDauId)).Distinct().ToList();
            var vdvMap = await GetVdvsForDangKyListAsync(allDkIds);

            var detectedKeys = new HashSet<string>();

            for (int i = 0; i < matches.Count; i++)
            {
                var m1 = matches[i];
                var m1Start = m1.ThoiGianBatDau!.Value;
                var m1End = m1.ThoiGianKetThuc!.Value;

                var m1DkIds = m1.ThanhPhanTranDaus.Where(tp => tp.IsDeleted != true).Select(tp => tp.DangKyThiDauId).ToList();
                var m1Vdvs = m1DkIds.SelectMany(dkId => vdvMap.GetValueOrDefault(dkId) ?? new List<VdvParticipantInfo>()).ToList();
                var m1TtIds = m1.PhanCongTrongTais.Where(pc => pc.IsDeleted != true).Select(pc => pc.TrongTaiId).ToHashSet();

                for (int j = i + 1; j < matches.Count; j++)
                {
                    var m2 = matches[j];
                    var m2Start = m2.ThoiGianBatDau!.Value;
                    var m2End = m2.ThoiGianKetThuc!.Value;

                    if (m2Start >= m1End) break;

                    if (m1Start < m2End && m1End > m2Start)
                    {
                        string m1Name = m1.TenTran ?? $"Trận #{m1.SoTran}";
                        string m2Name = m2.TenTran ?? $"Trận #{m2.SoTran}";
                        string m1Mon = m1.GiaiDauMonTheThao?.MonTheThao?.Ten ?? "Thể thao";
                        string m2Mon = m2.GiaiDauMonTheThao?.MonTheThao?.Ten ?? "Thể thao";
                        string m1Nd = m1.GiaiDauMonTheThao?.MonTheThao?.Ten ?? "";
                        string m2Nd = m2.GiaiDauMonTheThao?.MonTheThao?.Ten ?? "";
                        string m1San = m1.SanDau?.Ten ?? "Chưa xếp sân";

                        // 1. Kiểm tra Sân đấu
                        if (m1.SanDauId.HasValue && m2.SanDauId.HasValue && m1.SanDauId.Value == m2.SanDauId.Value)
                        {
                            var key = $"SAN_{m1.SanDauId}_{m1.Id}_{m2.Id}";
                            if (detectedKeys.Add(key))
                            {
                                report.HasConflict = true;
                                var msg = $"Sân đấu '{m1San}' bị trùng lịch giữa '{m1Name}' ({m1Mon}) và '{m2Name}' ({m2Mon}) trong khung giờ {m2Start:dd/MM HH:mm} - {m1End:HH:mm}.";
                                report.Conflicts.Add(msg);
                                report.ChiTietXungDot.Add(new ConflictDetailDto
                                {
                                    LoaiXungDot = "SanDau",
                                    ThongBao = msg,
                                    TenSanDau = m1San,
                                    TranDauBiTrungId = m2.Id,
                                    TenTranBiTrung = m2Name,
                                    TenMonTheThao = m2Mon,
                                    TenNoiDung = m2Nd,
                                    ThoiGianBatDau = m2Start,
                                    ThoiGianKetThuc = m2End
                                });
                            }
                        }

                        // 2. Kiểm tra Trọng tài
                        var m2Tt = m2.PhanCongTrongTais.Where(pc => pc.IsDeleted != true).ToList();
                        foreach (var pc in m2Tt)
                        {
                            if (m1TtIds.Contains(pc.TrongTaiId))
                            {
                                var key = $"TT_{pc.TrongTaiId}_{m1.Id}_{m2.Id}";
                                if (detectedKeys.Add(key))
                                {
                                    report.HasConflict = true;
                                    var msg = $"Trọng tài (ID: {pc.TrongTaiId}) bị phân công đồng thời tại cả 2 trận '{m1Name}' và '{m2Name}'.";
                                    report.Conflicts.Add(msg);
                                    report.ChiTietXungDot.Add(new ConflictDetailDto
                                    {
                                        LoaiXungDot = "TrongTai",
                                        ThongBao = msg,
                                        TranDauBiTrungId = m2.Id,
                                        TenTranBiTrung = m2Name,
                                        ThoiGianBatDau = m2Start,
                                        ThoiGianKetThuc = m2End
                                    });
                                }
                            }
                        }

                        // 3. Kiểm tra Vận động viên
                        var m2DkIds = m2.ThanhPhanTranDaus.Where(tp => tp.IsDeleted != true).Select(tp => tp.DangKyThiDauId).ToList();
                        var m2Vdvs = m2DkIds.SelectMany(dkId => vdvMap.GetValueOrDefault(dkId) ?? new List<VdvParticipantInfo>()).ToList();

                        foreach (var v1 in m1Vdvs)
                        {
                            var v2Match = m2Vdvs.FirstOrDefault(v2 => v2.IdentityKey == v1.IdentityKey);
                            if (v2Match != null)
                            {
                                var key = $"VDV_{v1.IdentityKey}_{m1.Id}_{m2.Id}";
                                if (detectedKeys.Add(key))
                                {
                                    report.HasConflict = true;
                                    string identityDesc = !string.IsNullOrWhiteSpace(v1.SoCCCD) ? $"CCCD: {v1.SoCCCD}" : $"Mã: {v1.Ma ?? v1.Id.ToString()}";
                                    var msg = $"VĐV '{v1.HoTen}' ({identityDesc}, thuộc {v1.TenDoi}) bị TRÙNG LỊCH THI ĐẤU: Trận '{m1Name}' ({m1Mon} - {m1Nd}) lúc {m1Start:HH:mm}-{m1End:HH:mm} và Trận '{m2Name}' ({m2Mon} - {m2Nd}) lúc {m2Start:HH:mm}-{m2End:HH:mm}.";
                                    report.Conflicts.Add(msg);
                                    report.ChiTietXungDot.Add(new ConflictDetailDto
                                    {
                                        LoaiXungDot = "VanDongVien",
                                        ThongBao = msg,
                                        VanDongVienId = v1.Id,
                                        TenVanDongVien = v1.HoTen,
                                        MaVanDongVien = v1.Ma,
                                        TenDoiHienTai = v1.TenDoi,
                                        TranDauBiTrungId = m2.Id,
                                        TenTranBiTrung = m2Name,
                                        TenMonTheThao = m2Mon,
                                        TenNoiDung = m2Nd,
                                        TenSanDau = m2.SanDau?.Ten,
                                        ThoiGianBatDau = m2Start,
                                        ThoiGianKetThuc = m2End
                                    });
                                }
                            }
                        }
                    }
                }
            }

            report.TotalConflicts = report.Conflicts.Count;
            return report;
        }

        private async Task<Dictionary<int, List<VdvParticipantInfo>>> GetVdvsForDangKyListAsync(IEnumerable<int> dangKyIds)
        {
            var dkIdList = dangKyIds.Distinct().ToList();
            var result = new Dictionary<int, List<VdvParticipantInfo>>();
            if (!dkIdList.Any()) return result;

            foreach (var id in dkIdList)
            {
                result[id] = new List<VdvParticipantInfo>();
            }

            // 1. Lấy thông tin DangKyThiDau (kèm Doi)
            var dangKys = (await _unitOfWork.DangKyThiDaus.GetPagedAsync(
                1, dkIdList.Count + 10,
                predicate: d => dkIdList.Contains(d.Id) && d.IsDeleted != true,
                orderBy: null,
                d => d.Doi!
            )).Items.ToList();

            // 2. Lấy ThanhVienDoi thay vì ChiTietDangKyThiDau
            var doiIds2661 = dangKys.Where(d => d.DoiId.HasValue).Select(d => d.DoiId!.Value).Distinct().ToList();
            var chiTiets = doiIds2661.Any()
                ? (await _unitOfWork.ThanhVienDois.FindAsync(tv => doiIds2661.Contains(tv.DoiId) && tv.IsDeleted != true)).ToList()
                : new List<ThanhVienDoi>();

            // 3. Lấy ThanhVienDoi cho các DoiId liên quan
            var doiIds = dangKys.Where(d => d.DoiId.HasValue).Select(d => d.DoiId!.Value).Distinct().ToList();
            var thanhViens = doiIds.Any()
                ? (await _unitOfWork.ThanhVienDois.FindAsync(tv => doiIds.Contains(tv.DoiId) && tv.IsDeleted != true)).ToList()
                : new List<ThanhVienDoi>();

            // 4. Lấy tất cả VanDongVien liên quan
            var allVdvIds = chiTiets.Select(c => c.VanDongVienId)
                .Union(thanhViens.Select(tv => tv.VanDongVienId))
                .Distinct()
                .ToList();

            var vdvDict = allVdvIds.Any()
                ? (await _unitOfWork.VanDongViens.FindAsync(v => allVdvIds.Contains(v.Id) && v.IsDeleted != true)).ToDictionary(v => v.Id)
                : new Dictionary<int, VanDongVien>();

            // 5. Gom VDV vào từng DangKyThiDau
            foreach (var dk in dangKys)
            {
                var list = new List<VdvParticipantInfo>();
                var seenVdv = new HashSet<int>();

                // Từ ThanhVienDoi (thay vì ChiTietDangKyThiDau)
                var ctList = dk.DoiId.HasValue
                    ? chiTiets.Where(tv => tv.DoiId == dk.DoiId.Value)
                    : Enumerable.Empty<ThanhVienDoi>();
                foreach (var ct in ctList)
                {
                    if (vdvDict.TryGetValue(ct.VanDongVienId, out var vdv) && seenVdv.Add(vdv.Id))
                    {
                        list.Add(new VdvParticipantInfo
                        {
                            Id = vdv.Id,
                            HoTen = vdv.HoTen,
                            Ma = vdv.Ma,
                            SoCCCD = vdv.SoCCCD,
                            DangKyThiDauId = dk.Id,
                            TenDangKy = dk.TenDangKy ?? dk.SoDangKy,
                            TenDoi = dk.Doi?.Ten ?? dk.TenDangKy ?? dk.SoDangKy
                        });
                    }
                }

                // Từ ThanhVienDoi nếu có DoiId
                if (dk.DoiId.HasValue)
                {
                    var tvList = thanhViens.Where(tv => tv.DoiId == dk.DoiId.Value);
                    foreach (var tv in tvList)
                    {
                        if (vdvDict.TryGetValue(tv.VanDongVienId, out var vdv) && seenVdv.Add(vdv.Id))
                        {
                            list.Add(new VdvParticipantInfo
                            {
                                Id = vdv.Id,
                                HoTen = vdv.HoTen,
                                Ma = vdv.Ma,
                                SoCCCD = vdv.SoCCCD,
                                DangKyThiDauId = dk.Id,
                                TenDangKy = dk.TenDangKy ?? dk.SoDangKy,
                                TenDoi = dk.Doi?.Ten ?? dk.TenDangKy ?? dk.SoDangKy
                            });
                        }
                    }
                }

                result[dk.Id] = list;
            }

            return result;
        }

        private class VdvParticipantInfo
        {
            public int Id { get; set; }
            public string HoTen { get; set; } = string.Empty;
            public string? Ma { get; set; }
            public string? SoCCCD { get; set; }
            public int DangKyThiDauId { get; set; }
            public string? TenDangKy { get; set; }
            public string? TenDoi { get; set; }

            /// <summary>
            /// Định danh VĐV: Nếu có CCCD hợp lệ thì định danh theo số CCCD để nhận diện VĐV trùng dù khác Id; ngược lại dùng Id
            /// </summary>
            public string IdentityKey => !string.IsNullOrWhiteSpace(SoCCCD) ? $"cccd_{SoCCCD.Trim()}" : $"vdv_{Id}";
        }

        /// <summary>
        /// Thuật toán Round-Robin kinh điển để sinh các cặp đấu xoay vòng cho từng lượt
        /// </summary>
        private static List<List<Tuple<int, int>>> GenerateRoundRobin(List<int> teams)
        {
            var rounds = new List<List<Tuple<int, int>>>();
            var list = new List<int>(teams);

            // Nếu số đội lẻ, thêm đội ảo (-1 đại diện cho Bye)
            if (list.Count % 2 != 0)
            {
                list.Add(-1);
            }

            int numTeams = list.Count;
            int numRounds = numTeams - 1;
            int halfSize = numTeams / 2;

            for (int r = 0; r < numRounds; r++)
            {
                var currentRound = new List<Tuple<int, int>>();
                for (int i = 0; i < halfSize; i++)
                {
                    int team1 = list[i];
                    int team2 = list[numTeams - 1 - i];

                    if (team1 != -1 && team2 != -1)
                    {
                        currentRound.Add(Tuple.Create(team1, team2));
                    }
                }

                rounds.Add(currentRound);

                // Xoay vòng (giữ nguyên phần tử đầu, xoay các phần tử còn lại)
                var last = list[numTeams - 1];
                list.RemoveAt(numTeams - 1);
                list.Insert(1, last);
            }

            return rounds;
        }

        private static string? GetPlaceholderTeam(string? ghiChu, string? tenTran, int position)
        {
            if (!string.IsNullOrWhiteSpace(ghiChu) && ghiChu.StartsWith("TBD: "))
            {
                var content = ghiChu.Substring(5);
                var vsIdx = content.IndexOf(" vs ", StringComparison.OrdinalIgnoreCase);
                if (vsIdx > 0)
                {
                    return position == 1 ? content.Substring(0, vsIdx).Trim() : content.Substring(vsIdx + 4).Trim();
                }
            }

            if (!string.IsNullOrWhiteSpace(tenTran))
            {
                var colonIdx = tenTran.IndexOf(':');
                var matchup = colonIdx >= 0 ? tenTran.Substring(colonIdx + 1).Trim() : tenTran.Trim();
                var vsIdx = matchup.IndexOf(" vs ", StringComparison.OrdinalIgnoreCase);
                if (vsIdx > 0)
                {
                    return position == 1 ? matchup.Substring(0, vsIdx).Trim() : matchup.Substring(vsIdx + 4).Trim();
                }
            }

            return null;
        }

        private class MatchFixture
        {
            public int FixtureId { get; set; }
            public string VongTen { get; set; } = string.Empty;
            public int VongThuTu { get; set; }
            public int? BangDauId { get; set; }
            public int Doi1DangKyId { get; set; }
            public int Doi2DangKyId { get; set; }
            public string TenTran { get; set; } = string.Empty;
            public bool IsPlaceholder { get; set; } = false;
            public string? TenDoi1Placeholder { get; set; }
            public string? TenDoi2Placeholder { get; set; }
            public string? GhiChu { get; set; }
            /// <summary>Đánh dấu đây là Heat (lượt thi nhiều VĐV) thay vì cặp đấu 1v1</summary>
            public bool IsHeat { get; set; } = false;
            /// <summary>Danh sách DangKyThiDauId của các VĐV trong Heat (khi IsHeat = true)</summary>
            public List<int>? HeatVdvIds { get; set; }

            // Liên kết DAG Knockout & Heat
            public int? NextFixtureId { get; set; }
            public int? NextSlot { get; set; }
            public int? LoserNextFixtureId { get; set; }
            public int? LoserNextSlot { get; set; }
            public string? MaTranBracket { get; set; }
        }

        /// <summary>Đại diện cho một Lượt thi (Heat) trong thể thức Tính điểm xếp hạng</summary>
        private class HeatFixture
        {
            public int FixtureId { get; set; }
            public string VongTen { get; set; } = string.Empty;
            public int VongThuTu { get; set; }
            public string TenTran { get; set; } = string.Empty;
            /// <summary>Danh sách DangKyThiDauId của các VĐV tham gia lượt thi này</summary>
            public List<int> DanhSachVdvIds { get; set; } = new();
            public int? NextFixtureId { get; set; }
        }

        private class BracketNode
        {
            public int FixtureId { get; set; }
            public int Team1Id { get; set; }
            public int Team2Id { get; set; }
            public bool IsBye { get; set; }
            public int WinnerTeamId { get; set; }
            public BracketNode? Child1 { get; set; }
            public BracketNode? Child2 { get; set; }
            public BracketNode? Parent { get; set; }
            public int ParentSlot { get; set; } = 1;
            public MatchFixture? Fixture { get; set; }
            public string? BracketCode { get; set; }
        }

        private static List<int> GetBracketSeedOrder(int size)
        {
            var list = new List<int> { 1, 2 };
            while (list.Count < size)
            {
                var next = new List<int>();
                int targetSum = list.Count * 2 + 1;
                foreach (var seed in list)
                {
                    next.Add(seed);
                    next.Add(targetSum - seed);
                }
                list = next;
            }
            return list;
        }

        /// <summary>
        /// Lấy toàn bộ dữ liệu cần thiết phục vụ giao diện xếp cặp đấu thủ công bằng kéo thả:
        /// bao gồm danh sách các đội/VĐV đã được duyệt, danh sách các cặp đấu hiện có của môn/vòng/bảng,
        /// và danh sách các đội chưa được gán vào bất kỳ cặp đấu nào.
        /// </summary>
        /// <param name="giaiDauMonTheThaoId">ID liên kết giải đấu và môn thể thao</param>
        /// <param name="vongDauId">Tùy chọn lọc theo vòng đấu</param>
        /// <param name="bangDauId">Tùy chọn lọc theo bảng đấu</param>
        /// <returns>Dữ liệu đầy đủ phục vụ layout xếp cặp và danh sách đội chưa xếp</returns>
        public async Task<ManualPairingDataDto> GetManualPairingDataAsync(int giaiDauMonTheThaoId, int? vongDauId = null, int? bangDauId = null)
        {
            var result = new ManualPairingDataDto
            {
                GiaiDauMonTheThaoId = giaiDauMonTheThaoId
            };

            // 1. Lấy thông tin GiaiDauMonTheThao
            var gdm = await _unitOfWork.GiaiDauMonTheThaos.GetByIdAsync(giaiDauMonTheThaoId);
            MonTheThao? mon = null;
            if (gdm != null)
            {
                mon = await _unitOfWork.MonTheThaos.GetByIdAsync(gdm.MonTheThaoId);
                result.TenMonTheThao = mon?.Ten;
                result.HinhThucThiDau = mon?.HinhThucThiDau.ToString();
                result.LaMonDongDoi = mon?.LaMonDongDoi ?? false;
            }
            // Nạp cấu hình thể thức thi đấu
            CauHinhTheThucThiDau? config = null;
            if (gdm != null)
            {
                var specificConfigs = await _unitOfWork.CauHinhTheThucThiDaus.FindAsync(c => c.GiaiDauMonTheThaoId == giaiDauMonTheThaoId && c.IsDeleted != true);
                var specific = specificConfigs.FirstOrDefault();

                var defaultConfigs = await _unitOfWork.CauHinhTheThucThiDaus.FindAsync(c => c.MonTheThaoId == gdm.MonTheThaoId && (c.GiaiDauMonTheThaoId == null || c.GiaiDauMonTheThaoId == 0) && c.IsDeleted != true);
                var def = defaultConfigs.FirstOrDefault();

                config = (specific != null && specific.SoBang > 0) ? specific : (def ?? specific);
            }

            int configuredSoBang = config?.SoBang ?? 0;
            int configuredSoDoiMoiBang = (config?.SoDoiMoiBang > 1) ? config.SoDoiMoiBang : 4;
            int configuredVaoVongTrong = (config?.SoDoiMoiBangVaoVongTrong > 0) ? config.SoDoiMoiBangVaoVongTrong : 2;

            bool isKetHop = result.HinhThucThiDau == "KetHopVongBangVaLoaiTrucTiep" ||
                            result.HinhThucThiDau == "3" ||
                            (mon != null && mon.HinhThucThiDau == Dms.Domain.Enums.HinhThucThiDau.KetHopVongBangVaLoaiTrucTiep) ||
                            (configuredSoBang > 1);

            result.IsKetHopVongBangKnockout = isKetHop;
            result.SoBangCauHinh = configuredSoBang;
            result.SoDoiMoiBang = configuredSoDoiMoiBang;
            result.SoDoiMoiBangVaoVongTrong = configuredVaoVongTrong;

            // 2. Lấy danh sách Vòng đấu
            var vongDaus = (await _unitOfWork.VongDaus.FindAsync(v => v.GiaiDauMonTheThaoId == giaiDauMonTheThaoId && v.IsDeleted != true))
                .OrderBy(v => v.ThuTu)
                .ToList();

            // Nếu chưa có vòng đấu nào, tự động tạo 1 Vòng 1 mặc định
            if (!vongDaus.Any())
            {
                var defaultVong = new VongDau
                {
                    GiaiDauMonTheThaoId = giaiDauMonTheThaoId,
                    Ten = "Vòng 1",
                    LoaiVong = "VongLoai",
                    ThuTu = 1,
                    Created = DateTime.UtcNow,
                    IsDeleted = false
                };
                await _unitOfWork.VongDaus.AddAsync(defaultVong);
                await _unitOfWork.CompleteAsync();
                vongDaus.Add(defaultVong);
            }

            result.VongDaus = vongDaus.Select(v => new VongDauDto
            {
                Id = v.Id,
                GiaiDauMonTheThaoId = v.GiaiDauMonTheThaoId,
                Ten = v.Ten,
                ThuTu = v.ThuTu,
                LoaiVongDau = v.LoaiVong
            }).ToList();

            // 3. Lấy danh sách Bảng đấu (nếu có)
            var bangDaus = (await _unitOfWork.BangDaus.FindAsync(b => b.GiaiDauMonTheThaoId == giaiDauMonTheThaoId && b.IsDeleted != true))
                .OrderBy(b => b.ThuTu)
                .ToList();

            // 4. Lấy danh sách Sân đấu
            var sanDaus = (await _unitOfWork.SanDaus.GetPagedAsync(
                1, 500,
                predicate: s => s.TrangThai && s.IsDeleted != true,
                orderBy: q => q.OrderBy(s => s.Ten),
                s => s.CumSan
            )).Items.ToList();

            result.SanDaus = sanDaus.Select(s => new SanDauDto
            {
                Id = s.Id,
                Ma = s.Ma,
                Ten = s.Ten,
                CumSanId = s.CumSanId,
                TenCumSan = s.CumSan?.Ten,
                MonTheThaoId = s.MonTheThaoId,
                TrangThai = s.TrangThai
            }).ToList();

            // 5. Lấy danh sách Đội / VĐV đã duyệt (DangKyThiDau)
            var pagedDangKy = await _unitOfWork.DangKyThiDaus.GetPagedAsync(
                1, 2000,
                predicate: d => d.GiaiDauMonTheThaoId == giaiDauMonTheThaoId && d.TrangThai == "DaDuyet" && d.IsDeleted != true,
                orderBy: q => q.OrderBy(d => d.Id),
                d => d.Doi!,
                d => d.Doi!.DonVi!,
                d => d.ThanhVienBangs
            );

            // Nếu là thể thức Bảng kết hợp Knockout hoặc môn có cấu hình số bảng > 1, chuẩn bị sẵn số lượng bảng đấu theo cấu hình nếu chưa đủ
            if (isKetHop || configuredSoBang > 1)
            {
                int totalTeams = pagedDangKy.TotalCount;
                int targetSoBang = configuredSoBang > 0
                    ? configuredSoBang
                    : Math.Max(2, (int)Math.Ceiling((double)Math.Max(1, totalTeams) / configuredSoDoiMoiBang));

                if (bangDaus.Count < targetSoBang)
                {
                    for (int i = bangDaus.Count; i < targetSoBang; i++)
                    {
                        char groupChar = (char)('A' + i);
                        var newBang = new BangDau
                        {
                            GiaiDauMonTheThaoId = giaiDauMonTheThaoId,
                            Ma = $"BANG_{groupChar}",
                            Ten = $"Bảng {groupChar}",
                            ThuTu = i + 1,
                            Created = DateTime.UtcNow,
                            IsDeleted = false
                        };
                        await _unitOfWork.BangDaus.AddAsync(newBang);
                        await _unitOfWork.CompleteAsync();
                        bangDaus.Add(newBang);
                    }
                }
            }

            result.BangDaus = bangDaus.OrderBy(b => b.ThuTu).Select(b => new BangDauDto
            {
                Id = b.Id,
                GiaiDauMonTheThaoId = b.GiaiDauMonTheThaoId,
                Ma = b.Ma,
                Ten = b.Ten,
                ThuTu = b.ThuTu
            }).ToList();

            // Lấy VDV từ ThanhVienDoi thay vì ChiTietDangKyThiDau
            var doiIdsPairing = pagedDangKy.Items.Where(d => d.DoiId.HasValue).Select(d => d.DoiId!.Value).Distinct().ToList();
            var thanhViensPairing = doiIdsPairing.Any()
                ? (await _unitOfWork.ThanhVienDois.FindAsync(tv => doiIdsPairing.Contains(tv.DoiId) && tv.IsDeleted != true)).ToList()
                : new List<ThanhVienDoi>();
            var allVdvIds = thanhViensPairing.Select(tv => tv.VanDongVienId).Distinct().ToList();
            var vdvMap = (await _unitOfWork.VanDongViens.FindAsync(v => allVdvIds.Contains(v.Id))).ToDictionary(v => v.Id, v => v.HoTen);

            var bangMap = bangDaus.ToDictionary(b => b.Id, b => b.Ten);

            var allTeams = new List<ManualPairingTeamDto>();
            foreach (var d in pagedDangKy.Items)
            {
                // Lấy tên VDV từ thành viên đội
                var vdvNames = d.DoiId.HasValue
                    ? thanhViensPairing
                        .Where(tv => tv.DoiId == d.DoiId.Value && tv.IsDeleted != true)
                        .Select(tv => vdvMap.GetValueOrDefault(tv.VanDongVienId, ""))
                        .Where(name => !string.IsNullOrEmpty(name))
                        .ToList()
                    : new List<string>();

                var activeTvBang = d.ThanhVienBangs.FirstOrDefault(tv => tv.IsDeleted != true);
                int? bId = activeTvBang?.BangDauId;
                string? bName = bId.HasValue ? bangMap.GetValueOrDefault(bId.Value) : null;

                bool laMonDongDoi = result.LaMonDongDoi;
                string displayTitle;
                if (laMonDongDoi)
                {
                    if (!string.IsNullOrWhiteSpace(d.Doi?.Ten) && !d.Doi.Ten.StartsWith("Tham gia -", StringComparison.OrdinalIgnoreCase))
                    {
                        displayTitle = d.Doi.Ten;
                    }
                    else if (!string.IsNullOrWhiteSpace(d.TenDangKy) && !d.TenDangKy.StartsWith("Tham gia -", StringComparison.OrdinalIgnoreCase))
                    {
                        displayTitle = d.TenDangKy;
                    }
                    else
                    {
                        displayTitle = $"Đội #{d.Id}";
                    }
                }
                else
                {
                    var firstVdv = vdvNames.FirstOrDefault();
                    if (!string.IsNullOrWhiteSpace(firstVdv))
                    {
                        displayTitle = firstVdv;
                    }
                    else if (!string.IsNullOrWhiteSpace(d.Doi?.Ten) && !d.Doi.Ten.StartsWith("Tham gia -", StringComparison.OrdinalIgnoreCase))
                    {
                        displayTitle = d.Doi.Ten;
                    }
                    else if (!string.IsNullOrWhiteSpace(d.TenDangKy) && !d.TenDangKy.StartsWith("Tham gia -", StringComparison.OrdinalIgnoreCase))
                    {
                        displayTitle = d.TenDangKy;
                    }
                    else
                    {
                        displayTitle = $"VĐV #{d.Id}";
                    }
                }

                var teamDto = new ManualPairingTeamDto
                {
                    DangKyThiDauId = d.Id,
                    TenDangKy = displayTitle,
                    TenDoi = d.Doi?.Ten,
                    TenDonVi = d.Doi?.DonVi?.Ten,
                    SoDangKy = d.SoDangKy,
                    BangDauId = bId,
                    TenBangDau = bName,
                    VanDongVienNames = vdvNames
                };
                allTeams.Add(teamDto);
            }
            result.AllTeams = allTeams;
            var teamLookup = allTeams.ToDictionary(t => t.DangKyThiDauId);

            // 6. Lấy danh sách trận đấu hiện có (theo vongDauId, bangDauId nếu truyền)
            var pagedTranDau = await _unitOfWork.TranDaus.GetPagedAsync(
                1, 2000,
                predicate: t => t.GiaiDauMonTheThaoId == giaiDauMonTheThaoId &&
                                t.IsDeleted != true &&
                                (!vongDauId.HasValue || t.VongDauId == vongDauId.Value) &&
                                (!bangDauId.HasValue || t.BangDauId == bangDauId.Value),
                orderBy: q => q.OrderBy(t => t.SoTran).ThenBy(t => t.Id),
                t => t.VongDau,
                t => t.BangDau!,
                t => t.SanDau!,
                t => t.ThanhPhanTranDaus,
                t => t.PhanCongTrongTais
            );

            var assignedTeamIds = new HashSet<int>();
            var existingPairs = new List<ManualMatchPairDto>();

            var matchIds = pagedTranDau.Items.Select(t => t.Id).ToList();
            var allPhanCongs = matchIds.Any()
                ? (await _unitOfWork.PhanCongTrongTais.FindAsync(pc => matchIds.Contains(pc.TranDauId) && pc.IsDeleted != true)).ToList()
                : new List<PhanCongTrongTai>();

            var allTtIds = allPhanCongs.Select(pc => pc.TrongTaiId).Distinct().ToList();
            var trongTaiMap = allTtIds.Any()
                ? (await _unitOfWork.TrongTais.FindAsync(tt => allTtIds.Contains(tt.Id))).ToDictionary(tt => tt.Id)
                : new Dictionary<int, TrongTai>();

            var phanCongByMatch = allPhanCongs.GroupBy(pc => pc.TranDauId).ToDictionary(g => g.Key, g => g.ToList());

            foreach (var t in pagedTranDau.Items)
            {
                var activeTps = t.ThanhPhanTranDaus?.Where(tp => tp.IsDeleted != true).OrderBy(tp => tp.ViTri ?? tp.SoLane ?? 0).ToList() ?? new List<ThanhPhanTranDau>();
                var tp1 = activeTps.FirstOrDefault(tp => tp.ViTri == 1) ?? (activeTps.All(tp => tp.ViTri == null) ? activeTps.ElementAtOrDefault(0) : null);
                var tp2 = activeTps.FirstOrDefault(tp => tp.ViTri == 2) ?? (activeTps.All(tp => tp.ViTri == null) ? (activeTps.Count > 1 ? activeTps.ElementAtOrDefault(1) : null) : null);
                if (tp1 != null && tp2 != null && tp1.Id == tp2.Id) tp2 = null;

                ManualPairingTeamDto? doi1 = null;
                if (tp1 != null && teamLookup.TryGetValue(tp1.DangKyThiDauId, out var found1))
                {
                    doi1 = found1;
                    assignedTeamIds.Add(found1.DangKyThiDauId);
                }

                ManualPairingTeamDto? doi2 = null;
                if (tp2 != null && teamLookup.TryGetValue(tp2.DangKyThiDauId, out var found2))
                {
                    doi2 = found2;
                    assignedTeamIds.Add(found2.DangKyThiDauId);
                }

                bool isHeatSport = result.HinhThucThiDau == "TinhDiemXepHang" || result.HinhThucThiDau == "6"
                    || result.HinhThucThiDau == "DuaThoiGian" || result.HinhThucThiDau == "8"
                    || result.HinhThucThiDau == "DoLuotThi" || result.HinhThucThiDau == "9"
                    || result.HinhThucThiDau == "BieuDienChamDiem" || result.HinhThucThiDau == "10";
                bool isMultiParticipant = activeTps.Count > 2;
                bool isHeatMatch = isHeatSport || isMultiParticipant;

                var danhSachVdv = new List<ManualPairingParticipantDto>();
                if (t.ThanhPhanTranDaus != null)
                {
                    foreach (var tp in t.ThanhPhanTranDaus.Where(x => x.IsDeleted != true).OrderBy(x => x.SoLane ?? x.ViTri))
                    {
                        assignedTeamIds.Add(tp.DangKyThiDauId);
                        if (teamLookup.TryGetValue(tp.DangKyThiDauId, out var tFound))
                        {
                            danhSachVdv.Add(new ManualPairingParticipantDto
                            {
                                DangKyThiDauId = tp.DangKyThiDauId,
                                TenDangKy = tFound.TenDangKy,
                                TenDoi = tFound.TenDoi,
                                TenDonVi = tFound.TenDonVi,
                                SoLane = tp.SoLane ?? tp.ViTri,
                                ViTri = tp.ViTri,
                                VanDongVienNames = tFound.VanDongVienNames
                            });
                        }
                    }
                }

                var matchPcs = phanCongByMatch.GetValueOrDefault(t.Id) ?? new List<PhanCongTrongTai>();
                var trongTaiDtos = matchPcs.Select(pc =>
                {
                    trongTaiMap.TryGetValue(pc.TrongTaiId, out var tt);
                    return new PhanCongTrongTaiItemDto
                    {
                        Id = pc.Id,
                        TranDauId = pc.TranDauId,
                        TrongTaiId = pc.TrongTaiId,
                        TenTrongTai = tt?.HoTen ?? $"Trọng tài #{pc.TrongTaiId}",
                        SoDienThoai = tt?.SoDienThoai,
                        CapBac = tt?.CapBac,
                        VaiTro = pc.VaiTro,
                        GhiChu = pc.GhiChu
                    };
                }).ToList();

                existingPairs.Add(new ManualMatchPairDto
                {
                    TranDauId = t.Id,
                    SoTran = t.SoTran,
                    TenTran = t.TenTran,
                    VongDauId = t.VongDauId,
                    TenVongDau = t.VongDau?.Ten,
                    VongThuTu = t.VongDau?.ThuTu ?? 1,
                    BangDauId = t.BangDauId,
                    TenBangDau = t.BangDau?.Ten,
                    SanDauId = t.SanDauId,
                    TenSanDau = t.SanDau?.Ten,
                    ThoiGianDuKien = t.ThoiGianDuKien ?? t.ThoiGianBatDau,
                    TrangThai = t.TrangThai,
                    DiemDoi1 = t.TySoDoi1,
                    DiemDoi2 = t.TySoDoi2,
                    DiemPenaltyDoi1 = t.DiemPenaltyDoi1,
                    DiemPenaltyDoi2 = t.DiemPenaltyDoi2,
                    DoiThangDangKyId = t.DoiThangDangKyId,
                    DoiThuaDangKyId = t.DoiThuaDangKyId,
                    MaTranBracket = t.MaTranBracket,
                    NextTranDauId = t.NextTranDauId,
                    NextTranDauViTri = t.NextTranDauViTri,
                    LoserNextTranDauId = t.LoserNextTranDauId,
                    LoserNextTranDauViTri = t.LoserNextTranDauViTri,
                    IsHeat = isHeatMatch,
                    GhiChu = t.GhiChu,
                    TenDoi1Placeholder = GetPlaceholderTeam(t.GhiChu, t.TenTran, 1),
                    TenDoi2Placeholder = GetPlaceholderTeam(t.GhiChu, t.TenTran, 2),
                    Doi1 = doi1,
                    Doi2 = doi2,
                    DanhSachVdv = danhSachVdv,
                    DanhSachTrongTai = trongTaiDtos
                });
            }

            result.ExistingPairs = existingPairs;

            // 7. Xác định danh sách đội chưa xếp cặp
            var eligibleTeams = bangDauId.HasValue
                ? allTeams.Where(t => t.BangDauId == bangDauId.Value).ToList()
                : allTeams;

            result.UnpairedTeams = eligibleTeams.Where(t => !assignedTeamIds.Contains(t.DangKyThiDauId)).ToList();

            return result;
        }

        /// <summary>
        /// Lưu danh sách các cặp đấu được xếp thủ công:
        /// tạo mới hoặc cập nhật các trận đấu TranDau và phân công thành phần thi đấu ThanhPhanTranDau
        /// cho từng cặp (Đội 1 tại ViTri = 1, Đội 2 tại ViTri = 2).
        /// Các trận đã bị xóa khỏi layout sẽ được chuyển sang xóa mềm IsDeleted = true.
        /// </summary>
        /// <param name="request">Yêu cầu lưu cấu hình các cặp đấu</param>
        /// <param name="username">Tên tài khoản thực hiện</param>
        /// <returns>True nếu thành công</returns>
        public async Task<bool> SaveManualPairingAsync(SaveManualPairingRequestDto request, string? username = null)
        {
            if (request == null || request.GiaiDauMonTheThaoId <= 0)
            {
                throw new ArgumentException("Thông tin môn thi đấu không hợp lệ.");
            }

            // 0. Nếu có cập nhật phân bảng đấu thủ công (TeamGroups)
            if (request.TeamGroups != null && request.TeamGroups.Any())
            {
                var monBangDaus = (await _unitOfWork.BangDaus.FindAsync(b => b.GiaiDauMonTheThaoId == request.GiaiDauMonTheThaoId && b.IsDeleted != true)).ToList();
                var monBangIds = monBangDaus.Select(b => b.Id).ToList();

                var oldTvbs = (await _unitOfWork.ThanhVienBangs.FindAsync(tv => monBangIds.Contains(tv.BangDauId))).ToList();
                foreach (var oldTv in oldTvbs)
                {
                    _unitOfWork.ThanhVienBangs.Delete(oldTv);
                }

                foreach (var tg in request.TeamGroups)
                {
                    if (tg.BangDauId > 0 && tg.DangKyThiDauId > 0 && monBangIds.Contains(tg.BangDauId))
                    {
                        await _unitOfWork.ThanhVienBangs.AddAsync(new ThanhVienBang
                        {
                            BangDauId = tg.BangDauId,
                            DangKyThiDauId = tg.DangKyThiDauId,
                            Created = DateTime.UtcNow,
                            CreatedBy = username,
                            IsDeleted = false
                        });
                    }
                }
                await _unitOfWork.CompleteAsync();
            }

            // 1. Xác định VongDau mặc định nếu request không truyền
            int defaultVongDauId = 0;
            if (request.VongDauId.HasValue && request.VongDauId.Value > 0)
            {
                defaultVongDauId = request.VongDauId.Value;
            }
            else
            {
                var existingVong = (await _unitOfWork.VongDaus.FindAsync(v => v.GiaiDauMonTheThaoId == request.GiaiDauMonTheThaoId && v.IsDeleted != true))
                    .OrderBy(v => v.ThuTu)
                    .FirstOrDefault();

                if (existingVong != null)
                {
                    defaultVongDauId = existingVong.Id;
                }
                else
                {
                    var newVong = new VongDau
                    {
                        GiaiDauMonTheThaoId = request.GiaiDauMonTheThaoId,
                        Ten = "Vòng 1",
                        LoaiVong = "VongLoai",
                        ThuTu = 1,
                        Created = DateTime.UtcNow,
                        CreatedBy = username,
                        IsDeleted = false
                    };
                    await _unitOfWork.VongDaus.AddAsync(newVong);
                    await _unitOfWork.CompleteAsync();
                    defaultVongDauId = newVong.Id;
                }
            }

            // 2. Lấy tên các đội để sinh TenTran tự động nếu chưa có
            var allDkIds = request.Pairs
                .SelectMany(p => new[] { p.Doi1DangKyId, p.Doi2DangKyId })
                .Where(id => id.HasValue && id.Value > 0)
                .Select(id => id!.Value)
                .Distinct()
                .ToList();

            var teamNameMap = (await _unitOfWork.DangKyThiDaus.GetPagedAsync(
                1, 2000,
                predicate: d => allDkIds.Contains(d.Id),
                orderBy: null,
                d => d.Doi!
            )).Items.ToDictionary(d => d.Id, d => !string.IsNullOrWhiteSpace(d.TenDangKy) ? d.TenDangKy : (d.Doi?.Ten ?? $"Đội #{d.Id}"));

            // 3. Lấy danh sách các trận hiện có trong phạm vi đang xếp
            var existingMatches = (await _unitOfWork.TranDaus.GetPagedAsync(
                1, 2000,
                predicate: t => t.GiaiDauMonTheThaoId == request.GiaiDauMonTheThaoId &&
                                t.IsDeleted != true &&
                                (!request.VongDauId.HasValue || t.VongDauId == request.VongDauId.Value) &&
                                (!request.BangDauId.HasValue || t.BangDauId == request.BangDauId.Value),
                orderBy: null,
                t => t.ThanhPhanTranDaus
            )).Items.ToList();

            var keptTranDauIds = new HashSet<int>();

            // 4. Duyệt từng cặp đấu được gửi lên từ client
            int matchIndex = 1;
            foreach (var pair in request.Pairs)
            {
                int soTran = pair.SoTran > 0 ? pair.SoTran : matchIndex;
                int vId = pair.VongDauId ?? (request.VongDauId ?? defaultVongDauId);
                int? bId = pair.BangDauId ?? request.BangDauId;

                string name1 = (pair.Doi1DangKyId.HasValue && teamNameMap.TryGetValue(pair.Doi1DangKyId.Value, out var n1)) ? n1 : "Chưa xác định";
                string name2 = (pair.Doi2DangKyId.HasValue && teamNameMap.TryGetValue(pair.Doi2DangKyId.Value, out var n2)) ? n2 : "Chưa xác định";
                string defaultTenTran = $"Trận {soTran}: {name1} vs {name2}";
                string tenTran = !string.IsNullOrWhiteSpace(pair.TenTran) ? pair.TenTran : defaultTenTran;

                TranDau? targetTran = null;

                if (pair.TranDauId.HasValue && pair.TranDauId.Value > 0)
                {
                    // Cập nhật trận đấu đã có
                    targetTran = existingMatches.FirstOrDefault(m => m.Id == pair.TranDauId.Value) ??
                                 await _unitOfWork.TranDaus.GetByIdAsync(pair.TranDauId.Value);

                    if (targetTran != null)
                    {
                        targetTran.SoTran = soTran;
                        targetTran.TenTran = tenTran;
                        targetTran.VongDauId = vId;
                        targetTran.BangDauId = bId;
                        targetTran.SanDauId = pair.SanDauId;
                        targetTran.ThoiGianDuKien = pair.ThoiGianDuKien;
                        if (pair.ThoiGianDuKien.HasValue)
                        {
                            targetTran.ThoiGianBatDau = pair.ThoiGianDuKien.Value;
                        }
                        targetTran.LastModified = DateTime.UtcNow;
                        targetTran.LastModifiedBy = username;

                        _unitOfWork.TranDaus.Update(targetTran);
                        keptTranDauIds.Add(targetTran.Id);

                        // Xóa các ThanhPhanTranDau cũ
                        var oldTps = (await _unitOfWork.ThanhPhanTranDaus.FindAsync(tp => tp.TranDauId == targetTran.Id)).ToList();
                        foreach (var oldTp in oldTps)
                        {
                            _unitOfWork.ThanhPhanTranDaus.Delete(oldTp);
                        }
                    }
                    else
                    {
                        // Nếu không tìm thấy, tạo mới
                        targetTran = new TranDau
                        {
                            GiaiDauMonTheThaoId = request.GiaiDauMonTheThaoId,
                            VongDauId = vId,
                            BangDauId = bId,
                            SanDauId = pair.SanDauId,
                            SoTran = soTran,
                            TenTran = tenTran,
                            ThoiGianDuKien = pair.ThoiGianDuKien,
                            ThoiGianBatDau = pair.ThoiGianDuKien,
                            TrangThai = "ChuaDau",
                            Created = DateTime.UtcNow,
                            CreatedBy = username,
                            IsDeleted = false
                        };
                        await _unitOfWork.TranDaus.AddAsync(targetTran);
                        await _unitOfWork.CompleteAsync();
                        keptTranDauIds.Add(targetTran.Id);
                    }
                }
                else
                {
                    // Tạo mới trận đấu
                    targetTran = new TranDau
                    {
                        GiaiDauMonTheThaoId = request.GiaiDauMonTheThaoId,
                        VongDauId = vId,
                        BangDauId = bId,
                        SanDauId = pair.SanDauId,
                        SoTran = soTran,
                        TenTran = tenTran,
                        ThoiGianDuKien = pair.ThoiGianDuKien,
                        ThoiGianBatDau = pair.ThoiGianDuKien,
                        TrangThai = "ChuaDau",
                        Created = DateTime.UtcNow,
                        CreatedBy = username,
                        IsDeleted = false
                    };
                    await _unitOfWork.TranDaus.AddAsync(targetTran);
                    await _unitOfWork.CompleteAsync();
                    keptTranDauIds.Add(targetTran.Id);
                }

                // Gán Đội 1 (ViTri = 1)
                if (pair.Doi1DangKyId.HasValue && pair.Doi1DangKyId.Value > 0)
                {
                    await _unitOfWork.ThanhPhanTranDaus.AddAsync(new ThanhPhanTranDau
                    {
                        TranDauId = targetTran.Id,
                        DangKyThiDauId = pair.Doi1DangKyId.Value,
                        ViTri = 1,
                        TrangThai = "ThamGia",
                        Created = DateTime.UtcNow,
                        CreatedBy = username,
                        IsDeleted = false
                    });
                }

                // Gán Đội 2 (ViTri = 2)
                if (pair.Doi2DangKyId.HasValue && pair.Doi2DangKyId.Value > 0)
                {
                    await _unitOfWork.ThanhPhanTranDaus.AddAsync(new ThanhPhanTranDau
                    {
                        TranDauId = targetTran.Id,
                        DangKyThiDauId = pair.Doi2DangKyId.Value,
                        ViTri = 2,
                        TrangThai = "ThamGia",
                        Created = DateTime.UtcNow,
                        CreatedBy = username,
                        IsDeleted = false
                    });
                }

                matchIndex++;
            }

            // 5. Xóa mềm các trận đấu cũ không còn nằm trong danh sách xếp cặp
            foreach (var oldMatch in existingMatches)
            {
                if (!keptTranDauIds.Contains(oldMatch.Id))
                {
                    oldMatch.IsDeleted = true;
                    oldMatch.LastModified = DateTime.UtcNow;
                    oldMatch.LastModifiedBy = username;
                    _unitOfWork.TranDaus.Update(oldMatch);
                }
            }

            await _unitOfWork.CompleteAsync();
            return true;
        }

        /// <summary>
        /// Ghi nhận kết quả trận đấu từ trọng tài: thẩm định tỷ số theo luật môn thể thao,
        /// tự động cập nhật bảng xếp hạng nếu là vòng bảng, tự động đưa đội thắng/thua vào vòng Knockout tiếp theo,
        /// hoặc tự động trao huy chương nếu là trận chung kết/tranh hạng 3.
        /// </summary>
        /// <param name="request">Dữ liệu kết quả trận đấu gửi từ trọng tài</param>
        /// <param name="username">Tài khoản người thực hiện thao tác</param>
        /// <returns>Đối tượng CompleteMatchResponseDto chứa trạng thái và thông báo kết quả cập nhật</returns>
        public async Task<CompleteMatchResponseDto> CompleteMatchResultAsync(CompleteMatchRequestDto request, string username)
        {
            var response = new CompleteMatchResponseDto { TranDauId = request.TranDauId };

            var match = await _unitOfWork.TranDaus.GetByIdAsync(request.TranDauId);
            if (match == null)
            {
                response.Success = false;
                response.Message = "Không tìm thấy thông tin trận đấu.";
                return response;
            }

            if (!string.Equals(match.TrangThai, "DangDau", StringComparison.OrdinalIgnoreCase))
            {
                response.Success = false;
                response.Message = "Chỉ có thể hoàn tất trận khi trận đang diễn ra. Hãy bắt đầu trận trước.";
                return response;
            }

            var gdm = await _unitOfWork.GiaiDauMonTheThaos.GetByIdAsync(match.GiaiDauMonTheThaoId);
            if (gdm == null)
            {
                response.Success = false;
                response.Message = "Không tìm thấy thông tin môn thi đấu của giải.";
                return response;
            }

            var config = await _theThucService.GetConfigByTranDauIdAsync(match.Id);
            if (config == null)
            {
                response.Success = false;
                response.Message = "Chưa tìm thấy cấu hình thể thức cho môn thể thao này.";
                return response;
            }

            // 1. Nếu là môn đo thành tích (Điền kinh, Bơi lội, Lượt thử, Biểu diễn...), chỉ chốt qua bảng kết quả từng VĐV.
            if (config.IsPerformanceSport)
            {
                if (request.HeatResults == null || !request.HeatResults.Any())
                {
                    response.Success = false;
                    response.Message = "Cần nhập kết quả từng VĐV trước khi hoàn tất lượt thi thành tích.";
                    return response;
                }

                var athleticsResult = await _athleticsProgressionEngine.ProcessHeatResultAsync(
                    match.Id, request.HeatResults, config, username);

                response.Success = athleticsResult.Success;
                response.Message = athleticsResult.Message;
                response.NextMatchNotice = athleticsResult.FinalHeatAdvancementNotice;
                response.MedalsAwarded = athleticsResult.MedalsAwarded;
                if (athleticsResult.IsRecordBroken)
                {
                    response.Message += " " + athleticsResult.RecordBreakerNotice;
                }
                if (athleticsResult.Success)
                {
                    match.TrangThaiDuyetKetQua = "ChoDuyet";
                    match.ThoiGianDuyetKetQua = null;
                    match.NguoiDuyetKetQua = null;
                    match.GhiChuDuyetKetQua = null;
                    match.LastModified = DateTime.UtcNow;
                    match.LastModifiedBy = username;
                    _unitOfWork.TranDaus.Update(match);
                    await _unitOfWork.CompleteAsync();
                }
                return response;
            }

            // 2. Môn thi đấu đối kháng (Bóng đá, Cầu lông, Bóng chuyền, Bóng bàn...)
            var periodScores = request.SetScores ?? new List<SetScoreDto>();
            try
            {
                ValidatePeriodScores(config, periodScores, requireComplete: true);
                var projectedScore = MatchScoreCalculator.CalculateCurrentScore(config, periodScores);
                request.Score1 = projectedScore.Score1;
                request.Score2 = projectedScore.Score2;
            }
            catch (InvalidOperationException ex)
            {
                response.Success = false;
                response.Message = ex.Message;
                return response;
            }

            bool isKnockout = !match.BangDauId.HasValue;
            var evaluation = _scoringEngine.EvaluateMatchResult(request, config, isKnockout);

            if (!evaluation.IsValid)
            {
                response.Success = false;
                response.Message = evaluation.ErrorMessage ?? "Kết quả trận đấu không hợp lệ theo thể thức thi đấu của môn.";
                return response;
            }

            // Lấy 2 đội tham gia trận
            var tps = (await _unitOfWork.ThanhPhanTranDaus.FindAsync(tp => tp.TranDauId == match.Id && tp.IsDeleted != true))
                      .OrderBy(tp => tp.ViTri ?? 1).ToList();

            var team1 = tps.FirstOrDefault(tp => tp.ViTri == 1) ?? (tps.All(tp => tp.ViTri == null) ? tps.ElementAtOrDefault(0) : null);
            var team2 = tps.FirstOrDefault(tp => tp.ViTri == 2) ?? (tps.All(tp => tp.ViTri == null) ? tps.ElementAtOrDefault(1) : null);
            if (team1 != null && team2 != null && team1.Id == team2.Id) team2 = null;

            int winnerDangKyId = 0;
            int loserDangKyId = 0;

            if (evaluation.WinnerTeamIndex == 1 && team1 != null)
            {
                winnerDangKyId = team1.DangKyThiDauId;
                loserDangKyId = team2 != null ? team2.DangKyThiDauId : 0;
            }
            else if (evaluation.WinnerTeamIndex == 2 && team2 != null)
            {
                winnerDangKyId = team2.DangKyThiDauId;
                loserDangKyId = team1 != null ? team1.DangKyThiDauId : 0;
            }

            // Chỉ ghi giờ bắt đầu thực tế khi trận được bắt đầu; giờ dự kiến được lưu riêng ở ThoiGianDuKien.
            if (match.TrangThai == "ChuaDau" || !match.ThoiGianBatDau.HasValue)
            {
                match.ThoiGianBatDau = DateTime.UtcNow;
            }

            // Cập nhật tỷ số trận đấu
            match.TySoDoi1 = evaluation.FinalScore1;
            match.TySoDoi2 = evaluation.FinalScore2;
            match.DiemPenaltyDoi1 = evaluation.PenaltyScore1;
            match.DiemPenaltyDoi2 = evaluation.PenaltyScore2;
            match.IsHoa = evaluation.IsDraw;
            match.TrangThai = "KetThuc";
            match.ThoiGianKetThuc = DateTime.UtcNow;
            match.DoiThangDangKyId = winnerDangKyId > 0 ? winnerDangKyId : null;
            match.DoiThuaDangKyId = loserDangKyId > 0 ? loserDangKyId : null;
            match.TrangThaiDuyetKetQua = "ChoDuyet";
            match.ThoiGianDuyetKetQua = null;
            match.NguoiDuyetKetQua = null;
            match.GhiChuDuyetKetQua = null;

            // Lưu điểm chi tiết / events vào GhiChu JSON
            var scoreData = new
            {
                score1 = request.Score1,
                score2 = request.Score2,
                finalScore1 = evaluation.FinalScore1,
                finalScore2 = evaluation.FinalScore2,
                penalty1 = evaluation.PenaltyScore1,
                penalty2 = evaluation.PenaltyScore2,
                extraTimeScore1 = request.ExtraTimeScore1,
                extraTimeScore2 = request.ExtraTimeScore2,
                winner = evaluation.WinnerTeamIndex == 1 ? "1" : (evaluation.WinnerTeamIndex == 2 ? "2" : "draw"),
                status = "KetThuc",
                notes = request.GhiChu,
                setScores = request.SetScores ?? new List<SetScoreDto>(),
                events = request.Events ?? new List<MatchEventItemDto>(),
                updatedBy = username,
                updatedAt = DateTime.UtcNow
            };
            match.GhiChu = System.Text.Json.JsonSerializer.Serialize(scoreData);
            match.LastModified = DateTime.UtcNow;
            match.LastModifiedBy = username;

            _unitOfWork.TranDaus.Update(match);

            await SaveMatchSetScoresAsync(match, periodScores, username, DateTime.UtcNow, markAllComplete: true);

            await _unitOfWork.CompleteAsync();

            response.WinnerId = winnerDangKyId;
            response.IsDraw = evaluation.IsDraw;

            var winnerDk = winnerDangKyId > 0 ? await _unitOfWork.DangKyThiDaus.GetByIdAsync(winnerDangKyId) : null;
            response.WinnerName = winnerDk?.TenDangKy;

            // 3. Nếu là trận VÒNG BẢNG: Tự động cập nhật bảng xếp hạng
            if (match.BangDauId.HasValue)
            {
                response.IsGroupStage = true;
                await _groupStandingsEngine.RecalculateGroupStandingsAsync(match.BangDauId.Value, config, username);
                response.StandingsUpdateNotice = "Đã cập nhật tỷ số và tự động tính lại Bảng xếp hạng bảng đấu theo đúng tiêu chí cấu hình.";

                try
                {
                    var adv = await AdvanceGroupStageWinnersAsync(match.GiaiDauMonTheThaoId, forceAdvance: false, username: username);
                    if (adv.Success && adv.TotalTeamsAdvanced > 0)
                    {
                        response.StandingsUpdateNotice += $" Đồng thời đã tự động chốt và đưa {adv.TotalTeamsAdvanced} đội vào vòng Knockout!";
                    }
                }
                catch { }

                try
                {
                    var rrMedals = await CheckAndAwardRoundRobinMedalsAsync(match.GiaiDauMonTheThaoId, match.BangDauId.Value, username);
                    if (rrMedals.Any())
                    {
                        response.MedalsAwarded.AddRange(rrMedals);
                        response.StandingsUpdateNotice += $" Bảng đấu đã hoàn thành và tự động trao {rrMedals.Count} huy chương!";
                    }
                }
                catch { }
            }

            // 4. Nếu là trận VÒNG KNOCKOUT: Tự động đưa đội thắng/thua đi tiếp và trao huy chương
            if (isKnockout && winnerDangKyId > 0)
            {
                response.IsKnockout = true;
                var knockoutResult = await _knockoutProgressionEngine.ProcessKnockoutProgressionAsync(
                    match.Id, winnerDangKyId, loserDangKyId, username);

                response.NextMatchNotice = knockoutResult.NextMatchNotice;
                response.MedalsAwarded = knockoutResult.MedalsAwarded;
            }

            response.Success = true;
            response.Message = "Đã xác nhận kết thúc trận đấu và cập nhật tiến trình thành công!";
            return response;
        }

        /// <summary>
        /// Tự động quét và đẩy các đội bóng/VĐV đạt thứ hạng cao từ Vòng Bảng (Top 1, Top 2, Top 3...) 
        /// vào các vị trí tương ứng trong các trận đấu Vòng Loại Trực Tiếp (Knockout: Tứ kết, Bán kết, Chung kết).
        /// </summary>
        /// <param name="giaiDauMonTheThaoId">Mã định danh liên kết giải đấu và môn thi đấu.</param>
        /// <param name="forceAdvance">Nếu true: Cho phép chốt và đẩy theo BXH hiện tại ngay cả khi một số bảng chưa đấu xong 100% số trận.</param>
        /// <param name="username">Tài khoản người thực hiện thao tác.</param>
        /// <returns>Kết quả thực thi chi tiết gồm số lượng trận đấu được gán đội và thông báo trạng thái từng bảng.</returns>
        public async Task<AdvanceGroupStageResultDto> AdvanceGroupStageWinnersAsync(
            int giaiDauMonTheThaoId,
            bool forceAdvance = false,
            string? username = null)
        {
            var result = new AdvanceGroupStageResultDto();

            // 1. Lấy danh sách bảng đấu của môn
            var bangDaus = (await _unitOfWork.BangDaus.FindAsync(
                b => b.GiaiDauMonTheThaoId == giaiDauMonTheThaoId && b.IsDeleted != true
            )).OrderBy(b => b.ThuTu).ToList();

            if (!bangDaus.Any())
            {
                result.Success = false;
                result.Message = "Không tìm thấy bảng đấu nào của môn thi đấu này.";
                return result;
            }

            // 2. Lấy cấu hình thể thức để tính lại BXH
            var gdm = await _unitOfWork.GiaiDauMonTheThaos.GetByIdAsync(giaiDauMonTheThaoId);
            var config = gdm != null ? await _theThucService.GetEffectiveConfigAsync(gdm.MonTheThaoId, giaiDauMonTheThaoId) : null;

            // 3. Lấy toàn bộ trận đấu của môn (bao gồm thành phần trận đấu để cập nhật slot knockout)
            var pagedMatches = await _unitOfWork.TranDaus.GetPagedAsync(
                1,
                10000,
                t => t.GiaiDauMonTheThaoId == giaiDauMonTheThaoId && t.IsDeleted != true,
                null,
                t => t.ThanhPhanTranDaus
            );
            var allMatches = pagedMatches.Items.ToList();

            // Tính toán lại BXH cho từng bảng và kiểm tra tiến độ
            var groupStatusList = new List<(BangDau Bang, bool IsCompleted, int CompletedCount, int TotalCount, List<ThanhVienBang> RankedMembers)>();

            foreach (var b in bangDaus)
            {
                var groupMatches = allMatches.Where(m => m.BangDauId == b.Id).ToList();
                int total = groupMatches.Count;
                int completed = groupMatches.Count(m => m.TrangThai == "KetThuc" || m.TrangThai == "DaDau");
                bool isCompleted = total > 0 && completed == total;

                if (config != null)
                {
                    await _groupStandingsEngine.RecalculateGroupStandingsAsync(b.Id, config, username);
                }

                // Lấy lại danh sách thành viên sau khi tính BXH
                var rankedMembers = (await _unitOfWork.ThanhVienBangs.FindAsync(
                    m => m.BangDauId == b.Id && m.IsDeleted != true
                )).OrderBy(m => m.XepHang ?? 999).ThenBy(m => m.Id).ToList();

                groupStatusList.Add((b, isCompleted, completed, total, rankedMembers));

                if (!isCompleted)
                {
                    result.PendingGroups.Add($"{b.Ten}: Đã đấu {completed}/{total} trận");
                }
            }

            bool allGroupsCompleted = groupStatusList.All(g => g.IsCompleted);
            result.AllGroupsCompleted = allGroupsCompleted;

            if (!allGroupsCompleted && !forceAdvance)
            {
                // Chỉ advance các bảng đã hoàn tất 100% nếu có
                var completedGroups = groupStatusList.Where(g => g.IsCompleted).ToList();
                if (!completedGroups.Any())
                {
                    result.Success = false;
                    result.Message = $"Vòng bảng chưa hoàn tất ({string.Join(", ", result.PendingGroups)}). Vui lòng hoàn thành toàn bộ các trận hoặc bật tùy chọn 'Chốt ngay' để ép buộc đưa đội theo BXH hiện tại.";
                    return result;
                }
            }

            // 4. Lấy danh sách các trận Knockout đầu tiên (trận có placeholder Vòng bảng)
            var knockoutMatches = allMatches
                .Where(m => !m.BangDauId.HasValue && m.TrangThai != "KetThuc" && m.TrangThai != "DangDau")
                .ToList();

            // Nếu chưa có trận Knockout hoặc chưa có trận nào có placeholder Nhất/Nhì bảng -> Tự động khởi tạo ngay các trận Knockout
            if (!knockoutMatches.Any() || !knockoutMatches.Any(m => !string.IsNullOrEmpty(GetPlaceholderTeam(m.GhiChu, m.TenTran, 1))))
            {
                int soDoiMoiBangVongTrong = config?.SoDoiMoiBangVaoVongTrong ?? 2;
                var createdKnockouts = await EnsureKnockoutMatchesExistAsync(giaiDauMonTheThaoId, bangDaus, soDoiMoiBangVongTrong, username);
                if (createdKnockouts.Any())
                {
                    knockoutMatches = createdKnockouts;
                }
            }

            if (!knockoutMatches.Any())
            {
                result.Success = false;
                result.Message = "Không tìm thấy hoặc không thể khởi tạo các trận đấu vòng Knockout.";
                return result;
            }

            // Chuẩn bị map tên đội để cập nhật TenTran thực tế
            var allDangKyIds = groupStatusList.SelectMany(g => g.RankedMembers).Select(m => m.DangKyThiDauId).Distinct().ToList();
            var dks = (await _unitOfWork.DangKyThiDaus.FindAsync(d => allDangKyIds.Contains(d.Id))).ToList();
            var teamNameMap = dks.ToDictionary(d => d.Id, d => d.TenDangKy ?? $"Đội #{d.Id}");

            int matchesUpdated = 0;
            int teamsAdvanced = 0;

            // Xử lý từng trận Knockout
            foreach (var match in knockoutMatches)
            {
                bool matchChanged = false;

                // Lấy placeholder của Vị trí 1 và Vị trí 2
                string? p1 = GetPlaceholderTeam(match.GhiChu, match.TenTran, 1);
                string? p2 = GetPlaceholderTeam(match.GhiChu, match.TenTran, 2);

                if (string.IsNullOrWhiteSpace(p1) && string.IsNullOrWhiteSpace(p2))
                    continue;

                // Slot 1
                if (!string.IsNullOrWhiteSpace(p1))
                {
                    int? teamId = ResolveTeamFromPlaceholder(p1, groupStatusList, forceAdvance);
                    if (teamId.HasValue && teamId.Value > 0)
                    {
                        var tp1 = match.ThanhPhanTranDaus.FirstOrDefault(tp => tp.ViTri == 1 && tp.IsDeleted != true);
                        if (tp1 == null)
                        {
                            var newTp = new ThanhPhanTranDau
                            {
                                TranDauId = match.Id,
                                DangKyThiDauId = teamId.Value,
                                ViTri = 1,
                                TrangThai = "ThamGia",
                                Created = DateTime.UtcNow,
                                CreatedBy = username,
                                IsDeleted = false
                            };
                            await _unitOfWork.ThanhPhanTranDaus.AddAsync(newTp);
                            match.ThanhPhanTranDaus.Add(newTp);
                            matchChanged = true;
                            teamsAdvanced++;
                            var teamName = teamNameMap.GetValueOrDefault(teamId.Value, $"Đội #{teamId.Value}");
                            result.Details.Add($"Gán {p1} -> '{teamName}' vào {match.TenTran} (Vị trí 1)");
                        }
                        else if (tp1.DangKyThiDauId != teamId.Value)
                        {
                            tp1.DangKyThiDauId = teamId.Value;
                            tp1.LastModified = DateTime.UtcNow;
                            tp1.LastModifiedBy = username;
                            _unitOfWork.ThanhPhanTranDaus.Update(tp1);
                            matchChanged = true;
                            teamsAdvanced++;
                            var teamName = teamNameMap.GetValueOrDefault(teamId.Value, $"Đội #{teamId.Value}");
                            result.Details.Add($"Cập nhật {p1} -> '{teamName}' vào {match.TenTran} (Vị trí 1)");
                        }
                    }
                }

                // Slot 2
                if (!string.IsNullOrWhiteSpace(p2))
                {
                    int? teamId = ResolveTeamFromPlaceholder(p2, groupStatusList, forceAdvance);
                    if (teamId.HasValue && teamId.Value > 0)
                    {
                        var tp2 = match.ThanhPhanTranDaus.FirstOrDefault(tp => tp.ViTri == 2 && tp.IsDeleted != true);
                        if (tp2 == null)
                        {
                            var newTp = new ThanhPhanTranDau
                            {
                                TranDauId = match.Id,
                                DangKyThiDauId = teamId.Value,
                                ViTri = 2,
                                TrangThai = "ThamGia",
                                Created = DateTime.UtcNow,
                                CreatedBy = username,
                                IsDeleted = false
                            };
                            await _unitOfWork.ThanhPhanTranDaus.AddAsync(newTp);
                            match.ThanhPhanTranDaus.Add(newTp);
                            matchChanged = true;
                            teamsAdvanced++;
                            var teamName = teamNameMap.GetValueOrDefault(teamId.Value, $"Đội #{teamId.Value}");
                            result.Details.Add($"Gán {p2} -> '{teamName}' vào {match.TenTran} (Vị trí 2)");
                        }
                        else if (tp2.DangKyThiDauId != teamId.Value)
                        {
                            tp2.DangKyThiDauId = teamId.Value;
                            tp2.LastModified = DateTime.UtcNow;
                            tp2.LastModifiedBy = username;
                            _unitOfWork.ThanhPhanTranDaus.Update(tp2);
                            matchChanged = true;
                            teamsAdvanced++;
                            var teamName = teamNameMap.GetValueOrDefault(teamId.Value, $"Đội #{teamId.Value}");
                            result.Details.Add($"Cập nhật {p2} -> '{teamName}' vào {match.TenTran} (Vị trí 2)");
                        }
                    }
                }

                if (matchChanged)
                {
                    // Cập nhật lại TenTran phản ánh tên đội thực tế
                    var t1Id = match.ThanhPhanTranDaus.FirstOrDefault(tp => tp.ViTri == 1 && tp.IsDeleted != true)?.DangKyThiDauId;
                    var t2Id = match.ThanhPhanTranDaus.FirstOrDefault(tp => tp.ViTri == 2 && tp.IsDeleted != true)?.DangKyThiDauId;
                    string name1 = t1Id.HasValue ? teamNameMap.GetValueOrDefault(t1Id.Value, p1 ?? "") : (p1 ?? "Chưa xác định");
                    string name2 = t2Id.HasValue ? teamNameMap.GetValueOrDefault(t2Id.Value, p2 ?? "") : (p2 ?? "Chưa xác định");
                    var prefix = !string.IsNullOrEmpty(match.TenTran) && match.TenTran.Contains(':')
                        ? match.TenTran.Substring(0, match.TenTran.IndexOf(':')).Trim()
                        : (match.VongDau?.Ten ?? "Knockout");
                    match.TenTran = $"{prefix}: {name1} vs {name2}";

                    matchesUpdated++;
                    match.LastModified = DateTime.UtcNow;
                    match.LastModifiedBy = username;
                    _unitOfWork.TranDaus.Update(match);
                }
            }

            if (teamsAdvanced > 0)
            {
                await _unitOfWork.CompleteAsync();
                result.Success = true;
                result.TotalMatchesUpdated = matchesUpdated;
                result.TotalTeamsAdvanced = teamsAdvanced;
                result.Message = $"Đã chốt và tự động đưa {teamsAdvanced} đội từ Vòng bảng vào {matchesUpdated} trận Knockout thành công!";
            }
            else
            {
                result.Success = true;
                result.Message = "Tất cả các đội từ vòng bảng đã được phân bổ vào các trận Knockout từ trước hoặc chưa có bảng nào hoàn tất.";
            }

            return result;
        }

        private static int? ResolveTeamFromPlaceholder(
            string placeholder,
            List<(BangDau Bang, bool IsCompleted, int CompletedCount, int TotalCount, List<ThanhVienBang> RankedMembers)> groupStatusList,
            bool forceAdvance)
        {
            if (string.IsNullOrWhiteSpace(placeholder)) return null;

            int targetRank = 0;
            if (placeholder.Contains("Nhất", StringComparison.OrdinalIgnoreCase)) targetRank = 1;
            else if (placeholder.Contains("Nhì", StringComparison.OrdinalIgnoreCase)) targetRank = 2;
            else if (placeholder.Contains("Ba", StringComparison.OrdinalIgnoreCase) || placeholder.Contains("Hạng 3", StringComparison.OrdinalIgnoreCase)) targetRank = 3;
            else if (placeholder.Contains("Hạng 4", StringComparison.OrdinalIgnoreCase)) targetRank = 4;

            if (targetRank == 0) return null;

            // Tìm bảng đấu tương ứng
            foreach (var g in groupStatusList)
            {
                string bName = g.Bang.Ten ?? string.Empty;
                string bSuffix = bName.Replace("Bảng", "").Trim();

                bool matchGroup = false;
                if (!string.IsNullOrEmpty(bName) && placeholder.Contains(bName, StringComparison.OrdinalIgnoreCase))
                {
                    matchGroup = true;
                }
                else if (!string.IsNullOrEmpty(bSuffix) && 
                    (placeholder.EndsWith(" " + bSuffix, StringComparison.OrdinalIgnoreCase) ||
                     placeholder.Contains(" " + bSuffix + " ", StringComparison.OrdinalIgnoreCase) ||
                     placeholder.Contains("Bảng " + bSuffix, StringComparison.OrdinalIgnoreCase)))
                {
                    matchGroup = true;
                }

                if (matchGroup)
                {
                    if (!g.IsCompleted && !forceAdvance) return null;

                    var member = g.RankedMembers.FirstOrDefault(m => m.XepHang == targetRank);
                    if (member == null && g.RankedMembers.Count >= targetRank)
                    {
                        member = g.RankedMembers[targetRank - 1];
                    }

                    return member?.DangKyThiDauId;
                }
            }

            // Xử lý trường hợp "Đội thứ 3 tốt nhất" (Best 3rd place teams across groups)
            if (placeholder.Contains("thứ 3 tốt nhất", StringComparison.OrdinalIgnoreCase) || placeholder.Contains("hạng 3 tốt nhất", StringComparison.OrdinalIgnoreCase))
            {
                int index = 1;
                if (placeholder.Contains("(1)")) index = 1;
                else if (placeholder.Contains("(2)")) index = 2;
                else if (placeholder.Contains("(3)")) index = 3;
                else if (placeholder.Contains("(4)")) index = 4;

                var thirdPlaceTeams = new List<ThanhVienBang>();
                foreach (var g in groupStatusList)
                {
                    if (!g.IsCompleted && !forceAdvance) continue;
                    var t3 = g.RankedMembers.FirstOrDefault(m => m.XepHang == 3) ?? (g.RankedMembers.Count >= 3 ? g.RankedMembers[2] : null);
                    if (t3 != null) thirdPlaceTeams.Add(t3);
                }

                var rankedThirds = thirdPlaceTeams
                    .OrderByDescending(t => t.Diem)
                    .ThenByDescending(t => t.HieuSo)
                    .ThenByDescending(t => t.DiemGhiDuoc)
                    .ToList();

                if (rankedThirds.Count >= index)
                {
                    return rankedThirds[index - 1].DangKyThiDauId;
                }
            }

            return null;
        }

        /// <summary>
        /// Khởi tạo tự động các trận đấu vòng Knockout (Tứ kết, Bán kết, Chung kết) với các nhãn placeholder
        /// nếu giải đấu chưa có trận Knockout nào.
        /// </summary>
        /// <param name="giaiDauMonTheThaoId">Mã định danh môn thi đấu trong giải</param>
        /// <param name="bangDaus">Danh sách các bảng đấu</param>
        /// <param name="soDoiMoiBangVaoVongTrong">Số đội mỗi bảng vào vòng Knockout</param>
        /// <param name="username">Người thực hiện</param>
        /// <returns>Danh sách các trận đấu Knockout đã được khởi tạo</returns>
        private async Task<List<TranDau>> EnsureKnockoutMatchesExistAsync(
            int giaiDauMonTheThaoId,
            List<BangDau> bangDaus,
            int soDoiMoiBangVaoVongTrong,
            string? username)
        {
            var knockoutMatches = (await _unitOfWork.TranDaus.FindAsync(
                t => t.GiaiDauMonTheThaoId == giaiDauMonTheThaoId && !t.BangDauId.HasValue && t.IsDeleted != true
            )).ToList();

            // Nếu đã có trận Knockout có placeholder thì tái sử dụng
            if (knockoutMatches.Any(m => !string.IsNullOrEmpty(GetPlaceholderTeam(m.GhiChu, m.TenTran, 1))))
            {
                return knockoutMatches;
            }

            // Nếu chưa có, xác định số trận lớn nhất hiện tại
            var allExistingMatches = (await _unitOfWork.TranDaus.FindAsync(
                t => t.GiaiDauMonTheThaoId == giaiDauMonTheThaoId && t.IsDeleted != true
            )).ToList();
            int maxSoTran = allExistingMatches.Any() ? allExistingMatches.Max(m => m.SoTran) : 0;

            // Lấy hoặc tạo các VongDau cho Knockout
            var existingVongs = (await _unitOfWork.VongDaus.FindAsync(
                v => v.GiaiDauMonTheThaoId == giaiDauMonTheThaoId && v.IsDeleted != true
            )).OrderBy(v => v.ThuTu).ToList();

            int maxVongThuTu = existingVongs.Any() ? existingVongs.Max(v => v.ThuTu) : 1;

            async Task<VongDau> GetOrCreateVong(string ten, string loaiVong)
            {
                var found = existingVongs.FirstOrDefault(v => v.Ten.Equals(ten, StringComparison.OrdinalIgnoreCase) || v.LoaiVong == loaiVong);
                if (found != null) return found;

                maxVongThuTu++;
                var newVong = new VongDau
                {
                    GiaiDauMonTheThaoId = giaiDauMonTheThaoId,
                    Ten = ten,
                    LoaiVong = loaiVong,
                    ThuTu = maxVongThuTu,
                    Created = DateTime.UtcNow,
                    CreatedBy = username,
                    IsDeleted = false
                };
                await _unitOfWork.VongDaus.AddAsync(newVong);
                await _unitOfWork.CompleteAsync();
                existingVongs.Add(newVong);
                return newVong;
            }

            int numGroups = bangDaus.Count;
            int soDoiMoiBang = soDoiMoiBangVaoVongTrong > 0 ? soDoiMoiBangVaoVongTrong : 2;
            string GetGName(int idx) => idx < bangDaus.Count ? (bangDaus[idx].Ten ?? $"Bảng {(char)('A' + idx)}") : $"Bảng {(char)('A' + idx)}";

            var createdList = new List<TranDau>();

            // 1. Trường hợp 4 Bảng đấu lấy Top 2 (8 đội vào Tứ kết)
            if (numGroups >= 4 && soDoiMoiBang >= 2)
            {
                var vongTK = await GetOrCreateVong("Tứ kết", "TuKet");
                var vongBK = await GetOrCreateVong("Bán kết", "BanKet");
                var vongCK = await GetOrCreateVong("Chung kết", "ChungKet");

                var tkSpecs = new[]
                {
                    new { Ten = "Tứ kết 1", P1 = $"Nhất {GetGName(0)}", P2 = $"Nhì {GetGName(1)}", Bracket = "TK1" },
                    new { Ten = "Tứ kết 2", P1 = $"Nhất {GetGName(2)}", P2 = $"Nhì {GetGName(3)}", Bracket = "TK2" },
                    new { Ten = "Tứ kết 3", P1 = $"Nhất {GetGName(1)}", P2 = $"Nhì {GetGName(0)}", Bracket = "TK3" },
                    new { Ten = "Tứ kết 4", P1 = $"Nhất {GetGName(3)}", P2 = $"Nhì {GetGName(2)}", Bracket = "TK4" }
                };

                foreach (var spec in tkSpecs)
                {
                    maxSoTran++;
                    var tk = new TranDau
                    {
                        GiaiDauMonTheThaoId = giaiDauMonTheThaoId,
                        VongDauId = vongTK.Id,
                        SoTran = maxSoTran,
                        TenTran = $"{spec.Ten}: {spec.P1} vs {spec.P2}",
                        TrangThai = "ChuaDau",
                        GhiChu = $"TBD: {spec.P1} vs {spec.P2}",
                        MaTranBracket = spec.Bracket,
                        Created = DateTime.UtcNow,
                        CreatedBy = username,
                        IsDeleted = false
                    };
                    await _unitOfWork.TranDaus.AddAsync(tk);
                    createdList.Add(tk);
                }

                // 2 trận Bán kết
                maxSoTran++;
                var bk1 = new TranDau
                {
                    GiaiDauMonTheThaoId = giaiDauMonTheThaoId,
                    VongDauId = vongBK.Id,
                    SoTran = maxSoTran,
                    TenTran = "Bán kết 1: Thắng Tứ kết 1 vs Thắng Tứ kết 2",
                    TrangThai = "ChuaDau",
                    GhiChu = "TBD: Thắng TK 1 vs Thắng TK 2",
                    MaTranBracket = "BK1",
                    Created = DateTime.UtcNow,
                    CreatedBy = username,
                    IsDeleted = false
                };
                await _unitOfWork.TranDaus.AddAsync(bk1);
                createdList.Add(bk1);

                maxSoTran++;
                var bk2 = new TranDau
                {
                    GiaiDauMonTheThaoId = giaiDauMonTheThaoId,
                    VongDauId = vongBK.Id,
                    SoTran = maxSoTran,
                    TenTran = "Bán kết 2: Thắng Tứ kết 3 vs Thắng Tứ kết 4",
                    TrangThai = "ChuaDau",
                    GhiChu = "TBD: Thắng TK 3 vs Thắng TK 4",
                    MaTranBracket = "BK2",
                    Created = DateTime.UtcNow,
                    CreatedBy = username,
                    IsDeleted = false
                };
                await _unitOfWork.TranDaus.AddAsync(bk2);
                createdList.Add(bk2);

                // Tranh 3-4
                maxSoTran++;
                var tranh3 = new TranDau
                {
                    GiaiDauMonTheThaoId = giaiDauMonTheThaoId,
                    VongDauId = vongCK.Id,
                    SoTran = maxSoTran,
                    TenTran = "Trận tranh hạng 3 - 4: Thua Bán kết 1 vs Thua Bán kết 2",
                    TrangThai = "ChuaDau",
                    GhiChu = "TBD: Thua BK 1 vs Thua BK 2",
                    MaTranBracket = "T34",
                    Created = DateTime.UtcNow,
                    CreatedBy = username,
                    IsDeleted = false
                };
                await _unitOfWork.TranDaus.AddAsync(tranh3);
                createdList.Add(tranh3);

                // Chung kết
                maxSoTran++;
                var ck = new TranDau
                {
                    GiaiDauMonTheThaoId = giaiDauMonTheThaoId,
                    VongDauId = vongCK.Id,
                    SoTran = maxSoTran,
                    TenTran = "Chung kết: Thắng Bán kết 1 vs Thắng Bán kết 2",
                    TrangThai = "ChuaDau",
                    GhiChu = "TBD: Thắng BK 1 vs Thắng BK 2",
                    MaTranBracket = "CK",
                    Created = DateTime.UtcNow,
                    CreatedBy = username,
                    IsDeleted = false
                };
                await _unitOfWork.TranDaus.AddAsync(ck);
                createdList.Add(ck);

                await _unitOfWork.CompleteAsync();
            }
            // 2. Trường hợp 2 Bảng đấu (Top 2 mỗi bảng -> 4 đội vào Bán kết)
            else if ((numGroups == 2 && soDoiMoiBang >= 2) || (numGroups == 4 && soDoiMoiBang == 1) || numGroups == 3)
            {
                var vongBK = await GetOrCreateVong("Bán kết", "BanKet");
                var vongCK = await GetOrCreateVong("Chung kết", "ChungKet");

                string bk1_p1, bk1_p2, bk2_p1, bk2_p2;
                if (numGroups == 2)
                {
                    bk1_p1 = $"Nhất {GetGName(0)}"; bk1_p2 = $"Nhì {GetGName(1)}";
                    bk2_p1 = $"Nhất {GetGName(1)}"; bk2_p2 = $"Nhì {GetGName(0)}";
                }
                else if (numGroups == 4)
                {
                    bk1_p1 = $"Nhất {GetGName(0)}"; bk1_p2 = $"Nhất {GetGName(1)}";
                    bk2_p1 = $"Nhất {GetGName(2)}"; bk2_p2 = $"Nhất {GetGName(3)}";
                }
                else
                {
                    bk1_p1 = $"Nhất {GetGName(0)}"; bk1_p2 = "Nhì tốt nhất";
                    bk2_p1 = $"Nhất {GetGName(1)}"; bk2_p2 = $"Nhất {GetGName(2)}";
                }

                maxSoTran++;
                var bk1 = new TranDau
                {
                    GiaiDauMonTheThaoId = giaiDauMonTheThaoId,
                    VongDauId = vongBK.Id,
                    SoTran = maxSoTran,
                    TenTran = $"Bán kết 1: {bk1_p1} vs {bk1_p2}",
                    TrangThai = "ChuaDau",
                    GhiChu = $"TBD: {bk1_p1} vs {bk1_p2}",
                    MaTranBracket = "BK1",
                    Created = DateTime.UtcNow,
                    CreatedBy = username,
                    IsDeleted = false
                };
                await _unitOfWork.TranDaus.AddAsync(bk1);
                createdList.Add(bk1);

                maxSoTran++;
                var bk2 = new TranDau
                {
                    GiaiDauMonTheThaoId = giaiDauMonTheThaoId,
                    VongDauId = vongBK.Id,
                    SoTran = maxSoTran,
                    TenTran = $"Bán kết 2: {bk2_p1} vs {bk2_p2}",
                    TrangThai = "ChuaDau",
                    GhiChu = $"TBD: {bk2_p1} vs {bk2_p2}",
                    MaTranBracket = "BK2",
                    Created = DateTime.UtcNow,
                    CreatedBy = username,
                    IsDeleted = false
                };
                await _unitOfWork.TranDaus.AddAsync(bk2);
                createdList.Add(bk2);

                maxSoTran++;
                var tranh3 = new TranDau
                {
                    GiaiDauMonTheThaoId = giaiDauMonTheThaoId,
                    VongDauId = vongCK.Id,
                    SoTran = maxSoTran,
                    TenTran = "Trận tranh hạng 3 - 4: Thua Bán kết 1 vs Thua Bán kết 2",
                    TrangThai = "ChuaDau",
                    GhiChu = "TBD: Thua BK 1 vs Thua BK 2",
                    MaTranBracket = "T34",
                    Created = DateTime.UtcNow,
                    CreatedBy = username,
                    IsDeleted = false
                };
                await _unitOfWork.TranDaus.AddAsync(tranh3);
                createdList.Add(tranh3);

                maxSoTran++;
                var ck = new TranDau
                {
                    GiaiDauMonTheThaoId = giaiDauMonTheThaoId,
                    VongDauId = vongCK.Id,
                    SoTran = maxSoTran,
                    TenTran = "Chung kết: Thắng Bán kết 1 vs Thắng Bán kết 2",
                    TrangThai = "ChuaDau",
                    GhiChu = "TBD: Thắng BK 1 vs Thắng BK 2",
                    MaTranBracket = "CK",
                    Created = DateTime.UtcNow,
                    CreatedBy = username,
                    IsDeleted = false
                };
                await _unitOfWork.TranDaus.AddAsync(ck);
                createdList.Add(ck);

                await _unitOfWork.CompleteAsync();
            }
            // 3. Trường hợp 2 đội vào thẳng Chung kết
            else
            {
                var vongCK = await GetOrCreateVong("Chung kết", "ChungKet");
                string ck_p1 = $"Nhất {GetGName(0)}";
                string ck_p2 = numGroups >= 2 ? $"Nhất {GetGName(1)}" : $"Nhì {GetGName(0)}";

                maxSoTran++;
                var ck = new TranDau
                {
                    GiaiDauMonTheThaoId = giaiDauMonTheThaoId,
                    VongDauId = vongCK.Id,
                    SoTran = maxSoTran,
                    TenTran = $"Chung kết: {ck_p1} vs {ck_p2}",
                    TrangThai = "ChuaDau",
                    GhiChu = $"TBD: {ck_p1} vs {ck_p2}",
                    MaTranBracket = "CK",
                    Created = DateTime.UtcNow,
                    CreatedBy = username,
                    IsDeleted = false
                };
                await _unitOfWork.TranDaus.AddAsync(ck);
                createdList.Add(ck);

                await _unitOfWork.CompleteAsync();
            }

            return createdList;
        }

        /// <summary>
        /// Tự động kiểm tra và trao huy chương (Vàng, Bạc, Đồng) cho thể thức Vòng Tròn khi tất cả các trận trong bảng đấu đã hoàn thành
        /// </summary>
        /// <param name="giaiDauMonTheThaoId">Mã định danh môn thi đấu trong giải</param>
        /// <param name="bangDauId">Mã định danh bảng đấu</param>
        /// <param name="username">Người thực hiện</param>
        /// <returns>Danh sách các thông báo huy chương được trao</returns>
        private async Task<List<string>> CheckAndAwardRoundRobinMedalsAsync(int giaiDauMonTheThaoId, int bangDauId, string? username = null)
        {
            var notices = new List<string>();
            try
            {
                var gdm = await _unitOfWork.GiaiDauMonTheThaos.GetByIdAsync(giaiDauMonTheThaoId);
                if (gdm == null || gdm.IsDeleted == true) return notices;

                var mon = await _unitOfWork.MonTheThaos.GetByIdAsync(gdm.MonTheThaoId);
                var hinhThuc = mon?.HinhThucThiDau ?? Dms.Domain.Enums.HinhThucThiDau.LoaiTrucTiep;

                // Kiểm tra xem môn này có trận Knockout nào không
                var allMatches = (await _unitOfWork.TranDaus.FindAsync(t => t.GiaiDauMonTheThaoId == giaiDauMonTheThaoId && t.IsDeleted != true)).ToList();
                bool hasKnockout = allMatches.Any(m => !m.BangDauId.HasValue);

                // Chỉ tự động trao huy chương theo vòng bảng nếu là thể thức Vòng Tròn hoặc môn không có vòng Knockout
                if (hinhThuc != Dms.Domain.Enums.HinhThucThiDau.VongBang && hasKnockout)
                {
                    return notices;
                }

                // Kiểm tra các trận của bảng này đã kết thúc hết chưa
                var groupMatches = allMatches.Where(m => m.BangDauId == bangDauId).ToList();
                if (!groupMatches.Any()) return notices;

                bool isCompleted = groupMatches.All(m => m.TrangThai == "KetThuc" || m.TrangThai == "DaDau");
                if (!isCompleted) return notices;

                // Đảm bảo loại huy chương chuẩn tồn tại
                var loaiHcs = (await _unitOfWork.LoaiHuyChuongs.FindAsync(l => l.IsDeleted != true)).ToList();
                var lhcVang = loaiHcs.FirstOrDefault(l => (l.Ma ?? "").ToUpper() == "VANG" || l.ThuTu == 1);
                var lhcBac = loaiHcs.FirstOrDefault(l => (l.Ma ?? "").ToUpper() == "BAC" || l.ThuTu == 2);
                var lhcDong = loaiHcs.FirstOrDefault(l => (l.Ma ?? "").ToUpper() == "DONG" || l.ThuTu == 3);

                bool needSaveLoai = false;
                if (lhcVang == null) { lhcVang = new LoaiHuyChuong { Ma = "VANG", Ten = "Huy chương Vàng", ThuTu = 1, Created = DateTime.UtcNow, IsDeleted = false }; await _unitOfWork.LoaiHuyChuongs.AddAsync(lhcVang); needSaveLoai = true; }
                if (lhcBac == null) { lhcBac = new LoaiHuyChuong { Ma = "BAC", Ten = "Huy chương Bạc", ThuTu = 2, Created = DateTime.UtcNow, IsDeleted = false }; await _unitOfWork.LoaiHuyChuongs.AddAsync(lhcBac); needSaveLoai = true; }
                if (lhcDong == null) { lhcDong = new LoaiHuyChuong { Ma = "DONG", Ten = "Huy chương Đồng", ThuTu = 3, Created = DateTime.UtcNow, IsDeleted = false }; await _unitOfWork.LoaiHuyChuongs.AddAsync(lhcDong); needSaveLoai = true; }
                if (needSaveLoai) await _unitOfWork.CompleteAsync();

                var bang = await _unitOfWork.BangDaus.GetByIdAsync(bangDauId);
                string tenBang = bang?.Ten ?? "Bảng đấu";

                var members = (await _unitOfWork.ThanhVienBangs.FindAsync(m => m.BangDauId == bangDauId && m.IsDeleted != true))
                    .OrderBy(m => m.XepHang ?? 999).ThenByDescending(m => m.Diem).ToList();

                if (!members.Any()) return notices;

                var top1 = members.FirstOrDefault(m => m.XepHang == 1) ?? members[0];
                var top2 = members.FirstOrDefault(m => m.XepHang == 2) ?? (members.Count > 1 ? members[1] : null);
                var top3 = members.FirstOrDefault(m => m.XepHang == 3) ?? (members.Count > 2 ? members[2] : null);

                var dks = (await _unitOfWork.DangKyThiDaus.FindAsync(d => d.GiaiDauMonTheThaoId == giaiDauMonTheThaoId && d.IsDeleted != true)).ToDictionary(d => d.Id);

                async Task UpsertGroupMedal(int dangKyId, int lhcId, int rank, string medalEmoji, string rankText)
                {
                    string name = dks.TryGetValue(dangKyId, out var dk) ? dk.TenDangKy : $"Đội {dangKyId}";
                    var existing = (await _unitOfWork.HuyChuongs.FindAsync(h =>
                        h.GiaiDauMonTheThaoId == giaiDauMonTheThaoId &&
                        h.DangKyThiDauId == dangKyId &&
                        h.IsDeleted != true
                    )).FirstOrDefault();

                    if (existing != null)
                    {
                        existing.LoaiHuyChuongId = lhcId;
                        existing.XepHang = rank;
                        existing.GhiChu = $"{medalEmoji} {rankText} - {tenBang} ({mon?.Ten})";
                        existing.NgayTrao = DateTime.UtcNow;
                        existing.LastModified = DateTime.UtcNow;
                        existing.LastModifiedBy = username;
                        _unitOfWork.HuyChuongs.Update(existing);
                    }
                    else
                    {
                        await _unitOfWork.HuyChuongs.AddAsync(new HuyChuong
                        {
                            GiaiDauId = gdm.GiaiDauId,
                            GiaiDauMonTheThaoId = giaiDauMonTheThaoId,
                            DangKyThiDauId = dangKyId,
                            LoaiHuyChuongId = lhcId,
                            XepHang = rank,
                            NgayTrao = DateTime.UtcNow,
                            GhiChu = $"{medalEmoji} {rankText} - {tenBang} ({mon?.Ten})",
                            Created = DateTime.UtcNow,
                            CreatedBy = username,
                            IsDeleted = false
                        });
                    }
                    notices.Add($"{medalEmoji} {rankText} trao cho '{name}' ({tenBang})");
                }

                if (top1 != null && lhcVang != null) await UpsertGroupMedal(top1.DangKyThiDauId, lhcVang.Id, 1, "🥇", "Huy chương Vàng");
                if (top2 != null && lhcBac != null) await UpsertGroupMedal(top2.DangKyThiDauId, lhcBac.Id, 2, "🥈", "Huy chương Bạc");
                if (top3 != null && lhcDong != null) await UpsertGroupMedal(top3.DangKyThiDauId, lhcDong.Id, 3, "🥉", "Huy chương Đồng");

                await _unitOfWork.CompleteAsync();
            }
            catch { }
            return notices;
        }

        /// <summary>
        /// Khởi tạo hoặc bổ sung danh sách các bảng đấu (Bảng A, B, C...) cho môn thi đấu theo số lượng chỉ định.
        /// </summary>
        /// <param name="giaiDauMonTheThaoId">Mã định danh môn thi đấu trong giải</param>
        /// <param name="targetSoBang">Số lượng bảng đấu cần đảm bảo</param>
        /// <param name="username">Tài khoản người thực hiện thao tác</param>
        /// <returns>Danh sách các bảng đấu sau khi khởi tạo</returns>
        public async Task<List<BangDauDto>> InitBangDausAsync(int giaiDauMonTheThaoId, int targetSoBang, string? username = null)
        {
            var bangDaus = (await _unitOfWork.BangDaus.FindAsync(b => b.GiaiDauMonTheThaoId == giaiDauMonTheThaoId && b.IsDeleted != true))
                .OrderBy(b => b.ThuTu)
                .ToList();

            if (targetSoBang <= 0)
            {
                var gdm = await _unitOfWork.GiaiDauMonTheThaos.GetByIdAsync(giaiDauMonTheThaoId);
                if (gdm != null)
                {
                    var specific = (await _unitOfWork.CauHinhTheThucThiDaus.FindAsync(c => c.GiaiDauMonTheThaoId == giaiDauMonTheThaoId && c.IsDeleted != true)).FirstOrDefault();
                    var def = (await _unitOfWork.CauHinhTheThucThiDaus.FindAsync(c => c.MonTheThaoId == gdm.MonTheThaoId && (c.GiaiDauMonTheThaoId == null || c.GiaiDauMonTheThaoId == 0) && c.IsDeleted != true)).FirstOrDefault();
                    var cfg = (specific != null && specific.SoBang > 0) ? specific : (def ?? specific);
                    targetSoBang = cfg?.SoBang > 0 ? cfg.SoBang : 4;
                }
                else
                {
                    targetSoBang = 4;
                }
            }

            if (bangDaus.Count < targetSoBang)
            {
                for (int i = bangDaus.Count; i < targetSoBang; i++)
                {
                    char groupChar = (char)('A' + i);
                    var newBang = new BangDau
                    {
                        GiaiDauMonTheThaoId = giaiDauMonTheThaoId,
                        Ma = $"BANG_{groupChar}",
                        Ten = $"Bảng {groupChar}",
                        ThuTu = i + 1,
                        Created = DateTime.UtcNow,
                        CreatedBy = username,
                        IsDeleted = false
                    };
                    await _unitOfWork.BangDaus.AddAsync(newBang);
                    bangDaus.Add(newBang);
                }
                await _unitOfWork.CompleteAsync();
            }

            return bangDaus.OrderBy(b => b.ThuTu).Select(b => new BangDauDto
            {
                Id = b.Id,
                GiaiDauMonTheThaoId = b.GiaiDauMonTheThaoId,
                Ma = b.Ma,
                Ten = b.Ten,
                ThuTu = b.ThuTu
            }).ToList();
        }
    }
}
