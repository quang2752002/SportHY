using Dms.Application.DTOs;
using Dms.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace API.Areas.Manager.Controllers
{
    public class SanDauController : BaseManagerController
    {
        private readonly ISanDauService _sanDauService;
        private readonly ICumSanService _cumSanService;
        private readonly IMonTheThaoService _monTheThaoService;

        public SanDauController(
            ISanDauService sanDauService,
            ICumSanService cumSanService,
            IMonTheThaoService monTheThaoService)
        {
            _sanDauService = sanDauService;
            _cumSanService = cumSanService;
            _monTheThaoService = monTheThaoService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            ViewBag.CumSans = await _cumSanService.GetAllAsync();
            ViewBag.MonTheThaos = await _monTheThaoService.GetAllAsync();
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetPagedData(
            int pageIndex = 1,
            int pageSize = 10,
            string? keyword = null,
            int? cumSanId = null,
            int? monTheThaoId = null,
            bool? trangThai = null)
        {
            var result = await _sanDauService.GetPagedAsync(pageIndex, pageSize, keyword, cumSanId, monTheThaoId, trangThai);
            return Json(new { success = true, data = result });
        }

        [HttpGet]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _sanDauService.GetByIdAsync(id);
            if (result == null) return Json(new { success = false, message = "Không tìm thấy sân đấu." });
            return Json(new { success = true, data = result });
        }

        [HttpPost]
        public async Task<IActionResult> Save([FromBody] CreateUpdateSanDauDto dto, [FromQuery] int? id = null)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Ten))
            {
                return Json(new { success = false, message = "Tên sân đấu không được để trống." });
            }

            if (dto.CumSanId <= 0)
            {
                return Json(new { success = false, message = "Vui lòng chọn cụm sân thi đấu." });
            }

            var username = User.FindFirst(ClaimTypes.Name)?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "Manager";

            try
            {
                if (id.HasValue && id.Value > 0)
                {
                    var updated = await _sanDauService.UpdateAsync(id.Value, dto, username);
                    if (updated == null) return Json(new { success = false, message = "Không tìm thấy sân đấu để cập nhật." });
                    return Json(new { success = true, message = "Cập nhật sân đấu thành công!", data = updated });
                }
                else
                {
                    var created = await _sanDauService.CreateAsync(dto, username);
                    return Json(new { success = true, message = "Thêm mới sân đấu thành công!", data = created });
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
            var success = await _sanDauService.DeleteAsync(id);
            if (!success)
            {
                return Json(new { success = false, message = "Không tìm thấy hoặc không thể xóa sân đấu này." });
            }

            return Json(new { success = true, message = "Đã xóa sân đấu thành công!" });
        }
    }
}
