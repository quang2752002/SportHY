using AutoMapper;
using Dms.Application.DTOs;
using Dms.Application.Interfaces;
using Dms.Domain.Common;
using Dms.Domain.Entities;
using Dms.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dms.Application.Services
{
    /// <summary>
    /// Service quản lý Cụm Sân (CumSan)
    /// </summary>
    public class CumSanService : ICumSanService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CumSanService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<PagedResult<CumSanDto>> GetPagedAsync(
            int pageIndex,
            int pageSize,
            string? keyword = null,
            bool? trangThai = null)
        {
            var pagedEntities = await _unitOfWork.CumSans.GetPagedAsync(
                pageIndex,
                pageSize,
                predicate: c => c.IsDeleted != true &&
                                (string.IsNullOrEmpty(keyword) || c.Ten.Contains(keyword) || c.Ma.Contains(keyword) || (c.DiaChi != null && c.DiaChi.Contains(keyword))) &&
                                (!trangThai.HasValue || c.TrangThai == trangThai.Value),
                orderBy: q => q.OrderByDescending(c => c.Created ?? DateTime.MinValue),
                includes: c => c.SanDaus
            );

            var dtos = _mapper.Map<IEnumerable<CumSanDto>>(pagedEntities.Items);
            return new PagedResult<CumSanDto>(dtos, pagedEntities.TotalCount, pageIndex, pageSize);
        }

        public async Task<IEnumerable<CumSanDto>> GetAllAsync()
        {
            var items = await _unitOfWork.CumSans.FindAsync(c => c.IsDeleted != true && c.TrangThai);
            return _mapper.Map<IEnumerable<CumSanDto>>(items.OrderBy(c => c.Ten));
        }

        public async Task<CumSanDto?> GetByIdAsync(int id)
        {
            var item = await _unitOfWork.CumSans.GetByIdAsync(id);
            if (item == null || item.IsDeleted == true) return null;

            return _mapper.Map<CumSanDto>(item);
        }

        public async Task<CumSanDto> CreateAsync(CreateUpdateCumSanDto dto, string? createdBy = null)
        {
            var entity = _mapper.Map<CumSan>(dto);
            entity.Created = DateTime.UtcNow;
            entity.CreatedBy = createdBy;
            entity.IsDeleted = false;

            await _unitOfWork.CumSans.AddAsync(entity);
            await _unitOfWork.CompleteAsync();

            return _mapper.Map<CumSanDto>(entity);
        }

        public async Task<CumSanDto?> UpdateAsync(int id, CreateUpdateCumSanDto dto, string? updatedBy = null)
        {
            var entity = await _unitOfWork.CumSans.GetByIdAsync(id);
            if (entity == null || entity.IsDeleted == true) return null;

            _mapper.Map(dto, entity);
            entity.LastModified = DateTime.UtcNow;
            entity.LastModifiedBy = updatedBy;

            _unitOfWork.CumSans.Update(entity);
            await _unitOfWork.CompleteAsync();

            return _mapper.Map<CumSanDto>(entity);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _unitOfWork.CumSans.GetByIdAsync(id);
            if (entity == null || entity.IsDeleted == true) return false;

            entity.IsDeleted = true;
            entity.LastModified = DateTime.UtcNow;
            _unitOfWork.CumSans.Update(entity);
            await _unitOfWork.CompleteAsync();

            return true;
        }
    }

    /// <summary>
    /// Service quản lý Sân Đấu (SanDau)
    /// </summary>
    public class SanDauService : ISanDauService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public SanDauService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<PagedResult<SanDauDto>> GetPagedAsync(
            int pageIndex,
            int pageSize,
            string? keyword = null,
            int? cumSanId = null,
            int? monTheThaoId = null,
            bool? trangThai = null)
        {
            var pagedEntities = await _unitOfWork.SanDaus.GetPagedAsync(
                pageIndex,
                pageSize,
                predicate: s => s.IsDeleted != true &&
                                (string.IsNullOrEmpty(keyword) || s.Ten.Contains(keyword) || s.Ma.Contains(keyword) || (s.LoaiSan != null && s.LoaiSan.Contains(keyword))) &&
                                (!cumSanId.HasValue || s.CumSanId == cumSanId.Value) &&
                                (!monTheThaoId.HasValue || s.MonTheThaoId == monTheThaoId.Value) &&
                                (!trangThai.HasValue || s.TrangThai == trangThai.Value),
                orderBy: q => q.OrderByDescending(s => s.Created ?? DateTime.MinValue),
                includes: new System.Linq.Expressions.Expression<Func<SanDau, object>>[] { s => s.CumSan, s => s.MonTheThao! }
            );

            var dtos = _mapper.Map<IEnumerable<SanDauDto>>(pagedEntities.Items);
            return new PagedResult<SanDauDto>(dtos, pagedEntities.TotalCount, pageIndex, pageSize);
        }

        public async Task<IEnumerable<SanDauDto>> GetAllAsync(int? cumSanId = null, int? monTheThaoId = null)
        {
            var paged = await _unitOfWork.SanDaus.GetPagedAsync(
                pageIndex: 1,
                pageSize: 1000,
                predicate: s => s.IsDeleted != true && s.TrangThai &&
                                (!cumSanId.HasValue || s.CumSanId == cumSanId.Value) &&
                                (!monTheThaoId.HasValue || s.MonTheThaoId == monTheThaoId.Value),
                orderBy: q => q.OrderBy(s => s.Ten),
                includes: new System.Linq.Expressions.Expression<Func<SanDau, object>>[] { s => s.CumSan, s => s.MonTheThao! }
            );
            return _mapper.Map<IEnumerable<SanDauDto>>(paged.Items);
        }

        public async Task<SanDauDto?> GetByIdAsync(int id)
        {
            var paged = await _unitOfWork.SanDaus.GetPagedAsync(
                pageIndex: 1,
                pageSize: 1,
                predicate: s => s.Id == id && s.IsDeleted != true,
                includes: new System.Linq.Expressions.Expression<Func<SanDau, object>>[] { s => s.CumSan, s => s.MonTheThao! }
            );
            var item = paged.Items.FirstOrDefault();
            if (item == null) return null;

            return _mapper.Map<SanDauDto>(item);
        }

        public async Task<SanDauDto> CreateAsync(CreateUpdateSanDauDto dto, string? createdBy = null)
        {
            var entity = _mapper.Map<SanDau>(dto);
            entity.Created = DateTime.UtcNow;
            entity.CreatedBy = createdBy;
            entity.IsDeleted = false;

            await _unitOfWork.SanDaus.AddAsync(entity);
            await _unitOfWork.CompleteAsync();

            return await GetByIdAsync(entity.Id) ?? _mapper.Map<SanDauDto>(entity);
        }

        public async Task<SanDauDto?> UpdateAsync(int id, CreateUpdateSanDauDto dto, string? updatedBy = null)
        {
            var entity = await _unitOfWork.SanDaus.GetByIdAsync(id);
            if (entity == null || entity.IsDeleted == true) return null;

            _mapper.Map(dto, entity);
            entity.LastModified = DateTime.UtcNow;
            entity.LastModifiedBy = updatedBy;

            _unitOfWork.SanDaus.Update(entity);
            await _unitOfWork.CompleteAsync();

            return await GetByIdAsync(entity.Id) ?? _mapper.Map<SanDauDto>(entity);
        }

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
    }
}
