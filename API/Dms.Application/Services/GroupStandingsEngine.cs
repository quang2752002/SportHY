using Dms.Application.DTOs;
using Dms.Application.Interfaces;
using Dms.Domain.Entities;
using Dms.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Dms.Application.Services
{
    /// <summary>
    /// Triển khai động cơ tính toán bảng xếp hạng vòng bảng tự động theo cấu hình tiêu chí động của từng môn thể thao.
    /// </summary>
    public class GroupStandingsEngine : IGroupStandingsEngine
    {
        private readonly IUnitOfWork _unitOfWork;

        public GroupStandingsEngine(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        private class TeamGroupStats
        {
            public ThanhVienBang Member { get; set; } = null!;
            public int DangKyId { get; set; }
            public int SoTran { get; set; }
            public int SoThang { get; set; }
            public int SoHoa { get; set; }
            public int SoThua { get; set; }
            public decimal DiemGhiDuoc { get; set; }
            public decimal DiemBiGhi { get; set; }
            public decimal HieuSo => DiemGhiDuoc - DiemBiGhi;
            public int SoSetThang { get; set; }
            public int SoSetThua { get; set; }
            public decimal Diem { get; set; }
        }

        /// <summary>
        /// Tính toán lại toàn bộ chỉ số (Số trận, Thắng, Hòa, Thua, Điểm, Hiệu số, Set thắng/thua)
        /// và sắp xếp lại thứ hạng cho toàn bộ các đội/VĐV trong bảng đấu dựa trên chuỗi tiêu chí cấu hình.
        /// </summary>
        /// <param name="bangDauId">Mã định danh bảng đấu cần tính toán</param>
        /// <param name="config">Cấu hình thể thức và luật tính điểm của môn thể thao</param>
        /// <param name="username">Tên người thực hiện cập nhật</param>
        /// <returns>True nếu tính toán và cập nhật thành công, False nếu không tìm thấy bảng đấu</returns>
        public async Task<bool> RecalculateGroupStandingsAsync(int bangDauId, CauHinhTheThucDto config, string? username = null)
        {
            var members = (await _unitOfWork.ThanhVienBangs.FindAsync(m => m.BangDauId == bangDauId && m.IsDeleted != true)).ToList();
            if (members.Count == 0) return false;

            var matches = (await _unitOfWork.TranDaus.GetPagedAsync(
                1, 1000,
                predicate: t => t.BangDauId == bangDauId && t.IsDeleted != true && (t.TrangThai == "KetThuc" || t.TrangThai == "DaDau"),
                orderBy: null,
                t => t.ThanhPhanTranDaus
            )).Items.ToList();

            var statsDict = members.ToDictionary(m => m.DangKyThiDauId, m => new TeamGroupStats
            {
                Member = m,
                DangKyId = m.DangKyThiDauId
            });

            // Dictionary lưu kết quả đối đầu trực tiếp: Key = "minId_maxId", Value = WinnerId (hoặc 0 nếu hòa)
            var headToHeadMap = new Dictionary<string, int>();

            foreach (var match in matches)
            {
                var tpList = match.ThanhPhanTranDaus.Where(tp => tp.IsDeleted != true).OrderBy(tp => tp.ViTri ?? 1).ToList();
                if (tpList.Count < 2) continue;

                int team1Id = tpList[0].DangKyThiDauId;
                int team2Id = tpList[1].DangKyThiDauId;

                if (!statsDict.ContainsKey(team1Id) || !statsDict.ContainsKey(team2Id)) continue;

                int score1 = match.TySoDoi1 ?? 0;
                int score2 = match.TySoDoi2 ?? 0;

                // Fallback nếu TySoDoi1 chưa được lưu ở cấp bảng, đọc từ GhiChu JSON
                if (!match.TySoDoi1.HasValue && !string.IsNullOrEmpty(match.GhiChu) && match.GhiChu.StartsWith("{"))
                {
                    try
                    {
                        using var doc = JsonDocument.Parse(match.GhiChu);
                        if (doc.RootElement.TryGetProperty("score1", out var p1)) score1 = p1.GetInt32();
                        if (doc.RootElement.TryGetProperty("score2", out var p2)) score2 = p2.GetInt32();
                    }
                    catch { }
                }

                var s1 = statsDict[team1Id];
                var s2 = statsDict[team2Id];

                s1.SoTran++;
                s2.SoTran++;

                s1.DiemGhiDuoc += score1;
                s1.DiemBiGhi += score2;

                s2.DiemGhiDuoc += score2;
                s2.DiemBiGhi += score1;

                string h2hKey = team1Id < team2Id ? $"{team1Id}_{team2Id}" : $"{team2Id}_{team1Id}";

                if (score1 > score2)
                {
                    s1.SoThang++;
                    s2.SoThua++;
                    headToHeadMap[h2hKey] = team1Id;

                    if (config.CachTinhDiemTheoSet)
                    {
                        // Bóng chuyền theo tỷ số set: 3-0 hoặc 3-1 được 3 điểm, 3-2 được 2 điểm cho đội thắng và 1 điểm cho đội thua
                        s1.SoSetThang += score1;
                        s1.SoSetThua += score2;
                        s2.SoSetThang += score2;
                        s2.SoSetThua += score1;

                        if (score1 == 3 && (score2 == 0 || score2 == 1))
                        {
                            s1.Diem += 3;
                            s2.Diem += 0;
                        }
                        else if (score1 == 3 && score2 == 2)
                        {
                            s1.Diem += 2;
                            s2.Diem += 1;
                        }
                        else
                        {
                            s1.Diem += config.DiemThang;
                        }
                    }
                    else
                    {
                        s1.Diem += config.DiemThang;
                        s2.Diem += config.DiemThua;
                    }
                }
                else if (score2 > score1)
                {
                    s2.SoThang++;
                    s1.SoThua++;
                    headToHeadMap[h2hKey] = team2Id;

                    if (config.CachTinhDiemTheoSet)
                    {
                        s1.SoSetThang += score1;
                        s1.SoSetThua += score2;
                        s2.SoSetThang += score2;
                        s2.SoSetThua += score1;

                        if (score2 == 3 && (score1 == 0 || score1 == 1))
                        {
                            s2.Diem += 3;
                            s1.Diem += 0;
                        }
                        else if (score2 == 3 && score1 == 2)
                        {
                            s2.Diem += 2;
                            s1.Diem += 1;
                        }
                        else
                        {
                            s2.Diem += config.DiemThang;
                        }
                    }
                    else
                    {
                        s2.Diem += config.DiemThang;
                        s1.Diem += config.DiemThua;
                    }
                }
                else
                {
                    // Hòa
                    s1.SoHoa++;
                    s2.SoHoa++;
                    s1.Diem += config.DiemHoa;
                    s2.Diem += config.DiemHoa;
                    headToHeadMap[h2hKey] = 0; // Hòa
                }
            }

            // Phân tích tiêu chí xếp hạng từ JSON cấu hình
            List<string> criteria;
            try
            {
                criteria = JsonSerializer.Deserialize<List<string>>(config.TieuChiXepHangJson)
                           ?? new List<string> { "Diem", "HieuSo", "DiemGhiDuoc", "DoiDau", "SoTranThang" };
            }
            catch
            {
                criteria = new List<string> { "Diem", "HieuSo", "DiemGhiDuoc", "DoiDau", "SoTranThang" };
            }

            var sortedList = statsDict.Values.ToList();
            sortedList.Sort((a, b) =>
            {
                foreach (var criterion in criteria)
                {
                    int cmp = 0;
                    switch (criterion.Trim())
                    {
                        case "Diem":
                            cmp = b.Diem.CompareTo(a.Diem); // Cao hơn xếp trên
                            break;

                        case "HieuSo":
                            cmp = b.HieuSo.CompareTo(a.HieuSo); // Hiệu số cao hơn xếp trên
                            break;

                        case "DiemGhiDuoc":
                            cmp = b.DiemGhiDuoc.CompareTo(a.DiemGhiDuoc); // Ghi nhiều điểm hơn xếp trên
                            break;

                        case "SoTranThang":
                            cmp = b.SoThang.CompareTo(a.SoThang);
                            break;

                        case "HieuSoSet":
                            int diffA = a.SoSetThang - a.SoSetThua;
                            int diffB = b.SoSetThang - b.SoSetThua;
                            cmp = diffB.CompareTo(diffA);
                            break;

                        case "DoiDau":
                            string key = a.DangKyId < b.DangKyId ? $"{a.DangKyId}_{b.DangKyId}" : $"{b.DangKyId}_{a.DangKyId}";
                            if (headToHeadMap.TryGetValue(key, out int winnerId))
                            {
                                if (winnerId == a.DangKyId) cmp = -1; // a thắng b -> a xếp trên
                                else if (winnerId == b.DangKyId) cmp = 1; // b thắng a -> b xếp trên
                            }
                            break;
                    }

                    if (cmp != 0) return cmp;
                }

                // Tiêu chí phụ cuối cùng: HatGiong hoặc Id
                int seedA = a.Member.HatGiong ?? 999;
                int seedB = b.Member.HatGiong ?? 999;
                return seedA != seedB ? seedA.CompareTo(seedB) : a.DangKyId.CompareTo(b.DangKyId);
            });

            // Gán lại kết quả vào Entity ThanhVienBang
            for (int i = 0; i < sortedList.Count; i++)
            {
                var s = sortedList[i];
                var m = s.Member;

                m.SoTran = s.SoTran;
                m.SoThang = s.SoThang;
                m.SoHoa = s.SoHoa;
                m.SoThua = s.SoThua;
                m.DiemGhiDuoc = s.DiemGhiDuoc;
                m.DiemBiGhi = s.DiemBiGhi;
                m.HieuSo = s.HieuSo;
                m.SoSetThang = s.SoSetThang;
                m.SoSetThua = s.SoSetThua;
                m.Diem = s.Diem;
                m.XepHang = i + 1; // Thứ hạng 1, 2, 3...
                m.LastModified = DateTime.UtcNow;
                m.LastModifiedBy = username;

                _unitOfWork.ThanhVienBangs.Update(m);
            }

            await _unitOfWork.CompleteAsync();
            return true;
        }
    }
}
