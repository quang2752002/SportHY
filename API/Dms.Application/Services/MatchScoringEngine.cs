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

        /// <summary>
        /// Kiểm tra điểm từng set theo luật môn, đếm số set thắng và xác định bên thắng trận.
        /// </summary>
        /// <param name="request">Kết quả từng set do trọng tài gửi.</param>
        /// <param name="config">Cấu hình điểm đích, điểm cách biệt và số set thắng để thắng trận.</param>
        /// <param name="isKnockout">Cho biết trận có thuộc vòng loại trực tiếp; thể thức set luôn phải có đội thắng.</param>
        /// <param name="result">Kết quả thẩm định được cập nhật trực tiếp vào đối tượng này.</param>
        private static void EvaluateSetBasedSport(
            CompleteMatchRequestDto request,
            CauHinhTheThucDto config,
            bool isKnockout,
            MatchEvaluationResult result)
        {
            var sets = (request.SetScores ?? new()).OrderBy(set => set.SetNumber).ToList();
            int targetSets = config.SoHiepThangDeThangTran ?? 2;
            int setsWon1 = 0;
            int setsWon2 = 0;

            if (sets.Count == 0 || sets.Count > config.SoHiepToiDa || targetSets < 1 || targetSets > config.SoHiepToiDa)
            {
                result.IsValid = false;
                result.ErrorMessage = "Cần nhập điểm từng set và kiểm tra cấu hình số set tối đa/số set thắng.";
                return;
            }

            for (var index = 0; index < sets.Count; index++)
            {
                var set = sets[index];
                if (set.SetNumber != index + 1 || set.Score1 < 0 || set.Score2 < 0)
                {
                    result.IsValid = false;
                    result.ErrorMessage = "Số thứ tự set phải liên tục và điểm không được âm.";
                    return;
                }

                var setWinner = MatchScoreCalculator.GetSetWinner(config, set);
                if (setWinner == 0)
                {
                    result.IsValid = false;
                    result.ErrorMessage = $"Set {set.SetNumber} chưa kết thúc theo luật điểm của môn.";
                    return;
                }

                if (setWinner == 1) setsWon1++;
                else setsWon2++;

                if (index < sets.Count - 1 && (setsWon1 >= targetSets || setsWon2 >= targetSets))
                {
                    result.IsValid = false;
                    result.ErrorMessage = "Không thể ghi thêm set sau khi một đội đã đủ số set thắng.";
                    return;
                }
            }

            if (setsWon1 < targetSets && setsWon2 < targetSets)
            {
                result.IsValid = false;
                result.ErrorMessage = $"Chưa đội nào thắng đủ {targetSets} set để kết thúc trận.";
                return;
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
            if (string.Equals(config.LoaiTheThuc, "ThoiGianHiep", StringComparison.OrdinalIgnoreCase) && request.SetScores?.Count > 0)
            {
                s1 = request.SetScores.Sum(period => period.Score1);
                s2 = request.SetScores.Sum(period => period.Score2);
            }

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
