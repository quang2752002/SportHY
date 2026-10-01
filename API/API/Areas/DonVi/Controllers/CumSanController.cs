using Dms.Application.DTOs;
using Dms.Application.Interfaces;
using Dms.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace API.Areas.DonVi.Controllers
{
    public class CumSanController : BaseDonViController
    {
        private readonly ICumSanService _cumSanService;
        private readonly ISanDauService _sanDauService;

        public CumSanController(
            UserManager<ApplicationUser> userManager,
            IDonViService donViService,
            ICumSanService cumSanService,
            ISanDauService sanDauService)
            : base(userManager, donViService)
        {
            _cumSanService = cumSanService;
            _sanDauService = sanDauService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            await GetCurrentDonViAsync();
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetPagedData(
            int pageIndex = 1,
            int pageSize = 10,
            string? keyword = null,
            bool? trangThai = null)
        {
            var currentUnit = await GetCurrentDonViAsync();
            if (currentUnit == null)
            {
                return Json(new { success = false, message = "Chưa xác định được đơn vị hiện tại." });
            }

            var result = await _cumSanService.GetPagedAsync(pageIndex, pageSize, keyword, trangThai, currentUnit.Id);
            return Json(new { success = true, data = result });
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var currentUnit = await GetCurrentDonViAsync();
            if (currentUnit == null)
            {
                return Json(new { success = false, message = "Chưa xác định được đơn vị hiện tại." });
            }

            var result = await _cumSanService.GetAllAsync(currentUnit.Id);
            return Json(new { success = true, data = result });
        }

        [HttpGet]
        public async Task<IActionResult> GetById(int id)
        {
            var currentUnit = await GetCurrentDonViAsync();
            if (currentUnit == null)
            {
                return Json(new { success = false, message = "Chưa xác định được đơn vị hiện tại." });
            }

            var result = await _cumSanService.GetByIdAsync(id, currentUnit.Id);
            if (result == null) return Json(new { success = false, message = "Không tìm thấy cụm sân thuộc đơn vị hiện tại." });
            return Json(new { success = true, data = result });
        }

        [HttpGet]
        public async Task<IActionResult> GetCourts(int cumSanId)
        {
            var currentUnit = await GetCurrentDonViAsync();
            if (currentUnit == null)
            {
                return Json(new { success = false, message = "Chưa xác định được đơn vị hiện tại." });
            }

            var result = await _sanDauService.GetPagedAsync(1, 1000, null, cumSanId, null, null, currentUnit.Id);
            return Json(new { success = true, data = result });
        }

        [HttpPost]
        public async Task<IActionResult> Save([FromBody] CreateUpdateCumSanDto dto, [FromQuery] int? id = null)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Ten))
            {
                return Json(new { success = false, message = "Tên cụm sân không được để trống." });
            }

            var currentUnit = await GetCurrentDonViAsync();
            if (currentUnit == null)
            {
                return Json(new { success = false, message = "Chưa xác định được đơn vị hiện tại." });
            }

            if (string.IsNullOrWhiteSpace(dto.Ma))
            {
                dto.Ma = "CS-" + DateTime.Now.ToString("yyMMddHHmmss");
            }

            var username = User.FindFirst(ClaimTypes.Name)?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "DonVi";

            try
            {
                if (id.HasValue && id.Value > 0)
                {
                    var updated = await _cumSanService.UpdateForDonViAsync(id.Value, dto, currentUnit.Id, username);
                    if (updated == null) return Json(new { success = false, message = "Không tìm thấy cụm sân thuộc đơn vị hiện tại." });
                    return Json(new { success = true, message = "Cập nhật cụm sân thành công!", data = updated });
                }

                var created = await _cumSanService.CreateForDonViAsync(dto, currentUnit.Id, username);
                return Json(new { success = true, message = "Thêm mới cụm sân thành công!", data = created });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var currentUnit = await GetCurrentDonViAsync();
            if (currentUnit == null)
            {
                return Json(new { success = false, message = "Chưa xác định được đơn vị hiện tại." });
            }

            var success = await _cumSanService.DeleteForDonViAsync(id, currentUnit.Id);
            if (!success)
            {
                return Json(new { success = false, message = "Không tìm thấy cụm sân thuộc đơn vị hiện tại." });
            }

            return Json(new { success = true, message = "Đã xóa mềm cụm sân thành công!" });
        }
    }
}
