using Dms.Application.DTOs;
using Dms.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace API.Areas.Manager.Controllers
{
    public class HuyChuongController : BaseManagerController
    {
        private readonly IHuyChuongService _huyChuongService;
        private readonly IGiaiDauService _giaiDauService;
        private readonly IDonViService _donViService;

        public HuyChuongController(
            IHuyChuongService huyChuongService,
            IGiaiDauService giaiDauService,
            IDonViService donViService)
        {
            _huyChuongService = huyChuongService;
            _giaiDauService = giaiDauService;
            _donViService = donViService;
        }

        [HttpGet]
        public async Task<IActionResult> Index([FromQuery] int? giaiDauId = null)
        {
            ViewBag.GiaiDaus = await _giaiDauService.GetAllAsync();
            ViewBag.DonVis = await _donViService.GetAllAsync();
            ViewBag.SelectedGiaiDauId = giaiDauId;
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(int? giaiDauId = null)
        {
            var result = await _huyChuongService.GetAllAsync(giaiDauId);
            return Json(new { success = true, data = result });
        }

        [HttpPost]
        public async Task<IActionResult> Save([FromBody] CreateUpdateHuyChuongDto dto)
        {
            var username = User.FindFirst(ClaimTypes.Name)?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "Manager";

            try
            {
                var created = await _huyChuongService.CreateAsync(dto, username);
                return Json(new { success = true, message = "Trao huy chương thành công!", data = created });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _huyChuongService.DeleteAsync(id);
            if (!success) return Json(new { success = false, message = "Không tìm thấy huy chương để xóa." });
            return Json(new { success = true, message = "Đã thu hồi huy chương thành công!" });
        }
    }
}
