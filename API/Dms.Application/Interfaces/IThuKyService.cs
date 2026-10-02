using Dms.Application.DTOs;
using Dms.Domain.Common;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dms.Application.Interfaces
{
    public interface IThuKyService
    {
        /// <summary>
        /// Lấy danh sách thư ký có phân trang kèm tìm kiếm theo từ khóa và trạng thái
        /// </summary>
        /// <param name="pageIndex">Trang hiện tại</param>
        /// <param name="pageSize">Số bản ghi mỗi trang</param>
        /// <param name="keyword">Từ khóa tìm kiếm theo tên, mã hoặc đơn vị công tác</param>
        /// <param name="trangThai">Trạng thái sẵn sàng làm nhiệm vụ</param>
        /// <returns>Danh sách thư ký có phân trang</returns>
        Task<PagedResult<ThuKyDto>> GetPagedAsync(int pageIndex, int pageSize, string? keyword = null, bool? trangThai = null);

        /// <summary>
        /// Lấy toàn bộ danh sách thư ký đang hoạt động (không bị xóa mềm)
        /// </summary>
        /// <returns>Danh sách thư ký hoạt động</returns>
        Task<IEnumerable<ThuKyDto>> GetAllAsync();

        /// <summary>
        /// Lấy chi tiết thông tin hồ sơ thư ký theo Id
        /// </summary>
        /// <param name="id">Mã định danh thư ký</param>
        /// <returns>Thông tin thư ký hoặc null nếu không tìm thấy</returns>
        Task<ThuKyDto?> GetByIdAsync(int id);

        /// <summary>
        /// Sinh mã thư ký ngẫu nhiên duy nhất có cả chữ và số
        /// </summary>
        /// <returns>Mã thư ký ngẫu nhiên không trùng lặp</returns>
        Task<string> GenerateCodeAsync();

        /// <summary>
        /// Thêm mới hồ sơ thư ký (tự động sinh mã nếu để trống)
        /// </summary>
        /// <param name="dto">Dữ liệu tạo thư ký mới</param>
        /// <param name="createdBy">Tài khoản người thực hiện tạo</param>
        /// <returns>Thông tin thư ký vừa được tạo</returns>
        Task<ThuKyDto> CreateAsync(CreateUpdateThuKyDto dto, string? createdBy = null);

        /// <summary>
        /// Cập nhật thông tin hồ sơ thư ký
        /// </summary>
        /// <param name="id">Mã định danh thư ký cần cập nhật</param>
        /// <param name="dto">Dữ liệu thông tin cập nhật</param>
        /// <param name="updatedBy">Tài khoản người thực hiện cập nhật</param>
        /// <returns>Thông tin thư ký sau khi cập nhật hoặc null nếu không tìm thấy</returns>
        Task<ThuKyDto?> UpdateAsync(int id, CreateUpdateThuKyDto dto, string? updatedBy = null);

        /// <summary>
        /// Xóa mềm một thư ký (đánh dấu IsDeleted = true)
        /// </summary>
        /// <param name="id">Mã định danh thư ký cần xóa</param>
        /// <returns>True nếu xóa thành công, ngược lại False</returns>
        Task<bool> DeleteAsync(int id);
    }
}
