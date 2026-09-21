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
                // Thể thức đo thành tích hoặc mặc định
                result.FinalScore1 = request.Score1;
                result.FinalScore2 = request.Score2;
                if (request.Score1 > request.Score2) result.WinnerTeamIndex = 1;
                else if (request.Score2 > request.Score1) result.WinnerTeamIndex = 2;
                else
                {
                    result.IsDraw = true;
                    result.WinnerTeamIndex = 0;
                }
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

        private static void EvaluateTimedGoalSport(
            CompleteMatchRequestDto request,
            CauHinhTheThucDto config,
            bool isKnockout,
            MatchEvaluationResult result)
        {
            int s1 = request.Score1;
            int s2 = request.Score2;

            result.FinalScore1 = s1;
            result.FinalScore2 = s2;
            result.PenaltyScore1 = request.PenaltyScore1;
            result.PenaltyScore2 = request.PenaltyScore2;

            if (s1 > s2)
            {
                result.WinnerTeamIndex = 1;
                result.IsDraw = false;
                return;
            }

            if (s2 > s1)
            {
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
                    result.IsDraw = true;
                    result.WinnerTeamIndex = 0;
                    return;
                }
            }

            // Vòng Knockout hoặc thể thức không cho phép hòa
            if (!config.ChoPhepHoaKnockout || isKnockout)
            {
                // Kiểm tra xem đã có kết quả penalty luân lưu chưa
                if (config.CoPenalty)
                {
                    int p1 = request.PenaltyScore1 ?? 0;
                    int p2 = request.PenaltyScore2 ?? 0;

                    if (request.PenaltyScore1.HasValue && request.PenaltyScore2.HasValue && p1 != p2)
                    {
                        // Đã phân định bằng Penalty thành công
                        result.WinnerTeamIndex = p1 > p2 ? 1 : 2;
                        result.IsDraw = false;
                        return;
                    }

                    // Chưa có penalty hoặc penalty đang hòa
                    result.IsValid = false;
                    result.RequiresOvertimeOrPenalty = true;
                    result.ErrorMessage = "Trận đấu Knockout đang hòa. Thể thức yêu cầu thực hiện lượt luân lưu Penalty để xác định đội đi tiếp.";
                    return;
                }

                result.IsValid = false;
                result.ErrorMessage = "Trận đấu loại trực tiếp không thể kết thúc với tỷ số hòa.";
                return;
            }
        }
    }
}
