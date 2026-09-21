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
    /// Service xử lý nghiệp vụ quản lý Thư ký bàn / Thư ký giải (ThuKy)
    /// </summary>
    public class ThuKyService : IThuKyService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ThuKyService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        /// <summary>
        /// Lấy danh sách thư ký có phân trang, tìm kiếm và lọc trạng thái
        /// </summary>
        public async Task<PagedResult<ThuKyDto>> GetPagedAsync(
            int pageIndex,
            int pageSize,
            string? keyword = null,
            bool? trangThai = null)
        {
            var pagedEntities = await _unitOfWork.ThuKys.GetPagedAsync(
                pageIndex,
                pageSize,
                predicate: t => t.IsDeleted != true &&
                                (string.IsNullOrEmpty(keyword) || t.HoTen.Contains(keyword) || t.Ma.Contains(keyword) || (t.SoDienThoai != null && t.SoDienThoai.Contains(keyword)) || (t.DonViCongTac != null && t.DonViCongTac.Contains(keyword))) &&
                                (!trangThai.HasValue || t.TrangThai == trangThai.Value),
                orderBy: q => q.OrderByDescending(t => t.Created ?? DateTime.MinValue)
            );

            var dtos = _mapper.Map<IEnumerable<ThuKyDto>>(pagedEntities.Items);
            return new PagedResult<ThuKyDto>(dtos, pagedEntities.TotalCount, pageIndex, pageSize);
        }

        /// <summary>
        /// Lấy tất cả thư ký chưa bị xóa
        /// </summary>
        public async Task<IEnumerable<ThuKyDto>> GetAllAsync()
        {
            var items = await _unitOfWork.ThuKys.FindAsync(t => t.IsDeleted != true && t.TrangThai);
            return _mapper.Map<IEnumerable<ThuKyDto>>(items.OrderBy(t => t.HoTen));
        }

        /// <summary>
        /// Lấy chi tiết thư ký theo ID
        /// </summary>
        public async Task<ThuKyDto?> GetByIdAsync(int id)
        {
            var item = await _unitOfWork.ThuKys.GetByIdAsync(id);
            if (item == null || item.IsDeleted == true) return null;

            return _mapper.Map<ThuKyDto>(item);
        }

        /// <summary>
        /// Thêm mới thư ký
        /// </summary>
        public async Task<ThuKyDto> CreateAsync(CreateUpdateThuKyDto dto, string? createdBy = null)
        {
            var entity = _mapper.Map<ThuKy>(dto);
            entity.Created = DateTime.UtcNow;
            entity.CreatedBy = createdBy;
            entity.IsDeleted = false;

            await _unitOfWork.ThuKys.AddAsync(entity);
            await _unitOfWork.CompleteAsync();

            return _mapper.Map<ThuKyDto>(entity);
        }

        /// <summary>
        /// Cập nhật thông tin thư ký
        /// </summary>
        public async Task<ThuKyDto?> UpdateAsync(int id, CreateUpdateThuKyDto dto, string? updatedBy = null)
        {
            var entity = await _unitOfWork.ThuKys.GetByIdAsync(id);
            if (entity == null || entity.IsDeleted == true) return null;

            _mapper.Map(dto, entity);
            entity.LastModified = DateTime.UtcNow;
            entity.LastModifiedBy = updatedBy;

            _unitOfWork.ThuKys.Update(entity);
            await _unitOfWork.CompleteAsync();

            return _mapper.Map<ThuKyDto>(entity);
        }

        /// <summary>
        /// Xóa thư ký (soft delete)
        /// </summary>
        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _unitOfWork.ThuKys.GetByIdAsync(id);
            if (entity == null || entity.IsDeleted == true) return false;

            entity.IsDeleted = true;
            entity.LastModified = DateTime.UtcNow;

            _unitOfWork.ThuKys.Update(entity);
            await _unitOfWork.CompleteAsync();

            return true;
        }
    }
}
