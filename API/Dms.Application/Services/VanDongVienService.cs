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
    public class VanDongVienService : IVanDongVienService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public VanDongVienService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<PagedResult<VanDongVienDto>> GetPagedAsync(
            int pageIndex,
            int pageSize,
            string? keyword = null,
            int? donViId = null,
            bool? trangThai = null)
        {
            var pagedEntities = await _unitOfWork.VanDongViens.GetPagedAsync(
                pageIndex,
                pageSize,
                predicate: v => v.IsDeleted != true &&
                                (string.IsNullOrEmpty(keyword) || v.HoTen.Contains(keyword) || v.Ma.Contains(keyword) || (v.SoCCCD != null && v.SoCCCD.Contains(keyword)) || (v.SoDienThoai != null && v.SoDienThoai.Contains(keyword))) &&
                                (!donViId.HasValue || v.DonViId == donViId.Value) &&
                                (!trangThai.HasValue || v.TrangThai == trangThai.Value),
                orderBy: q => q.OrderBy(v => v.Ma),
                includes: new System.Linq.Expressions.Expression<Func<VanDongVien, object>>[]
                {
                    v => v.DonVi!
                }
            );

            var dtos = _mapper.Map<IEnumerable<VanDongVienDto>>(pagedEntities.Items);
            return new PagedResult<VanDongVienDto>(dtos, pagedEntities.TotalCount, pageIndex, pageSize);
        }

        public async Task<IEnumerable<VanDongVienDto>> GetAllAsync(int? donViId = null)
        {
            var items = await _unitOfWork.VanDongViens.FindAsync(
                v => v.IsDeleted != true && v.TrangThai &&
                     (!donViId.HasValue || v.DonViId == donViId.Value)
            );
            return _mapper.Map<IEnumerable<VanDongVienDto>>(items.OrderBy(v => v.HoTen));
        }

        public async Task<VanDongVienDto?> GetByIdAsync(int id)
        {
            var entity = await _unitOfWork.VanDongViens.GetByIdAsync(id);
            if (entity == null || entity.IsDeleted == true)
                return null;

            return _mapper.Map<VanDongVienDto>(entity);
        }

        public async Task<VanDongVienDto> CreateAsync(CreateUpdateVanDongVienDto dto, string? createdBy = null)
        {
            var entity = _mapper.Map<VanDongVien>(dto);
            entity.Created = DateTime.UtcNow;
            entity.CreatedBy = createdBy;
            entity.IsDeleted = false;

            await _unitOfWork.VanDongViens.AddAsync(entity);
            await _unitOfWork.CompleteAsync();

            return _mapper.Map<VanDongVienDto>(entity);
        }

        public async Task<VanDongVienDto?> UpdateAsync(int id, CreateUpdateVanDongVienDto dto, string? updatedBy = null)
        {
            var entity = await _unitOfWork.VanDongViens.GetByIdAsync(id);
            if (entity == null || entity.IsDeleted == true)
                return null;

            _mapper.Map(dto, entity);
            entity.LastModified = DateTime.UtcNow;
            entity.LastModifiedBy = updatedBy;

            _unitOfWork.VanDongViens.Update(entity);
            await _unitOfWork.CompleteAsync();

            return _mapper.Map<VanDongVienDto>(entity);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _unitOfWork.VanDongViens.GetByIdAsync(id);
            if (entity == null || entity.IsDeleted == true)
                return false;

            entity.IsDeleted = true;
            entity.LastModified = DateTime.UtcNow;
            _unitOfWork.VanDongViens.Update(entity);
            await _unitOfWork.CompleteAsync();

            return true;
        }
    }
}
