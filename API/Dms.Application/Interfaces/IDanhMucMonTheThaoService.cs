using Dms.Application.DTOs;
using Dms.Domain.Common;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dms.Application.Interfaces
{
    public interface IDanhMucMonTheThaoService
    {
        Task<PagedResult<DanhMucMonTheThaoDto>> GetPagedAsync(int pageIndex, int pageSize, string? keyword = null, bool? trangThai = null);
        Task<IEnumerable<DanhMucMonTheThaoDto>> GetAllAsync();
        Task<DanhMucMonTheThaoDto?> GetByIdAsync(int id);
        Task<DanhMucMonTheThaoDto> CreateAsync(CreateUpdateDanhMucMonTheThaoDto dto, string? createdBy = null);
        Task<DanhMucMonTheThaoDto?> UpdateAsync(int id, CreateUpdateDanhMucMonTheThaoDto dto, string? updatedBy = null);
        /// <summary>Cập nhật trạng thái hoạt động của một danh mục môn mà không thay đổi thông tin khác.</summary>
        /// <param name="id">Mã định danh danh mục cần cập nhật.</param>
        /// <param name="trangThai">Trạng thái mới; true là hoạt động, false là tạm dừng.</param>
        /// <param name="updatedBy">Tài khoản thực hiện thay đổi trạng thái.</param>
        /// <returns>True nếu cập nhật thành công; false nếu danh mục không tồn tại hoặc đã bị xóa mềm.</returns>
        Task<bool> SetStatusAsync(int id, bool trangThai, string? updatedBy = null);
        /// <summary>
        /// Sinh mã danh mục môn thể thao tự động duy nhất dựa trên tên danh mục hoặc số thứ tự tiếp theo
        /// </summary>
        /// <param name="name">Tên danh mục môn thể thao (tùy chọn)</param>
        /// <returns>Mã danh mục duy nhất không bị trùng lặp</returns>
        Task<string> GenerateCodeAsync(string? name = null);

        Task<bool> DeleteAsync(int id);
    }
}
