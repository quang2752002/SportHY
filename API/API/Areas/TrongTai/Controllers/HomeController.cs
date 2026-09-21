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
            IMonTheThaoService monTheThaoService)
            : base(userManager, trongTaiService)
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
            var tournaments = (await _giaiDauService.GetAllAsync())?.ToList() ?? new();
            ViewBag.Tournaments = tournaments;

            int? selectedGiaiDauId = giaiDauId;
            if (!selectedGiaiDauId.HasValue && tournaments.Count > 0)
            {
                selectedGiaiDauId = tournaments[0].Id;
            }
            ViewBag.SelectedGiaiDauId = selectedGiaiDauId;
            ViewBag.FilterScope = filterScope ?? "my_matches";

            var allMatches = (await _tranDauService.GetAllAsync(giaiDauId: selectedGiaiDauId))?.ToList() ?? new();

            bool IsAssigned(TranDauDto m)
            {
                if (currentRef == null) return false;
                if (m.DanhSachTrongTai != null && m.DanhSachTrongTai.Any(tt =>
                    tt.TrongTaiId == currentRef.Id ||
                    (!string.IsNullOrEmpty(tt.TenTrongTai) && tt.TenTrongTai.Contains(currentRef.HoTen, StringComparison.OrdinalIgnoreCase))))
                {
                    return true;
                }
                return false;
            }

            ViewBag.AllMatchesCount = allMatches.Count;
            ViewBag.MyAssignedCount = allMatches.Count(IsAssigned);
            ViewBag.LiveMatchesCount = allMatches.Count(m => m.TrangThai == "DangDau" || m.TrangThai == "DangDienRa");
            ViewBag.FinishedCount = allMatches.Count(m => m.TrangThai == "KetThuc" || m.TrangThai == "DaKetThuc");
            ViewBag.UpcomingCount = allMatches.Count(m => m.TrangThai == "ChuaDau");

            var displayMatches = allMatches;
            if (filterScope == "my_matches")
            {
                displayMatches = allMatches.Where(IsAssigned).ToList();
                // Nếu chưa có phân công nào cụ thể, cho phép hiển thị tất cả để trọng tài dễ quan sát
                if (displayMatches.Count == 0 && allMatches.Count > 0)
                {
                    ViewBag.NoMyMatchesNotice = true;
                }
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
