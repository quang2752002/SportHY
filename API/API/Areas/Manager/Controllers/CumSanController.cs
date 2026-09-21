using Dms.Application.DTOs;
using Dms.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace API.Areas.Manager.Controllers
{
    public class CumSanController : BaseManagerController
    {
        private readonly ICumSanService _cumSanService;

        public CumSanController(ICumSanService cumSanService)
        {
            _cumSanService = cumSanService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetPagedData(int pageIndex = 1, int pageSize = 10, string? keyword = null, bool? trangThai = null)
        {
            var result = await _cumSanService.GetPagedAsync(pageIndex, pageSize, keyword, trangThai);
            return Json(new { success = true, data = result });
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _cumSanService.GetAllAsync();
            return Json(new { success = true, data = result });
        }

        [HttpGet]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _cumSanService.GetByIdAsync(id);
            if (result == null) return Json(new { success = false, message = "Không tìm thấy cụm sân." });
            return Json(new { success = true, data = result });
        }

        [HttpPost]
        public async Task<IActionResult> Save([FromBody] CreateUpdateCumSanDto dto, [FromQuery] int? id = null)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Ten))
            {
                return Json(new { success = false, message = "Tên cụm sân không được để trống." });
            }

            if (string.IsNullOrWhiteSpace(dto.Ma))
            {
                dto.Ma = "CS-" + DateTime.Now.ToString("yyMMddHHmmss");
            }

            var username = User.FindFirst(ClaimTypes.Name)?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "Manager";

            try
            {
                if (id.HasValue && id.Value > 0)
                {
                    var updated = await _cumSanService.UpdateAsync(id.Value, dto, username);
                    if (updated == null) return Json(new { success = false, message = "Không tìm thấy cụm sân để cập nhật." });
                    return Json(new { success = true, message = "Cập nhật cụm sân thành công!", data = updated });
                }
                else
                {
                    var created = await _cumSanService.CreateAsync(dto, username);
                    return Json(new { success = true, message = "Thêm mới cụm sân thành công!", data = created });
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
            var success = await _cumSanService.DeleteAsync(id);
            if (!success)
            {
                return Json(new { success = false, message = "Không tìm thấy hoặc không thể xóa cụm sân này." });
            }

            return Json(new { success = true, message = "Đã xóa cụm sân thành công!" });
        }
    }
}
