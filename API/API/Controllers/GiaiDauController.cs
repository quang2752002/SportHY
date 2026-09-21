using Dms.Application.Common;
using Dms.Application.DTOs;
using Dms.Application.Interfaces;
using Dms.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GiaiDauController : ControllerBase
    {
        private readonly IGiaiDauService _giaiDauService;

        public GiaiDauController(IGiaiDauService giaiDauService)
        {
            _giaiDauService = giaiDauService;
        }

        [HttpGet("paged")]
        [AllowAnonymous]
        public async Task<IActionResult> GetPaged(
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? keyword = null,
            [FromQuery] TrangThaiGiaiDau? trangThai = null,
            [FromQuery] PhamViGiaiDau? phamVi = null)
        {
            var result = await _giaiDauService.GetPagedAsync(pageIndex, pageSize, keyword, trangThai, phamVi);
            return Ok(result);
        }

        [HttpGet]
        [Authorize(Policy = Permissions.GiaiDau.View)]
        public async Task<IActionResult> GetAll()
        {
            var result = await _giaiDauService.GetAllAsync();
            return Ok(result);
        }

        [HttpGet("{id:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _giaiDauService.GetByIdAsync(id);
            if (result == null) return NotFound(new { message = "Không tìm thấy giải đấu." });
            return Ok(result);
        }

        [HttpGet("slug/{slug}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetBySlug(string slug)
        {
            var result = await _giaiDauService.GetBySlugAsync(slug);
            if (result == null) return NotFound(new { message = "Không tìm thấy giải đấu với đường dẫn này." });
            return Ok(result);
        }

        [HttpPost]
        [Authorize(Policy = Permissions.GiaiDau.Create)]
        public async Task<IActionResult> Create([FromBody] CreateUpdateGiaiDauDto dto)
        {
            var username = User.FindFirst(ClaimTypes.Name)?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var result = await _giaiDauService.CreateAsync(dto, username);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [HttpPut("{id}")]
        [Authorize(Policy = Permissions.GiaiDau.Edit)]
        public async Task<IActionResult> Update(int id, [FromBody] CreateUpdateGiaiDauDto dto)
        {
            try
            {
                var username = User.FindFirst(ClaimTypes.Name)?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var result = await _giaiDauService.UpdateAsync(id, dto, username);
                if (result == null) return NotFound(new { message = "Không tìm thấy giải đấu để cập nhật." });
                return Ok(result);
            }
            catch (System.InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("upload-banner")]
        [Authorize]
        public async Task<IActionResult> UploadBanner([FromForm] Microsoft.AspNetCore.Http.IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { message = "Vui lòng chọn tệp hình ảnh banner." });
            }

            // Kiểm tra định dạng ảnh
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
            var extension = System.IO.Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension))
            {
                return BadRequest(new { message = "Chỉ chấp nhận các định dạng ảnh: .jpg, .jpeg, .png, .webp, .gif" });
            }

            // Tạo thư mục wwwroot/banner nếu chưa có
            var webRoot = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "wwwroot");
            var bannerFolder = System.IO.Path.Combine(webRoot, "banner");
            if (!System.IO.Directory.Exists(bannerFolder))
            {
                System.IO.Directory.CreateDirectory(bannerFolder);
            }

            // Tạo tên file duy nhất tránh trùng lặp
            var fileName = $"{System.Guid.NewGuid():N}{extension}";
            var fullPath = System.IO.Path.Combine(bannerFolder, fileName);

            using (var stream = new System.IO.FileStream(fullPath, System.IO.FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Trả về đường dẫn lưu vào DB
            var relativeUrl = $"/banner/{fileName}";
            return Ok(new { url = relativeUrl });
        }

        [HttpPost("upload-dieule")]
        [Authorize]
        public async Task<IActionResult> UploadDieuLeFile([FromForm] Microsoft.AspNetCore.Http.IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { message = "Vui lòng chọn tệp tài liệu điều lệ." });
            }

            // Kiểm tra định dạng tệp (PDF, Word, Excel, hình ảnh)
            var allowedExtensions = new[] { ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".jpg", ".jpeg", ".png", ".webp" };
            var extension = System.IO.Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension))
            {
                return BadRequest(new { message = "Chỉ chấp nhận các tệp tài liệu: .pdf, .doc, .docx, .xls, .xlsx, hoặc file ảnh." });
            }

            // Giới hạn kích thước tối đa 50MB
            if (file.Length > 50 * 1024 * 1024)
            {
                return BadRequest(new { message = "Kích thước tệp không được vượt quá 50MB." });
            }

            // Tạo thư mục wwwroot/dieu-le nếu chưa có
            var webRoot = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "wwwroot");
            var targetFolder = System.IO.Path.Combine(webRoot, "dieu-le");
            if (!System.IO.Directory.Exists(targetFolder))
            {
                System.IO.Directory.CreateDirectory(targetFolder);
            }

            var fileName = $"{System.Guid.NewGuid():N}{extension}";
            var fullPath = System.IO.Path.Combine(targetFolder, fileName);

            using (var stream = new System.IO.FileStream(fullPath, System.IO.FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var relativeUrl = $"/dieu-le/{fileName}";
            return Ok(new { url = relativeUrl, fileName = file.FileName });
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = Permissions.GiaiDau.Delete)]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _giaiDauService.DeleteAsync(id);
            if (!success) return NotFound(new { message = "Không tìm thấy giải đấu để xóa." });
            return Ok(new { message = "Đã xóa giải đấu thành công." });
        }
    }
}
