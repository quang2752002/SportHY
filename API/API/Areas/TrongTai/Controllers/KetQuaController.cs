using Dms.Application.DTOs;
using Dms.Application.Interfaces;
using Dms.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;

namespace API.Areas.TrongTai.Controllers
{

    public class SaveMatchResultDto
    {
        public int TranDauId { get; set; }
        public int Score1 { get; set; }
        public int Score2 { get; set; }
        public string? Winner { get; set; } // "1", "2", "draw"
        public string TrangThai { get; set; } = "DangDau";
        public List<SetScoreDto>? SetScores { get; set; }
        public List<MatchEventItemDto>? Events { get; set; }
        public string? GhiChu { get; set; }
    }

    public class KetQuaController : BaseTrongTaiController
    {
        private readonly IGiaiDauService _giaiDauService;
        private readonly ICauHinhTheThucService _cauHinhTheThucService;

        public KetQuaController(
            UserManager<ApplicationUser> userManager,
            ITrongTaiService trongTaiService,
            ITranDauService tranDauService,
            IGiaiDauService giaiDauService,
            ICauHinhTheThucService cauHinhTheThucService)
            : base(userManager, trongTaiService, tranDauService)
        {
            _giaiDauService = giaiDauService;
            _cauHinhTheThucService = cauHinhTheThucService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int? tranDauId = null, int? giaiDauId = null)
        {
            var currentReferee = await GetCurrentRefereeAsync();
            var tournaments = (await _giaiDauService.GetAllAsync())?.ToList() ?? new();
            ViewBag.Tournaments = tournaments;

            int? selectedGiaiDauId = giaiDauId;
            if (!selectedGiaiDauId.HasValue && tournaments.Count > 0)
            {
                selectedGiaiDauId = tournaments[0].Id;
            }
            ViewBag.SelectedGiaiDauId = selectedGiaiDauId;

            var matches = (await _tranDauService.GetAccessibleMatchesAsync(selectedGiaiDauId, currentReferee?.Id, CanBrowseAllMatches()))?.ToList() ?? new();
            ViewBag.Matches = matches;

            TranDauDto? currentMatch = null;
            if (tranDauId.HasValue)
            {
                currentMatch = matches.FirstOrDefault(m => m.Id == tranDauId.Value);
                if (currentMatch == null) return Forbid();
            }
            else if (matches.Count > 0)
            {
                currentMatch = matches[0];
            }

            ViewBag.CurrentMatch = currentMatch;

            CauHinhTheThucDto? matchFormat = null;
            if (currentMatch != null)
            {
                matchFormat = await _cauHinhTheThucService.GetConfigByTranDauIdAsync(currentMatch.Id);
            }
            ViewBag.MatchFormat = matchFormat;

            // Đọc dữ liệu JSON chi tiết từ GhiChu nếu có
            int score1 = 0, score2 = 0;
            string winner = "";
            string notes = "";
            var setScores = new List<SetScoreDto>();
            var events = new List<MatchEventItemDto>();

            if (currentMatch != null && !string.IsNullOrEmpty(currentMatch.GhiChu) && currentMatch.GhiChu.StartsWith("{"))
            {
                try
                {
                    using var doc = JsonDocument.Parse(currentMatch.GhiChu);
                    var root = doc.RootElement;
                    if (root.TryGetProperty("score1", out var p1)) score1 = p1.GetInt32();
                    if (root.TryGetProperty("score2", out var p2)) score2 = p2.GetInt32();
                    if (root.TryGetProperty("winner", out var pw)) winner = pw.GetString() ?? "";
                    if (root.TryGetProperty("notes", out var pn)) notes = pn.GetString() ?? "";

                    if (root.TryGetProperty("setScores", out var pSets) && pSets.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var el in pSets.EnumerateArray())
                        {
                            setScores.Add(new SetScoreDto
                            {
                                SetNumber = el.TryGetProperty("setNumber", out var sn) ? sn.GetInt32() : 1,
                                Score1 = el.TryGetProperty("score1", out var s1) ? s1.GetInt32() : 0,
                                Score2 = el.TryGetProperty("score2", out var s2) ? s2.GetInt32() : 0,
                            });
                        }
                    }

                    if (root.TryGetProperty("events", out var pEvents) && pEvents.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var el in pEvents.EnumerateArray())
                        {
                            events.Add(new MatchEventItemDto
                            {
                                Id = el.TryGetProperty("id", out var eid) ? eid.GetInt32() : 0,
                                Minute = el.TryGetProperty("minute", out var em) ? em.GetInt32() : 0,
                                Type = el.TryGetProperty("type", out var et) ? et.GetString() ?? "goal" : "goal",
                                Team = el.TryGetProperty("team", out var etm) ? etm.GetInt32() : 1,
                                Player = el.TryGetProperty("player", out var ep) ? ep.GetString() : null,
                                Notes = el.TryGetProperty("notes", out var en) ? en.GetString() : null,
                            });
                        }
                    }
                }
                catch { }
            }

