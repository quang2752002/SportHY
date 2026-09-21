using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dms.Application.Interfaces
{
    /// <summary>
    /// Kết quả của tiến trình đẩy nhánh đấu Knockout và trao huy chương
    /// </summary>
    public class KnockoutProgressionResult
    {
        public bool Success { get; set; } = true;
        public string? NextMatchNotice { get; set; }
        public string? BronzeMatchNotice { get; set; }
        public List<string> MedalsAwarded { get; set; } = new();
    }

    /// <summary>
    /// Động cơ quản lý tiến trình nhánh đấu Knockout: tự động đưa đội thắng vào vòng tiếp theo,
    /// đưa đội thua bán kết vào trận tranh hạng 3, và tự động trao huy chương khi giải kết thúc.
    /// </summary>
    public interface IKnockoutProgressionEngine
    {
        /// <summary>
        /// Xử lý tiến trình sau khi một trận đấu loại trực tiếp (Knockout) kết thúc:
        /// 1. Tự động đưa đội thắng vào vị trí tương ứng của trận đấu kế tiếp.
        /// 2. Tự động đưa đội thua bán kết vào trận tranh hạng 3 (nếu có).
        /// 3. Cập nhật trạng thái trận kế tiếp thành "ChuaDau" (Sẵn sàng) nếu đã đủ 2 đội.
        /// 4. Tự động tạo bản ghi trao Huy chương Vàng, Bạc, Đồng nếu là trận Chung kết hoặc Tranh hạng 3.
        /// </summary>
        /// <param name="tranDauId">Mã định danh trận đấu vừa kết thúc</param>
        /// <param name="winnerDangKyId">Mã đăng ký thi đấu của đội/VĐV chiến thắng</param>
        /// <param name="loserDangKyId">Mã đăng ký thi đấu của đội/VĐV thua cuộc</param>
        /// <param name="username">Tên người thực hiện cập nhật</param>
        /// <returns>Đối tượng KnockoutProgressionResult chứa chi tiết tiến trình đã cập nhật</returns>
        Task<KnockoutProgressionResult> ProcessKnockoutProgressionAsync(
            int tranDauId,
            int winnerDangKyId,
            int loserDangKyId,
            string? username = null);
    }
}
