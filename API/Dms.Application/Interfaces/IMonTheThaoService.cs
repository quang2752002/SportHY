using Dms.Application.DTOs;
using Dms.Domain.Common;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dms.Application.Interfaces
{
    /// <summary>
    /// Giao diện dịch vụ quản lý các môn thể thao trong hệ thống
    /// </summary>
    public interface IMonTheThaoService
    {
        /// <summary>
        /// Lấy danh sách môn thể thao phân trang theo điều kiện tìm kiếm và lọc
        /// </summary>
        /// <param name="pageIndex">Số trang hiện tại (bắt đầu từ 1)</param>
        /// <param name="pageSize">Số bản ghi trên mỗi trang</param>
        /// <param name="keyword">Từ khóa tìm kiếm theo tên hoặc mã môn</param>
        /// <param name="danhMucId">Lọc theo mã danh mục môn</param>
        /// <param name="trangThai">Lọc theo trạng thái hoạt động</param>
        /// <param name="gioiTinh">Lọc theo giới tính thi đấu (Nam, Nu, HonHop)</param>
        /// <param name="hinhThucThiDau">Lọc theo sơ đồ thi đấu (LoaiTrucTiep, VongBang...)</param>
        /// <param name="loaiThiDau">Lọc theo quy mô thi đấu (DongDoi, CaNhan)</param>
        /// <returns>Danh sách môn thể thao phân trang kèm tổng số bản ghi</returns>
        Task<PagedResult<MonTheThaoDto>> GetPagedAsync(
            int pageIndex,
            int pageSize,
            string? keyword = null,
            int? danhMucId = null,
            bool? trangThai = null,
            string? gioiTinh = null,
            string? hinhThucThiDau = null,
            string? loaiThiDau = null);

        /// <summary>
        /// Lấy tất cả các môn thể thao đang hoạt động theo bộ lọc
        /// </summary>
        /// <param name="danhMucId">Lọc theo mã danh mục môn</param>
        /// <param name="gioiTinh">Lọc theo giới tính thi đấu</param>
        /// <returns>Danh sách toàn bộ môn thể thao thỏa mãn điều kiện</returns>
        Task<IEnumerable<MonTheThaoDto>> GetAllAsync(int? danhMucId = null, string? gioiTinh = null);

        /// <summary>
        /// Lấy thông tin chi tiết một môn thể thao theo Id
        /// </summary>
        /// <param name="id">Mã định danh môn thể thao</param>
        /// <returns>Thông tin môn thể thao hoặc null nếu không tồn tại hoặc đã bị xóa</returns>
        Task<MonTheThaoDto?> GetByIdAsync(int id);

        /// <summary>
        /// Tạo mới một môn thể thao vào hệ thống
        /// </summary>
        /// <param name="dto">Dữ liệu tạo mới môn thể thao</param>
        /// <param name="createdBy">Tên tài khoản người tạo</param>
        /// <returns>Thông tin môn thể thao sau khi được tạo</returns>
        Task<MonTheThaoDto> CreateAsync(CreateUpdateMonTheThaoDto dto, string? createdBy = null);

        /// <summary>
        /// Cập nhật thông tin môn thể thao theo Id
        /// </summary>
        /// <param name="id">Mã định danh môn thể thao cần cập nhật</param>
        /// <param name="dto">Dữ liệu cập nhật mới</param>
        /// <param name="updatedBy">Tên tài khoản người cập nhật</param>
        /// <returns>Thông tin môn thể thao sau khi cập nhật hoặc null nếu không tìm thấy</returns>
        Task<MonTheThaoDto?> UpdateAsync(int id, CreateUpdateMonTheThaoDto dto, string? updatedBy = null);

        /// <summary>
        /// Xóa mềm một môn thể thao khỏi hệ thống
        /// </summary>
        /// <param name="id">Mã định danh môn thể thao cần xóa</param>
        /// <returns>True nếu xóa thành công, False nếu không tìm thấy</returns>
        Task<bool> DeleteAsync(int id);
    }
}