            ViewBag.Score1 = score1;
            ViewBag.Score2 = score2;
            ViewBag.Winner = winner;
            ViewBag.Notes = notes;
            ViewBag.SetScores = setScores;
            ViewBag.Events = events;

            return View(currentMatch);
        }

        [HttpPost]
        public async Task<IActionResult> SaveResult([FromBody] SaveMatchResultDto dto)
        {
            if (dto == null || dto.TranDauId <= 0)
            {
                return Json(new { success = false, message = "Dữ liệu không hợp lệ." });
            }
            if (dto.Score1 < 0 || dto.Score2 < 0 || !new[] { "ChuaDau", "DangDau", "KetThuc" }.Contains(dto.TrangThai))
            {
                return BadRequest(new { success = false, message = "Tỷ số hoặc trạng thái trận đấu không hợp lệ." });
            }

            var match = await _tranDauService.GetByIdAsync(dto.TranDauId);
            if (match == null)
            {
                return Json(new { success = false, message = "Không tìm thấy trận đấu." });
            }

            if (!await CanAccessMatchAsync(match)) return Forbid();

            var matchFormat = await _cauHinhTheThucService.GetConfigByTranDauIdAsync(match.Id);
            if (matchFormat?.CoThePhat != true && dto.Events?.Any(IsPenaltyCardEvent) == true)
            {
                return BadRequest(new { success = false, message = "Môn thi đấu này không áp dụng thẻ phạt theo cấu hình." });
            }

            var username = User.FindFirst(ClaimTypes.Name)?.Value ?? "TrongTai";
            var nowUtc = DateTime.UtcNow;
            try
            {
                var scoreData = new
                {
                    score1 = dto.Score1,
                    score2 = dto.Score2,
                    winner = dto.Winner,
                    status = dto.TrangThai,
                    notes = dto.GhiChu,
                    setScores = dto.SetScores ?? new List<SetScoreDto>(),
                    events = dto.Events ?? new List<MatchEventItemDto>(),
                    updatedBy = username,
                    updatedAt = nowUtc
                };

                string scoreJson = JsonSerializer.Serialize(scoreData);

                var isDraw = dto.TrangThai == "KetThuc" && dto.Winner == "draw";
                var winnerId = dto.TrangThai == "KetThuc" && dto.Winner == "1" ? match.Doi1DangKyId
                    : dto.TrangThai == "KetThuc" && dto.Winner == "2" ? match.Doi2DangKyId
                    : null;
                var loserId = dto.TrangThai == "KetThuc" && dto.Winner == "1" ? match.Doi2DangKyId
                    : dto.TrangThai == "KetThuc" && dto.Winner == "2" ? match.Doi1DangKyId
                    : null;
                var updateDto = new UpdateMatchProgressDto
                {
                    Score1 = dto.Score1,
                    Score2 = dto.Score2,
                    TrangThai = dto.TrangThai,
                    GhiChu = scoreJson,
                    IsHoa = isDraw,
                    DoiThangDangKyId = winnerId,
                    DoiThuaDangKyId = loserId
                };

                await _tranDauService.UpdateMatchProgressAsync(dto.TranDauId, updateDto, username);
                return Json(new { success = true, message = "Đã lưu kết quả, điểm số và diễn biến trận đấu thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetMatchFormat(int tranDauId)
        {
            try
            {
                var match = await _tranDauService.GetByIdAsync(tranDauId);
                if (match == null) return NotFound(new { success = false, message = "Không tìm thấy trận đấu." });
                if (!await CanAccessMatchAsync(match)) return Forbid();

                var config = await _cauHinhTheThucService.GetConfigByTranDauIdAsync(tranDauId);
                return Json(new { success = true, data = config });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CompleteMatch([FromBody] CompleteMatchRequestDto dto)
        {
            if (dto == null || dto.TranDauId <= 0)
            {
                return Json(new { success = false, message = "Dữ liệu không hợp lệ." });
            }

            try
            {
                var match = await _tranDauService.GetByIdAsync(dto.TranDauId);
                if (match == null) return NotFound(new { success = false, message = "Không tìm thấy trận đấu." });
                if (!await CanAccessMatchAsync(match)) return Forbid();

                var matchFormat = await _cauHinhTheThucService.GetConfigByTranDauIdAsync(match.Id);
                if (matchFormat?.CoThePhat != true && dto.Events?.Any(IsPenaltyCardEvent) == true)
                {
                    return BadRequest(new { success = false, message = "Môn thi đấu này không áp dụng thẻ phạt theo cấu hình." });
                }

                var username = User.FindFirst(ClaimTypes.Name)?.Value ?? "TrongTai";
                var response = await _tranDauService.CompleteMatchResultAsync(dto, username);
                return Json(response);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        private static bool IsPenaltyCardEvent(MatchEventItemDto matchEvent)
        {
            return string.Equals(matchEvent.Type, "yellow_card", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(matchEvent.Type, "red_card", StringComparison.OrdinalIgnoreCase);
        }
    }
}
