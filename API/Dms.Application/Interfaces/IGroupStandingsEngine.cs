using Dms.Application.DTOs;
using System.Threading.Tasks;

namespace Dms.Application.Interfaces
{
    /// <summary>
    /// Động cơ tính toán bảng xếp hạng vòng bảng tự động theo cấu hình tiêu chí động của từng môn thể thao.
    /// </summary>
    public interface IGroupStandingsEngine
    {
        /// <summary>
        /// Tính toán lại toàn bộ chỉ số (Số trận, Thắng, Hòa, Thua, Điểm, Hiệu số, Set thắng/thua)
        /// và sắp xếp lại thứ hạng cho toàn bộ các đội/VĐV trong bảng đấu dựa trên chuỗi tiêu chí cấu hình.
        /// </summary>
        /// <param name="bangDauId">Mã định danh bảng đấu cần tính toán</param>
        /// <param name="config">Cấu hình thể thức và luật tính điểm của môn thể thao</param>
        /// <param name="username">Tên người thực hiện cập nhật</param>
        /// <returns>True nếu tính toán và cập nhật thành công, False nếu không tìm thấy bảng đấu</returns>
        Task<bool> RecalculateGroupStandingsAsync(int bangDauId, CauHinhTheThucDto config, string? username = null);
    }
}
