using AutoMapper;
using Dms.Application.DTOs;
using Dms.Application.Interfaces;
using Dms.Domain.Common;
using Dms.Domain.Entities;
using Dms.Domain.Enums;
using Dms.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dms.Application.Services
{
    /// <summary>
    /// Service xử lý nghiệp vụ quản lý Giải đấu (GiaiDau) sử dụng Enum
    /// </summary>
    public class GiaiDauService : IGiaiDauService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GiaiDauService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        /// <summary>
        /// Lấy danh sách giải đấu có phân trang, hỗ trợ tìm kiếm và lọc trạng thái / phạm vi
        /// </summary>
        public async Task<PagedResult<GiaiDauDto>> GetPagedAsync(
            int pageIndex,
            int pageSize,
            string? keyword = null,
            TrangThaiGiaiDau? trangThai = null,
            PhamViGiaiDau? phamVi = null)
        {
            var pagedEntities = await _unitOfWork.GiaiDaus.GetPagedAsync(
                pageIndex,
                pageSize,
                predicate: g => g.IsDeleted != true &&
                                (string.IsNullOrEmpty(keyword) || g.Ten.Contains(keyword) || g.Ma.Contains(keyword)) &&
                                (!trangThai.HasValue || g.TrangThai == trangThai.Value) &&
                                (!phamVi.HasValue || g.PhamVi == phamVi.Value),
                orderBy: q => q.OrderByDescending(g => g.Created ?? DateTime.MinValue),
                includes: g => g.GiaiDauKhois
            );

            // Nạp thông tin Trưởng ban trọng tài cho các giải đấu
            var truongBanIds = pagedEntities.Items.Where(g => g.TruongBanTrongTaiId.HasValue).Select(g => g.TruongBanTrongTaiId!.Value).Distinct().ToList();
            if (truongBanIds.Any())
            {
                var ttDict = (await _unitOfWork.TrongTais.FindAsync(t => truongBanIds.Contains(t.Id) && t.IsDeleted != true)).ToDictionary(t => t.Id);
                foreach (var entity in pagedEntities.Items)
                {
                    if (entity.TruongBanTrongTaiId.HasValue && ttDict.TryGetValue(entity.TruongBanTrongTaiId.Value, out var tt))
                    {
                        entity.TruongBanTrongTai = tt;
                    }
                }
            }

            var dtos = _mapper.Map<List<GiaiDauDto>>(pagedEntities.Items);

            // Nạp danh sách môn thể thao cho từng giải đấu
            var giaiDauIds = dtos.Select(d => d.Id).ToList();
            if (giaiDauIds.Any())
            {
                var gdmList = await _unitOfWork.GiaiDauMonTheThaos.FindAsync(gm => giaiDauIds.Contains(gm.GiaiDauId) && gm.IsDeleted != true);
                var monIds = gdmList.Select(gm => gm.MonTheThaoId).Distinct().ToList();
                var monList = monIds.Any()
                    ? (await _unitOfWork.MonTheThaos.FindAsync(m => monIds.Contains(m.Id) && m.IsDeleted != true)).ToDictionary(m => m.Id)
                    : new Dictionary<int, Dms.Domain.Entities.MonTheThao>();

                var danhMucIds = monList.Values.Select(m => m.DanhMucId).Distinct().ToList();
                var danhMucDict = danhMucIds.Any()
                    ? (await _unitOfWork.DanhMucMonTheThaos.FindAsync(dm => danhMucIds.Contains(dm.Id) && dm.IsDeleted != true)).ToDictionary(dm => dm.Id, dm => dm.Ten)
                    : new Dictionary<int, string>();

                var groupedMons = gdmList.GroupBy(gm => gm.GiaiDauId).ToDictionary(
                    g => g.Key, 
                    g => g.GroupBy(x => x.MonTheThaoId).Select(x => x.First()).ToList()
                );
                foreach (var dto in dtos)
                {
                    if (groupedMons.TryGetValue(dto.Id, out var items))
                    {
                        dto.MonTheThaoIds = items.Select(x => x.MonTheThaoId).Distinct().ToList();
                        dto.MonTheThaos = items
                            .Where(x => monList.ContainsKey(x.MonTheThaoId))
                            .Select(x =>
                            {
                                var m = monList[x.MonTheThaoId];
                                danhMucDict.TryGetValue(m.DanhMucId, out var tenDM);
                                return new GiaiDauMonTheThaoDto
                                {
                                    Id = x.Id,
                                    MonTheThaoId = m.Id,
                                    DanhMucId = m.DanhMucId,
                                    Ma = m.Ma,
                                    Ten = m.Ten,
                                    MoTa = x.MoTa ?? m.MoTa,
                                    LaMonDongDoi = m.LaMonDongDoi,
                                    TenDanhMuc = tenDM,
                                    HinhThucThiDau = m.HinhThucThiDau.ToString(),
                                    GioiTinh = m.GioiTinh
                                };
                            })
                            .ToList();
                    }
                }
            }

            return new PagedResult<GiaiDauDto>(dtos, pagedEntities.TotalCount, pageIndex, pageSize);
        }

        /// <summary>
        /// Lấy tất cả giải đấu chưa bị xóa
        /// </summary>
        public async Task<IEnumerable<GiaiDauDto>> GetAllAsync()
        {
            var items = (await _unitOfWork.GiaiDaus.FindAsync(g => g.IsDeleted != true)).ToList();

            // Nạp thông tin Trưởng ban trọng tài cho các giải đấu
            var truongBanIds = items.Where(g => g.TruongBanTrongTaiId.HasValue).Select(g => g.TruongBanTrongTaiId!.Value).Distinct().ToList();
            if (truongBanIds.Any())
            {
                var ttDict = (await _unitOfWork.TrongTais.FindAsync(t => truongBanIds.Contains(t.Id) && t.IsDeleted != true)).ToDictionary(t => t.Id);
                foreach (var entity in items)
                {
                    if (entity.TruongBanTrongTaiId.HasValue && ttDict.TryGetValue(entity.TruongBanTrongTaiId.Value, out var tt))
                    {
                        entity.TruongBanTrongTai = tt;
                    }
                }
            }

            var dtos = _mapper.Map<List<GiaiDauDto>>(items.OrderByDescending(g => g.NgayBatDau));

            var giaiDauIds = dtos.Select(d => d.Id).ToList();
            if (giaiDauIds.Any())
            {
                var gdmList = await _unitOfWork.GiaiDauMonTheThaos.FindAsync(gm => giaiDauIds.Contains(gm.GiaiDauId) && gm.IsDeleted != true);
                var monIds = gdmList.Select(gm => gm.MonTheThaoId).Distinct().ToList();
                var monList = monIds.Any()
                    ? (await _unitOfWork.MonTheThaos.FindAsync(m => monIds.Contains(m.Id) && m.IsDeleted != true)).ToDictionary(m => m.Id)
                    : new Dictionary<int, Dms.Domain.Entities.MonTheThao>();

                var danhMucIds = monList.Values.Select(m => m.DanhMucId).Distinct().ToList();
                var danhMucDict = danhMucIds.Any()
                    ? (await _unitOfWork.DanhMucMonTheThaos.FindAsync(dm => danhMucIds.Contains(dm.Id) && dm.IsDeleted != true)).ToDictionary(dm => dm.Id, dm => dm.Ten)
                    : new Dictionary<int, string>();

                var groupedMons = gdmList.GroupBy(gm => gm.GiaiDauId).ToDictionary(
                    g => g.Key, 
                    g => g.GroupBy(x => x.MonTheThaoId).Select(x => x.First()).ToList()
                );
                foreach (var dto in dtos)
                {
                    if (groupedMons.TryGetValue(dto.Id, out var gItems))
                    {
                        dto.MonTheThaoIds = gItems.Select(x => x.MonTheThaoId).Distinct().ToList();
                        dto.MonTheThaos = gItems
                            .Where(x => monList.ContainsKey(x.MonTheThaoId))
                            .Select(x =>
                            {
                                var m = monList[x.MonTheThaoId];
                                danhMucDict.TryGetValue(m.DanhMucId, out var tenDM);
                                return new GiaiDauMonTheThaoDto
                                {
                                    Id = x.Id,
                                    MonTheThaoId = m.Id,
                                    DanhMucId = m.DanhMucId,
                                    Ma = m.Ma,
                                    Ten = m.Ten,
                                    MoTa = x.MoTa ?? m.MoTa,
                                    LaMonDongDoi = m.LaMonDongDoi,
                                    TenDanhMuc = tenDM,
                                    HinhThucThiDau = m.HinhThucThiDau.ToString(),
                                    GioiTinh = m.GioiTinh
                                };
                            })
                            .ToList();
                    }
                }
            }

            return dtos;
        }

        /// <summary>
        /// Lấy chi tiết giải đấu theo Id
        /// </summary>
        public async Task<GiaiDauDto?> GetByIdAsync(int id)
        {
            var list = await _unitOfWork.GiaiDaus.FindAsync(g => g.Id == id && g.IsDeleted != true);
            var entity = list.FirstOrDefault();
            if (entity == null) return null;

            // Load Trưởng ban trọng tài nếu có
            if (entity.TruongBanTrongTaiId.HasValue)
            {
                var ttList = await _unitOfWork.TrongTais.FindAsync(t => t.Id == entity.TruongBanTrongTaiId.Value && t.IsDeleted != true);
                entity.TruongBanTrongTai = ttList.FirstOrDefault();
            }

            // Load kèm các khối tham gia
            var giaiDauKhois = await _unitOfWork.GiaiDauKhois.FindAsync(gk => gk.GiaiDauId == id && gk.IsDeleted != true);
            entity.GiaiDauKhois = giaiDauKhois.ToList();

            // Load kèm các môn thể thao tổ chức (chỉ lấy 1 bản ghi duy nhất cho mỗi môn)
            var giaiDauMons = (await _unitOfWork.GiaiDauMonTheThaos.FindAsync(gm => gm.GiaiDauId == id && gm.IsDeleted != true))
                .GroupBy(gm => gm.MonTheThaoId)
                .Select(g => g.First())
                .ToList();
            entity.GiaiDauMonTheThaos = giaiDauMons;

            // Load kèm các điều lệ giải đấu
            var dieuLes = await _unitOfWork.DieuLeGiaiDaus.FindAsync(dl => dl.GiaiDauId == id && dl.IsDeleted != true);
            entity.DieuLeGiaiDaus = dieuLes.OrderBy(dl => dl.ThuTu).ToList();

            var dto = _mapper.Map<GiaiDauDto>(entity);
            dto.MonTheThaoIds = dto.MonTheThaoIds.Distinct().ToList();

            // Nạp chi tiết môn thể thao
            if (giaiDauMons.Any())
            {
                var monIds = giaiDauMons.Select(x => x.MonTheThaoId).Distinct().ToList();
                var monList = (await _unitOfWork.MonTheThaos.FindAsync(m => monIds.Contains(m.Id) && m.IsDeleted != true)).ToDictionary(m => m.Id);

                var danhMucIds = monList.Values.Select(m => m.DanhMucId).Distinct().ToList();
                var danhMucDict = danhMucIds.Any()
                    ? (await _unitOfWork.DanhMucMonTheThaos.FindAsync(dm => danhMucIds.Contains(dm.Id) && dm.IsDeleted != true)).ToDictionary(dm => dm.Id, dm => dm.Ten)
                    : new Dictionary<int, string>();

                dto.MonTheThaos = giaiDauMons
                    .Where(x => monList.ContainsKey(x.MonTheThaoId))
                    .Select(x =>
                    {
                        var m = monList[x.MonTheThaoId];
                        danhMucDict.TryGetValue(m.DanhMucId, out var tenDM);
                        return new GiaiDauMonTheThaoDto
                        {
                            Id = x.Id,
                            MonTheThaoId = m.Id,
                            DanhMucId = m.DanhMucId,
                            Ma = m.Ma,
                            Ten = m.Ten,
                            MoTa = x.MoTa ?? m.MoTa,
                            LaMonDongDoi = m.LaMonDongDoi,
                            TenDanhMuc = tenDM,
                            HinhThucThiDau = m.HinhThucThiDau.ToString(),
                            GioiTinh = m.GioiTinh
                        };
                    })
                    .ToList();
            }

            return dto;
        }

        /// <summary>
        /// Lấy các điều lệ đang được công bố của một giải và các môn thi đấu thuộc giải đó.
        /// </summary>
        /// <param name="giaiDauId">Mã định danh giải đấu cần tra cứu.</param>
        /// <param name="giaiDauMonTheThaoId">Mã liên kết môn trong giải để lọc riêng một môn; null nghĩa là lấy mọi môn.</param>
        /// <returns>Điều lệ giải cùng điều lệ từng môn đang hoạt động; null nếu giải không công khai hoặc không tồn tại.</returns>
        public async Task<GiaiDauDieuLeCongKhaiDto?> GetPublicRegulationsAsync(
            int giaiDauId,
            int? giaiDauMonTheThaoId = null)
        {
            var tournament = await GetByIdAsync(giaiDauId);
            if (tournament == null ||
                tournament.TrangThai == TrangThaiGiaiDau.Nhap ||
                tournament.TrangThai == TrangThaiGiaiDau.Huy)
            {
                return null;
            }

            var activeTournamentSports = await _unitOfWork.GiaiDauMonTheThaos.FindAsync(item =>
                item.GiaiDauId == giaiDauId && item.IsDeleted != true && item.TrangThai);
            var activeTournamentSportIds = activeTournamentSports
                .Select(item => item.Id)
                .ToHashSet();
            var sports = (tournament.MonTheThaos ?? new List<GiaiDauMonTheThaoDto>())
                .Where(sport => activeTournamentSportIds.Contains(sport.Id))
                .ToList();
            if (giaiDauMonTheThaoId.HasValue && giaiDauMonTheThaoId.Value > 0)
            {
                sports = sports.Where(sport => sport.Id == giaiDauMonTheThaoId.Value).ToList();
            }

            var monIds = sports.Select(sport => sport.MonTheThaoId).Distinct().ToList();
            var regulations = monIds.Count == 0
                ? new List<DieuLeMonTheThao>()
                : (await _unitOfWork.DieuLeMonTheThaos.FindAsync(item =>
                    monIds.Contains(item.MonTheThaoId) && item.IsDeleted != true && item.TrangThai))
                    .OrderBy(item => item.ThuTu)
                    .ThenBy(item => item.Id)
                    .ToList();
            var regulationsBySport = regulations
                .GroupBy(item => item.MonTheThaoId)
                .ToDictionary(group => group.Key, group => _mapper.Map<List<DieuLeMonTheThaoDto>>(group));

            var tournamentRules = tournament.DieuLeGiaiDaus
                .Where(item => item.TrangThai)
                .OrderBy(item => item.ThuTu)
                .ToList();

            return new GiaiDauDieuLeCongKhaiDto
            {
                GiaiDauId = tournament.Id,
                TenGiaiDau = tournament.Ten,
                NgayBatDau = tournament.NgayBatDau,
                NgayKetThuc = tournament.NgayKetThuc,
                DieuLeGiaiDaus = tournamentRules,
                Mons = sports.Select(sport => new MonTheThaoDieuLeCongKhaiDto
                {
                    GiaiDauMonTheThaoId = sport.Id,
                    MonTheThaoId = sport.MonTheThaoId,
                    TenMon = sport.Ten,
                    TenDanhMuc = sport.TenDanhMuc,
                    DieuLes = regulationsBySport.TryGetValue(sport.MonTheThaoId, out var sportRules)
                        ? sportRules
                        : new List<DieuLeMonTheThaoDto>()
                }).ToList()
            };
        }

        /// <summary>
        /// Lấy chi tiết giải đấu theo Slug URL
        /// </summary>
        public async Task<GiaiDauDto?> GetBySlugAsync(string slug)
        {
            if (string.IsNullOrWhiteSpace(slug)) return null;

            var list = await _unitOfWork.GiaiDaus.FindAsync(g => g.Slug == slug && g.IsDeleted != true);
            var entity = list.FirstOrDefault();
            if (entity == null) return null;

            return await GetByIdAsync(entity.Id);
        }

        /// <summary>
        /// Tạo mới giải đấu, thực hiện kiểm tra tính hợp lệ của thời gian tổ chức:
        /// Hạn nộp hồ sơ có thể là ngày hiện tại (có hiệu lực đến hết ngày đó); ngày bắt đầu và ngày kết thúc phải lớn hơn ngày hiện tại;
        /// Hạn nộp hồ sơ phải nhỏ hơn ngày bắt đầu; ngày bắt đầu phải nhỏ hơn ngày kết thúc. Giải phải có ít nhất một môn;
        /// nếu phạm vi tổ chức theo khối thì phải chọn ít nhất một khối tham gia.
        /// </summary>
        /// <param name="dto">Dữ liệu thông tin giải đấu cần tạo mới</param>
        /// <param name="createdBy">Tài khoản thực hiện tạo giải</param>
        /// <returns>Đối tượng thông tin giải đấu đã được lưu</returns>
        public async Task<GiaiDauDto> CreateAsync(CreateUpdateGiaiDauDto dto, string? createdBy = null)
        {
            var today = DateTime.UtcNow.AddHours(7).Date;

            if (dto.NgayBatDau == default)
                throw new ArgumentException("Vui lòng chọn ngày bắt đầu giải đấu.");

            if (dto.NgayKetThuc == default)
                throw new ArgumentException("Vui lòng chọn ngày kết thúc giải đấu.");

            if (!dto.HanDangKy.HasValue || dto.HanDangKy.Value == default)
                throw new ArgumentException("Vui lòng chọn hạn nộp hồ sơ đăng ký.");

            if (dto.HanDangKy.Value.Date < today)
                throw new ArgumentException("Hạn nộp hồ sơ đăng ký không được trước ngày hiện tại.");

            if (dto.NgayBatDau.Date <= today)
                throw new ArgumentException("Ngày bắt đầu giải đấu phải lớn hơn ngày hiện tại.");

            if (dto.NgayKetThuc.Date <= today)
                throw new ArgumentException("Ngày kết thúc giải đấu phải lớn hơn ngày hiện tại.");

            if (dto.HanDangKy.Value.Date >= dto.NgayBatDau.Date)
                throw new ArgumentException("Hạn nộp hồ sơ đăng ký phải nhỏ hơn ngày bắt đầu giải đấu.");

            if (dto.NgayBatDau.Date >= dto.NgayKetThuc.Date)
                throw new ArgumentException("Ngày bắt đầu phải nhỏ hơn ngày kết thúc (ngày kết thúc phải sau ngày bắt đầu).");

            if (dto.MonTheThaoIds == null || !dto.MonTheThaoIds.Any())
                throw new ArgumentException("Vui lòng chọn ít nhất một môn thi đấu cho giải.");

            if (dto.PhamVi == PhamViGiaiDau.TheoKhoi && (dto.KhoiIds == null || !dto.KhoiIds.Any()))
                throw new ArgumentException("Vui lòng chọn ít nhất một khối tham gia khi phạm vi tổ chức theo khối.");

            var entity = _mapper.Map<GiaiDau>(dto);
            entity.Created = DateTime.UtcNow;
            entity.CreatedBy = createdBy;
            entity.IsDeleted = false;

            // Tự động sinh slug nếu chưa có hoặc cập nhật từ tên giải đấu
            var baseSlug = !string.IsNullOrWhiteSpace(dto.Slug)
                ? Dms.Application.Common.SlugHelper.GenerateSlug(dto.Slug)
                : Dms.Application.Common.SlugHelper.GenerateSlug(dto.Ten);

            entity.Slug = baseSlug;

            await _unitOfWork.GiaiDaus.AddAsync(entity);
            await _unitOfWork.CompleteAsync();

            // Nếu slug bị trùng lặp với giải khác, gắn thêm id để đảm bảo duy nhất
            var duplicate = await _unitOfWork.GiaiDaus.FindAsync(g => g.Slug == entity.Slug && g.Id != entity.Id && g.IsDeleted != true);
            if (duplicate.Any())
            {
                entity.Slug = $"{baseSlug}-{entity.Id}";
                _unitOfWork.GiaiDaus.Update(entity);
                await _unitOfWork.CompleteAsync();
            }

            // 1. Nếu phạm vi là TheoKhoi và có chọn khối
            if (dto.PhamVi == PhamViGiaiDau.TheoKhoi && dto.KhoiIds != null && dto.KhoiIds.Any())
            {
                foreach (var khoiId in dto.KhoiIds)
                {
                    await _unitOfWork.GiaiDauKhois.AddAsync(new GiaiDauKhoi
                    {
                        GiaiDauId = entity.Id,
                        KhoiId = khoiId,
                        Created = DateTime.UtcNow,
                        CreatedBy = createdBy,
                        IsDeleted = false
                    });
                }
            }

            // 2. Lưu danh sách môn thể thao tổ chức
            if (dto.MonTheThaoIds != null && dto.MonTheThaoIds.Any())
            {
                foreach (var monId in dto.MonTheThaoIds.Distinct())
                {
                    await _unitOfWork.GiaiDauMonTheThaos.AddAsync(new GiaiDauMonTheThao
                    {
                        GiaiDauId = entity.Id,
                        MonTheThaoId = monId,
                        Created = DateTime.UtcNow,
                        CreatedBy = createdBy,
                        IsDeleted = false,
                        TrangThai = true
                    });
                }
            }

            // 3. Lưu danh sách điều lệ giải đấu
            if (dto.DieuLes != null && dto.DieuLes.Any())
            {
                int order = 1;
                foreach (var dl in dto.DieuLes)
                {
                    if (string.IsNullOrWhiteSpace(dl.TieuDe)) continue;
                    await _unitOfWork.DieuLeGiaiDaus.AddAsync(new DieuLeGiaiDau
                    {
                        GiaiDauId = entity.Id,
                        TieuDe = dl.TieuDe.Trim(),
                        NoiDung = dl.NoiDung ?? string.Empty,
                        TepDinhKem = dl.TepDinhKem,
                        ThuTu = dl.ThuTu > 0 ? dl.ThuTu : order++,
                        TrangThai = dl.TrangThai,
                        Created = DateTime.UtcNow,
                        CreatedBy = createdBy,
                        IsDeleted = false
                    });
                }
            }

            // 4. Lưu danh sách phân công thư ký nếu có
            if (dto.ThuKyIds != null && dto.ThuKyIds.Any())
            {
                foreach (var tkId in dto.ThuKyIds.Distinct())
                {
                    await _unitOfWork.PhanCongThuKys.AddAsync(new PhanCongThuKy
                    {
                        GiaiDauId = entity.Id,
                        ThuKyId = tkId,
                        Created = DateTime.UtcNow,
                        CreatedBy = createdBy,
                        IsDeleted = false
                    });
                }
            }

            // 5. Lưu danh sách phân công người điều hành môn nếu có
            if (dto.DieuHanhMonAssignments != null && dto.DieuHanhMonAssignments.Any())
            {
                foreach (var assign in dto.DieuHanhMonAssignments)
                {
                    if (assign.NguoiDieuHanhMonId.HasValue)
                    {
                        await _unitOfWork.PhanCongDieuHanhMons.AddAsync(new PhanCongDieuHanhMon
                        {
                            GiaiDauId = entity.Id,
                            DanhMucId = assign.DanhMucId,
                            NguoiDieuHanhMonId = assign.NguoiDieuHanhMonId.Value,
                            Created = DateTime.UtcNow,
                            CreatedBy = createdBy,
                            IsDeleted = false
                        });
                    }
                }
            }

            await _unitOfWork.CompleteAsync();

            return await GetByIdAsync(entity.Id) ?? _mapper.Map<GiaiDauDto>(entity);
        }

        /// <summary>
        /// Cập nhật thông tin giải đấu, thực hiện kiểm tra tính hợp lệ của thời gian tổ chức:
        /// Hạn nộp hồ sơ có thể là ngày hiện tại (có hiệu lực đến hết ngày đó); ngày bắt đầu và ngày kết thúc phải lớn hơn ngày hiện tại;
        /// Hạn nộp hồ sơ phải nhỏ hơn ngày bắt đầu; ngày bắt đầu phải nhỏ hơn ngày kết thúc. Giải phải có ít nhất một môn;
        /// nếu phạm vi tổ chức theo khối thì phải chọn ít nhất một khối tham gia.
        /// </summary>
        /// <param name="id">ID giải đấu cần cập nhật</param>
        /// <param name="dto">Dữ liệu cập nhật giải đấu</param>
        /// <param name="updatedBy">Tài khoản thực hiện cập nhật</param>
        /// <returns>Thông tin giải đấu sau khi cập nhật hoặc null nếu không tìm thấy</returns>
        public async Task<GiaiDauDto?> UpdateAsync(int id, CreateUpdateGiaiDauDto dto, string? updatedBy = null)
        {
            var today = DateTime.UtcNow.AddHours(7).Date;

            if (dto.NgayBatDau == default)
                throw new ArgumentException("Vui lòng chọn ngày bắt đầu giải đấu.");

            if (dto.NgayKetThuc == default)
                throw new ArgumentException("Vui lòng chọn ngày kết thúc giải đấu.");

            if (!dto.HanDangKy.HasValue || dto.HanDangKy.Value == default)
                throw new ArgumentException("Vui lòng chọn hạn nộp hồ sơ đăng ký.");

            if (dto.HanDangKy.Value.Date < today)
                throw new ArgumentException("Hạn nộp hồ sơ đăng ký không được trước ngày hiện tại.");

            if (dto.NgayBatDau.Date <= today)
                throw new ArgumentException("Ngày bắt đầu giải đấu phải lớn hơn ngày hiện tại.");

            if (dto.NgayKetThuc.Date <= today)
                throw new ArgumentException("Ngày kết thúc giải đấu phải lớn hơn ngày hiện tại.");

            if (dto.HanDangKy.Value.Date >= dto.NgayBatDau.Date)
                throw new ArgumentException("Hạn nộp hồ sơ đăng ký phải nhỏ hơn ngày bắt đầu giải đấu.");

            if (dto.NgayBatDau.Date >= dto.NgayKetThuc.Date)
                throw new ArgumentException("Ngày bắt đầu phải nhỏ hơn ngày kết thúc (ngày kết thúc phải sau ngày bắt đầu).");

            if (dto.MonTheThaoIds == null || !dto.MonTheThaoIds.Any())
                throw new ArgumentException("Vui lòng chọn ít nhất một môn thi đấu cho giải.");

            if (dto.PhamVi == PhamViGiaiDau.TheoKhoi && (dto.KhoiIds == null || !dto.KhoiIds.Any()))
                throw new ArgumentException("Vui lòng chọn ít nhất một khối tham gia khi phạm vi tổ chức theo khối.");

            var list = await _unitOfWork.GiaiDaus.FindAsync(g => g.Id == id && g.IsDeleted != true);
            var entity = list.FirstOrDefault();
            if (entity == null) return null;

            _mapper.Map(dto, entity);
            entity.LastModified = DateTime.UtcNow;
            entity.LastModifiedBy = updatedBy;

            // Cập nhật slug nếu được truyền vào hoặc sinh lại theo tên
            var baseSlug = !string.IsNullOrWhiteSpace(dto.Slug)
                ? Dms.Application.Common.SlugHelper.GenerateSlug(dto.Slug)
                : Dms.Application.Common.SlugHelper.GenerateSlug(entity.Ten);

            entity.Slug = baseSlug;

            // Kiểm tra trùng lặp slug với giải khác
            var duplicate = await _unitOfWork.GiaiDaus.FindAsync(g => g.Slug == entity.Slug && g.Id != entity.Id && g.IsDeleted != true);
            if (duplicate.Any())
            {
                entity.Slug = $"{baseSlug}-{entity.Id}";
            }

            _unitOfWork.GiaiDaus.Update(entity);

            // 1. Cập nhật lại GiaiDauKhoi nếu là TheoKhoi
            var existingKhois = await _unitOfWork.GiaiDauKhois.FindAsync(gk => gk.GiaiDauId == id);
            foreach (var existing in existingKhois)
            {
                _unitOfWork.GiaiDauKhois.Delete(existing);
            }

            if (dto.PhamVi == PhamViGiaiDau.TheoKhoi && dto.KhoiIds != null && dto.KhoiIds.Any())
            {
                foreach (var khoiId in dto.KhoiIds)
                {
                    await _unitOfWork.GiaiDauKhois.AddAsync(new GiaiDauKhoi
                    {
                        GiaiDauId = entity.Id,
                        KhoiId = khoiId,
                        Created = DateTime.UtcNow,
                        CreatedBy = updatedBy,
                        IsDeleted = false
                    });
                }
            }

            // 2. Cập nhật lại danh sách môn thể thao tổ chức (Diff sync có kiểm tra ràng buộc thi đấu)
            var existingMons = (await _unitOfWork.GiaiDauMonTheThaos.FindAsync(gm => gm.GiaiDauId == id)).ToList();
            var targetMonIds = (dto.MonTheThaoIds ?? new List<int>()).Distinct().ToList();

            // Xử lý các môn không còn được chọn (bị bỏ tích)
            var monsToRemove = existingMons.Where(m => m.IsDeleted != true && !targetMonIds.Contains(m.MonTheThaoId)).ToList();
            foreach (var toRemove in monsToRemove)
            {
                var hasDangKy = (await _unitOfWork.DangKyThiDaus.FindAsync(dk => dk.GiaiDauMonTheThaoId == toRemove.Id && dk.IsDeleted != true)).Any();
                var hasTranDau = (await _unitOfWork.TranDaus.FindAsync(td => td.GiaiDauMonTheThaoId == toRemove.Id && td.IsDeleted != true)).Any();
                var hasBangDau = (await _unitOfWork.BangDaus.FindAsync(bd => bd.GiaiDauMonTheThaoId == toRemove.Id && bd.IsDeleted != true)).Any();

                if (hasDangKy || hasTranDau || hasBangDau)
                {
                    var mon = (await _unitOfWork.MonTheThaos.FindAsync(m => m.Id == toRemove.MonTheThaoId)).FirstOrDefault();
                    var tenMon = mon?.Ten ?? $"ID {toRemove.MonTheThaoId}";
                    var details = new List<string>();
                    if (hasDangKy) details.Add("hồ sơ đội/VĐV đăng ký");
                    if (hasTranDau) details.Add("trận đấu đã xếp lịch");
                    if (hasBangDau) details.Add("bảng đấu");

                    throw new InvalidOperationException(
                        $"Không thể bỏ chọn môn '{tenMon}' vì đang có {string.Join(", ", details)}. " +
                        $"Vui lòng xóa các dữ liệu thi đấu này trước khi bỏ chọn môn."
                    );
                }

                // Xóa mềm liên kết GiaiDauMonTheThao
                toRemove.IsDeleted = true;
                toRemove.LastModified = DateTime.UtcNow;
                toRemove.LastModifiedBy = updatedBy;
                _unitOfWork.GiaiDauMonTheThaos.Update(toRemove);
            }

            // Thêm mới hoặc khôi phục các môn được chọn
            foreach (var monId in targetMonIds)
            {
                var matchingGdms = existingMons.Where(m => m.MonTheThaoId == monId).ToList();
                if (matchingGdms.Any())
                {
                    var primary = matchingGdms.First();
                    if (primary.IsDeleted == true)
                    {
                        primary.IsDeleted = false;
                        primary.LastModified = DateTime.UtcNow;
                        primary.LastModifiedBy = updatedBy;
                        _unitOfWork.GiaiDauMonTheThaos.Update(primary);
                    }
                    // Nếu có các bản ghi phụ bị trùng lặp, đảm bảo chúng bị xóa mềm
                    foreach (var dup in matchingGdms.Skip(1))
                    {
                        if (dup.IsDeleted != true)
                        {
                            dup.IsDeleted = true;
                            dup.LastModified = DateTime.UtcNow;
                            dup.LastModifiedBy = updatedBy;
                            _unitOfWork.GiaiDauMonTheThaos.Update(dup);
                        }
                    }
                }
                else
                {
                    await _unitOfWork.GiaiDauMonTheThaos.AddAsync(new GiaiDauMonTheThao
                    {
                        GiaiDauId = entity.Id,
                        MonTheThaoId = monId,
                        Created = DateTime.UtcNow,
                        CreatedBy = updatedBy,
                        IsDeleted = false,
                        TrangThai = true
                    });
                }
            }

            // 3. Cập nhật lại danh sách điều lệ giải đấu
            var existingDieuLes = await _unitOfWork.DieuLeGiaiDaus.FindAsync(dl => dl.GiaiDauId == id);
            foreach (var existing in existingDieuLes)
            {
                _unitOfWork.DieuLeGiaiDaus.Delete(existing);
            }

            if (dto.DieuLes != null && dto.DieuLes.Any())
            {
                int order = 1;
                foreach (var dl in dto.DieuLes)
                {
                    if (string.IsNullOrWhiteSpace(dl.TieuDe)) continue;
                    await _unitOfWork.DieuLeGiaiDaus.AddAsync(new DieuLeGiaiDau
                    {
                        GiaiDauId = entity.Id,
                        TieuDe = dl.TieuDe.Trim(),
                        NoiDung = dl.NoiDung ?? string.Empty,
                        TepDinhKem = dl.TepDinhKem,
                        ThuTu = dl.ThuTu > 0 ? dl.ThuTu : order++,
                        TrangThai = dl.TrangThai,
                        Created = DateTime.UtcNow,
                        CreatedBy = updatedBy,
                        IsDeleted = false
                    });
                }
            }

            // 4. Cập nhật danh sách phân công thư ký nếu có
            if (dto.ThuKyIds != null)
            {
                var existingThuKys = (await _unitOfWork.PhanCongThuKys.FindAsync(p => p.GiaiDauId == id)).ToList();
                var targetTkIds = dto.ThuKyIds.Distinct().ToHashSet();

                foreach (var existing in existingThuKys.Where(p => p.IsDeleted != true && !targetTkIds.Contains(p.ThuKyId)))
                {
                    existing.IsDeleted = true;
                    existing.LastModified = DateTime.UtcNow;
                    existing.LastModifiedBy = updatedBy;
                    _unitOfWork.PhanCongThuKys.Update(existing);
                }

                foreach (var tkId in targetTkIds)
                {
                    var existing = existingThuKys.FirstOrDefault(p => p.ThuKyId == tkId);
                    if (existing != null)
                    {
                        if (existing.IsDeleted == true)
                        {
                            existing.IsDeleted = false;
                            existing.LastModified = DateTime.UtcNow;
                            existing.LastModifiedBy = updatedBy;
                            _unitOfWork.PhanCongThuKys.Update(existing);
                        }
                    }
                    else
                    {
                        await _unitOfWork.PhanCongThuKys.AddAsync(new PhanCongThuKy
                        {
                            GiaiDauId = id,
                            ThuKyId = tkId,
                            Created = DateTime.UtcNow,
                            CreatedBy = updatedBy,
                            IsDeleted = false
                        });
                    }
                }
            }

            // 5. Cập nhật phân công người điều hành môn nếu có
            if (dto.DieuHanhMonAssignments != null)
            {
                var existingAssignments = (await _unitOfWork.PhanCongDieuHanhMons.FindAsync(p => p.GiaiDauId == id)).ToList();
                foreach (var assign in dto.DieuHanhMonAssignments)
                {
                    var existing = existingAssignments.FirstOrDefault(p => p.DanhMucId == assign.DanhMucId);
                    if (assign.NguoiDieuHanhMonId.HasValue)
                    {
                        if (existing != null)
                        {
                            existing.NguoiDieuHanhMonId = assign.NguoiDieuHanhMonId.Value;
                            existing.IsDeleted = false;
                            existing.LastModified = DateTime.UtcNow;
                            existing.LastModifiedBy = updatedBy;
                            _unitOfWork.PhanCongDieuHanhMons.Update(existing);
                        }
                        else
                        {
                            await _unitOfWork.PhanCongDieuHanhMons.AddAsync(new PhanCongDieuHanhMon
                            {
                                GiaiDauId = id,
                                DanhMucId = assign.DanhMucId,
                                NguoiDieuHanhMonId = assign.NguoiDieuHanhMonId.Value,
                                Created = DateTime.UtcNow,
                                CreatedBy = updatedBy,
                                IsDeleted = false
                            });
                        }
                    }
                    else if (existing != null && existing.IsDeleted != true)
                    {
                        existing.IsDeleted = true;
                        existing.LastModified = DateTime.UtcNow;
                        existing.LastModifiedBy = updatedBy;
                        _unitOfWork.PhanCongDieuHanhMons.Update(existing);
                    }
                }
            }

            await _unitOfWork.CompleteAsync();
            return await GetByIdAsync(entity.Id);
        }

        /// <summary>
        /// Xóa mềm giải đấu
        /// </summary>
        public async Task<bool> DeleteAsync(int id)
        {
            var list = await _unitOfWork.GiaiDaus.FindAsync(g => g.Id == id && g.IsDeleted != true);
            var entity = list.FirstOrDefault();
            if (entity == null) return false;

            entity.IsDeleted = true;
            entity.LastModified = DateTime.UtcNow;
            _unitOfWork.GiaiDaus.Update(entity);
            await _unitOfWork.CompleteAsync();
            return true;
        }
    }
}
