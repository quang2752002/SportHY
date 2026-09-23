using Dms.Application.DTOs;
using Dms.Application.Interfaces;
using System;
using System.Linq;

namespace Dms.Application.Services
{
    /// <summary>
    /// Triển khai động cơ thẩm định và xác định kết quả trận đấu theo thể thức từng môn thể thao.
    /// Hỗ trợ cả 3 loại thể thức: SetDiem (Cầu lông, Bóng chuyền, Bóng bàn), ThoiGianHiep (Bóng đá), TinhDiemXepHang.
    /// </summary>
    public class MatchScoringEngine : IMatchScoringEngine
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
        public MatchEvaluationResult EvaluateMatchResult(CompleteMatchRequestDto request, CauHinhTheThucDto config, bool isKnockout)
        {
            var result = new MatchEvaluationResult();

            if (config.LoaiTheThuc == "SetDiem")
            {
                EvaluateSetBasedSport(request, config, isKnockout, result);
            }
            else if (config.LoaiTheThuc == "ThoiGianHiep")
            {
                EvaluateTimedGoalSport(request, config, isKnockout, result);
            }
            else
            {
                // Thể thức điểm số mặc định vẫn phải tuân thủ luật hòa theo giai đoạn.
                EvaluateTimedGoalSport(request, config, isKnockout, result);
            }

            return result;
        }

        private static void EvaluateSetBasedSport(
            CompleteMatchRequestDto request,
            CauHinhTheThucDto config,
            bool isKnockout,
            MatchEvaluationResult result)
        {
            var sets = request.SetScores ?? new();
            int targetSets = config.SoHiepThangDeThangTran ?? 2;
            int setsWon1 = 0;
            int setsWon2 = 0;

            int setIndex = 0;
            foreach (var s in sets)
            {
                setIndex++;
                bool isDecidingSet = (setIndex == config.SoHiepToiDa);
                int minPoint = isDecidingSet ? (config.DiemHiepQuyetDinh ?? config.DiemMoiHiep ?? 21) : (config.DiemMoiHiep ?? 21);
                int gap = config.CachBietDiemToiThieu;
                int? maxCap = config.DiemToiDaMoiHiep;

                // Xác định đội thắng set
                bool team1WonSet = false;
                bool team2WonSet = false;

                if (maxCap.HasValue && s.Score1 >= maxCap.Value && s.Score1 > s.Score2)
                {
                    team1WonSet = true;
                }
                else if (maxCap.HasValue && s.Score2 >= maxCap.Value && s.Score2 > s.Score1)
                {
                    team2WonSet = true;
                }
                else if (s.Score1 >= minPoint && (s.Score1 - s.Score2) >= gap)
                {
                    team1WonSet = true;
                }
                else if (s.Score2 >= minPoint && (s.Score2 - s.Score1) >= gap)
                {
                    team2WonSet = true;
                }
                else
                {
                    // Nếu chưa đạt điểm tối thiểu chuẩn nhưng trận đã được yêu cầu kết thúc, so sánh điểm trực tiếp
                    if (s.Score1 > s.Score2) team1WonSet = true;
                    else if (s.Score2 > s.Score1) team2WonSet = true;
                }

                if (team1WonSet) setsWon1++;
                else if (team2WonSet) setsWon2++;

                // Nếu một đội đã đạt số set thắng yêu cầu thì có thể kết thúc trận sớm (ví dụ thắng 2-0 trong Best of 3)
                if (setsWon1 == targetSets || setsWon2 == targetSets)
                {
                    break;
                }
            }

            // Nếu trọng tài đã nhập trực tiếp Score1, Score2 mà không có danh sách Set
            if (sets.Count == 0)
            {
                setsWon1 = request.Score1;
                setsWon2 = request.Score2;
            }

            result.FinalScore1 = setsWon1;
            result.FinalScore2 = setsWon2;

            if (setsWon1 == setsWon2)
            {
                result.IsValid = false;
                result.ErrorMessage = $"Môn thể thao theo set ({config.TenMonTheThao ?? "Cầu lông/Bóng chuyền"}) không cho phép kết quả hòa. Cần tiếp tục thi đấu để xác định đội thắng.";
                result.IsDraw = true;
                return;
            }

            if (setsWon1 > setsWon2)
            {
                result.WinnerTeamIndex = 1;
            }
            else
            {
                result.WinnerTeamIndex = 2;
            }
        }

