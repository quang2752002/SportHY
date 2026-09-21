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
    public class DanhMucMonTheThaoService : IDanhMucMonTheThaoService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public DanhMucMonTheThaoService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<PagedResult<DanhMucMonTheThaoDto>> GetPagedAsync(
            int pageIndex,
            int pageSize,
            string? keyword = null,
            bool? trangThai = null)
        {
            var pagedEntities = await _unitOfWork.DanhMucMonTheThaos.GetPagedAsync(
                pageIndex,
                pageSize,
                predicate: d => d.IsDeleted != true &&
                                (string.IsNullOrEmpty(keyword) || d.Ten.Contains(keyword) || d.Ma.Contains(keyword)) &&
                                (!trangThai.HasValue || d.TrangThai == trangThai.Value),
                orderBy: q => q.OrderBy(d => d.Ma),
                includes: d => d.MonTheThaos
            );

            var dtos = _mapper.Map<IEnumerable<DanhMucMonTheThaoDto>>(pagedEntities.Items);
            return new PagedResult<DanhMucMonTheThaoDto>(dtos, pagedEntities.TotalCount, pageIndex, pageSize);
        }

        public async Task<IEnumerable<DanhMucMonTheThaoDto>> GetAllAsync()
        {
            var items = await _unitOfWork.DanhMucMonTheThaos.FindAsync(
                d => d.IsDeleted != true && d.TrangThai
            );
            return _mapper.Map<IEnumerable<DanhMucMonTheThaoDto>>(items.OrderBy(d => d.Ten));
        }

        public async Task<DanhMucMonTheThaoDto?> GetByIdAsync(int id)
        {
            var entity = await _unitOfWork.DanhMucMonTheThaos.GetByIdAsync(id);
            if (entity == null || entity.IsDeleted == true)
                return null;

            return _mapper.Map<DanhMucMonTheThaoDto>(entity);
        }

        public async Task<DanhMucMonTheThaoDto> CreateAsync(CreateUpdateDanhMucMonTheThaoDto dto, string? createdBy = null)
        {
            var entity = _mapper.Map<DanhMucMonTheThao>(dto);
            entity.Created = DateTime.UtcNow;
            entity.CreatedBy = createdBy;
            entity.IsDeleted = false;

            await _unitOfWork.DanhMucMonTheThaos.AddAsync(entity);
            await _unitOfWork.CompleteAsync();

            return _mapper.Map<DanhMucMonTheThaoDto>(entity);
        }

        public async Task<DanhMucMonTheThaoDto?> UpdateAsync(int id, CreateUpdateDanhMucMonTheThaoDto dto, string? updatedBy = null)
        {
            var entity = await _unitOfWork.DanhMucMonTheThaos.GetByIdAsync(id);
            if (entity == null || entity.IsDeleted == true)
                return null;

            _mapper.Map(dto, entity);
            entity.LastModified = DateTime.UtcNow;
            entity.LastModifiedBy = updatedBy;

            _unitOfWork.DanhMucMonTheThaos.Update(entity);
            await _unitOfWork.CompleteAsync();

            return _mapper.Map<DanhMucMonTheThaoDto>(entity);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _unitOfWork.DanhMucMonTheThaos.GetByIdAsync(id);
            if (entity == null || entity.IsDeleted == true)
                return false;

            entity.IsDeleted = true;
            entity.LastModified = DateTime.UtcNow;
            _unitOfWork.DanhMucMonTheThaos.Update(entity);
            await _unitOfWork.CompleteAsync();

            return true;
        }
    }
}
