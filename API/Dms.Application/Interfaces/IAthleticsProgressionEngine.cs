using Dms.Application.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dms.Application.Interfaces
{
    /// <summary>
    /// Kết quả xử lý lượt thi đo thành tích (Điền kinh / Bơi lội)
    /// </summary>
    public class AthleticsProcessResult
    {
        public bool Success { get; set; } = true;
        public string Message { get; set; } = string.Empty;
        public bool IsRecordBroken { get; set; }
        public string? RecordBreakerNotice { get; set; }
        public string? FinalHeatAdvancementNotice { get; set; }
        public List<string> MedalsAwarded { get; set; } = new();
    }

    /// <summary>
    /// Động cơ quản lý lượt thi và bảng xếp hạng thành tích cho môn Điền kinh và Bơi lội:
    /// lưu kết quả từng làn, xử lý DNS/DNF/DQ, tự động gom Top thành tích vào Lượt Chung kết,
    /// xếp làn ưu tiên hạt giống, và tự động trao huy chương.
    /// </summary>
    public interface IAthleticsProgressionEngine
    {
        /// <summary>
        /// Ghi nhận kết quả lượt thi (Heat) bơi lội / điền kinh, cập nhật thứ hạng theo làn,
        /// kiểm tra phá kỷ lục giải, và tự động đưa Top VĐV vào Lượt Chung kết hoặc trao huy chương.
        /// </summary>
        /// <param name="tranDauId">Mã định danh lượt thi vừa hoàn thành</param>
        /// <param name="heatResults">Danh sách thành tích của các VĐV theo làn</param>
        /// <param name="config">Cấu hình thể thức đo thành tích của môn</param>
        /// <param name="username">Tên người thực hiện thao tác</param>
        /// <returns>Đối tượng AthleticsProcessResult chứa thông tin chi tiết tiến trình</returns>
        Task<AthleticsProcessResult> ProcessHeatResultAsync(
            int tranDauId,
            List<HeatParticipantResultDto> heatResults,
            CauHinhTheThucDto config,
            string? username = null);

        /// <summary>
        /// Lưu tạm kết quả lượt thi (Heat / Lượt thử / Biểu diễn) của các VĐV vào cơ sở dữ liệu mà không chốt kết thúc trận đấu.
        /// Cập nhật thông tin làn chạy, số BIB, thứ tự thi đấu, chi tiết JSON và bảng điểm thành tích.
        /// </summary>
        /// <param name="tranDauId">Mã định danh lượt thi đang diễn ra</param>
        /// <param name="heatResults">Danh sách kết quả hoặc lượt thi của từng VĐV</param>
        /// <param name="config">Cấu hình thể thức thi đấu</param>
        /// <param name="username">Tên tài khoản người thực hiện lưu tạm</param>
        /// <returns>True nếu lưu tạm thành công</returns>
        Task<bool> SaveDraftHeatResultsAsync(
            int tranDauId,
            List<HeatParticipantResultDto> heatResults,
            CauHinhTheThucDto config,
            string? username = null);
    }
}
