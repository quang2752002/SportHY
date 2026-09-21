using Dms.Application.DTOs;
using Dms.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace API.Areas.Admin.Controllers
{
    public class DanhMucMonTheThaoController : BaseAdminController
    {
        private readonly IDanhMucMonTheThaoService _danhMucService;

        public DanhMucMonTheThaoController(IDanhMucMonTheThaoService danhMucService)
        {
            _danhMucService = danhMucService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetPagedData(int pageIndex = 1, int pageSize = 10, string? keyword = null, bool? trangThai = null)
        {
            var result = await _danhMucService.GetPagedAsync(pageIndex, pageSize, keyword, trangThai);
            return Json(new { success = true, data = result });
        }

        [HttpGet]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _danhMucService.GetByIdAsync(id);
            if (result == null) return Json(new { success = false, message = "Không tìm thấy danh mục môn thể thao." });
            return Json(new { success = true, data = result });
        }

        [HttpPost]
        public async Task<IActionResult> Save([FromBody] CreateUpdateDanhMucMonTheThaoDto dto, [FromQuery] int? id = null)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Ten))
            {
                return Json(new { success = false, message = "Tên danh mục không được để trống." });
            }

            var username = User.FindFirst(ClaimTypes.Name)?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "Admin";

            try
            {
                if (id.HasValue && id.Value > 0)
                {
                    var updated = await _danhMucService.UpdateAsync(id.Value, dto, username);
                    if (updated == null) return Json(new { success = false, message = "Không tìm thấy danh mục để cập nhật." });
                    return Json(new { success = true, message = "Cập nhật danh mục môn thành công!", data = updated });
                }
                else
                {
                    var created = await _danhMucService.CreateAsync(dto, username);
                    return Json(new { success = true, message = "Thêm mới danh mục môn thành công!", data = created });
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
            var success = await _danhMucService.DeleteAsync(id);
            if (!success)
            {
                return Json(new { success = false, message = "Không tìm thấy hoặc không thể xóa danh mục môn này." });
            }

            return Json(new { success = true, message = "Đã xóa danh mục môn thành công!" });
        }
    }
}
