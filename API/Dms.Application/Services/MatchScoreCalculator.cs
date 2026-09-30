using Dms.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Dms.Application.Services
{
    /// <summary>
    /// Tính tỷ số hiện tại từ điểm đã ghi theo từng hiệp hoặc set, theo cấu hình thể thức của môn.
    /// </summary>
    public static class MatchScoreCalculator
    {
        /// <summary>
        /// Tính tỉ số trận đang diễn ra: cộng điểm các hiệp chính với thể thức thời gian, hoặc đếm set thắng với thể thức set.
        /// </summary>
        /// <param name="config">Cấu hình thể thức áp dụng cho môn thi đấu.</param>
        /// <param name="periodScores">Danh sách điểm đã ghi theo thứ tự hiệp/set.</param>
        /// <returns>Tỉ số trận hiện tại của đội 1 và đội 2.</returns>
        public static (int Score1, int Score2) CalculateCurrentScore(
            CauHinhTheThucDto config,
            IEnumerable<SetScoreDto>? periodScores)
        {
            var periods = (periodScores ?? Enumerable.Empty<SetScoreDto>())
                .OrderBy(period => period.SetNumber)
                .ToList();

            if (string.Equals(config.LoaiTheThuc, "ThoiGianHiep", StringComparison.OrdinalIgnoreCase))
            {
                return (periods.Sum(period => period.Score1), periods.Sum(period => period.Score2));
            }

            if (string.Equals(config.LoaiTheThuc, "SetDiem", StringComparison.OrdinalIgnoreCase))
            {
                var score1 = 0;
                var score2 = 0;
                foreach (var period in periods)
                {
                    var setWinner = GetSetWinner(config, period);
                    if (setWinner == 1) score1++;
                    else if (setWinner == 2) score2++;
                }

                return (score1, score2);
            }

            throw new InvalidOperationException("Môn đo thành tích không sử dụng cơ chế tỷ số trận đối kháng.");
        }

        /// <summary>
        /// Xác định bên thắng một set theo điểm đích, cách biệt tối thiểu và điểm trần đã cấu hình.
        /// </summary>
        /// <param name="config">Cấu hình thể thức set của môn.</param>
        /// <param name="set">Điểm hai đội trong set cần xác định.</param>
        /// <returns>1 nếu đội 1 thắng, 2 nếu đội 2 thắng, hoặc 0 nếu set chưa có người thắng.</returns>
        public static int GetSetWinner(CauHinhTheThucDto config, SetScoreDto set)
        {
            var targetPoints = set.SetNumber == config.SoHiepToiDa
                ? config.DiemHiepQuyetDinh ?? config.DiemMoiHiep ?? 21
                : config.DiemMoiHiep ?? 21;
            var minimumLead = Math.Max(1, config.CachBietDiemToiThieu);
            var pointCap = config.DiemToiDaMoiHiep.GetValueOrDefault();

            if (pointCap > 0 && set.Score1 >= pointCap && set.Score1 > set.Score2) return 1;
            if (pointCap > 0 && set.Score2 >= pointCap && set.Score2 > set.Score1) return 2;
            if (set.Score1 >= targetPoints && set.Score1 - set.Score2 >= minimumLead) return 1;
            if (set.Score2 >= targetPoints && set.Score2 - set.Score1 >= minimumLead) return 2;

            return 0;
        }
    }
}
