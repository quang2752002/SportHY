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
    public class MonTheThaoService : IMonTheThaoService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public MonTheThaoService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<PagedResult<MonTheThaoDto>> GetPagedAsync(
            int pageIndex,
            int pageSize,
            string? keyword = null,
            int? danhMucId = null,
            bool? trangThai = null,
            string? gioiTinh = null)
        {
            var pagedEntities = await _unitOfWork.MonTheThaos.GetPagedAsync(
                pageIndex,
                pageSize,
                predicate: m => m.IsDeleted != true &&
                                (string.IsNullOrEmpty(keyword) || m.Ten.Contains(keyword) || m.Ma.Contains(keyword)) &&
                                (!danhMucId.HasValue || m.DanhMucId == danhMucId.Value) &&
                                (!trangThai.HasValue || m.TrangThai == trangThai.Value) &&
                                (string.IsNullOrEmpty(gioiTinh) || m.GioiTinh == gioiTinh),
                orderBy: q => q.OrderBy(m => m.Ma),
                includes: m => m.DanhMuc
            );

            var dtos = _mapper.Map<IEnumerable<MonTheThaoDto>>(pagedEntities.Items);
            return new PagedResult<MonTheThaoDto>(dtos, pagedEntities.TotalCount, pageIndex, pageSize);
        }

        public async Task<IEnumerable<MonTheThaoDto>> GetAllAsync(int? danhMucId = null, string? gioiTinh = null)
        {
            var items = await _unitOfWork.MonTheThaos.FindAsync(
                m => m.IsDeleted != true && m.TrangThai &&
                     (!danhMucId.HasValue || m.DanhMucId == danhMucId.Value) &&
                     (string.IsNullOrEmpty(gioiTinh) || m.GioiTinh == gioiTinh)
            );
            return _mapper.Map<IEnumerable<MonTheThaoDto>>(items.OrderBy(m => m.Ten));
        }

        public async Task<MonTheThaoDto?> GetByIdAsync(int id)
        {
            var entity = await _unitOfWork.MonTheThaos.GetByIdAsync(id);
            if (entity == null || entity.IsDeleted == true)
                return null;

            return _mapper.Map<MonTheThaoDto>(entity);
        }

        public async Task<MonTheThaoDto> CreateAsync(CreateUpdateMonTheThaoDto dto, string? createdBy = null)
        {
            var entity = _mapper.Map<MonTheThao>(dto);
            entity.Created = DateTime.UtcNow;
            entity.CreatedBy = createdBy;
            entity.IsDeleted = false;

            await _unitOfWork.MonTheThaos.AddAsync(entity);
            await _unitOfWork.CompleteAsync();

            return _mapper.Map<MonTheThaoDto>(entity);
        }

        public async Task<MonTheThaoDto?> UpdateAsync(int id, CreateUpdateMonTheThaoDto dto, string? updatedBy = null)
        {
            var entity = await _unitOfWork.MonTheThaos.GetByIdAsync(id);
            if (entity == null || entity.IsDeleted == true)
                return null;

            _mapper.Map(dto, entity);
            entity.LastModified = DateTime.UtcNow;
            entity.LastModifiedBy = updatedBy;

            _unitOfWork.MonTheThaos.Update(entity);
            await _unitOfWork.CompleteAsync();

            return _mapper.Map<MonTheThaoDto>(entity);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _unitOfWork.MonTheThaos.GetByIdAsync(id);
            if (entity == null || entity.IsDeleted == true)
                return false;

            entity.IsDeleted = true;
            entity.LastModified = DateTime.UtcNow;
            _unitOfWork.MonTheThaos.Update(entity);
            await _unitOfWork.CompleteAsync();

            return true;
        }
    }
}
