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

                var doi1 = thanhPhanDtos.FirstOrDefault(x => x.ViTri == 1) ?? thanhPhanDtos.ElementAtOrDefault(0);
                var doi2 = thanhPhanDtos.FirstOrDefault(x => x.ViTri == 2) ?? thanhPhanDtos.ElementAtOrDefault(1);

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
        /// Before marking a tied match complete, validates the configured draw, extra-time, or penalty rule;
        /// athletics heats must use the heat-result completion workflow.
        /// </summary>
        /// <param name="id">The match ID to update.</param>
        /// <param name="dto">The score, status, notes, and outcome to persist.</param>
        /// <param name="updatedBy">The account performing the update.</param>
        /// <returns>True when the match was updated; false when it does not exist.</returns>
        public async Task<bool> UpdateMatchProgressAsync(int id, UpdateMatchProgressDto dto, string? updatedBy = null)
        {
            var entity = await _unitOfWork.TranDaus.GetByIdAsync(id);
            if (entity == null || entity.IsDeleted == true) return false;
            if (dto.Score1 < 0 || dto.Score2 < 0 || !new[] { "ChuaDau", "DangDau", "KetThuc" }.Contains(dto.TrangThai))
            {
                throw new ArgumentException("Tỷ số hoặc trạng thái trận đấu không hợp lệ.");
            }

            if (dto.TrangThai == "KetThuc")
            {
                var config = await _theThucService.GetConfigByTranDauIdAsync(id);
                if (config?.LoaiTheThuc == "TinhDiemXepHang")
                {
                    throw new InvalidOperationException("Lượt thi thành tích cần gửi kết quả từng VĐV; không thể chốt bằng cập nhật tỷ số chung.");
                }
                if (dto.Score1 == dto.Score2 && config == null)
                {
                    throw new InvalidOperationException("Không tìm thấy cấu hình thể thức để kiểm tra kết quả hòa.");
                }

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
                            ExtraTimeScore2 = dto.ExtraTimeScore2
                        },
                        config!,
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
            entity.LastModified = nowUtc;
            entity.LastModifiedBy = updatedBy;

            _unitOfWork.TranDaus.Update(entity);
            await _unitOfWork.CompleteAsync();
            return true;
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

            entity.GhiChu = ghiChu;
            entity.LastModified = DateTime.UtcNow;
            entity.LastModifiedBy = updatedBy;
            _unitOfWork.TranDaus.Update(entity);
            await _unitOfWork.CompleteAsync();
            return true;
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
                    XepHang = result?.XepHang
                };
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
            entity.NextTranDauId = dto.NextTranDauId;
            entity.NextTranDauViTri = dto.NextTranDauViTri;
            entity.LoserNextTranDauId = dto.LoserNextTranDauId;
            entity.LoserNextTranDauViTri = dto.LoserNextTranDauViTri;
            entity.MaTranBracket = dto.MaTranBracket;
            entity.LastModified = DateTime.UtcNow;
            entity.LastModifiedBy = updatedBy;

            _unitOfWork.TranDaus.Update(entity);

            // Cập nhật lại ThanhPhanTranDau
            var oldTp = (await _unitOfWork.ThanhPhanTranDaus.FindAsync(tp => tp.TranDauId == id)).ToList();
            foreach (var tp in oldTp)
            {
                _unitOfWork.ThanhPhanTranDaus.Delete(tp);
            }

            if (dto.Doi1DangKyId.HasValue && dto.Doi1DangKyId.Value > 0)
            {
                await _unitOfWork.ThanhPhanTranDaus.AddAsync(new ThanhPhanTranDau
                {
                    TranDauId = id,
                    DangKyThiDauId = dto.Doi1DangKyId.Value,
                    ViTri = 1,
                    TrangThai = "ThamGia",
                    Created = DateTime.UtcNow,
                    CreatedBy = updatedBy,
                    IsDeleted = false
                });
            }

            if (dto.Doi2DangKyId.HasValue && dto.Doi2DangKyId.Value > 0)
            {
                await _unitOfWork.ThanhPhanTranDaus.AddAsync(new ThanhPhanTranDau
                {
                    TranDauId = id,
                    DangKyThiDauId = dto.Doi2DangKyId.Value,
                    ViTri = 2,
                    TrangThai = "ThamGia",
                    Created = DateTime.UtcNow,
                    CreatedBy = updatedBy,
                    IsDeleted = false
                });
            }

            // Cập nhật lại PhanCongTrongTai
            var oldPc = (await _unitOfWork.PhanCongTrongTais.FindAsync(pc => pc.TranDauId == id)).ToList();
            foreach (var pc in oldPc)
            {
                _unitOfWork.PhanCongTrongTais.Delete(pc);
            }

            if (dto.DanhSachTrongTai != null && dto.DanhSachTrongTai.Any())
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

            await _unitOfWork.CompleteAsync();
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
            foreach (var t in trans)
            {
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

            // 5. Xóa các vòng đấu cũ của nội dung này để sinh lại vòng đấu chuẩn theo thể thức mới
            var vongs = (await _unitOfWork.VongDaus.FindAsync(v => v.GiaiDauMonTheThaoId == giaiDauMonTheThaoId)).ToList();
            foreach (var v in vongs)
            {
                _unitOfWork.VongDaus.Delete(v);
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
                d => d.GiaiDauMonTheThaoId == request.GiaiDauMonTheThaoId && d.IsDeleted != true
            )).ToList();

            if (dangKyList.Count < 2)
            {
                result.Success = false;
                result.Message = "Giải đấu - Môn thi đấu cần ít nhất 2 đội/vận động viên để xếp lịch.";
                return result;
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
            else if (effectiveHinhThuc == HinhThucThiDau.TinhDiemXepHang)
            {
                // ====================================================================
                // THỂ THỨC TÍNH ĐIỂM XẾP HẠNG / TÍNH GIỜ (LEADERBOARD)
                // Phân VĐV thành các Lượt thi (Heat) theo số làn / vị trí khả dụng.
                // Phù hợp: Bơi lội, Điền kinh, Cử tạ, Bắn súng...
                // ====================================================================
                int heatSize = request.SoVdvMoiLuotThi > 0 ? request.SoVdvMoiLuotThi : 8;
                int soVong = request.SoVongThi > 0 ? request.SoVongThi : 1;
                string phuongThuc = request.PhuongThucPhanNhom ?? "random";

                var allVdvIds = dangKyList.Select(d => d.Id).ToList();

                // Phân nhóm VDV theo phương thức
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

                // Tạo các Heat cho từng vòng thi
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

            TimeSpan caSangStart = TimeSpan.ParseExact(string.IsNullOrWhiteSpace(request.GioBatDauMoiNgay) ? (cauHinh?.CaSangBatDau ?? "08:00") : request.GioBatDauMoiNgay, "hh\\:mm", CultureInfo.InvariantCulture);
            TimeSpan caSangEnd = TimeSpan.ParseExact(cauHinh?.CaSangKetThuc ?? "11:30", "hh\\:mm", CultureInfo.InvariantCulture);
            TimeSpan caChieuStart = TimeSpan.ParseExact(cauHinh?.CaChieuBatDau ?? "14:00", "hh\\:mm", CultureInfo.InvariantCulture);
            TimeSpan caChieuEnd = TimeSpan.ParseExact(string.IsNullOrWhiteSpace(request.GioKetThucMoiNgay) ? (cauHinh?.CaChieuKetThuc ?? "17:30") : request.GioKetThucMoiNgay, "hh\\:mm", CultureInfo.InvariantCulture);
            TimeSpan? caToStart = !string.IsNullOrEmpty(cauHinh?.CaToBatDau) ? TimeSpan.ParseExact(cauHinh.CaToBatDau, "hh\\:mm", CultureInfo.InvariantCulture) : null;
            TimeSpan? caToEnd = !string.IsNullOrEmpty(cauHinh?.CaToKetThuc) ? TimeSpan.ParseExact(cauHinh.CaToKetThuc, "hh\\:mm", CultureInfo.InvariantCulture) : null;

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
            DateTime tournamentEnd = giaiDau != null ? giaiDau.NgayKetThuc.Date : tournamentStart;
            if (tournamentEnd < tournamentStart) tournamentEnd = tournamentStart;

            int totalDaysAvailable = (int)(tournamentEnd - tournamentStart).TotalDays + 1;
            var distinctRoundOrders = fixtures.Select(f => f.VongThuTu).Distinct().OrderBy(x => x).ToList();

            var roundTargetDate = new Dictionary<int, DateTime>();
            if (effMoiVongMotNgay && totalDaysAvailable > 1 && distinctRoundOrders.Count > 1)
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

            foreach (var f in fixtures)
            {
                var fVdvs = (fixtureVdvMap.GetValueOrDefault(f.Doi1DangKyId) ?? new List<VdvParticipantInfo>())
                    .Union(fixtureVdvMap.GetValueOrDefault(f.Doi2DangKyId) ?? new List<VdvParticipantInfo>())
                    .ToList();
                var fVdvKeys = fVdvs.Select(v => v.IdentityKey).Distinct().ToList();
                if (f.Doi1DangKyId > 0) fVdvKeys.Add($"team_{f.Doi1DangKyId}");
                if (f.Doi2DangKyId > 0) fVdvKeys.Add($"team_{f.Doi2DangKyId}");

                // --- Xác định earliestAllowed dựa trên Macro Target Date ---
                DateTime earliestAllowed = roundTargetDate.TryGetValue(f.VongThuTu, out var targetDate) 
                    ? targetDate.Date.Add(caSangStart)
                    : tournamentStart.Add(caSangStart);

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

                DateTime searchSlot = earliestAllowed;
                bool scheduled = false;
                int attempts = 0;
                DateTime maxLookaheadDate = tournamentEnd.AddDays(14);

                while (!scheduled && searchSlot.Date <= maxLookaheadDate && attempts < 2000)
                {
                    attempts++;

                    // --- CHIA CA THI ĐẤU (SÁNG / CHIỀU / TỐI) ---
                    var tod = searchSlot.TimeOfDay;
                    if (effChiaCaThiDau)
                    {
                        if (tod < caSangStart)
                        {
                            searchSlot = searchSlot.Date.Add(caSangStart);
                            tod = caSangStart;
                        }

                        // Nếu lỡ lọt vào khoảng nghỉ trưa (ví dụ 11:30 - 14:00) -> nhảy lên đầu Ca Chiều
                        if (tod >= caSangEnd && tod < caChieuStart)
                        {
                            searchSlot = searchSlot.Date.Add(caChieuStart);
                            tod = caChieuStart;
                        }

                        // Nếu trận kết thúc vượt quá Ca Chiều
                        if (tod.Add(TimeSpan.FromMinutes(matchMinutes)) > caChieuEnd && tod >= caChieuStart)
                        {
                            if (caToStart.HasValue && caToEnd.HasValue && tod < caToStart.Value)
                            {
                                searchSlot = searchSlot.Date.Add(caToStart.Value);
                                tod = caToStart.Value;
                            }
                            else
                            {
                                // Sang ca sáng ngày hôm sau
                                searchSlot = searchSlot.Date.AddDays(1).Add(caSangStart);
                                continue;
                            }
                        }
                    }
                    else
                    {
                        if (tod < caSangStart) searchSlot = searchSlot.Date.Add(caSangStart);
                        if (tod.Add(TimeSpan.FromMinutes(matchMinutes)) > caChieuEnd)
                        {
                            searchSlot = searchSlot.Date.AddDays(1).Add(caSangStart);
                            continue;
                        }
                    }

                    var matchStart = searchSlot;
                    var matchEnd = searchSlot.AddMinutes(matchMinutes);

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
                        searchSlot = searchSlot.Date.AddDays(1).Add(caSangStart);
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

                    // --- RÀNG BUỘC SÂN ĐẤU ---
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
                        candSan = request.CanBangTaiSanDau
                            ? availableCourts.OrderBy(s => courtMatchCount.GetValueOrDefault(s.Id, 0)).First()
                            : availableCourts.First();
                    }

                    // --- RÀNG BUỘC TRỌNG TÀI ---
                    var assignedReferees = new List<TrongTai>();
                    if (trongTaiList.Any())
                    {
                        int soTt = Math.Min(request.SoTrongTaiMoiTran, trongTaiList.Count);
                        var availableReferees = trongTaiList.Where(tt =>
                        {
                            if (refereeBusy.TryGetValue(tt.Id, out var rBusyList) &&
                                rBusyList.Any(iv => matchStart < iv.end.AddMinutes(effNghiToiThieuTrongTaiPhut) && iv.start < matchEnd.AddMinutes(effNghiToiThieuTrongTaiPhut)))
                                return false;

                            int dayCount = refereeMatchCountPerDay.GetValueOrDefault(tt.Id, new Dictionary<DateTime, int>())
                                                                 .GetValueOrDefault(matchStart.Date, 0);
                            return dayCount < effMaxTTPerDay;
                        }).ToList();

                        if (availableReferees.Count < soTt)
                        {
                            searchSlot = searchSlot.Add(slotDuration);
                            continue;
                        }

                        assignedReferees = request.CanBangTaiTrongTai
                            ? availableReferees.OrderBy(tt => refereeMatchCount.GetValueOrDefault(tt.Id, 0)).Take(soTt).ToList()
                            : availableReferees.Take(soTt).ToList();
                    }

                    // === TẠO TRẬN ĐẤU ===
                    var tran = new TranDau
                    {
                        GiaiDauMonTheThaoId = request.GiaiDauMonTheThaoId,
                        VongDauId = vongDauMap[f.VongTen].Id,
                        BangDauId = f.BangDauId,
                        SanDauId = candSan?.Id,
                        SoTran = matchCounter,
                        TenTran = !string.IsNullOrWhiteSpace(f.TenTran) ? f.TenTran : $"Trận {matchCounter} - {f.VongTen}",
                        ThoiGianDuKien = matchStart,
                        ThoiGianBatDau = matchStart,
                        ThoiGianKetThuc = matchEnd,
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

                    // Gán 2 đội vào trận (chỉ thêm nếu Đội đã xác định > 0 để tránh lỗi khóa ngoại)
                    if (f.IsHeat && f.HeatVdvIds != null && f.HeatVdvIds.Count > 0)
                    {
                        // ==== HEAT: Gán tất cả VĐV trong lượt thi với số làn tương ứng ====
                        for (int lane = 0; lane < f.HeatVdvIds.Count; lane++)
                        {
                            await _unitOfWork.ThanhPhanTranDaus.AddAsync(new ThanhPhanTranDau
                            {
                                TranDauId = tran.Id,
                                DangKyThiDauId = f.HeatVdvIds[lane],
                                SoLane = lane + 1, // Số làn / vị trí (1-based)
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

                    // Gán Trọng tài
                    for (int tIdx = 0; tIdx < assignedReferees.Count; tIdx++)
                    {
                        var tt = assignedReferees[tIdx];
                        string vaiTro = tIdx == 0 ? "TrongTaiChinh" : (tIdx == 1 ? "TrongTaiPhu" : "TrongTaiBan");
                        await _unitOfWork.PhanCongTrongTais.AddAsync(new PhanCongTrongTai
                        {
                            TranDauId = tran.Id,
                            TrongTaiId = tt.Id,
                            VaiTro = vaiTro,
                            Created = DateTime.UtcNow,
                            CreatedBy = createdBy,
                            IsDeleted = false
                        });

                        if (!refereeBusy.ContainsKey(tt.Id)) refereeBusy[tt.Id] = new List<(DateTime, DateTime)>();
                        refereeBusy[tt.Id].Add((matchStart, matchEnd));
                        refereeMatchCount[tt.Id] = refereeMatchCount.GetValueOrDefault(tt.Id, 0) + 1;
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

                    // Cập nhật timeline bận và đếm tải
                    int candSanId = candSan?.Id ?? 0;
                    if (candSanId > 0)
                    {
                        if (!courtBusy.ContainsKey(candSanId)) courtBusy[candSanId] = new List<(DateTime, DateTime)>();
                        courtBusy[candSanId].Add((matchStart, matchEnd));
                        courtMatchCount[candSanId] = courtMatchCount.GetValueOrDefault(candSanId, 0) + 1;
                    }

                    // Cap nhat tracking doi / VDV (chi cho match thuong; Heat da tu xu ly trong block IsHeat tren)
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

                    foreach (var tt in assignedReferees)
                    {
                        if (!refereeMatchCountPerDay.ContainsKey(tt.Id)) refereeMatchCountPerDay[tt.Id] = new Dictionary<DateTime, int>();
                        refereeMatchCountPerDay[tt.Id][matchStart.Date] = refereeMatchCountPerDay[tt.Id].GetValueOrDefault(matchStart.Date, 0) + 1;
                    }

                    if (!roundMaxEndTime.ContainsKey(f.VongThuTu) || matchEnd > roundMaxEndTime[f.VongThuTu])
                    {
                        roundMaxEndTime[f.VongThuTu] = matchEnd;
                    }

                    createdTranList.Add(tran);
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
            if (gdm != null)
            {
                var mon = await _unitOfWork.MonTheThaos.GetByIdAsync(gdm.MonTheThaoId);
                result.TenMonTheThao = mon?.Ten;
                result.HinhThucThiDau = mon?.HinhThucThiDau.ToString();
                result.LaMonDongDoi = mon?.LaMonDongDoi ?? false;
            }

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

            result.BangDaus = bangDaus.Select(b => new BangDauDto
            {
                Id = b.Id,
                GiaiDauMonTheThaoId = b.GiaiDauMonTheThaoId,
                Ma = b.Ma,
                Ten = b.Ten,
                ThuTu = b.ThuTu
            }).ToList();

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
                t => t.ThanhPhanTranDaus
            );

            var assignedTeamIds = new HashSet<int>();
            var existingPairs = new List<ManualMatchPairDto>();

            foreach (var t in pagedTranDau.Items)
            {
                var activeTps = t.ThanhPhanTranDaus?.Where(tp => tp.IsDeleted != true).OrderBy(tp => tp.ViTri ?? tp.SoLane ?? 0).ToList() ?? new List<ThanhPhanTranDau>();
                var tp1 = activeTps.FirstOrDefault(tp => tp.ViTri == 1) ?? activeTps.ElementAtOrDefault(0);
                var tp2 = activeTps.FirstOrDefault(tp => tp.ViTri == 2) ?? (activeTps.Count > 1 ? activeTps.ElementAtOrDefault(1) : null);

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

                bool isHeatSport = result.HinhThucThiDau == "TinhDiemXepHang" || result.HinhThucThiDau == "6";
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
                    Doi1 = doi1,
                    Doi2 = doi2,
                    DanhSachVdv = danhSachVdv
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

            // 1. Nếu là môn đo thành tích (Điền kinh, Bơi lội), chỉ chốt qua bảng kết quả từng VĐV.
            if (config.LoaiTheThuc == "TinhDiemXepHang")
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
                return response;
            }

            // 2. Môn thi đấu đối kháng (Bóng đá, Cầu lông, Bóng chuyền, Bóng bàn...)
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

            var team1 = tps.FirstOrDefault(tp => tp.ViTri == 1) ?? tps.ElementAtOrDefault(0);
            var team2 = tps.FirstOrDefault(tp => tp.ViTri == 2) ?? tps.ElementAtOrDefault(1);

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

            // Lưu chi tiết các hiệp đấu (HiepDau & KetQuaHiepDau)
            if (request.SetScores != null && request.SetScores.Any())
            {
                // Xóa hiệp đấu cũ của trận này nếu có
                var oldHds = (await _unitOfWork.HiepDaus.FindAsync(h => h.TranDauId == match.Id)).ToList();
                foreach (var hd in oldHds)
                {
                    var oldKqs = (await _unitOfWork.KetQuaHiepDaus.FindAsync(kq => kq.HiepDauId == hd.Id)).ToList();
                    foreach (var kq in oldKqs) _unitOfWork.KetQuaHiepDaus.Delete(kq);
                    _unitOfWork.HiepDaus.Delete(hd);
                }

                foreach (var s in request.SetScores)
                {
                    var hiepEntity = new HiepDau
                    {
                        TranDauId = match.Id,
                        SoHiep = s.SetNumber,
                        LoaiHiep = "HiepChinh",
                        TenHiep = $"Hiệp {s.SetNumber}",
                        TrangThai = "KetThuc",
                        Created = DateTime.UtcNow,
                        CreatedBy = username,
                        IsDeleted = false
                    };
                    await _unitOfWork.HiepDaus.AddAsync(hiepEntity);
                    await _unitOfWork.CompleteAsync();

                    if (team1 != null)
                    {
                        await _unitOfWork.KetQuaHiepDaus.AddAsync(new KetQuaHiepDau
                        {
                            HiepDauId = hiepEntity.Id,
                            ThanhPhanTranDauId = team1.Id,
                            Diem = s.Score1,
                            Created = DateTime.UtcNow,
                            CreatedBy = username,
                            IsDeleted = false
                        });
                    }

                    if (team2 != null)
                    {
                        await _unitOfWork.KetQuaHiepDaus.AddAsync(new KetQuaHiepDau
                        {
                            HiepDauId = hiepEntity.Id,
                            ThanhPhanTranDauId = team2.Id,
                            Diem = s.Score2,
                            Created = DateTime.UtcNow,
                            CreatedBy = username,
                            IsDeleted = false
                        });
                    }
                }
            }

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
    }
}
