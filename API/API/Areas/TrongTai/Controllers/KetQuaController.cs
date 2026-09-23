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
        private readonly ITranDauService _tranDauService;
        private readonly IGiaiDauService _giaiDauService;
        private readonly ICauHinhTheThucService _cauHinhTheThucService;

        public KetQuaController(
            UserManager<ApplicationUser> userManager,
            ITrongTaiService trongTaiService,
            ITranDauService tranDauService,
            IGiaiDauService giaiDauService,
            ICauHinhTheThucService cauHinhTheThucService,
            ITruongBanTrongTaiService refereeAccessService)
            : base(userManager, trongTaiService, refereeAccessService)
        {
            _tranDauService = tranDauService;
            _giaiDauService = giaiDauService;
            _cauHinhTheThucService = cauHinhTheThucService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int? tranDauId = null, int? giaiDauId = null)
        {
            var currentReferee = await GetCurrentRefereeAsync();
            var allTournaments = (await _giaiDauService.GetAllAsync())?.ToList() ?? new();
            var assignedTournamentIds = await GetAssignedTournamentIdsAsync(currentReferee);
            var tournaments = CanViewAllTournamentMatches
                ? allTournaments
                : allTournaments.Where(tournament => assignedTournamentIds.Contains(tournament.Id)).ToList();
            ViewBag.Tournaments = tournaments;

            int? selectedGiaiDauId = giaiDauId;
            if ((!selectedGiaiDauId.HasValue || tournaments.All(tournament => tournament.Id != selectedGiaiDauId.Value)) && tournaments.Count > 0)
            {
                selectedGiaiDauId = tournaments[0].Id;
            }
            ViewBag.SelectedGiaiDauId = selectedGiaiDauId;

            var matches = selectedGiaiDauId.HasValue
                ? (await _tranDauService.GetAllAsync(giaiDauId: selectedGiaiDauId))?.ToList() ?? new()
                : new List<TranDauDto>();
            if (!CanViewAllTournamentMatches && currentReferee != null)
            {
                matches = matches.Where(match => match.DanhSachTrongTai?.Any(assignment => assignment.TrongTaiId == currentReferee.Id) == true).ToList();
            }
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

            if (!await CanAccessMatchAsync(dto.TranDauId)) return Forbid();

            var match = await _tranDauService.GetByIdAsync(dto.TranDauId);
            if (match == null)
            {
                return Json(new { success = false, message = "Không tìm thấy trận đấu." });
            }

            var username = User.FindFirst(ClaimTypes.Name)?.Value ?? "TrongTai";

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
                    updatedAt = DateTime.Now
                };

                string scoreJson = JsonSerializer.Serialize(scoreData);

                var updateDto = new CreateUpdateTranDauDto
                {
                    GiaiDauMonTheThaoId = match.GiaiDauMonTheThaoId,
                    VongDauId = match.VongDauId,
                    BangDauId = match.BangDauId,
                    SanDauId = match.SanDauId,
                    SoTran = match.SoTran,
                    TenTran = match.TenTran,
                    ThoiGianDuKien = match.ThoiGianDuKien,
                    ThoiGianBatDau = match.ThoiGianBatDau ?? (dto.TrangThai == "DangDau" ? DateTime.Now : null),
                    ThoiGianKetThuc = dto.TrangThai == "KetThuc" ? (match.ThoiGianKetThuc ?? DateTime.Now) : null,
                    TrangThai = dto.TrangThai,
                    GhiChu = scoreJson,
                    Doi1DangKyId = match.Doi1DangKyId,
                    Doi2DangKyId = match.Doi2DangKyId,
                    DanhSachTrongTai = match.DanhSachTrongTai.Select(t => new AssignTrongTaiDto
                    {
                        TrongTaiId = t.TrongTaiId,
                        VaiTro = t.VaiTro,
                        GhiChu = t.GhiChu
                    }).ToList()
                };

                await _tranDauService.UpdateAsync(dto.TranDauId, updateDto, username);
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
            if (!await CanAccessMatchAsync(tranDauId)) return Forbid();

            try
            {
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

            if (!await CanAccessMatchAsync(dto.TranDauId)) return Forbid();

            try
            {
                var username = User.FindFirst(ClaimTypes.Name)?.Value ?? "TrongTai";
                var response = await _tranDauService.CompleteMatchResultAsync(dto, username);
                return Json(response);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}
