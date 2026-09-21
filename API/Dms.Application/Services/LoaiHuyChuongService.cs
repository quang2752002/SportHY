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
    /// Service quản lý Loại Huy Chương (LoaiHuyChuong)
    /// </summary>
    public class LoaiHuyChuongService : ILoaiHuyChuongService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public LoaiHuyChuongService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<PagedResult<LoaiHuyChuongDto>> GetPagedAsync(
            int pageIndex,
            int pageSize,
            string? keyword = null)
        {
            var pagedEntities = await _unitOfWork.LoaiHuyChuongs.GetPagedAsync(
                pageIndex,
                pageSize,
                predicate: l => l.IsDeleted != true &&
                                (string.IsNullOrEmpty(keyword) || l.Ten.Contains(keyword) || l.Ma.Contains(keyword)),
                orderBy: q => q.OrderBy(l => l.ThuTu).ThenBy(l => l.Ma),
                includes: l => l.HuyChuongs
            );

            var dtos = _mapper.Map<IEnumerable<LoaiHuyChuongDto>>(pagedEntities.Items);
            return new PagedResult<LoaiHuyChuongDto>(dtos, pagedEntities.TotalCount, pageIndex, pageSize);
        }

        public async Task<IEnumerable<LoaiHuyChuongDto>> GetAllAsync()
        {
            var items = await _unitOfWork.LoaiHuyChuongs.GetPagedAsync(
                pageIndex: 1,
                pageSize: 1000,
                predicate: l => l.IsDeleted != true,
                orderBy: q => q.OrderBy(l => l.ThuTu).ThenBy(l => l.Ma),
                includes: l => l.HuyChuongs
            );
            return _mapper.Map<IEnumerable<LoaiHuyChuongDto>>(items.Items);
        }

        public async Task<LoaiHuyChuongDto?> GetByIdAsync(int id)
        {
            var item = await _unitOfWork.LoaiHuyChuongs.GetByIdAsync(id);
            if (item == null || item.IsDeleted == true) return null;

            return _mapper.Map<LoaiHuyChuongDto>(item);
        }

        public async Task<LoaiHuyChuongDto> CreateAsync(CreateUpdateLoaiHuyChuongDto dto, string? createdBy = null)
        {
            var entity = _mapper.Map<LoaiHuyChuong>(dto);
            entity.Created = DateTime.UtcNow;
            entity.CreatedBy = createdBy;
            entity.IsDeleted = false;

            await _unitOfWork.LoaiHuyChuongs.AddAsync(entity);
            await _unitOfWork.CompleteAsync();

            return _mapper.Map<LoaiHuyChuongDto>(entity);
        }

        public async Task<LoaiHuyChuongDto?> UpdateAsync(int id, CreateUpdateLoaiHuyChuongDto dto, string? updatedBy = null)
        {
            var entity = await _unitOfWork.LoaiHuyChuongs.GetByIdAsync(id);
            if (entity == null || entity.IsDeleted == true) return null;

            _mapper.Map(dto, entity);
            entity.LastModified = DateTime.UtcNow;
            entity.LastModifiedBy = updatedBy;

            _unitOfWork.LoaiHuyChuongs.Update(entity);
            await _unitOfWork.CompleteAsync();

            return _mapper.Map<LoaiHuyChuongDto>(entity);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _unitOfWork.LoaiHuyChuongs.GetByIdAsync(id);
            if (entity == null || entity.IsDeleted == true) return false;

            entity.IsDeleted = true;
            entity.LastModified = DateTime.UtcNow;
            _unitOfWork.LoaiHuyChuongs.Update(entity);
            await _unitOfWork.CompleteAsync();

            return true;
        }
    }
}
