using AutoMapper;
using Dms.Application.DTOs;
using Dms.Application.Interfaces;
using Dms.Domain.Common;
using Dms.Domain.Entities;
using Dms.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Dms.Application.Services
{
    /// <summary>
    /// Service quản lý cụm sân và phạm vi sở hữu của đơn vị.
    /// </summary>
    public class CumSanService : ICumSanService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        /// <summary>
        /// Khởi tạo service quản lý cụm sân.
        /// </summary>
        /// <param name="unitOfWork">Unit of Work dùng để truy cập dữ liệu.</param>
        /// <param name="mapper">Bộ ánh xạ entity và DTO.</param>
        public CumSanService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        /// <inheritdoc />
        public async Task<PagedResult<CumSanDto>> GetPagedAsync(
            int pageIndex,
            int pageSize,
            string? keyword = null,
            bool? trangThai = null,
            int? donViId = null)
        {
            var pagedEntities = await _unitOfWork.CumSans.GetPagedAsync(
                pageIndex,
                pageSize,
                predicate: c => c.IsDeleted != true &&
                                c.DonVi.IsDeleted != true &&
                                (!donViId.HasValue || c.DonViId == donViId.Value) &&
                                (string.IsNullOrEmpty(keyword) || c.Ten.Contains(keyword) || c.Ma.Contains(keyword) || (c.DiaChi != null && c.DiaChi.Contains(keyword))) &&
                                (!trangThai.HasValue || c.TrangThai == trangThai.Value),
                orderBy: q => q.OrderByDescending(c => c.Created ?? DateTime.MinValue),
                includes: new Expression<Func<CumSan, object>>[] { c => c.SanDaus, c => c.DonVi }
            );

            var dtos = _mapper.Map<IEnumerable<CumSanDto>>(pagedEntities.Items);
            return new PagedResult<CumSanDto>(dtos, pagedEntities.TotalCount, pageIndex, pageSize);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<CumSanDto>> GetAllAsync(int? donViId = null)
        {
            var paged = await _unitOfWork.CumSans.GetPagedAsync(
                pageIndex: 1,
                pageSize: 1000,
                predicate: c => c.IsDeleted != true &&
                                c.TrangThai &&
                                c.DonVi.IsDeleted != true &&
                                (!donViId.HasValue || c.DonViId == donViId.Value),
                orderBy: q => q.OrderBy(c => c.Ten),
                includes: new Expression<Func<CumSan, object>>[] { c => c.SanDaus, c => c.DonVi }
            );

            return _mapper.Map<IEnumerable<CumSanDto>>(paged.Items);
        }

        /// <inheritdoc />
        public async Task<CumSanDto?> GetByIdAsync(int id, int? donViId = null)
        {
            var paged = await _unitOfWork.CumSans.GetPagedAsync(
                pageIndex: 1,
                pageSize: 1,
                predicate: c => c.Id == id &&
                                c.IsDeleted != true &&
                                c.DonVi.IsDeleted != true &&
                                (!donViId.HasValue || c.DonViId == donViId.Value),
                includes: new Expression<Func<CumSan, object>>[] { c => c.SanDaus, c => c.DonVi }
            );

            return _mapper.Map<CumSanDto?>(paged.Items.FirstOrDefault());
        }

        /// <inheritdoc />
        public async Task<CumSanDto> CreateAsync(CreateUpdateCumSanDto dto, string? createdBy = null)
        {
            if (!dto.DonViId.HasValue || dto.DonViId.Value <= 0)
            {
                throw new InvalidOperationException("Cụm sân phải được gán cho một đơn vị.");
            }

            await EnsureActiveDonViAsync(dto.DonViId.Value);

            var entity = _mapper.Map<CumSan>(dto);
            entity.DonViId = dto.DonViId.Value;
            entity.Created = DateTime.UtcNow;
            entity.CreatedBy = createdBy;
            entity.IsDeleted = false;

            await _unitOfWork.CumSans.AddAsync(entity);
            await _unitOfWork.CompleteAsync();

            return await GetByIdAsync(entity.Id) ?? _mapper.Map<CumSanDto>(entity);
        }

        /// <inheritdoc />
        public async Task<CumSanDto?> UpdateAsync(int id, CreateUpdateCumSanDto dto, string? updatedBy = null)
        {
            var entity = await _unitOfWork.CumSans.GetByIdAsync(id);
            if (entity == null || entity.IsDeleted == true) return null;

            _mapper.Map(dto, entity);
            entity.LastModified = DateTime.UtcNow;
            entity.LastModifiedBy = updatedBy;

            _unitOfWork.CumSans.Update(entity);
            await _unitOfWork.CompleteAsync();

            return await GetByIdAsync(entity.Id);
        }

        /// <inheritdoc />
        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _unitOfWork.CumSans.GetByIdAsync(id);
            if (entity == null || entity.IsDeleted == true) return false;

            await SoftDeleteChildCourtsAsync(id);
            entity.IsDeleted = true;
            entity.LastModified = DateTime.UtcNow;
            _unitOfWork.CumSans.Update(entity);
            await _unitOfWork.CompleteAsync();

            return true;
        }

        /// <inheritdoc />
        public async Task<CumSanDto> CreateForDonViAsync(CreateUpdateCumSanDto dto, int donViId, string? createdBy = null)
        {
            dto.DonViId = donViId;
            return await CreateAsync(dto, createdBy);
        }

        /// <inheritdoc />
        public async Task<CumSanDto?> UpdateForDonViAsync(int id, CreateUpdateCumSanDto dto, int donViId, string? updatedBy = null)
        {
            var entity = await _unitOfWork.CumSans.GetByIdAsync(id);
            if (entity == null || entity.IsDeleted == true || entity.DonViId != donViId)
            {
                return null;
            }

            _mapper.Map(dto, entity);
            entity.DonViId = donViId;
            entity.LastModified = DateTime.UtcNow;
            entity.LastModifiedBy = updatedBy;

            _unitOfWork.CumSans.Update(entity);
            await _unitOfWork.CompleteAsync();

            return await GetByIdAsync(entity.Id, donViId);
        }

        /// <inheritdoc />
        public async Task<bool> DeleteForDonViAsync(int id, int donViId)
        {
            var entity = await _unitOfWork.CumSans.GetByIdAsync(id);
            if (entity == null || entity.IsDeleted == true || entity.DonViId != donViId)
            {
                return false;
            }

            await SoftDeleteChildCourtsAsync(id);
            entity.IsDeleted = true;
            entity.LastModified = DateTime.UtcNow;
            _unitOfWork.CumSans.Update(entity);
            await _unitOfWork.CompleteAsync();

            return true;
        }

        /// <summary>
        /// Kiểm tra đơn vị tồn tại và đang hoạt động trước khi gán cụm sân.
        /// </summary>
        /// <param name="donViId">ID đơn vị cần kiểm tra.</param>
        /// <returns>Không trả về dữ liệu; ném lỗi nếu đơn vị không hợp lệ.</returns>
        private async Task EnsureActiveDonViAsync(int donViId)
        {
            var donVi = await _unitOfWork.DonVis.GetByIdAsync(donViId);
            if (donVi == null || donVi.IsDeleted == true || !donVi.TrangThai)
            {
                throw new InvalidOperationException("Đơn vị sở hữu không tồn tại hoặc đang ngừng hoạt động.");
            }
        }

        /// <summary>
        /// Xóa mềm toàn bộ sân đấu đang hoạt động thuộc cụm sân bị xóa mềm.
        /// </summary>
        /// <param name="cumSanId">ID cụm sân có các sân con cần cập nhật.</param>
        /// <returns>Không trả về dữ liệu; các sân con được cập nhật trong cùng Unit of Work.</returns>
        private async Task SoftDeleteChildCourtsAsync(int cumSanId)
        {
            var childCourts = await _unitOfWork.SanDaus.FindAsync(s => s.CumSanId == cumSanId && s.IsDeleted != true);
            foreach (var childCourt in childCourts)
            {
                childCourt.IsDeleted = true;
                childCourt.LastModified = DateTime.UtcNow;
                _unitOfWork.SanDaus.Update(childCourt);
            }
        }
    }

    /// <summary>
    /// Service quản lý sân đấu, trong đó ownership được xác định thông qua cụm sân cha.
    /// </summary>
    public class SanDauService : ISanDauService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        /// <summary>
        /// Khởi tạo service quản lý sân đấu.
        /// </summary>
        /// <param name="unitOfWork">Unit of Work dùng để truy cập dữ liệu.</param>
        /// <param name="mapper">Bộ ánh xạ entity và DTO.</param>
        public SanDauService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        /// <inheritdoc />
        public async Task<PagedResult<SanDauDto>> GetPagedAsync(
            int pageIndex,
            int pageSize,
            string? keyword = null,
            int? cumSanId = null,
            int? monTheThaoId = null,
            bool? trangThai = null,
            int? donViId = null)
        {
            var pagedEntities = await _unitOfWork.SanDaus.GetPagedAsync(
                pageIndex,
                pageSize,
                predicate: s => s.IsDeleted != true &&
                                s.CumSan.IsDeleted != true &&
                                s.CumSan.DonVi.IsDeleted != true &&
                                (!donViId.HasValue || s.CumSan.DonViId == donViId.Value) &&
                                (string.IsNullOrEmpty(keyword) || s.Ten.Contains(keyword) || s.Ma.Contains(keyword) || (s.LoaiSan != null && s.LoaiSan.Contains(keyword))) &&
                                (!cumSanId.HasValue || s.CumSanId == cumSanId.Value) &&
                                (!monTheThaoId.HasValue || s.MonTheThaoId == monTheThaoId.Value) &&
                                (!trangThai.HasValue || s.TrangThai == trangThai.Value),
                orderBy: q => q.OrderByDescending(s => s.Created ?? DateTime.MinValue),
                includes: new Expression<Func<SanDau, object>>[] { s => s.CumSan, s => s.MonTheThao! }
            );

            var dtos = _mapper.Map<List<SanDauDto>>(pagedEntities.Items);
            await AttachUnitNamesAsync(dtos);
            return new PagedResult<SanDauDto>(dtos, pagedEntities.TotalCount, pageIndex, pageSize);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<SanDauDto>> GetAllAsync(int? cumSanId = null, int? monTheThaoId = null, int? donViId = null)
        {
            var paged = await _unitOfWork.SanDaus.GetPagedAsync(
                pageIndex: 1,
                pageSize: 1000,
                predicate: s => s.IsDeleted != true &&
                                s.TrangThai &&
                                s.CumSan.IsDeleted != true &&
                                s.CumSan.DonVi.IsDeleted != true &&
                                (!donViId.HasValue || s.CumSan.DonViId == donViId.Value) &&
                                (!cumSanId.HasValue || s.CumSanId == cumSanId.Value) &&
                                (!monTheThaoId.HasValue || s.MonTheThaoId == monTheThaoId.Value),
                orderBy: q => q.OrderBy(s => s.Ten),
                includes: new Expression<Func<SanDau, object>>[] { s => s.CumSan, s => s.MonTheThao! }
            );

            var dtos = _mapper.Map<List<SanDauDto>>(paged.Items);
            await AttachUnitNamesAsync(dtos);
            return dtos;
        }

        /// <inheritdoc />
        public async Task<SanDauDto?> GetByIdAsync(int id, int? donViId = null)
        {
            var paged = await _unitOfWork.SanDaus.GetPagedAsync(
                pageIndex: 1,
                pageSize: 1,
                predicate: s => s.Id == id &&
                                s.IsDeleted != true &&
                                s.CumSan.IsDeleted != true &&
                                s.CumSan.DonVi.IsDeleted != true &&
                                (!donViId.HasValue || s.CumSan.DonViId == donViId.Value),
                includes: new Expression<Func<SanDau, object>>[] { s => s.CumSan, s => s.MonTheThao! }
            );

            var item = paged.Items.FirstOrDefault();
            if (item == null) return null;

            var dto = _mapper.Map<SanDauDto>(item);
            await AttachUnitNamesAsync(new List<SanDauDto> { dto });
            return dto;
        }

        /// <inheritdoc />
        public async Task<SanDauDto> CreateAsync(CreateUpdateSanDauDto dto, string? createdBy = null)
        {
            await EnsureActiveCumSanAsync(dto.CumSanId, null);

            var entity = _mapper.Map<SanDau>(dto);
            entity.Created = DateTime.UtcNow;
            entity.CreatedBy = createdBy;
            entity.IsDeleted = false;

            await _unitOfWork.SanDaus.AddAsync(entity);
            await _unitOfWork.CompleteAsync();

            return await GetByIdAsync(entity.Id) ?? _mapper.Map<SanDauDto>(entity);
        }

        /// <inheritdoc />
        public async Task<SanDauDto?> UpdateAsync(int id, CreateUpdateSanDauDto dto, string? updatedBy = null)
        {
            var entity = await _unitOfWork.SanDaus.GetByIdAsync(id);
            if (entity == null || entity.IsDeleted == true) return null;

            await EnsureActiveCumSanAsync(dto.CumSanId, null);
            _mapper.Map(dto, entity);
            entity.LastModified = DateTime.UtcNow;
            entity.LastModifiedBy = updatedBy;

            _unitOfWork.SanDaus.Update(entity);
            await _unitOfWork.CompleteAsync();

            return await GetByIdAsync(entity.Id);
        }

        /// <inheritdoc />
        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _unitOfWork.SanDaus.GetByIdAsync(id);
            if (entity == null || entity.IsDeleted == true) return false;

            entity.IsDeleted = true;
            entity.LastModified = DateTime.UtcNow;
            _unitOfWork.SanDaus.Update(entity);
            await _unitOfWork.CompleteAsync();

            return true;
        }

        /// <inheritdoc />
        public async Task<SanDauDto> CreateForDonViAsync(CreateUpdateSanDauDto dto, int donViId, string? createdBy = null)
        {
            await EnsureActiveCumSanAsync(dto.CumSanId, donViId);

            var entity = _mapper.Map<SanDau>(dto);
            entity.Created = DateTime.UtcNow;
            entity.CreatedBy = createdBy;
            entity.IsDeleted = false;

            await _unitOfWork.SanDaus.AddAsync(entity);
            await _unitOfWork.CompleteAsync();

            return await GetByIdAsync(entity.Id, donViId) ?? _mapper.Map<SanDauDto>(entity);
        }

        /// <inheritdoc />
        public async Task<SanDauDto?> UpdateForDonViAsync(int id, CreateUpdateSanDauDto dto, int donViId, string? updatedBy = null)
        {
            var entity = await _unitOfWork.SanDaus.GetByIdAsync(id);
            if (entity == null || entity.IsDeleted == true)
            {
                return null;
            }

            var currentCumSan = await _unitOfWork.CumSans.GetByIdAsync(entity.CumSanId);
            if (currentCumSan == null || currentCumSan.IsDeleted == true || currentCumSan.DonViId != donViId)
            {
                return null;
            }

            await EnsureActiveCumSanAsync(dto.CumSanId, donViId);
            _mapper.Map(dto, entity);
            entity.LastModified = DateTime.UtcNow;
            entity.LastModifiedBy = updatedBy;

            _unitOfWork.SanDaus.Update(entity);
            await _unitOfWork.CompleteAsync();

            return await GetByIdAsync(entity.Id, donViId);
        }

        /// <inheritdoc />
        public async Task<bool> DeleteForDonViAsync(int id, int donViId)
        {
            var entity = await _unitOfWork.SanDaus.GetByIdAsync(id);
            if (entity == null || entity.IsDeleted == true)
            {
                return false;
            }

            var currentCumSan = await _unitOfWork.CumSans.GetByIdAsync(entity.CumSanId);
            if (currentCumSan == null || currentCumSan.IsDeleted == true || currentCumSan.DonViId != donViId)
            {
                return false;
            }

            entity.IsDeleted = true;
            entity.LastModified = DateTime.UtcNow;
            _unitOfWork.SanDaus.Update(entity);
            await _unitOfWork.CompleteAsync();

            return true;
        }

        /// <summary>
        /// Kiểm tra cụm sân đang hoạt động và thuộc đúng đơn vị nếu có yêu cầu giới hạn phạm vi.
        /// </summary>
        /// <param name="cumSanId">ID cụm sân cần kiểm tra.</param>
        /// <param name="donViId">ID đơn vị bắt buộc sở hữu cụm sân; null khi kiểm tra toàn hệ thống.</param>
        /// <returns>Không trả về dữ liệu; ném lỗi nếu cụm sân không hợp lệ.</returns>
        private async Task EnsureActiveCumSanAsync(int cumSanId, int? donViId)
        {
            var cumSan = await _unitOfWork.CumSans.GetByIdAsync(cumSanId);
            if (cumSan == null || cumSan.IsDeleted == true || !cumSan.TrangThai || (donViId.HasValue && cumSan.DonViId != donViId.Value))
            {
                throw new InvalidOperationException("Cụm sân không tồn tại, không hoạt động hoặc không thuộc đơn vị hiện tại.");
            }
        }

        /// <summary>
        /// Bổ sung tên đơn vị sở hữu cho DTO sân đấu sau khi truy vấn qua cụm sân.
        /// </summary>
        /// <param name="items">Danh sách DTO sân đấu cần bổ sung tên đơn vị.</param>
        /// <returns>Không trả về dữ liệu; cập nhật trực tiếp các DTO trong danh sách.</returns>
        private async Task AttachUnitNamesAsync(ICollection<SanDauDto> items)
        {
            var donViIds = items
                .Where(item => item.DonViId.HasValue)
                .Select(item => item.DonViId!.Value)
                .Distinct()
                .ToList();

            if (donViIds.Count == 0) return;

            var donVis = await _unitOfWork.DonVis.FindAsync(d => donViIds.Contains(d.Id) && d.IsDeleted != true);
            var nameById = donVis.ToDictionary(d => d.Id, d => d.Ten);

            foreach (var item in items)
            {
                if (item.DonViId.HasValue && nameById.TryGetValue(item.DonViId.Value, out var tenDonVi))
                {
                    item.TenDonVi = tenDonVi;
                }
            }
        }
    }
}
