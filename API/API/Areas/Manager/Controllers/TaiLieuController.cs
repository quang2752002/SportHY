using Dms.Application.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Security.Claims;
using System.Threading.Tasks;

namespace API.Areas.Manager.Controllers
{
    public class TaiLieuController : BaseManagerController
    {
        private readonly ITaiLieuCongKhaiService _taiLieuService;
        private readonly IWebHostEnvironment _environment;

        public TaiLieuController(ITaiLieuCongKhaiService taiLieuService, IWebHostEnvironment environment)
        {
            _taiLieuService = taiLieuService;
            _environment = environment;
        }

        [HttpGet]
        public IActionResult Index() => View();

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var documents = await _taiLieuService.GetAllForManagerAsync();
            return Json(new { success = true, data = documents });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(31 * 1024 * 1024)]
        public async Task<IActionResult> Upload(
            [FromForm] string tieuDe,
            [FromForm] string loaiTaiLieu,
            [FromForm] string? moTa,
            [FromForm] IFormFile? tep)
        {
            if (tep == null || tep.Length == 0)
                return Json(new { success = false, message = "Vui lòng chọn tệp tài liệu cần tải lên." });

            var storageDirectory = GetDocumentStorageDirectory();
            var username = User.FindFirst(ClaimTypes.Name)?.Value
                ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? "Manager";

            try
            {
                await using var content = tep.OpenReadStream();
                var document = await _taiLieuService.UploadAsync(
                    tieuDe, loaiTaiLieu, moTa, content, tep.FileName, tep.Length, storageDirectory, username);
                return Json(new { success = true, message = "Đã lưu tài liệu.", data = document });
            }
            catch (ArgumentException ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetPublic(int id, bool congKhai)
        {
            var username = User.FindFirst(ClaimTypes.Name)?.Value
                ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? "Manager";
            var updated = await _taiLieuService.SetPublicAsync(id, congKhai, username);
            return Json(new
            {
                success = updated,
                message = updated ? (congKhai ? "Đã công khai tài liệu." : "Đã ẩn tài liệu khỏi trang chủ.") : "Không tìm thấy tài liệu."
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var username = User.FindFirst(ClaimTypes.Name)?.Value
                ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? "Manager";
            var deleted = await _taiLieuService.SoftDeleteAsync(id, username);
            return Json(new
            {
                success = deleted,
                message = deleted ? "Đã chuyển tài liệu vào thùng lưu trữ." : "Không tìm thấy tài liệu."
            });
        }

        [HttpGet]
        public async Task<IActionResult> OpenFile(int id, bool download = false)
        {
            var storageDirectory = GetDocumentStorageDirectory();
            var legacyStorageDirectory = GetLegacyDocumentStorageDirectory();
            var file = await _taiLieuService.GetFileForManagerAsync(id, storageDirectory, legacyStorageDirectory);
            if (file == null) return NotFound();

            if (!download)
                Response.Headers["Content-Disposition"] = $"inline; filename*=UTF-8''{Uri.EscapeDataString(file.TenTaiXuong)}";
            var stream = new FileStream(file.DuongDanTuyetDoi, FileMode.Open, FileAccess.Read, FileShare.Read);
            return download
                ? File(stream, file.LoaiNoiDung, file.TenTaiXuong)
                : File(stream, file.LoaiNoiDung);
        }

        /// <summary>Mở trang xem trước DOCX trong màn quản lý mà không tải tệp về máy.</summary>
        /// <param name="id">Mã tài liệu cần xem.</param>
        /// <returns>Trang xem trước tài liệu; trả về 404 nếu tài liệu không tồn tại hoặc đã xóa mềm.</returns>
        [HttpGet]
        public async Task<IActionResult> PreviewFile(int id)
        {
            var storageDirectory = GetDocumentStorageDirectory();
            var legacyStorageDirectory = GetLegacyDocumentStorageDirectory();
            var html = await _taiLieuService.GetDocxPreviewHtmlAsync(id, storageDirectory, requirePublic: false, legacyStorageDirectory: legacyStorageDirectory);
            return html == null ? NotFound() : Content(html, "text/html; charset=utf-8");
        }

        private string GetDocumentStorageDirectory()
        {
            var webRoot = _environment.WebRootPath ?? Path.Combine(_environment.ContentRootPath, "wwwroot");
            return Path.Combine(webRoot, "TaiLieuCongKhai");
        }

        private string GetLegacyDocumentStorageDirectory() => Path.Combine(_environment.ContentRootPath, "App_Data", "TaiLieuCongKhai");
    }
}
