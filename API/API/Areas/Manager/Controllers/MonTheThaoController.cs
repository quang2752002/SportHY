using Dms.Application.DTOs;
using Dms.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace API.Areas.Manager.Controllers
{
    public class MonTheThaoController : BaseManagerController
    {
        private readonly IMonTheThaoService _monTheThaoService;
        private readonly IDanhMucMonTheThaoService _danhMucMonService;

        public MonTheThaoController(
            IMonTheThaoService monTheThaoService,
            IDanhMucMonTheThaoService danhMucMonService)
        {
            _monTheThaoService = monTheThaoService;
            _danhMucMonService = danhMucMonService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            ViewBag.DanhMucs = await _danhMucMonService.GetAllAsync();
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetPagedData(
            int pageIndex = 1,
            int pageSize = 10,
            string? keyword = null,
            int? danhMucId = null,
            bool? trangThai = null,
            string? gioiTinh = null,
            string? hinhThucThiDau = null,
            string? loaiThiDau = null)
        {
            var result = await _monTheThaoService.GetPagedAsync(pageIndex, pageSize, keyword, danhMucId, trangThai, gioiTinh, hinhThucThiDau, loaiThiDau);
            return Json(new { success = true, data = result });
        }

        [HttpGet]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _monTheThaoService.GetByIdAsync(id);
            if (result == null) return Json(new { success = false, message = "Không tìm thấy môn thể thao." });
            return Json(new { success = true, data = result });
        }

        [HttpPost]
        public async Task<IActionResult> Save([FromBody] CreateUpdateMonTheThaoDto dto, [FromQuery] int? id = null)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Ten))
            {
                return Json(new { success = false, message = "Tên môn thể thao không được để trống." });
            }

            var username = User.FindFirst(ClaimTypes.Name)?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "Manager";

            try
            {
                if (id.HasValue && id.Value > 0)
                {
                    var updated = await _monTheThaoService.UpdateAsync(id.Value, dto, username);
                    if (updated == null) return Json(new { success = false, message = "Không tìm thấy môn thể thao để cập nhật." });
                    return Json(new { success = true, message = "Cập nhật môn thể thao thành công!", data = updated });
                }
                else
                {
                    var created = await _monTheThaoService.CreateAsync(dto, username);
                    return Json(new { success = true, message = "Thêm mới môn thể thao thành công!", data = created });
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
            var success = await _monTheThaoService.DeleteAsync(id);
            if (!success)
            {
                return Json(new { success = false, message = "Không tìm thấy hoặc không thể xóa môn thể thao này." });
            }

            return Json(new { success = true, message = "Đã xóa môn thể thao thành công!" });
        }
    }
}
