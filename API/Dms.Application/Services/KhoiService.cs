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
    public class KhoiService : IKhoiService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public KhoiService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<PagedResult<KhoiDto>> GetPagedAsync(
            int pageIndex,
            int pageSize,
            string? keyword = null,
            bool? trangThai = null)
        {
            var pagedEntities = await _unitOfWork.Khois.GetPagedAsync(
                pageIndex,
                pageSize,
                predicate: k => k.IsDeleted != true &&
                                (string.IsNullOrEmpty(keyword) || k.Ten.Contains(keyword) || k.Ma.Contains(keyword)) &&
                                (!trangThai.HasValue || k.TrangThai == trangThai.Value),
                orderBy: q => q.OrderBy(k => k.Ma),
                includes: k => k.DonVis
            );

            var dtos = _mapper.Map<IEnumerable<KhoiDto>>(pagedEntities.Items);
            return new PagedResult<KhoiDto>(dtos, pagedEntities.TotalCount, pageIndex, pageSize);
        }

        public async Task<IEnumerable<KhoiDto>> GetAllAsync()
        {
            var items = await _unitOfWork.Khois.FindAsync(
                k => k.IsDeleted != true && k.TrangThai
            );
            return _mapper.Map<IEnumerable<KhoiDto>>(items.OrderBy(k => k.Ten));
        }

        public async Task<KhoiDto?> GetByIdAsync(int id)
        {
            var entity = await _unitOfWork.Khois.GetByIdAsync(id);
            if (entity == null || entity.IsDeleted == true)
                return null;

            return _mapper.Map<KhoiDto>(entity);
        }

        public async Task<KhoiDto> CreateAsync(CreateUpdateKhoiDto dto, string? createdBy = null)
        {
            var entity = _mapper.Map<Khoi>(dto);
            entity.Created = DateTime.UtcNow;
            entity.CreatedBy = createdBy;
            entity.IsDeleted = false;

            await _unitOfWork.Khois.AddAsync(entity);
            await _unitOfWork.CompleteAsync();

            return _mapper.Map<KhoiDto>(entity);
        }

        public async Task<KhoiDto?> UpdateAsync(int id, CreateUpdateKhoiDto dto, string? updatedBy = null)
        {
            var entity = await _unitOfWork.Khois.GetByIdAsync(id);
            if (entity == null || entity.IsDeleted == true)
                return null;

            _mapper.Map(dto, entity);
            entity.LastModified = DateTime.UtcNow;
            entity.LastModifiedBy = updatedBy;

            _unitOfWork.Khois.Update(entity);
            await _unitOfWork.CompleteAsync();

            return _mapper.Map<KhoiDto>(entity);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _unitOfWork.Khois.GetByIdAsync(id);
            if (entity == null || entity.IsDeleted == true)
                return false;

            // Soft delete
            entity.IsDeleted = true;
            entity.LastModified = DateTime.UtcNow;
            _unitOfWork.Khois.Update(entity);
            await _unitOfWork.CompleteAsync();

            return true;
        }
    }
}
