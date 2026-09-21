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
    public class DonViService : IDonViService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public DonViService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        /// <summary>
        /// Lấy danh sách đơn vị / đoàn thể thao có phân trang và lọc theo từ khóa, khối, trạng thái
        /// </summary>
        /// <param name="pageIndex">Trang hiện tại (bắt đầu từ 1)</param>
        /// <param name="pageSize">Số bản ghi trên mỗi trang</param>
        /// <param name="keyword">Từ khóa tìm kiếm theo tên, mã, người đại diện</param>
        /// <param name="khoiId">Lọc theo khối áp dụng (tùy chọn)</param>
        /// <param name="trangThai">Lọc theo trạng thái hoạt động (tùy chọn)</param>
        /// <returns>Kết quả phân trang danh sách đơn vị</returns>
        public async Task<PagedResult<DonViDto>> GetPagedAsync(
            int pageIndex,
            int pageSize,
            string? keyword = null,
            int? khoiId = null,
            bool? trangThai = null)
        {
            var pagedEntities = await _unitOfWork.DonVis.GetPagedAsync(
                pageIndex,
                pageSize,
                predicate: d => d.IsDeleted != true &&
                                (string.IsNullOrEmpty(keyword) || d.Ten.Contains(keyword) || d.Ma.Contains(keyword) || (d.NguoiDaiDien != null && d.NguoiDaiDien.Contains(keyword))) &&
                                (!khoiId.HasValue || d.KhoiId == khoiId.Value) &&
                                (!trangThai.HasValue || d.TrangThai == trangThai.Value),
                orderBy: q => q.OrderBy(d => d.Ma),
                includes: new System.Linq.Expressions.Expression<Func<DonVi, object>>[]
                {
                    d => d.Khoi!,
                    d => d.DonViCha!,
                    d => d.VanDongViens,
                    d => d.Dois
                }
            );

            var dtos = _mapper.Map<IEnumerable<DonViDto>>(pagedEntities.Items);
            return new PagedResult<DonViDto>(dtos, pagedEntities.TotalCount, pageIndex, pageSize);
        }

        /// <summary>
        /// Lấy toàn bộ danh sách đơn vị đang hoạt động (không bị xóa mềm)
        /// </summary>
        /// <param name="khoiId">Lọc theo khối áp dụng (tùy chọn)</param>
        /// <returns>Danh sách đơn vị hoạt động</returns>
        public async Task<IEnumerable<DonViDto>> GetAllAsync(int? khoiId = null)
        {
            var items = await _unitOfWork.DonVis.FindAsync(
                d => d.IsDeleted != true && d.TrangThai &&
                     (!khoiId.HasValue || d.KhoiId == khoiId.Value)
            );
            return _mapper.Map<IEnumerable<DonViDto>>(items.OrderBy(d => d.Ten));
        }

        /// <summary>
        /// Lấy thông tin chi tiết của một đơn vị theo ID
        /// </summary>
        /// <param name="id">ID của đơn vị cần lấy</param>
        /// <returns>Thông tin chi tiết đơn vị hoặc null nếu không tìm thấy</returns>
        public async Task<DonViDto?> GetByIdAsync(int id)
        {
            var entity = await _unitOfWork.DonVis.GetByIdAsync(id);
            if (entity == null || entity.IsDeleted == true)
                return null;

            return _mapper.Map<DonViDto>(entity);
        }

        /// <summary>
        /// Thêm mới một đơn vị / đoàn thể thao vào hệ thống (có kiểm tra trùng lặp mã và tên)
        /// </summary>
        /// <param name="dto">Dữ liệu thông tin đơn vị cần tạo</param>
        /// <param name="createdBy">Tài khoản người thực hiện tạo</param>
        /// <returns>Thông tin đơn vị vừa được tạo</returns>
        public async Task<DonViDto> CreateAsync(CreateUpdateDonViDto dto, string? createdBy = null)
        {
            var tenTrim = dto.Ten.Trim();
            var maTrim = dto.Ma?.Trim();

            var existingUnits = await _unitOfWork.DonVis.FindAsync(d => d.IsDeleted != true &&
                (d.Ten.ToLower() == tenTrim.ToLower() || (!string.IsNullOrEmpty(maTrim) && d.Ma.ToLower() == maTrim.ToLower())));

            if (existingUnits.Any())
            {
                throw new InvalidOperationException($"Đơn vị có tên '{dto.Ten}' hoặc mã '{dto.Ma}' đã tồn tại trong hệ thống.");
            }

            var entity = _mapper.Map<DonVi>(dto);
            entity.Ten = tenTrim;
            entity.Ma = maTrim ?? entity.Ma;
            entity.Created = DateTime.UtcNow;
            entity.CreatedBy = createdBy;
            entity.IsDeleted = false;

            await _unitOfWork.DonVis.AddAsync(entity);
            await _unitOfWork.CompleteAsync();

            return _mapper.Map<DonViDto>(entity);
        }

        /// <summary>
        /// Cập nhật thông tin đơn vị / đoàn thể thao
        /// </summary>
        /// <param name="id">ID đơn vị cần cập nhật</param>
        /// <param name="dto">Dữ liệu thông tin cập nhật</param>
        /// <param name="updatedBy">Tài khoản người thực hiện cập nhật</param>
        /// <returns>Thông tin đơn vị sau khi cập nhật hoặc null nếu không tìm thấy</returns>
        public async Task<DonViDto?> UpdateAsync(int id, CreateUpdateDonViDto dto, string? updatedBy = null)
        {
            var entity = await _unitOfWork.DonVis.GetByIdAsync(id);
            if (entity == null || entity.IsDeleted == true)
                return null;

            var tenTrim = dto.Ten.Trim();
            var maTrim = dto.Ma?.Trim();

            var duplicateUnits = await _unitOfWork.DonVis.FindAsync(d => d.IsDeleted != true && d.Id != id &&
                (d.Ten.ToLower() == tenTrim.ToLower() || (!string.IsNullOrEmpty(maTrim) && d.Ma.ToLower() == maTrim.ToLower())));

            if (duplicateUnits.Any())
            {
                throw new InvalidOperationException($"Đơn vị khác có tên '{dto.Ten}' hoặc mã '{dto.Ma}' đã tồn tại trong hệ thống.");
            }

            _mapper.Map(dto, entity);
            entity.Ten = tenTrim;
            entity.Ma = maTrim ?? entity.Ma;
            entity.LastModified = DateTime.UtcNow;
            entity.LastModifiedBy = updatedBy;

            _unitOfWork.DonVis.Update(entity);
            await _unitOfWork.CompleteAsync();

            return _mapper.Map<DonViDto>(entity);
        }

        /// <summary>
        /// Xóa mềm một đơn vị / đoàn thể thao (đánh dấu IsDeleted = true)
        /// </summary>
        /// <param name="id">ID đơn vị cần xóa</param>
        /// <returns>True nếu xóa thành công, False nếu không tìm thấy hoặc lỗi</returns>
        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _unitOfWork.DonVis.GetByIdAsync(id);
            if (entity == null || entity.IsDeleted == true)
                return false;

            // Soft delete
            entity.IsDeleted = true;
            entity.LastModified = DateTime.UtcNow;
            _unitOfWork.DonVis.Update(entity);
            await _unitOfWork.CompleteAsync();

            return true;
        }
    }
}
