using Dms.Application.DTOs;
using Dms.Domain.Common;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dms.Application.Interfaces
{
    public interface IKhoiService
    {
        /// <summary>
        /// Lấy danh sách khối tham gia có phân trang và tìm kiếm theo từ khóa, trạng thái
        /// </summary>
        /// <param name="pageIndex">Trang hiện tại</param>
        /// <param name="pageSize">Số bản ghi mỗi trang</param>
        /// <param name="keyword">Từ khóa tìm kiếm theo tên hoặc mã khối</param>
        /// <param name="trangThai">Trạng thái hoạt động</param>
        /// <returns>Danh sách khối có phân trang</returns>
        Task<PagedResult<KhoiDto>> GetPagedAsync(int pageIndex, int pageSize, string? keyword = null, bool? trangThai = null);

        /// <summary>
        /// Lấy toàn bộ danh sách khối đang hoạt động (không bị xóa mềm)
        /// </summary>
        /// <returns>Danh sách khối hoạt động</returns>
        Task<IEnumerable<KhoiDto>> GetAllAsync();

        /// <summary>
        /// Lấy thông tin chi tiết một khối theo Id
        /// </summary>
        /// <param name="id">Mã định danh khối</param>
        /// <returns>Thông tin khối hoặc null nếu không tìm thấy</returns>
        Task<KhoiDto?> GetByIdAsync(int id);

        /// <summary>
        /// Sinh mã khối tham gia ngẫu nhiên duy nhất có cả chữ và số
        /// </summary>
        /// <returns>Mã khối ngẫu nhiên không trùng lặp</returns>
        Task<string> GenerateCodeAsync();

        /// <summary>
        /// Thêm mới một khối tham gia (tự động sinh mã nếu để trống)
        /// </summary>
        /// <param name="dto">Dữ liệu tạo khối mới</param>
        /// <param name="createdBy">Tài khoản người thực hiện tạo</param>
        /// <returns>Thông tin khối vừa được tạo</returns>
        Task<KhoiDto> CreateAsync(CreateUpdateKhoiDto dto, string? createdBy = null);

        /// <summary>
        /// Cập nhật thông tin khối tham gia
        /// </summary>
        /// <param name="id">Mã định danh khối cần cập nhật</param>
        /// <param name="dto">Dữ liệu cập nhật</param>
        /// <param name="updatedBy">Tài khoản người thực hiện cập nhật</param>
        /// <returns>Thông tin khối sau cập nhật hoặc null nếu không tìm thấy</returns>
        Task<KhoiDto?> UpdateAsync(int id, CreateUpdateKhoiDto dto, string? updatedBy = null);

        /// <summary>
        /// Xóa mềm một khối tham gia (đánh dấu IsDeleted = true)
        /// </summary>
        /// <param name="id">Mã định danh khối cần xóa</param>
        /// <returns>True nếu xóa thành công, ngược lại False</returns>
        Task<bool> DeleteAsync(int id);
    }
}
