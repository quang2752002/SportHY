using Dms.Application.DTOs;
using Dms.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace API.Areas.Admin.Controllers
{
    /// <summary>
    /// Controller quản lý cấu hình hệ thống dành cho Admin
    /// </summary>
    public class SystemSettingController : BaseAdminController
    {
        private readonly ISystemSettingService _systemSettingService;

        public SystemSettingController(ISystemSettingService systemSettingService)
        {
            _systemSettingService = systemSettingService;
        }

        /// <summary>
        /// Màn hình quản lý cấu hình hệ thống & chân trang Footer
        /// </summary>
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Lấy thông tin cấu hình Footer hiện tại
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetFooterSettings()
        {
            var data = await _systemSettingService.GetFooterSettingsAsync();
            return Json(new { success = true, data });
        }

        /// <summary>
        /// Lưu hoặc cập nhật thông tin cấu hình Footer
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> SaveFooterSettings([FromBody] UpdateFooterSettingRequest request)
        {
            if (request == null)
            {
                return Json(new { success = false, message = "Dữ liệu cấu hình không hợp lệ." });
            }

            var username = User.FindFirst(ClaimTypes.Name)?.Value 
                        ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                        ?? "Admin";

            var success = await _systemSettingService.SaveFooterSettingsAsync(request, username);
            if (!success)
            {
                return Json(new { success = false, message = "Không thể lưu cấu hình hệ thống." });
            }

            return Json(new { success = true, message = "Lưu cấu hình hệ thống thành công!" });
        }

        /// <summary>
        /// Lấy toàn bộ từ điển cấu hình hệ thống
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAllSettings()
        {
            var data = await _systemSettingService.GetAllSettingsAsync();
            return Json(new { success = true, data });
        }
    }
}