        /// <summary>
        /// Phân định kết quả thể thức tính điểm theo luật hòa của vòng bảng hoặc loại trực tiếp,
        /// bao gồm tổng điểm hiệp phụ và luân lưu khi được cấu hình.
        /// </summary>
        /// <param name="request">Tỷ số chính, điểm hiệp phụ và kết quả luân lưu do trọng tài gửi</param>
        /// <param name="config">Quy tắc hòa và các cách phân định được cấu hình cho môn</param>
        /// <param name="isKnockout">Cho biết trận thuộc giai đoạn loại trực tiếp</param>
        /// <param name="result">Đối tượng được điền kết quả hợp lệ hoặc lý do chưa thể chốt</param>
        private static void EvaluateTimedGoalSport(
            CompleteMatchRequestDto request,
            CauHinhTheThucDto config,
            bool isKnockout,
            MatchEvaluationResult result)
        {
            int s1 = request.Score1;
            int s2 = request.Score2;

            result.PenaltyScore1 = request.PenaltyScore1;
            result.PenaltyScore2 = request.PenaltyScore2;

            if (request.Score1 < 0 || request.Score2 < 0 ||
                request.ExtraTimeScore1 < 0 || request.ExtraTimeScore2 < 0 ||
                request.PenaltyScore1 < 0 || request.PenaltyScore2 < 0)
            {
                result.IsValid = false;
                result.ErrorMessage = "Tỷ số, điểm hiệp phụ và luân lưu không được âm.";
                return;
            }

            if (s1 > s2)
            {
                result.FinalScore1 = s1;
                result.FinalScore2 = s2;
                result.WinnerTeamIndex = 1;
                result.IsDraw = false;
                return;
            }

            if (s2 > s1)
            {
                result.FinalScore1 = s1;
                result.FinalScore2 = s2;
                result.WinnerTeamIndex = 2;
                result.IsDraw = false;
                return;
            }

            // Trường hợp tỷ số bằng nhau (Hòa)
            if (!isKnockout)
            {
                // Vòng bảng
                if (config.ChoPhepHoaVongBang)
                {
                    result.FinalScore1 = s1;
                    result.FinalScore2 = s2;
                    result.IsDraw = true;
                    result.WinnerTeamIndex = 0;
                    return;
                }
            }

            if (isKnockout && config.CoHiepPhu)
            {
                if (!request.ExtraTimeScore1.HasValue || !request.ExtraTimeScore2.HasValue)
                {
                    result.IsValid = false;
                    result.RequiresOvertimeOrPenalty = true;
                    result.ErrorMessage = "Trận đấu đang hòa. Hãy nhập tỷ số hiệp phụ trước khi chốt kết quả.";
                    return;
                }

                s1 += request.ExtraTimeScore1.Value;
                s2 += request.ExtraTimeScore2.Value;
                if (s1 != s2)
                {
                    result.FinalScore1 = s1;
                    result.FinalScore2 = s2;
                    result.WinnerTeamIndex = s1 > s2 ? 1 : 2;
                    result.IsDraw = false;
                    return;
                }
            }

            result.FinalScore1 = s1;
            result.FinalScore2 = s2;

            if (config.CoPenalty)
            {
                if (request.PenaltyScore1.HasValue && request.PenaltyScore2.HasValue &&
                    request.PenaltyScore1.Value != request.PenaltyScore2.Value)
                {
                    result.WinnerTeamIndex = request.PenaltyScore1.Value > request.PenaltyScore2.Value ? 1 : 2;
                    result.IsDraw = false;
                    return;
                }

                result.IsValid = false;
                result.RequiresOvertimeOrPenalty = true;
                result.ErrorMessage = "Tỷ số vẫn hòa. Hãy nhập kết quả luân lưu khác nhau để xác định đội thắng.";
                return;
            }

            result.IsValid = false;
            result.ErrorMessage = isKnockout
                ? "Trận loại trực tiếp vẫn hòa và chưa được cấu hình cách phân định tiếp theo."
                : "Thể thức môn này không cho phép hòa ở vòng bảng; cần cấu hình cách phân định kết quả.";
        }
    }
}
