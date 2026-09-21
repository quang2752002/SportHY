using Dms.Application.DTOs;
using Dms.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace API.Areas.Admin.Controllers
{
    public class TrongTaiController : BaseAdminController
    {
        private readonly ITrongTaiService _trongTaiService;

        public TrongTaiController(ITrongTaiService trongTaiService)
        {
            _trongTaiService = trongTaiService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetPagedData(
            int pageIndex = 1,
            int pageSize = 10,
            string? keyword = null,
            bool? trangThai = null)
        {
            var result = await _trongTaiService.GetPagedAsync(pageIndex, pageSize, keyword, trangThai);
            return Json(new { success = true, data = result });
        }

        [HttpGet]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _trongTaiService.GetByIdAsync(id);
            if (result == null) return Json(new { success = false, message = "Không tìm thấy thông tin trọng tài." });
            return Json(new { success = true, data = result });
        }

        [HttpPost]
        public async Task<IActionResult> Save([FromBody] CreateUpdateTrongTaiDto dto, [FromQuery] int? id = null)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.HoTen))
            {
                return Json(new { success = false, message = "Họ tên trọng tài không được để trống." });
            }

            var username = User.FindFirst(ClaimTypes.Name)?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "Admin";

            try
            {
                if (id.HasValue && id.Value > 0)
                {
                    var updated = await _trongTaiService.UpdateAsync(id.Value, dto, username);
                    if (updated == null) return Json(new { success = false, message = "Không tìm thấy trọng tài để cập nhật." });
                    return Json(new { success = true, message = "Cập nhật thông tin trọng tài thành công!", data = updated });
                }
                else
                {
                    var created = await _trongTaiService.CreateAsync(dto, username);
                    return Json(new { success = true, message = "Thêm mới trọng tài thành công!", data = created });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _trongTaiService.DeleteAsync(id);
            if (!success)
            {
                return Json(new { success = false, message = "Không tìm thấy hoặc không thể xóa trọng tài này." });
            }

            return Json(new { success = true, message = "Đã xóa trọng tài thành công!" });
        }
    }
}
