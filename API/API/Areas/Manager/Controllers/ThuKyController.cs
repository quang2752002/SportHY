using Dms.Application.DTOs;
using Dms.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace API.Areas.Manager.Controllers
{
    public class ThuKyController : BaseManagerController
    {
        private readonly IThuKyService _thuKyService;

        public ThuKyController(IThuKyService thuKyService)
        {
            _thuKyService = thuKyService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetPagedData(int pageIndex = 1, int pageSize = 10, string? keyword = null, bool? trangThai = null)
        {
            var result = await _thuKyService.GetPagedAsync(pageIndex, pageSize, keyword, trangThai);
            return Json(new { success = true, data = result });
        }

        [HttpGet]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _thuKyService.GetByIdAsync(id);
            if (result == null) return Json(new { success = false, message = "Không tìm thấy hồ sơ thư ký." });
            return Json(new { success = true, data = result });
        }

        [HttpPost]
        public async Task<IActionResult> Save([FromBody] CreateUpdateThuKyDto dto, [FromQuery] int? id = null)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.HoTen))
            {
                return Json(new { success = false, message = "Họ tên thư ký không được để trống." });
            }

            var username = User.FindFirst(ClaimTypes.Name)?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "Manager";

            try
            {
                if (id.HasValue && id.Value > 0)
                {
                    var updated = await _thuKyService.UpdateAsync(id.Value, dto, username);
                    if (updated == null) return Json(new { success = false, message = "Không tìm thấy thư ký để cập nhật." });
                    return Json(new { success = true, message = "Cập nhật hồ sơ thư ký thành công!", data = updated });
                }
                else
                {
                    var created = await _thuKyService.CreateAsync(dto, username);
                    return Json(new { success = true, message = "Thêm mới thư ký thành công!", data = created });
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
            var success = await _thuKyService.DeleteAsync(id);
            if (!success)
            {
                return Json(new { success = false, message = "Không tìm thấy hoặc không thể xóa thư ký này." });
            }

            return Json(new { success = true, message = "Đã xóa thư ký thành công!" });
        }
    }
}
