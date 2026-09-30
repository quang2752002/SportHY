using Dms.Application.Common;
using Dms.Application.DTOs;
using Dms.Application.Interfaces;
using Dms.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
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

        /// <summary>Nhận file Excel từ màn hình đơn vị và ủy quyền dịch vụ nhập danh sách vận động viên.</summary>
        /// <param name="file">File Excel .xlsx người dùng tải lên.</param>
        /// <returns>Kết quả nhập dữ liệu dạng JSON, gồm các dòng thành công và các dòng lỗi.</returns>
        [HttpPost]
        [RequestSizeLimit(10 * 1024 * 1024)]
        public async Task<IActionResult> ImportExcel(IFormFile? file)
        {
            if (file == null || file.Length == 0)
                return Json(new { success = false, message = "Vui lòng chọn file Excel cần nhập." });

            if (!string.Equals(Path.GetExtension(file.FileName), ".xlsx", StringComparison.OrdinalIgnoreCase))
                return Json(new { success = false, message = "Chỉ hỗ trợ file Excel định dạng .xlsx." });

            var currentUnit = await GetCurrentDonViAsync();
            if (currentUnit == null)
                return Json(new { success = false, message = "Không xác định được đơn vị thi đấu." });

            try
            {
                await using var stream = file.OpenReadStream();
                var preview = await _vanDongVienService.PreviewFromExcelAsync(stream, file.FileName, currentUnit.Id);
                return Json(new
                {
                    success = true,
                    message = $"Đã đọc {preview.Rows.Count} dòng. Vui lòng kiểm tra và bấm Lưu danh sách để ghi dữ liệu.",
                    data = preview
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>Nhận danh sách tạm đã được người dùng chỉnh sửa và ủy quyền dịch vụ kiểm tra để lưu.</summary>
        /// <param name="rows">Các dòng vận động viên người dùng xác nhận lưu.</param>
        /// <returns>Kết quả lưu dạng JSON; nếu còn lỗi thì không ghi dữ liệu.</returns>
        [HttpPost]
        [RequestSizeLimit(10 * 1024 * 1024)]
        public async Task<IActionResult> SaveImported([FromBody] List<VanDongVienImportRowDto>? rows)
        {
            if (rows == null || rows.Count == 0)
                return Json(new { success = false, message = "Danh sách tạm chưa có vận động viên nào." });

            var currentUnit = await GetCurrentDonViAsync();
            if (currentUnit == null)
                return Json(new { success = false, message = "Không xác định được đơn vị thi đấu." });

            var username = User.FindFirst(ClaimTypes.Name)?.Value ?? "DonVi";
            try
            {
                var result = await _vanDongVienService.SaveImportedAsync(rows, currentUnit.Id, username);
                var success = result.Errors.Count == 0 && result.ImportedCount > 0;
                var message = success
                    ? $"Đã lưu thành công {result.ImportedCount} vận động viên."
                    : "Danh sách còn dữ liệu chưa hợp lệ, chưa có hồ sơ nào được lưu.";
                return Json(new { success, message, data = result });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>Tạo và trả về file Excel mẫu cho chức năng nhập danh sách vận động viên.</summary>
        /// <returns>File Excel mẫu .xlsx để người dùng tải xuống.</returns>
        [HttpGet]
        public async Task<IActionResult> DownloadImportTemplate()
        {
            var content = await _vanDongVienService.GenerateImportTemplateAsync();
            return File(
                content,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "Mau_Nhap_Danh_Sach_Van_Dong_Vien.xlsx");
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
