using Dms.Application.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dms.Application.Interfaces
{
    /// <summary>
    /// Giao diện dịch vụ quản lý cấu hình thể thức thi đấu và luật tính điểm cho các môn thể thao.
    /// </summary>
    public interface ICauHinhTheThucService
    {
        /// <summary>
        /// Lấy thông tin cấu hình thể thức thi đấu áp dụng cho môn thể thao hoặc giải đấu cụ thể.
        /// Ưu tiên cấu hình riêng theo giải đấu nếu có, nếu không lấy cấu hình mặc định của môn.
        /// </summary>
        /// <param name="monTheThaoId">Mã định danh môn thể thao</param>
        /// <param name="giaiDauMonTheThaoId">Mã định danh môn trong giải đấu (tùy chọn)</param>
        /// <returns>Đối tượng DTO cấu hình thể thức thi đấu tương ứng</returns>
        Task<CauHinhTheThucDto?> GetEffectiveConfigAsync(int monTheThaoId, int? giaiDauMonTheThaoId = null);

        /// <summary>
        /// Lấy cấu hình thể thức áp dụng trực tiếp cho một trận đấu cụ thể dựa trên ID trận đấu.
        /// </summary>
        /// <param name="tranDauId">Mã định danh trận đấu</param>
        /// <returns>Đối tượng DTO cấu hình thể thức của môn trong trận đấu đó</returns>
        Task<CauHinhTheThucDto?> GetConfigByTranDauIdAsync(int tranDauId);

        /// <summary>
        /// Lấy toàn bộ danh sách cấu hình thể thức thi đấu trong hệ thống.
        /// </summary>
        /// <returns>Danh sách các cấu hình thể thức</returns>
        Task<IEnumerable<CauHinhTheThucDto>> GetAllAsync();

        /// <summary>
        /// Thêm mới hoặc cập nhật cấu hình thể thức cho môn thể thao / giải đấu.
        /// </summary>
        /// <param name="dto">Dữ liệu cấu hình cần lưu</param>
        /// <param name="username">Tên người thực hiện thao tác</param>
        /// <returns>Cấu hình đã lưu sau khi cập nhật</returns>
        Task<CauHinhTheThucDto> UpsertConfigAsync(CreateUpdateCauHinhTheThucDto dto, string? username = null);
    }
}
