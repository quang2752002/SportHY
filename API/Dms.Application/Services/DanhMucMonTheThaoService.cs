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

        /// <summary>
        /// Tạo mới một danh mục môn thể thao vào hệ thống (tự động sinh mã nếu để trống)
        /// </summary>
        /// <param name="dto">Dữ liệu tạo mới danh mục</param>
        /// <param name="createdBy">Tài khoản người tạo</param>
        /// <returns>Thông tin danh mục sau khi tạo</returns>
        public async Task<DanhMucMonTheThaoDto> CreateAsync(CreateUpdateDanhMucMonTheThaoDto dto, string? createdBy = null)
        {
            if (string.IsNullOrWhiteSpace(dto.Ma))
            {
                dto.Ma = await GenerateCodeAsync(dto.Ten);
            }
            else
            {
                dto.Ma = dto.Ma.Trim().ToUpperInvariant();
            }

            var entity = _mapper.Map<DanhMucMonTheThao>(dto);
            entity.Created = DateTime.UtcNow;
            entity.CreatedBy = createdBy;
            entity.IsDeleted = false;

            await _unitOfWork.DanhMucMonTheThaos.AddAsync(entity);
            await _unitOfWork.CompleteAsync();

            return _mapper.Map<DanhMucMonTheThaoDto>(entity);
        }

        /// <summary>
        /// Sinh mã danh mục môn thể thao tự động duy nhất (ngẫu nhiên có cả chữ và số)
        /// </summary>
        /// <param name="name">Tên danh mục môn thể thao (tùy chọn)</param>
        /// <returns>Mã danh mục ngẫu nhiên có cả số và chữ không bị trùng lặp</returns>
        public async Task<string> GenerateCodeAsync(string? name = null)
        {
            string code;
            do
            {
                code = Dms.Application.Common.CodeGeneratorHelper.GenerateRandomCode("DM", 6);
            } while ((await _unitOfWork.DanhMucMonTheThaos.FindAsync(d => d.Ma == code && d.IsDeleted != true)).Any());

            return code;
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

        /// <summary>Cập nhật trạng thái hoạt động của một danh mục môn mà không thay đổi thông tin khác.</summary>
        /// <param name="id">Mã định danh danh mục cần cập nhật.</param>
        /// <param name="trangThai">Trạng thái mới; true là hoạt động, false là tạm dừng.</param>
        /// <param name="updatedBy">Tài khoản thực hiện thay đổi trạng thái.</param>
        /// <returns>True nếu cập nhật thành công; false nếu danh mục không tồn tại hoặc đã bị xóa mềm.</returns>
        public async Task<bool> SetStatusAsync(int id, bool trangThai, string? updatedBy = null)
        {
            var entity = await _unitOfWork.DanhMucMonTheThaos.GetByIdAsync(id);
            if (entity == null || entity.IsDeleted == true)
                return false;

            entity.TrangThai = trangThai;
            entity.LastModified = DateTime.UtcNow;
            entity.LastModifiedBy = updatedBy;
            _unitOfWork.DanhMucMonTheThaos.Update(entity);
            await _unitOfWork.CompleteAsync();

            return true;
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
