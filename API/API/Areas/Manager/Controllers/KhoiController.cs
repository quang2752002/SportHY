using Dms.Application.DTOs;
using Dms.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace API.Areas.Manager.Controllers
{
    public class KhoiController : BaseManagerController
    {
        private readonly IKhoiService _khoiService;

        public KhoiController(IKhoiService khoiService)
        {
            _khoiService = khoiService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetPagedData(int pageIndex = 1, int pageSize = 10, string? keyword = null, bool? trangThai = null)
        {
            var result = await _khoiService.GetPagedAsync(pageIndex, pageSize, keyword, trangThai);
            return Json(new { success = true, data = result });
        }

        [HttpGet]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _khoiService.GetByIdAsync(id);
            if (result == null) return Json(new { success = false, message = "Không tìm thấy khối tham gia." });
            return Json(new { success = true, data = result });
        }

        [HttpPost]
        public async Task<IActionResult> Save([FromBody] CreateUpdateKhoiDto dto, [FromQuery] int? id = null)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Ten))
            {
                return Json(new { success = false, message = "Tên khối không được để trống." });
            }

            var username = User.FindFirst(ClaimTypes.Name)?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "Manager";

            try
            {
                if (id.HasValue && id.Value > 0)
                {
                    var updated = await _khoiService.UpdateAsync(id.Value, dto, username);
                    if (updated == null) return Json(new { success = false, message = "Không tìm thấy khối để cập nhật." });
                    return Json(new { success = true, message = "Cập nhật khối thành công!", data = updated });
                }
                else
                {
                    var created = await _khoiService.CreateAsync(dto, username);
                    return Json(new { success = true, message = "Thêm mới khối thành công!", data = created });
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
            var success = await _khoiService.DeleteAsync(id);
            if (!success)
            {
                return Json(new { success = false, message = "Không tìm thấy hoặc không thể xóa khối này." });
            }

            return Json(new { success = true, message = "Đã xóa khối thành công!" });
        }
    }
}
