using Dms.Application.DTOs;
using System.Threading.Tasks;

namespace Dms.Application.Interfaces
{
    public interface ICauHinhLichThiDauService
    {
        /// <summary>
        /// Lấy cấu hình tham số xếp lịch thi đấu và bảo vệ thể lực VĐV theo ID môn thể thao.
        /// </summary>
        /// <param name="monTheThaoId">Mã định danh môn thể thao</param>
        /// <returns>Thông tin cấu hình chi tiết hoặc null nếu chưa thiết lập</returns>
        Task<CauHinhLichThiDauDto?> GetByMonTheThaoAsync(int monTheThaoId);

        /// <summary>
        /// Thêm mới hoặc cập nhật cấu hình tham số xếp lịch (số trận tối đa/ngày, thời gian nghỉ, ca thi đấu) cho môn thể thao.
        /// </summary>
        /// <param name="dto">Dữ liệu cấu hình cần lưu</param>
        /// <param name="by">Tên người dùng thực hiện</param>
        /// <returns>Dữ liệu cấu hình sau khi lưu thành công</returns>
        Task<CauHinhLichThiDauDto> UpsertAsync(CreateUpdateCauHinhLichThiDauDto dto, string? by = null);

        /// <summary>
        /// Xóa mềm cấu hình xếp lịch của môn thể thao để hoàn nguyên về thiết lập mặc định của hệ thống.
        /// </summary>
        /// <param name="monTheThaoId">Mã định danh môn thể thao cần reset cấu hình</param>
        /// <returns>True nếu xóa thành công, False nếu không tìm thấy bản ghi</returns>
        Task<bool> DeleteAsync(int monTheThaoId);
    }
}
