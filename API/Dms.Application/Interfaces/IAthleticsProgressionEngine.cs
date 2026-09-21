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
    }
}
