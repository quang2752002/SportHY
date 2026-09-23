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
    public class QuickScoreRequestDto
    {
        public int TranDauId { get; set; }
        public int Score1 { get; set; }
        public int Score2 { get; set; }
        public string TrangThai { get; set; } = "ChuaDau";
        public string? GhiChu { get; set; }
    }

    public class HomeController : BaseTrongTaiController
    {
        private readonly IGiaiDauService _giaiDauService;
        private readonly ISanDauService _sanDauService;
        private readonly IMonTheThaoService _monTheThaoService;

        public HomeController(
            UserManager<ApplicationUser> userManager,
            ITrongTaiService trongTaiService,
            ITranDauService tranDauService,
            IGiaiDauService giaiDauService,
            ISanDauService sanDauService,
            IMonTheThaoService monTheThaoService)
            : base(userManager, trongTaiService, tranDauService)
        {
            _giaiDauService = giaiDauService;
            _sanDauService = sanDauService;
            _monTheThaoService = monTheThaoService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int? giaiDauId = null, string? filterScope = null)
        {
            var currentRef = await GetCurrentRefereeAsync();
            var canBrowseAll = CanBrowseAllMatches();
            var tournaments = (await _giaiDauService.GetAllAsync())?.ToList() ?? new();
            ViewBag.Tournaments = tournaments;

            int? selectedGiaiDauId = giaiDauId;
            if (!selectedGiaiDauId.HasValue && tournaments.Count > 0)
            {
                selectedGiaiDauId = tournaments[0].Id;
            }
            ViewBag.SelectedGiaiDauId = selectedGiaiDauId;
            ViewBag.CanBrowseAllMatches = canBrowseAll;

            var allMatches = (await _tranDauService.GetAccessibleMatchesAsync(selectedGiaiDauId, currentRef?.Id, canBrowseAll))?.ToList() ?? new();
            var assignedMatches = currentRef == null
                ? new List<TranDauDto>()
                : (await _tranDauService.GetAccessibleMatchesAsync(selectedGiaiDauId, currentRef.Id, false))?.ToList() ?? new();
            var selectedScope = canBrowseAll && filterScope == "my_matches" ? "my_matches" : (canBrowseAll ? "all" : "my_matches");
            var displayMatches = selectedScope == "all" ? allMatches : assignedMatches;

            ViewBag.FilterScope = selectedScope;
            ViewBag.AllMatchesCount = canBrowseAll ? allMatches.Count : assignedMatches.Count;
            ViewBag.MyAssignedCount = assignedMatches.Count;
            ViewBag.LiveMatchesCount = displayMatches.Count(m => m.TrangThai == "DangDau" || m.TrangThai == "DangDienRa");
            ViewBag.FinishedCount = displayMatches.Count(m => m.TrangThai == "KetThuc" || m.TrangThai == "DaKetThuc");
            ViewBag.UpcomingCount = displayMatches.Count(m => m.TrangThai == "ChuaDau");
            ViewBag.NoMyMatchesNotice = !canBrowseAll && assignedMatches.Count == 0;

            return View(displayMatches);
        }

        [HttpPost]
        public async Task<IActionResult> QuickUpdateScore([FromBody] QuickScoreRequestDto dto)
        {
            if (dto == null || dto.TranDauId <= 0)
            {
                return Json(new { success = false, message = "Dữ liệu cập nhật không hợp lệ." });
            }

            var match = await _tranDauService.GetByIdAsync(dto.TranDauId);
            if (match == null)
            {
                return Json(new { success = false, message = "Không tìm thấy trận đấu." });
            }

            if (!await CanAccessMatchAsync(match))
            {
                return Forbid();
            }

            if (dto.Score1 < 0 || dto.Score2 < 0 || !new[] { "ChuaDau", "DangDau", "KetThuc" }.Contains(dto.TrangThai))
            {
                return Json(new { success = false, message = "Tỷ số hoặc trạng thái trận đấu không hợp lệ." });
            }

            var username = User.FindFirst(ClaimTypes.Name)?.Value ?? "TrongTai";
            var nowUtc = DateTime.UtcNow;
            try
            {
                var scoreData = new Dictionary<string, object?>();
                if (!string.IsNullOrWhiteSpace(match.GhiChu) && match.GhiChu.TrimStart().StartsWith("{"))
                {
                    try
                    {
                        using var document = JsonDocument.Parse(match.GhiChu);
                        foreach (var property in document.RootElement.EnumerateObject())
                        {
                            scoreData[property.Name] = property.Value.Clone();
                        }
                    }
                    catch (JsonException) { }
                }
                scoreData["score1"] = dto.Score1;
                scoreData["score2"] = dto.Score2;
                scoreData["status"] = dto.TrangThai;
                scoreData["notes"] = dto.GhiChu;
                scoreData["updatedBy"] = username;
                scoreData["updatedAt"] = nowUtc;
                string scoreJson = JsonSerializer.Serialize(scoreData);

                var isDraw = dto.TrangThai == "KetThuc" && dto.Score1 == dto.Score2;
                var winnerId = dto.TrangThai == "KetThuc" && dto.Score1 != dto.Score2
                    ? (dto.Score1 > dto.Score2 ? match.Doi1DangKyId : match.Doi2DangKyId)
                    : null;
                var loserId = dto.TrangThai == "KetThuc" && dto.Score1 != dto.Score2
                    ? (dto.Score1 > dto.Score2 ? match.Doi2DangKyId : match.Doi1DangKyId)
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
                return Json(new { success = true, message = "Đã cập nhật tỷ số và trạng thái trận đấu thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}
