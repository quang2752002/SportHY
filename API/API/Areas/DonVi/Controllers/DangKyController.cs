using Dms.Application.Common;
using Dms.Application.DTOs;
using Dms.Application.Interfaces;
using Dms.Domain.Entities;
using Dms.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace API.Areas.DonVi.Controllers
{
    public class DangKyController : BaseDonViController
    {
        private readonly IGiaiDauService _giaiDauService;
        private readonly IDangKyThiDauService _dangKyThiDauService;
        private readonly IVanDongVienService _vanDongVienService;
        private readonly IMonTheThaoService _monTheThaoService;
        private readonly IDanhMucMonTheThaoService _danhMucMonTheThaoService;

        public DangKyController(
            UserManager<ApplicationUser> userManager,
            IDonViService donViService,
            IGiaiDauService giaiDauService,
            IDangKyThiDauService dangKyThiDauService,
            IVanDongVienService vanDongVienService,
            IMonTheThaoService monTheThaoService,
            IDanhMucMonTheThaoService danhMucMonTheThaoService)
            : base(userManager, donViService)
        {
            _giaiDauService = giaiDauService;
            _dangKyThiDauService = dangKyThiDauService;
            _vanDongVienService = vanDongVienService;
            _monTheThaoService = monTheThaoService;
            _danhMucMonTheThaoService = danhMucMonTheThaoService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var currentUnit = await GetCurrentDonViAsync();
            var tournaments = (await _giaiDauService.GetAllAsync())?.ToList() ?? new();

            // Lấy toàn bộ đăng ký của đơn vị này để đếm số lượng hồ sơ nộp vào từng giải
            var unitRegs = currentUnit != null
                ? (await _dangKyThiDauService.GetAllAsync(donViId: currentUnit.Id))?.ToList() ?? new()
                : new();

            ViewBag.UnitRegistrations = unitRegs;
            return View(tournaments);
        }

        [HttpGet]
        public async Task<IActionResult> ChiTiet(int id)
        {
            var currentUnit = await GetCurrentDonViAsync();
            var giaiDau = await _giaiDauService.GetByIdAsync(id);
            if (giaiDau == null)
            {
                return NotFound("Không tìm thấy giải đấu.");
            }

            int? donViId = currentUnit?.Id;
            var athletes = (await _vanDongVienService.GetAllAsync(donViId))?.ToList() ?? new();
            var categories = (await _danhMucMonTheThaoService.GetAllAsync())?.ToList() ?? new();
            var allSports = (await _monTheThaoService.GetAllAsync())?.ToList() ?? new();

            ViewBag.Athletes = athletes;
            ViewBag.Categories = categories;
            ViewBag.AllSports = allSports;

            return View(giaiDau);
        }

        [HttpGet]
        public async Task<IActionResult> GetRegistrations(int giaiDauId)
        {
            var currentUnit = await GetCurrentDonViAsync();
            int? donViId = currentUnit?.Id;

            var regs = await _dangKyThiDauService.GetAllAsync(giaiDauId: giaiDauId, donViId: donViId);
            return Json(new { success = true, data = regs });
        }

        [HttpPost]
        public async Task<IActionResult> SubmitRegistration([FromBody] CreateUpdateDangKyThiDauDto dto)
        {
            if (dto == null || dto.GiaiDauMonTheThaoId <= 0)
            {
                return Json(new { success = false, message = "Vui lòng chọn môn thi đấu." });
            }

            if (dto.VanDongVienIds == null || dto.VanDongVienIds.Count == 0)
            {
                return Json(new { success = false, message = "Vui lòng chọn ít nhất một vận động viên." });
            }

            var currentUnit = await GetCurrentDonViAsync();
            if (currentUnit == null)
            {
                return Json(new { success = false, message = "Chưa xác định được đơn vị quản lý." });
            }

            dto.DonViId = currentUnit.Id;
            dto.TuDongTaoDoi = true;
            dto.TrangThai = "DaDuyet"; // Mặc định tự động duyệt theo yêu cầu
            dto.NgayDangKy = DateTime.Now;

            if (string.IsNullOrWhiteSpace(dto.SoDangKy))
            {
                dto.SoDangKy = $"DK-{DateTime.Now:yyyyMMddHHmmss}";
            }

            var username = User.FindFirst(ClaimTypes.Name)?.Value ?? "DonVi";

            try
            {
                var created = await _dangKyThiDauService.CreateAsync(dto, username, isPrivileged: true);
                return Json(new { success = true, message = "Đã nộp hồ sơ đăng ký thi đấu thành công! Hồ sơ đã được duyệt.", data = created });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteRegistration(int id)
        {
            var existing = await _dangKyThiDauService.GetByIdAsync(id);
            if (existing == null)
            {
                return Json(new { success = false, message = "Hồ sơ đăng ký không tồn tại." });
            }

            var currentUnit = await GetCurrentDonViAsync();
            if (currentUnit != null && existing.DonViId.HasValue && existing.DonViId.Value != currentUnit.Id)
            {
                if (!User.IsInRole(AppRoles.Admin) && !User.IsInRole(AppRoles.Manager))
                {
                    return Json(new { success = false, message = "Bạn không có quyền hủy hồ sơ của đơn vị khác." });
                }
            }

            var success = await _dangKyThiDauService.DeleteAsync(id, isPrivileged: true);
            if (!success)
            {
                return Json(new { success = false, message = "Không thể hủy hồ sơ do đang có lịch thi đấu hoặc bảng đấu liên quan." });
            }

            return Json(new { success = true, message = "Đã hủy hồ sơ đăng ký thành công." });
        }
    }
}
