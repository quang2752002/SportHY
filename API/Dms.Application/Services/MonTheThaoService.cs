using AutoMapper;
using Dms.Application.Common;
using Dms.Application.DTOs;
using Dms.Application.Interfaces;
using Dms.Domain.Common;
using Dms.Domain.Entities;
using Dms.Domain.Enums;
using Dms.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dms.Application.Services
{
    /// <summary>
    /// Dịch vụ quản lý các môn thể thao trong hệ thống
    /// </summary>
    public class MonTheThaoService : IMonTheThaoService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public MonTheThaoService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        /// <summary>
        /// Lấy danh sách môn thể thao phân trang theo điều kiện tìm kiếm và lọc
        /// </summary>
        /// <param name="pageIndex">Số trang hiện tại (bắt đầu từ 1)</param>
        /// <param name="pageSize">Số bản ghi trên mỗi trang</param>
        /// <param name="keyword">Từ khóa tìm kiếm theo tên hoặc mã môn</param>
        /// <param name="danhMucId">Lọc theo mã danh mục môn</param>
        /// <param name="trangThai">Lọc theo trạng thái hoạt động</param>
        /// <param name="gioiTinh">Lọc theo giới tính thi đấu (Nam, Nu, HonHop)</param>
        /// <param name="hinhThucThiDau">Lọc theo sơ đồ thi đấu</param>
        /// <param name="loaiThiDau">Lọc theo quy mô thi đấu (DongDoi, CaNhan)</param>
        /// <param name="danhMucTrangThai">Lọc theo trạng thái hoạt động của danh mục chứa môn</param>
        /// <returns>Danh sách môn thể thao phân trang kèm tổng số bản ghi</returns>
        public async Task<PagedResult<MonTheThaoDto>> GetPagedAsync(
            int pageIndex,
            int pageSize,
            string? keyword = null,
            int? danhMucId = null,
            bool? trangThai = null,
            string? gioiTinh = null,
            string? hinhThucThiDau = null,
            string? loaiThiDau = null,
            bool? danhMucTrangThai = null)
        {
            HinhThucThiDau? hinhThucEnum = null;
            if (!string.IsNullOrEmpty(hinhThucThiDau) && Enum.TryParse<HinhThucThiDau>(hinhThucThiDau, true, out var parsedHinhThuc))
            {
                hinhThucEnum = parsedHinhThuc;
            }

            var pagedEntities = await _unitOfWork.MonTheThaos.GetPagedAsync(
                pageIndex,
                pageSize,
                predicate: m => m.IsDeleted != true &&
                                (string.IsNullOrEmpty(keyword) || m.Ten.Contains(keyword) || m.Ma.Contains(keyword)) &&
                                (!danhMucId.HasValue || m.DanhMucId == danhMucId.Value) &&
                                (!trangThai.HasValue || m.TrangThai == trangThai.Value) &&
                                (string.IsNullOrEmpty(gioiTinh) || m.GioiTinh == gioiTinh) &&
                                (!hinhThucEnum.HasValue || m.HinhThucThiDau == hinhThucEnum.Value) &&
                                (string.IsNullOrEmpty(loaiThiDau) || m.LoaiThiDau == loaiThiDau) &&
                                (!danhMucTrangThai.HasValue || (m.DanhMuc != null && m.DanhMuc.TrangThai == danhMucTrangThai.Value)),
                orderBy: q => q.OrderBy(m => m.Ma),
                includes: m => m.DanhMuc
            );

            var dtos = _mapper.Map<IEnumerable<MonTheThaoDto>>(pagedEntities.Items);
            return new PagedResult<MonTheThaoDto>(dtos, pagedEntities.TotalCount, pageIndex, pageSize);
        }

        /// <summary>
        /// Lấy tất cả các môn thể thao đang hoạt động theo bộ lọc
        /// </summary>
        /// <param name="danhMucId">Lọc theo mã danh mục môn</param>
        /// <param name="gioiTinh">Lọc theo giới tính thi đấu</param>
        /// <returns>Danh sách toàn bộ môn thể thao thỏa mãn điều kiện</returns>
        public async Task<IEnumerable<MonTheThaoDto>> GetAllAsync(int? danhMucId = null, string? gioiTinh = null)
        {
            var items = await _unitOfWork.MonTheThaos.FindAsync(
                m => m.IsDeleted != true && m.TrangThai &&
                     (!danhMucId.HasValue || m.DanhMucId == danhMucId.Value) &&
                     (string.IsNullOrEmpty(gioiTinh) || m.GioiTinh == gioiTinh)
            );
            return _mapper.Map<IEnumerable<MonTheThaoDto>>(items.OrderBy(m => m.Ten));
        }

        /// <summary>
        /// Lấy thông tin chi tiết một môn thể thao theo Id
        /// </summary>
        /// <param name="id">Mã định danh môn thể thao</param>
        /// <returns>Thông tin môn thể thao hoặc null nếu không tồn tại hoặc đã bị xóa</returns>
        public async Task<MonTheThaoDto?> GetByIdAsync(int id)
        {
            var entity = await _unitOfWork.MonTheThaos.GetByIdAsync(id);
            if (entity == null || entity.IsDeleted == true)
                return null;

            return _mapper.Map<MonTheThaoDto>(entity);
        }

        /// <summary>
        /// Tạo mới một môn thể thao vào hệ thống
        /// </summary>
        /// <param name="dto">Dữ liệu tạo mới môn thể thao</param>
        /// <param name="createdBy">Tên tài khoản người tạo</param>
        /// <param name="requireConfig">Yêu cầu request phải kèm cấu hình thể thức hợp lệ hay không</param>
        /// <returns>Thông tin môn thể thao sau khi được tạo</returns>
        public async Task<MonTheThaoDto> CreateAsync(CreateUpdateMonTheThaoDto dto, string? createdBy = null, bool requireConfig = false)
        {
            if (requireConfig && dto.CauHinhTheThuc == null)
            {
                throw new ArgumentException("Vui lòng cấu hình thể thức thi đấu trước khi lưu môn.");
            }
            if (dto.CauHinhTheThuc != null)
            {
                CauHinhTheThucService.ValidateConfig(dto.CauHinhTheThuc);
            }
            if (string.IsNullOrEmpty(dto.LoaiThiDau) && dto.LaMonDongDoi)
            {
                dto.LoaiThiDau = "DongDoi";
            }
            else if (!string.IsNullOrEmpty(dto.LoaiThiDau))
            {
                dto.LaMonDongDoi = (dto.LoaiThiDau == "DongDoi");
            }

            if (!dto.SoLuongVanDongVienToiThieu.HasValue && dto.SoLuongVdvToiThieu.HasValue)
            {
                dto.SoLuongVanDongVienToiThieu = dto.SoLuongVdvToiThieu;
            }
            if (!dto.SoLuongVanDongVienToiDa.HasValue && dto.SoLuongVdvToiDa.HasValue)
            {
                dto.SoLuongVanDongVienToiDa = dto.SoLuongVdvToiDa;
            }

            if (string.IsNullOrWhiteSpace(dto.Ma))
            {
                dto.Ma = await GenerateCodeAsync(dto.Ten, dto.DanhMucId);
            }
            else
            {
                dto.Ma = dto.Ma.Trim().ToUpperInvariant();
            }

            var entity = _mapper.Map<MonTheThao>(dto);

            // Xử lý hình thức / sơ đồ thi đấu an toàn từ enum
            if (!string.IsNullOrEmpty(dto.HinhThucThiDau) && Enum.TryParse<HinhThucThiDau>(dto.HinhThucThiDau, true, out var hinhThuc))
            {
                entity.HinhThucThiDau = hinhThuc;
            }
            else
            {
                entity.HinhThucThiDau = HinhThucThiDau.LoaiTrucTiep;
            }

            entity.Created = DateTime.UtcNow;
            entity.CreatedBy = createdBy;
            entity.IsDeleted = false;

            await _unitOfWork.MonTheThaos.AddAsync(entity);
            if (dto.CauHinhTheThuc != null)
            {
                var configEntity = new CauHinhTheThucThiDau
                {
                    MonTheThao = entity,
                    Created = DateTime.UtcNow,
                    CreatedBy = createdBy,
                    IsDeleted = false
                };
                dto.CauHinhTheThuc.MonTheThaoId = entity.Id;
                dto.CauHinhTheThuc.GiaiDauMonTheThaoId = null;
                dto.CauHinhTheThuc.HinhThucThiDau = entity.HinhThucThiDau.ToString();
                CauHinhTheThucService.CopyProperties(dto.CauHinhTheThuc, configEntity);
                await _unitOfWork.CauHinhTheThucThiDaus.AddAsync(configEntity);
            }
            await _unitOfWork.CompleteAsync();

            return _mapper.Map<MonTheThaoDto>(entity);
        }

        /// <summary>
        /// Sinh mã môn thể thao tự động duy nhất (ngẫu nhiên có cả chữ và số)
        /// </summary>
        /// <param name="name">Tên môn thể thao (tùy chọn)</param>
        /// <param name="danhMucId">Mã định danh danh mục môn trực thuộc (tùy chọn)</param>
        /// <returns>Mã môn thể thao ngẫu nhiên có cả số và chữ không bị trùng lặp</returns>
        public async Task<string> GenerateCodeAsync(string? name = null, int? danhMucId = null)
        {
            string prefix = "MON";
            if (danhMucId.HasValue && danhMucId.Value > 0)
            {
                var dm = await _unitOfWork.DanhMucMonTheThaos.GetByIdAsync(danhMucId.Value);
                if (dm != null && !string.IsNullOrWhiteSpace(dm.Ma))
                {
                    var dmCode = dm.Ma.Trim();
                    if (dmCode.StartsWith("DM_", StringComparison.OrdinalIgnoreCase))
                    {
                        dmCode = dmCode.Substring(3);
                    }
                    if (!string.IsNullOrWhiteSpace(dmCode))
                    {
                        prefix = dmCode.Length > 6 ? dmCode.Substring(0, 6) : dmCode;
                    }
                }
            }

            string code;
            do
            {
                code = CodeGeneratorHelper.GenerateRandomCode(prefix, 6);
            } while ((await _unitOfWork.MonTheThaos.FindAsync(m => m.Ma == code && m.IsDeleted != true)).Any());

            return code;
        }

        /// <summary>
        /// Cập nhật thông tin môn thể thao theo Id
        /// </summary>
        /// <param name="id">Mã định danh môn thể thao cần cập nhật</param>
        /// <param name="dto">Dữ liệu cập nhật mới</param>
        /// <param name="updatedBy">Tên tài khoản người cập nhật</param>
        /// <param name="requireConfig">Yêu cầu request phải kèm cấu hình thể thức hợp lệ hay không</param>
        /// <returns>Thông tin môn thể thao sau khi cập nhật hoặc null nếu không tìm thấy</returns>
        public async Task<MonTheThaoDto?> UpdateAsync(int id, CreateUpdateMonTheThaoDto dto, string? updatedBy = null, bool requireConfig = false)
        {
            if (requireConfig && dto.CauHinhTheThuc == null)
            {
                throw new ArgumentException("Vui lòng cấu hình thể thức thi đấu trước khi lưu môn.");
            }
            if (dto.CauHinhTheThuc != null)
            {
                CauHinhTheThucService.ValidateConfig(dto.CauHinhTheThuc);
            }
            var entity = await _unitOfWork.MonTheThaos.GetByIdAsync(id);
            if (entity == null || entity.IsDeleted == true)
                return null;

            if (string.IsNullOrEmpty(dto.LoaiThiDau) && dto.LaMonDongDoi)
            {
                dto.LoaiThiDau = "DongDoi";
            }
            else if (!string.IsNullOrEmpty(dto.LoaiThiDau))
            {
                dto.LaMonDongDoi = (dto.LoaiThiDau == "DongDoi");
            }

            if (!dto.SoLuongVanDongVienToiThieu.HasValue && dto.SoLuongVdvToiThieu.HasValue)
            {
                dto.SoLuongVanDongVienToiThieu = dto.SoLuongVdvToiThieu;
            }
            if (!dto.SoLuongVanDongVienToiDa.HasValue && dto.SoLuongVdvToiDa.HasValue)
            {
                dto.SoLuongVanDongVienToiDa = dto.SoLuongVdvToiDa;
            }

            _mapper.Map(dto, entity);

            // Xử lý hình thức / sơ đồ thi đấu an toàn từ enum
            if (!string.IsNullOrEmpty(dto.HinhThucThiDau) && Enum.TryParse<HinhThucThiDau>(dto.HinhThucThiDau, true, out var hinhThuc))
            {
                entity.HinhThucThiDau = hinhThuc;
            }

            entity.LastModified = DateTime.UtcNow;
            entity.LastModifiedBy = updatedBy;

            _unitOfWork.MonTheThaos.Update(entity);
            if (dto.CauHinhTheThuc != null)
            {
                var configs = await _unitOfWork.CauHinhTheThucThiDaus.FindAsync(c =>
                    c.MonTheThaoId == id && c.GiaiDauMonTheThaoId == null && c.IsDeleted != true);
                var configEntity = configs.FirstOrDefault();
                if (configEntity == null)
                {
                    configEntity = new CauHinhTheThucThiDau
                    {
                        MonTheThaoId = id,
                        Created = DateTime.UtcNow,
                        CreatedBy = updatedBy,
                        IsDeleted = false
                    };
                    await _unitOfWork.CauHinhTheThucThiDaus.AddAsync(configEntity);
                }
                else
                {
                    configEntity.LastModified = DateTime.UtcNow;
                    configEntity.LastModifiedBy = updatedBy;
                    _unitOfWork.CauHinhTheThucThiDaus.Update(configEntity);
                }
                dto.CauHinhTheThuc.MonTheThaoId = id;
                dto.CauHinhTheThuc.GiaiDauMonTheThaoId = null;
                dto.CauHinhTheThuc.HinhThucThiDau = entity.HinhThucThiDau.ToString();
                CauHinhTheThucService.CopyProperties(dto.CauHinhTheThuc, configEntity);
            }
            await _unitOfWork.CompleteAsync();

            return _mapper.Map<MonTheThaoDto>(entity);
        }

        /// <summary>Cập nhật trạng thái hoạt động của một môn thể thao mà không yêu cầu gửi lại cấu hình thi đấu.</summary>
        /// <param name="id">Mã định danh môn thể thao cần cập nhật.</param>
        /// <param name="trangThai">Trạng thái mới; true là hoạt động, false là tạm dừng.</param>
        /// <param name="updatedBy">Tài khoản thực hiện thay đổi trạng thái.</param>
        /// <returns>True nếu cập nhật thành công; false nếu môn không tồn tại hoặc đã bị xóa mềm.</returns>
        public async Task<bool> SetStatusAsync(int id, bool trangThai, string? updatedBy = null)
        {
            var entity = await _unitOfWork.MonTheThaos.GetByIdAsync(id);
            if (entity == null || entity.IsDeleted == true)
                return false;

            entity.TrangThai = trangThai;
            entity.LastModified = DateTime.UtcNow;
            entity.LastModifiedBy = updatedBy;
            _unitOfWork.MonTheThaos.Update(entity);
            await _unitOfWork.CompleteAsync();

            return true;
        }

        /// <summary>
        /// Xóa mềm một môn thể thao khỏi hệ thống
        /// </summary>
        /// <param name="id">Mã định danh môn thể thao cần xóa</param>
        /// <returns>True nếu xóa thành công, False nếu không tìm thấy</returns>
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
