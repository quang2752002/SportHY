using Dms.Application.DTOs;
using Dms.Application.Interfaces;
using Dms.Domain.Enums;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace API.Areas.Manager.Controllers
{
    public class GiaiDauController : BaseManagerController
    {
        private readonly IGiaiDauService _giaiDauService;
        private readonly IMonTheThaoService _monTheThaoService;
        private readonly IKhoiService _khoiService;
        private readonly IWebHostEnvironment _env;

        public GiaiDauController(
            IGiaiDauService giaiDauService,
            IMonTheThaoService monTheThaoService,
            IKhoiService khoiService,
            IWebHostEnvironment env)
        {
            _giaiDauService = giaiDauService;
            _monTheThaoService = monTheThaoService;
            _khoiService = khoiService;
            _env = env;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewBag.Khois = await _khoiService.GetAllAsync();
            ViewBag.MonTheThaos = await _monTheThaoService.GetAllAsync();
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var item = await _giaiDauService.GetByIdAsync(id);
            if (item == null)
            {
                return NotFound("Không tìm thấy giải đấu cần chỉnh sửa.");
            }

            ViewBag.Khois = await _khoiService.GetAllAsync();
            ViewBag.MonTheThaos = await _monTheThaoService.GetAllAsync();
            return View(item);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var item = await _giaiDauService.GetByIdAsync(id);
            if (item == null) return NotFound("Không tìm thấy giải đấu.");
            return View(item);
        }

        [HttpGet]
        public async Task<IActionResult> GetPagedData(
            int pageIndex = 1,
            int pageSize = 10,
            string? keyword = null,
            TrangThaiGiaiDau? trangThai = null)
        {
            var result = await _giaiDauService.GetPagedAsync(pageIndex, pageSize, keyword, trangThai, null);
            return Json(new { success = true, data = result });
        }

        [HttpGet]
        public async Task<IActionResult> GetById(int id)
        {
            var item = await _giaiDauService.GetByIdAsync(id);
            if (item == null) return Json(new { success = false, message = "Không tìm thấy giải đấu." });
            return Json(new { success = true, data = item });
        }

        [HttpPost]
        public async Task<IActionResult> Save([FromBody] CreateUpdateGiaiDauDto dto, [FromQuery] int? id = null)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Ten))
            {
                return Json(new { success = false, message = "Tên giải đấu không được để trống." });
            }

            var username = User.FindFirst(ClaimTypes.Name)?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "Manager";

            try
            {
                if (id.HasValue && id.Value > 0)
                {
                    var updated = await _giaiDauService.UpdateAsync(id.Value, dto, username);
                    if (updated == null) return Json(new { success = false, message = "Không tìm thấy giải đấu để cập nhật." });
                    return Json(new { success = true, message = "Cập nhật giải đấu thành công!", data = updated });
                }
                else
                {
                    var created = await _giaiDauService.CreateAsync(dto, username);
                    return Json(new { success = true, message = "Thêm mới giải đấu thành công!", data = created });
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
            var success = await _giaiDauService.DeleteAsync(id);
            if (!success)
            {
                return Json(new { success = false, message = "Không tìm thấy hoặc không thể xóa giải đấu này." });
            }

            return Json(new { success = true, message = "Đã xóa giải đấu thành công!" });
        }

        [HttpPost]
        public async Task<IActionResult> UploadBanner([FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return Json(new { success = false, message = "Vui lòng chọn tệp hình ảnh banner." });
            }

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension))
            {
                return Json(new { success = false, message = "Chỉ chấp nhận các định dạng ảnh: .jpg, .jpeg, .png, .webp, .gif" });
            }

            var webRoot = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var bannerFolder = Path.Combine(webRoot, "banner");
            if (!Directory.Exists(bannerFolder))
            {
                Directory.CreateDirectory(bannerFolder);
            }

            var fileName = $"{Guid.NewGuid():N}{extension}";
            var fullPath = Path.Combine(bannerFolder, fileName);

            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var relativeUrl = $"/banner/{fileName}";
            return Json(new { success = true, url = relativeUrl });
        }

        [HttpPost]
        public async Task<IActionResult> UploadDieuLe([FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return Json(new { success = false, message = "Vui lòng chọn tệp tài liệu điều lệ." });
            }

            var allowedExtensions = new[] { ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".jpg", ".jpeg", ".png", ".webp" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension))
            {
                return Json(new { success = false, message = "Chỉ chấp nhận các định dạng: .pdf, .doc, .docx, .xls, .xlsx, ảnh." });
            }

            var webRoot = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var targetFolder = Path.Combine(webRoot, "dieu-le");
            if (!Directory.Exists(targetFolder))
            {
                Directory.CreateDirectory(targetFolder);
            }

            var fileName = $"{Guid.NewGuid():N}{extension}";
            var fullPath = Path.Combine(targetFolder, fileName);

            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var relativeUrl = $"/dieu-le/{fileName}";
            return Json(new { success = true, url = relativeUrl, fileName = file.FileName });
        }
    }
}
