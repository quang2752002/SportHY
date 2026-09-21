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
    /// Service xử lý nghiệp vụ quản lý Trọng tài (TrongTai)
    /// </summary>
    public class TrongTaiService : ITrongTaiService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public TrongTaiService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        /// <summary>
        /// Lấy danh sách trọng tài có phân trang, tìm kiếm và lọc trạng thái
        /// </summary>
        public async Task<PagedResult<TrongTaiDto>> GetPagedAsync(
            int pageIndex,
            int pageSize,
            string? keyword = null,
            bool? trangThai = null)
        {
            var pagedEntities = await _unitOfWork.TrongTais.GetPagedAsync(
                pageIndex,
                pageSize,
                predicate: t => t.IsDeleted != true &&
                                (string.IsNullOrEmpty(keyword) || t.HoTen.Contains(keyword) || t.Ma.Contains(keyword) || (t.SoDienThoai != null && t.SoDienThoai.Contains(keyword))) &&
                                (!trangThai.HasValue || t.TrangThai == trangThai.Value),
                orderBy: q => q.OrderByDescending(t => t.Created ?? DateTime.MinValue)
            );

            var dtos = _mapper.Map<IEnumerable<TrongTaiDto>>(pagedEntities.Items);
            return new PagedResult<TrongTaiDto>(dtos, pagedEntities.TotalCount, pageIndex, pageSize);
        }

        /// <summary>
        /// Lấy tất cả trọng tài chưa bị xóa
        /// </summary>
        public async Task<IEnumerable<TrongTaiDto>> GetAllAsync()
        {
            var items = await _unitOfWork.TrongTais.FindAsync(t => t.IsDeleted != true && t.TrangThai);
            return _mapper.Map<IEnumerable<TrongTaiDto>>(items.OrderBy(t => t.HoTen));
        }

        /// <summary>
        /// Lấy chi tiết trọng tài theo ID
        /// </summary>
        public async Task<TrongTaiDto?> GetByIdAsync(int id)
        {
            var item = await _unitOfWork.TrongTais.GetByIdAsync(id);
            if (item == null || item.IsDeleted == true) return null;

            return _mapper.Map<TrongTaiDto>(item);
        }

        /// <summary>
        /// Thêm mới trọng tài
        /// </summary>
        public async Task<TrongTaiDto> CreateAsync(CreateUpdateTrongTaiDto dto, string? createdBy = null)
        {
            var entity = _mapper.Map<TrongTai>(dto);
            entity.Created = DateTime.UtcNow;
            entity.CreatedBy = createdBy;
            entity.IsDeleted = false;

            await _unitOfWork.TrongTais.AddAsync(entity);
            await _unitOfWork.CompleteAsync();

            return _mapper.Map<TrongTaiDto>(entity);
        }

        /// <summary>
        /// Cập nhật thông tin trọng tài
        /// </summary>
        public async Task<TrongTaiDto?> UpdateAsync(int id, CreateUpdateTrongTaiDto dto, string? updatedBy = null)
        {
            var entity = await _unitOfWork.TrongTais.GetByIdAsync(id);
            if (entity == null || entity.IsDeleted == true) return null;

            _mapper.Map(dto, entity);
            entity.LastModified = DateTime.UtcNow;
            entity.LastModifiedBy = updatedBy;

            _unitOfWork.TrongTais.Update(entity);
            await _unitOfWork.CompleteAsync();

            return _mapper.Map<TrongTaiDto>(entity);
        }

        /// <summary>
        /// Xóa mềm trọng tài
        /// </summary>
        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _unitOfWork.TrongTais.GetByIdAsync(id);
            if (entity == null || entity.IsDeleted == true) return false;

            entity.IsDeleted = true;
            entity.LastModified = DateTime.UtcNow;
            _unitOfWork.TrongTais.Update(entity);
            await _unitOfWork.CompleteAsync();

            return true;
        }
    }
}
