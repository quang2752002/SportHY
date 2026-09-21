using Dms.Application.DTOs;
using Dms.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace API.Areas.Manager.Controllers
{
    public class BangDauController : BaseManagerController
    {
        private readonly IBangDauService _bangDauService;
        private readonly IGiaiDauService _giaiDauService;
        private readonly IMonTheThaoService _monTheThaoService;

        public BangDauController(
            IBangDauService bangDauService,
            IGiaiDauService giaiDauService,
            IMonTheThaoService monTheThaoService)
        {
            _bangDauService = bangDauService;
            _giaiDauService = giaiDauService;
            _monTheThaoService = monTheThaoService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            ViewBag.GiaiDaus = await _giaiDauService.GetAllAsync();
            ViewBag.MonTheThaos = await _monTheThaoService.GetAllAsync();
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(int? giaiDauMonTheThaoId = null)
        {
            var result = await _bangDauService.GetAllAsync(giaiDauMonTheThaoId);
            return Json(new { success = true, data = result });
        }

        [HttpGet]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _bangDauService.GetByIdAsync(id);
            if (result == null) return Json(new { success = false, message = "Không tìm thấy bảng đấu." });
            return Json(new { success = true, data = result });
        }

        [HttpPost]
        public async Task<IActionResult> Save([FromBody] CreateUpdateBangDauDto dto, [FromQuery] int? id = null)
        {
            var username = User.FindFirst(ClaimTypes.Name)?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "Manager";

            try
            {
                if (id.HasValue && id.Value > 0)
                {
                    var updated = await _bangDauService.UpdateAsync(id.Value, dto, username);
                    if (updated == null) return Json(new { success = false, message = "Không tìm thấy bảng đấu để cập nhật." });
                    return Json(new { success = true, message = "Cập nhật bảng đấu thành công!", data = updated });
                }
                else
                {
                    var created = await _bangDauService.CreateAsync(dto, username);
                    return Json(new { success = true, message = "Tạo bảng đấu mới thành công!", data = created });
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
            var success = await _bangDauService.DeleteAsync(id);
            if (!success) return Json(new { success = false, message = "Không tìm thấy bảng đấu để xóa." });
            return Json(new { success = true, message = "Đã xóa bảng đấu thành công!" });
        }
    }
}
