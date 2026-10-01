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
    public class SanDauController : BaseDonViController
    {
        private readonly ISanDauService _sanDauService;
        private readonly ICumSanService _cumSanService;
        private readonly IMonTheThaoService _monTheThaoService;

        public SanDauController(
            UserManager<ApplicationUser> userManager,
            IDonViService donViService,
            ISanDauService sanDauService,
            ICumSanService cumSanService,
            IMonTheThaoService monTheThaoService)
            : base(userManager, donViService)
        {
            _sanDauService = sanDauService;
            _cumSanService = cumSanService;
            _monTheThaoService = monTheThaoService;
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
            int? cumSanId = null,
            int? monTheThaoId = null,
            bool? trangThai = null)
        {
            var currentUnit = await GetCurrentDonViAsync();
            if (currentUnit == null)
            {
                return Json(new { success = false, message = "Chưa xác định được đơn vị hiện tại." });
            }

            var result = await _sanDauService.GetPagedAsync(pageIndex, pageSize, keyword, cumSanId, monTheThaoId, trangThai, currentUnit.Id);
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

            var result = await _sanDauService.GetByIdAsync(id, currentUnit.Id);
            if (result == null) return Json(new { success = false, message = "Không tìm thấy sân đấu thuộc đơn vị hiện tại." });
            return Json(new { success = true, data = result });
        }

        [HttpGet]
        public async Task<IActionResult> GetLookupData()
        {
            var currentUnit = await GetCurrentDonViAsync();
            if (currentUnit == null)
            {
                return Json(new { success = false, message = "Chưa xác định được đơn vị hiện tại." });
            }

            var cumSans = await _cumSanService.GetAllAsync(currentUnit.Id);
            var monTheThaos = await _monTheThaoService.GetAllAsync();
            return Json(new { success = true, data = new { cumSans, monTheThaos } });
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

            var currentUnit = await GetCurrentDonViAsync();
            if (currentUnit == null)
            {
                return Json(new { success = false, message = "Chưa xác định được đơn vị hiện tại." });
            }

            if (string.IsNullOrWhiteSpace(dto.Ma))
            {
                dto.Ma = "SAN-" + DateTime.Now.ToString("yyMMddHHmmss");
            }

            var username = User.FindFirst(ClaimTypes.Name)?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "DonVi";

            try
            {
                if (id.HasValue && id.Value > 0)
                {
                    var updated = await _sanDauService.UpdateForDonViAsync(id.Value, dto, currentUnit.Id, username);
                    if (updated == null) return Json(new { success = false, message = "Không tìm thấy sân đấu thuộc đơn vị hiện tại." });
                    return Json(new { success = true, message = "Cập nhật sân đấu thành công!", data = updated });
                }

                var created = await _sanDauService.CreateForDonViAsync(dto, currentUnit.Id, username);
                return Json(new { success = true, message = "Thêm mới sân đấu thành công!", data = created });
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

            var success = await _sanDauService.DeleteForDonViAsync(id, currentUnit.Id);
            if (!success)
            {
                return Json(new { success = false, message = "Không tìm thấy sân đấu thuộc đơn vị hiện tại." });
            }

            return Json(new { success = true, message = "Đã xóa mềm sân đấu thành công!" });
        }
    }
}
