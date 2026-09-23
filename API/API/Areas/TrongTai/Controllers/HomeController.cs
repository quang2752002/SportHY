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
        private readonly ITranDauService _tranDauService;
        private readonly IGiaiDauService _giaiDauService;
        private readonly ISanDauService _sanDauService;
        private readonly IMonTheThaoService _monTheThaoService;

        public HomeController(
            UserManager<ApplicationUser> userManager,
            ITrongTaiService trongTaiService,
            ITranDauService tranDauService,
            IGiaiDauService giaiDauService,
            ISanDauService sanDauService,
            IMonTheThaoService monTheThaoService,
            ITruongBanTrongTaiService refereeAccessService)
            : base(userManager, trongTaiService, refereeAccessService)
        {
            _tranDauService = tranDauService;
            _giaiDauService = giaiDauService;
            _sanDauService = sanDauService;
            _monTheThaoService = monTheThaoService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int? giaiDauId = null, string? filterScope = "my_matches")
        {
            var currentRef = await GetCurrentRefereeAsync();
            var allTournaments = (await _giaiDauService.GetAllAsync())?.ToList() ?? new();
            var assignedTournamentIds = await GetAssignedTournamentIdsAsync(currentRef);
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
            ViewBag.CanViewAllTournamentMatches = CanViewAllTournamentMatches;
            ViewBag.FilterScope = CanViewAllTournamentMatches ? (filterScope ?? "my_matches") : "my_matches";

            var allMatches = selectedGiaiDauId.HasValue
                ? (await _tranDauService.GetAllAsync(giaiDauId: selectedGiaiDauId))?.ToList() ?? new()
                : new List<TranDauDto>();

            bool IsAssigned(TranDauDto m)
            {
                if (currentRef == null) return false;
                return m.DanhSachTrongTai?.Any(assignment => assignment.TrongTaiId == currentRef.Id) == true;
            }

            var assignedMatches = allMatches.Where(IsAssigned).ToList();
            var displayMatches = CanViewAllTournamentMatches && filterScope == "all"
                ? allMatches
                : assignedMatches;

            ViewBag.AllMatchesCount = CanViewAllTournamentMatches ? allMatches.Count : assignedMatches.Count;
            ViewBag.MyAssignedCount = assignedMatches.Count;
            ViewBag.LiveMatchesCount = displayMatches.Count(m => m.TrangThai == "DangDau" || m.TrangThai == "DangDienRa");
            ViewBag.FinishedCount = displayMatches.Count(m => m.TrangThai == "KetThuc" || m.TrangThai == "DaKetThuc");
            ViewBag.UpcomingCount = displayMatches.Count(m => m.TrangThai == "ChuaDau");

            if (!CanViewAllTournamentMatches && assignedMatches.Count == 0 && allMatches.Count > 0)
            {
                ViewBag.NoMyMatchesNotice = true;
            }

            return View(displayMatches);
        }

        [HttpPost]
        public async Task<IActionResult> QuickUpdateScore([FromBody] QuickScoreRequestDto dto)
        {
            if (dto == null || dto.TranDauId <= 0)
            {
                return Json(new { success = false, message = "Dữ liệu cập nhật không hợp lệ." });
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
                // Lưu thông tin tỷ số vào GhiChu dạng JSON có cấu trúc
                var scoreData = new
                {
                    score1 = dto.Score1,
                    score2 = dto.Score2,
                    status = dto.TrangThai,
                    notes = dto.GhiChu,
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
                    ThoiGianKetThuc = dto.TrangThai == "KetThuc" ? DateTime.Now : match.ThoiGianKetThuc,
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
                return Json(new { success = true, message = "Đã cập nhật tỷ số và trạng thái trận đấu thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}
