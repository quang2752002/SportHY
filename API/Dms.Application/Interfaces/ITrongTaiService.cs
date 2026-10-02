using Dms.Application.DTOs;
using Dms.Domain.Common;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dms.Application.Interfaces
{
    public interface ITrongTaiService
    {
        /// <summary>
        /// Lấy danh sách trọng tài có phân trang kèm tìm kiếm theo từ khóa và trạng thái
        /// </summary>
        /// <param name="pageIndex">Trang hiện tại</param>
        /// <param name="pageSize">Số bản ghi mỗi trang</param>
        /// <param name="keyword">Từ khóa tìm kiếm theo tên, mã hoặc số điện thoại</param>
        /// <param name="trangThai">Trạng thái sẵn sàng làm nhiệm vụ</param>
        /// <returns>Danh sách trọng tài có phân trang</returns>
        Task<PagedResult<TrongTaiDto>> GetPagedAsync(int pageIndex, int pageSize, string? keyword = null, bool? trangThai = null);

        /// <summary>
        /// Lấy toàn bộ danh sách trọng tài đang hoạt động (không bị xóa mềm)
        /// </summary>
        /// <returns>Danh sách trọng tài hoạt động</returns>
        Task<IEnumerable<TrongTaiDto>> GetAllAsync();

        /// <summary>
        /// Lấy chi tiết thông tin trọng tài theo Id
        /// </summary>
        /// <param name="id">Mã định danh trọng tài</param>
        /// <returns>Thông tin trọng tài hoặc null nếu không tìm thấy</returns>
        Task<TrongTaiDto?> GetByIdAsync(int id);

        /// <summary>
        /// Sinh mã trọng tài ngẫu nhiên duy nhất có cả chữ và số
        /// </summary>
        /// <returns>Mã trọng tài ngẫu nhiên không trùng lặp</returns>
        Task<string> GenerateCodeAsync();

        /// <summary>
        /// Thêm mới trọng tài vào hệ thống (tự động sinh mã nếu để trống)
        /// </summary>
        /// <param name="dto">Dữ liệu thông tin trọng tài cần tạo</param>
        /// <param name="createdBy">Tài khoản người thực hiện tạo</param>
        /// <returns>Thông tin trọng tài vừa được tạo</returns>
        Task<TrongTaiDto> CreateAsync(CreateUpdateTrongTaiDto dto, string? createdBy = null);

        /// <summary>
        /// Cập nhật thông tin trọng tài
        /// </summary>
        /// <param name="id">Mã định danh trọng tài cần cập nhật</param>
        /// <param name="dto">Dữ liệu thông tin cập nhật</param>
        /// <param name="updatedBy">Tài khoản người thực hiện cập nhật</param>
        /// <returns>Thông tin trọng tài sau khi cập nhật hoặc null nếu không tìm thấy</returns>
        Task<TrongTaiDto?> UpdateAsync(int id, CreateUpdateTrongTaiDto dto, string? updatedBy = null);

        /// <summary>
        /// Xóa mềm một trọng tài (đánh dấu IsDeleted = true)
        /// </summary>
        /// <param name="id">Mã định danh trọng tài cần xóa</param>
        /// <returns>True nếu xóa thành công, ngược lại False</returns>
        Task<bool> DeleteAsync(int id);
    }
}
