using Dms.Application.Common;
using Dms.Application.DTOs;
using Dms.Application.Interfaces;
using Dms.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace API.Areas.DonVi.Controllers
{
    public class VanDongVienController : BaseDonViController
    {
        private readonly IVanDongVienService _vanDongVienService;

        public VanDongVienController(
            UserManager<ApplicationUser> userManager,
            IDonViService donViService,
            IVanDongVienService vanDongVienService)
            : base(userManager, donViService)
        {
            _vanDongVienService = vanDongVienService;
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
            string? gioiTinh = null,
            bool? trangThai = null)
        {
            var currentUnit = await GetCurrentDonViAsync();
            int? donViId = currentUnit?.Id;

            var result = await _vanDongVienService.GetPagedAsync(pageIndex, pageSize, keyword, donViId, trangThai);

            // Filter giới tính if provided and not ALL
            if (!string.IsNullOrEmpty(gioiTinh) && !gioiTinh.Equals("ALL", StringComparison.OrdinalIgnoreCase))
            {
                result.Items = result.Items.Where(i => string.Equals(i.GioiTinh, gioiTinh, StringComparison.OrdinalIgnoreCase)).ToList();
                result.TotalCount = result.Items.Count();
            }

            return Json(new { success = true, data = result });
        }

        [HttpGet]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _vanDongVienService.GetByIdAsync(id);
            if (result == null)
            {
                return Json(new { success = false, message = "Không tìm thấy vận động viên." });
            }

            return Json(new { success = true, data = result });
        }

        [HttpPost]
        public async Task<IActionResult> Save([FromBody] CreateUpdateDonViVanDongVienDto dto, [FromQuery] int? id = null)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.HoTen))
            {
                return Json(new { success = false, message = "Vui lòng nhập thông tin" });
            }

            if (!ModelState.IsValid)
            {
                var validationMessage = ModelState.Values
                    .SelectMany(entry => entry.Errors)
                    .Select(error => error.ErrorMessage)
                    .FirstOrDefault(message => !string.IsNullOrWhiteSpace(message));
                return Json(new { success = false, message = validationMessage ?? "Thông tin vận động viên chưa hợp lệ." });
            }

            var currentUnit = await GetCurrentDonViAsync();
            if (currentUnit == null)
            {
                return Json(new { success = false, message = "Không xác định được đơn vị thi đấu." });
            }

            var username = User.FindFirst(ClaimTypes.Name)?.Value ?? "DonVi";

            try
            {
                if (id.HasValue && id.Value > 0)
                {
                    var existing = await _vanDongVienService.GetByIdAsync(id.Value);
                    if (existing == null)
                    {
                        return Json(new { success = false, message = "Vận động viên cần cập nhật không tồn tại." });
                    }

                    var updateDto = dto.ToServiceDto(currentUnit.Id, existing.Ma);
                    var updated = await _vanDongVienService.UpdateAsync(id.Value, updateDto, username);
                    return Json(new { success = true, message = "Cập nhật hồ sơ vận động viên thành công!", data = updated });
                }
                else
                {
                    var createDto = dto.ToServiceDto(currentUnit.Id, GenerateAthleteCode(currentUnit.Ma));
                    var created = await _vanDongVienService.CreateAsync(createDto, username);
                    return Json(new { success = true, message = "Thêm mới vận động viên vào đoàn thành công!", data = created });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        private static string GenerateAthleteCode(string unitCode)
        {
            var normalizedUnitCode = string.IsNullOrWhiteSpace(unitCode) ? "DV" : unitCode.Trim();
            const int maxUnitCodeLength = 13;
            if (normalizedUnitCode.Length > maxUnitCodeLength)
            {
                normalizedUnitCode = normalizedUnitCode.Substring(0, maxUnitCodeLength);
            }

            return $"VDV-{normalizedUnitCode}-{Guid.NewGuid():N}";
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var vdv = await _vanDongVienService.GetByIdAsync(id);
            if (vdv == null)
            {
                return Json(new { success = false, message = "Vận động viên không tồn tại." });
            }

            var currentUnit = await GetCurrentDonViAsync();
            if (currentUnit != null && vdv.DonViId.HasValue && vdv.DonViId.Value != currentUnit.Id)
            {
                if (!User.IsInRole(AppRoles.Admin) && !User.IsInRole(AppRoles.Manager))
                {
                    return Json(new { success = false, message = "Bạn không có quyền xóa VĐV của đơn vị khác." });
                }
            }

            var success = await _vanDongVienService.DeleteAsync(id);
            if (!success)
            {
                return Json(new { success = false, message = "Không thể xóa VĐV do đang có dữ liệu thi đấu hoặc liên kết liên quan." });
            }

            return Json(new { success = true, message = "Đã xóa vận động viên khỏi danh sách của đoàn thành công." });
        }
    }
}
