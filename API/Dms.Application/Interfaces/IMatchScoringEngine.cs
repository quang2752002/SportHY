using Dms.Application.DTOs;
using System.Threading.Tasks;

namespace Dms.Application.Interfaces
{
    /// <summary>
    /// Kết quả thẩm định và tính toán tỷ số trận đấu theo luật của môn thể thao
    /// </summary>
    public class MatchEvaluationResult
    {
        public bool IsValid { get; set; } = true;
        public string? ErrorMessage { get; set; }
        public int FinalScore1 { get; set; }
        public int FinalScore2 { get; set; }
        public int? PenaltyScore1 { get; set; }
        public int? PenaltyScore2 { get; set; }
        public int WinnerTeamIndex { get; set; } // 1: Đội 1 thắng, 2: Đội 2 thắng, 0: Hòa
        public bool IsDraw { get; set; }
        public bool RequiresOvertimeOrPenalty { get; set; }
    }

    /// <summary>
    /// Động cơ thẩm định và xác định kết quả trận đấu theo thể thức riêng của từng môn thể thao.
    /// </summary>
    public interface IMatchScoringEngine
    {
        /// <summary>
        /// Thẩm định và tính toán kết quả trận đấu dựa trên dữ liệu gửi từ trọng tài và cấu hình luật của môn.
        /// Kiểm tra tính hợp lệ của số set/hiệp, điểm số tối thiểu, cách biệt điểm (deuce),
        /// và chặn kết thúc trận nếu vòng Knockout đang hòa mà chưa phân định penalty.
        /// </summary>
        /// <param name="request">Dữ liệu kết quả trận đấu gửi từ trọng tài</param>
        /// <param name="config">Cấu hình thể thức thi đấu áp dụng cho môn/trận đấu</param>
        /// <param name="isKnockout">Cờ xác định trận đấu có thuộc vòng Knockout loại trực tiếp hay không</param>
        /// <returns>Đối tượng MatchEvaluationResult chứa trạng thái hợp lệ và tỷ số phân định</returns>
        MatchEvaluationResult EvaluateMatchResult(CompleteMatchRequestDto request, CauHinhTheThucDto config, bool isKnockout);
    }
}
