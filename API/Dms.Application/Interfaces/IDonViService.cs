using Dms.Application.DTOs;
using Dms.Domain.Common;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dms.Application.Interfaces
{
    public interface IDonViService
    {
        /// <summary>
        /// Lấy danh sách đơn vị / đoàn thể thao có phân trang và lọc theo từ khóa, khối, trạng thái
        /// </summary>
        /// <param name="pageIndex">Trang hiện tại (bắt đầu từ 1)</param>
        /// <param name="pageSize">Số bản ghi trên mỗi trang</param>
        /// <param name="keyword">Từ khóa tìm kiếm theo tên, mã, người đại diện</param>
        /// <param name="khoiId">Lọc theo khối áp dụng (tùy chọn)</param>
        /// <param name="trangThai">Lọc theo trạng thái hoạt động (tùy chọn)</param>
        /// <returns>Kết quả phân trang danh sách đơn vị</returns>
        Task<PagedResult<DonViDto>> GetPagedAsync(int pageIndex, int pageSize, string? keyword = null, int? khoiId = null, bool? trangThai = null);

        /// <summary>
        /// Lấy toàn bộ danh sách đơn vị đang hoạt động (không bị xóa mềm)
        /// </summary>
        /// <param name="khoiId">Lọc theo khối áp dụng (tùy chọn)</param>
        /// <returns>Danh sách đơn vị hoạt động</returns>
        Task<IEnumerable<DonViDto>> GetAllAsync(int? khoiId = null);

        /// <summary>
        /// Lấy thông tin chi tiết của một đơn vị theo ID
        /// </summary>
        /// <param name="id">ID của đơn vị cần lấy</param>
        /// <returns>Thông tin chi tiết đơn vị hoặc null nếu không tìm thấy</returns>
        Task<DonViDto?> GetByIdAsync(int id);

        /// <summary>
        /// Thêm mới một đơn vị / đoàn thể thao vào hệ thống (có kiểm tra trùng lặp mã và tên)
        /// </summary>
        /// <param name="dto">Dữ liệu thông tin đơn vị cần tạo</param>
        /// <param name="createdBy">Tài khoản người thực hiện tạo</param>
        /// <returns>Thông tin đơn vị vừa được tạo</returns>
        Task<DonViDto> CreateAsync(CreateUpdateDonViDto dto, string? createdBy = null);

        /// <summary>
        /// Cập nhật thông tin đơn vị / đoàn thể thao
        /// </summary>
        /// <param name="id">ID đơn vị cần cập nhật</param>
        /// <param name="dto">Dữ liệu thông tin cập nhật</param>
        /// <param name="updatedBy">Tài khoản người thực hiện cập nhật</param>
        /// <returns>Thông tin đơn vị sau khi cập nhật hoặc null nếu không tìm thấy</returns>
        Task<DonViDto?> UpdateAsync(int id, CreateUpdateDonViDto dto, string? updatedBy = null);

        /// <summary>
        /// Xóa mềm một đơn vị / đoàn thể thao (đánh dấu IsDeleted = true)
        /// </summary>
        /// <param name="id">ID đơn vị cần xóa</param>
        /// <returns>True nếu xóa thành công, False nếu không tìm thấy hoặc lỗi</returns>
        Task<bool> DeleteAsync(int id);
    }
}
