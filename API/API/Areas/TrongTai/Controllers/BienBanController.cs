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

    public class SignReportRequestDto
    {
        public int TranDauId { get; set; }
        public string Role { get; set; } = "referee"; // referee, secretary, team1, team2
        public string SignerName { get; set; } = string.Empty;
    }

    public class BienBanController : BaseTrongTaiController
    {
        private readonly ITranDauService _tranDauService;
        private readonly IGiaiDauService _giaiDauService;
        private readonly IDangKyThiDauService _dangKyThiDauService;

        public BienBanController(
            UserManager<ApplicationUser> userManager,
            ITrongTaiService trongTaiService,
            ITranDauService tranDauService,
            IGiaiDauService giaiDauService,
            IDangKyThiDauService dangKyThiDauService)
            : base(userManager, trongTaiService)
        {
            _tranDauService = tranDauService;
            _giaiDauService = giaiDauService;
            _dangKyThiDauService = dangKyThiDauService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int? tranDauId = null, int? giaiDauId = null)
        {
            var currentRef = await GetCurrentRefereeAsync();
            var tournaments = (await _giaiDauService.GetAllAsync())?.ToList() ?? new();
            ViewBag.Tournaments = tournaments;

            int? selectedGiaiDauId = giaiDauId;
            if (!selectedGiaiDauId.HasValue && tournaments.Count > 0)
            {
                selectedGiaiDauId = tournaments[0].Id;
            }
            ViewBag.SelectedGiaiDauId = selectedGiaiDauId;

            var matches = (await _tranDauService.GetAllAsync(giaiDauId: selectedGiaiDauId))?.ToList() ?? new();
            ViewBag.Matches = matches;

            TranDauDto? currentMatch = null;
            if (tranDauId.HasValue)
            {
                currentMatch = matches.FirstOrDefault(m => m.Id == tranDauId.Value) ?? await _tranDauService.GetByIdAsync(tranDauId.Value);
            }
            else if (matches.Count > 0)
            {
                currentMatch = matches[0];
            }

            ViewBag.CurrentMatch = currentMatch;

            // Đọc thông tin đăng ký của 2 đội để lấy danh sách vận động viên
            DangKyThiDauDto? regTeam1 = null;
            DangKyThiDauDto? regTeam2 = null;
            if (currentMatch != null)
            {
                if (currentMatch.Doi1DangKyId.HasValue)
                {
                    regTeam1 = await _dangKyThiDauService.GetByIdAsync(currentMatch.Doi1DangKyId.Value);
                }
                if (currentMatch.Doi2DangKyId.HasValue)
                {
                    regTeam2 = await _dangKyThiDauService.GetByIdAsync(currentMatch.Doi2DangKyId.Value);
                }
            }
            ViewBag.RegTeam1 = regTeam1;
            ViewBag.RegTeam2 = regTeam2;

            // Đọc dữ liệu tỷ số, hiệp và sự kiện từ GhiChu
            int score1 = 0, score2 = 0;
            string winner = "";
            string notes = "";
            var setScores = new List<SetScoreDto>();
            var events = new List<MatchEventItemDto>();
            var signatures = new Dictionary<string, SignatureInfoDto>();

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

                    if (root.TryGetProperty("signatures", out var pSigs) && pSigs.ValueKind == JsonValueKind.Object)
                    {
                        foreach (var prop in pSigs.EnumerateObject())
                        {
                            signatures[prop.Name] = new SignatureInfoDto
                            {
                                SignerName = prop.Value.TryGetProperty("signerName", out var ps) ? ps.GetString() : null,
                                Role = prop.Value.TryGetProperty("role", out var pr) ? pr.GetString() : null,
                                SignedAt = prop.Value.TryGetProperty("signedAt", out var pt) && pt.TryGetDateTime(out var dt) ? dt : null
                            };
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
            ViewBag.Signatures = signatures;

            return View(currentMatch);
        }

        [HttpPost]
        public async Task<IActionResult> SignReport([FromBody] SignReportRequestDto dto)
        {
            if (dto == null || dto.TranDauId <= 0 || string.IsNullOrWhiteSpace(dto.SignerName))
            {
                return Json(new { success = false, message = "Vui lòng nhập họ tên người ký xác nhận." });
            }

            var match = await _tranDauService.GetByIdAsync(dto.TranDauId);
            if (match == null)
            {
                return Json(new { success = false, message = "Không tìm thấy trận đấu." });
            }

            var username = User.FindFirst(ClaimTypes.Name)?.Value ?? "TrongTai";

            try
            {
                // Đọc json cũ nếu có
                var dict = new Dictionary<string, object>();
                var sigs = new Dictionary<string, object>();

                if (!string.IsNullOrEmpty(match.GhiChu) && match.GhiChu.StartsWith("{"))
                {
                    try
                    {
                        using var doc = JsonDocument.Parse(match.GhiChu);
                        foreach (var prop in doc.RootElement.EnumerateObject())
                        {
                            if (prop.Name == "signatures")
                            {
                                foreach (var s in prop.Value.EnumerateObject())
                                {
                                    sigs[s.Name] = s.Value.Clone();
                                }
                            }
                            else
                            {
                                dict[prop.Name] = prop.Value.Clone();
                            }
                        }
                    }
                    catch { }
                }

                sigs[dto.Role] = new
                {
                    signerName = dto.SignerName.Trim(),
                    role = dto.Role,
                    signedAt = DateTime.Now
                };
                dict["signatures"] = sigs;

                string newJson = JsonSerializer.Serialize(dict);

                var updateDto = new CreateUpdateTranDauDto
                {
                    GiaiDauMonTheThaoId = match.GiaiDauMonTheThaoId,
                    VongDauId = match.VongDauId,
                    BangDauId = match.BangDauId,
                    SanDauId = match.SanDauId,
                    SoTran = match.SoTran,
                    TenTran = match.TenTran,
                    ThoiGianDuKien = match.ThoiGianDuKien,
                    ThoiGianBatDau = match.ThoiGianBatDau,
                    ThoiGianKetThuc = match.ThoiGianKetThuc,
                    TrangThai = match.TrangThai,
                    GhiChu = newJson,
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
                return Json(new { success = true, message = $"Đã ký xác nhận biên bản trận đấu với vai trò [{dto.Role}] thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}
