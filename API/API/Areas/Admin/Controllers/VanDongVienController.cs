using Dms.Application.DTOs;
using Dms.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace API.Areas.Admin.Controllers
{
    public class VanDongVienController : BaseAdminController
    {
        private readonly IVanDongVienService _vanDongVienService;
        private readonly IDonViService _donViService;

        public VanDongVienController(
            IVanDongVienService vanDongVienService,
            IDonViService donViService)
        {
            _vanDongVienService = vanDongVienService;
            _donViService = donViService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            ViewBag.DonVis = await _donViService.GetAllAsync();
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetPagedData(
            int pageIndex = 1,
            int pageSize = 10,
            string? keyword = null,
            int? donViId = null,
            bool? trangThai = null)
        {
            var result = await _vanDongVienService.GetPagedAsync(pageIndex, pageSize, keyword, donViId, trangThai);
            return Json(new { success = true, data = result });
        }

        [HttpGet]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _vanDongVienService.GetByIdAsync(id);
            if (result == null) return Json(new { success = false, message = "Không tìm thấy vận động viên." });
            return Json(new { success = true, data = result });
        }

        [HttpPost]
        public async Task<IActionResult> Save([FromBody] CreateUpdateVanDongVienDto dto, [FromQuery] int? id = null)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.HoTen))
            {
                return Json(new { success = false, message = "Họ tên vận động viên không được để trống." });
            }

            var username = User.FindFirst(ClaimTypes.Name)?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "Admin";

            try
            {
                if (id.HasValue && id.Value > 0)
                {
                    var updated = await _vanDongVienService.UpdateAsync(id.Value, dto, username);
                    if (updated == null) return Json(new { success = false, message = "Không tìm thấy vận động viên để cập nhật." });
                    return Json(new { success = true, message = "Cập nhật hồ sơ VĐV thành công!", data = updated });
                }
                else
                {
                    var created = await _vanDongVienService.CreateAsync(dto, username);
                    return Json(new { success = true, message = "Thêm mới hồ sơ VĐV thành công!", data = created });
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
            var success = await _vanDongVienService.DeleteAsync(id);
            if (!success)
            {
                return Json(new { success = false, message = "Không tìm thấy hoặc không thể xóa vận động viên này." });
            }

            return Json(new { success = true, message = "Đã xóa vận động viên thành công!" });
        }
    }
}
