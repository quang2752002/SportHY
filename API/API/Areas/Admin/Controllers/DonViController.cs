using Dms.Application.DTOs;
using Dms.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace API.Areas.Admin.Controllers
{
    public class DonViController : BaseAdminController
    {
        private readonly IDonViService _donViService;
        private readonly IKhoiService _khoiService;

        public DonViController(
            IDonViService donViService,
            IKhoiService khoiService)
        {
            _donViService = donViService;
            _khoiService = khoiService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            ViewBag.Khois = await _khoiService.GetAllAsync();
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetPagedData(
            int pageIndex = 1,
            int pageSize = 10,
            string? keyword = null,
            int? khoiId = null,
            bool? trangThai = null)
        {
            var result = await _donViService.GetPagedAsync(pageIndex, pageSize, keyword, khoiId, trangThai);
            return Json(new { success = true, data = result });
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(int? khoiId = null)
        {
            var result = await _donViService.GetAllAsync(khoiId);
            return Json(new { success = true, data = result });
        }

        [HttpGet]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _donViService.GetByIdAsync(id);
            if (result == null) return Json(new { success = false, message = "Không tìm thấy đơn vị / đoàn thể thao." });
            return Json(new { success = true, data = result });
        }

        [HttpPost]
        public async Task<IActionResult> Save([FromBody] CreateUpdateDonViDto dto, [FromQuery] int? id = null)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Ten))
            {
                return Json(new { success = false, message = "Tên đơn vị không được để trống." });
            }

            var username = User.FindFirst(ClaimTypes.Name)?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "Admin";

            try
            {
                if (id.HasValue && id.Value > 0)
                {
                    var updated = await _donViService.UpdateAsync(id.Value, dto, username);
                    if (updated == null) return Json(new { success = false, message = "Không tìm thấy đơn vị để cập nhật." });
                    return Json(new { success = true, message = "Cập nhật đơn vị thành công!", data = updated });
                }
                else
                {
                    var created = await _donViService.CreateAsync(dto, username);
                    return Json(new { success = true, message = "Thêm mới đơn vị thành công!", data = created });
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
            var success = await _donViService.DeleteAsync(id);
            if (!success)
            {
                return Json(new { success = false, message = "Không tìm thấy hoặc không thể xóa đơn vị này." });
            }

            return Json(new { success = true, message = "Đã xóa đơn vị thành công!" });
        }
    }
}
