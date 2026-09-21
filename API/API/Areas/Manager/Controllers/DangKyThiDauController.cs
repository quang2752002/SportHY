using Dms.Application.DTOs;
using Dms.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace API.Areas.Manager.Controllers
{
    public class DangKyThiDauController : BaseManagerController
    {
        private readonly IDangKyThiDauService _dangKyService;
        private readonly IGiaiDauService _giaiDauService;
        private readonly IMonTheThaoService _monService;
        private readonly IDonViService _donViService;

        public DangKyThiDauController(
            IDangKyThiDauService dangKyService,
            IGiaiDauService giaiDauService,
            IMonTheThaoService monService,
            IDonViService donViService)
        {
            _dangKyService = dangKyService;
            _giaiDauService = giaiDauService;
            _monService = monService;
            _donViService = donViService;
        }

        [HttpGet]
        public async Task<IActionResult> Index([FromQuery] int? giaiDauId = null)
        {
            ViewBag.GiaiDaus = await _giaiDauService.GetAllAsync();
            ViewBag.MonTheThaos = await _monService.GetAllAsync();
            ViewBag.DonVis = await _donViService.GetAllAsync();
            ViewBag.SelectedGiaiDauId = giaiDauId;
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetPagedData(
            int pageIndex = 1,
            int pageSize = 10,
            string? keyword = null,
            int? giaiDauId = null,
            int? giaiDauMonTheThaoId = null,
            int? donViId = null,
            string? trangThai = null)
        {
            var result = await _dangKyService.GetPagedAsync(pageIndex, pageSize, keyword, giaiDauId, giaiDauMonTheThaoId, donViId, trangThai);
            return Json(new { success = true, data = result });
        }

        [HttpGet]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _dangKyService.GetByIdAsync(id);
            if (result == null) return Json(new { success = false, message = "Không tìm thấy hồ sơ đăng ký." });
            return Json(new { success = true, data = result });
        }

        [HttpPost]
        public async Task<IActionResult> Approve(int id)
        {
            var existing = await _dangKyService.GetByIdAsync(id);
            if (existing == null) return Json(new { success = false, message = "Không tìm thấy hồ sơ đăng ký." });

            var username = User.FindFirst(ClaimTypes.Name)?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "Manager";

            var updateDto = new CreateUpdateDangKyThiDauDto
            {
                GiaiDauMonTheThaoId = existing.GiaiDauMonTheThaoId,
                DoiId = existing.DoiId,
                SoDangKy = existing.SoDangKy,
                TenDangKy = existing.TenDangKy,
                TrangThai = "DaDuyet",
                NgayDangKy = existing.NgayDangKy,
                GhiChu = existing.GhiChu
            };

            try
            {
                var updated = await _dangKyService.UpdateAsync(id, updateDto, username, isPrivileged: true);
                return Json(new { success = true, message = "Đã phê duyệt hồ sơ đăng ký thi đấu thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Reject(int id, string? reason = null)
        {
            var existing = await _dangKyService.GetByIdAsync(id);
            if (existing == null) return Json(new { success = false, message = "Không tìm thấy hồ sơ đăng ký." });

            var username = User.FindFirst(ClaimTypes.Name)?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "Manager";

            var ghiChu = string.IsNullOrWhiteSpace(reason) 
                ? $"Từ chối duyệt bởi {username} lúc {DateTime.Now:dd/MM/yyyy HH:mm}" 
                : $"Từ chối bởi {username}: {reason} ({DateTime.Now:dd/MM/yyyy HH:mm})";

            var updateDto = new CreateUpdateDangKyThiDauDto
            {
                GiaiDauMonTheThaoId = existing.GiaiDauMonTheThaoId,
                DoiId = existing.DoiId,
                SoDangKy = existing.SoDangKy,
                TenDangKy = existing.TenDangKy,
                TrangThai = "TuChoi",
                NgayDangKy = existing.NgayDangKy,
                GhiChu = ghiChu
            };

            try
            {
                var updated = await _dangKyService.UpdateAsync(id, updateDto, username, isPrivileged: true);
                return Json(new { success = true, message = "Đã từ chối hồ sơ đăng ký!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var success = await _dangKyService.DeleteAsync(id, isPrivileged: true);
                if (!success) return Json(new { success = false, message = "Không tìm thấy hoặc không thể xóa hồ sơ này." });
                return Json(new { success = true, message = "Đã xóa hồ sơ đăng ký thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}
