using Dms.Application.Common;
using Dms.Application.DTOs;
using Dms.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DonViController : ControllerBase
    {
        private readonly IDonViService _donViService;

        public DonViController(IDonViService donViService)
        {
            _donViService = donViService;
        }

        [HttpGet("paged")]
        [Authorize(Policy = Permissions.DonVi.View)]
        public async Task<IActionResult> GetPaged(
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? keyword = null,
            [FromQuery] int? khoiId = null,
            [FromQuery] bool? trangThai = null)
        {
            var result = await _donViService.GetPagedAsync(pageIndex, pageSize, keyword, khoiId, trangThai);
            return Ok(result);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAll([FromQuery] int? khoiId = null)
        {
            var result = await _donViService.GetAllAsync(khoiId);
            return Ok(result);
        }

        [HttpGet("{id}")]
        [Authorize(Policy = Permissions.DonVi.View)]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _donViService.GetByIdAsync(id);
            if (result == null) return NotFound(new { message = "Không tìm thấy đơn vị." });
            return Ok(result);
        }

        [HttpPost]
        [Authorize(Policy = Permissions.DonVi.Create)]
        public async Task<IActionResult> Create([FromBody] CreateUpdateDonViDto dto)
        {
            var username = User.FindFirst(ClaimTypes.Name)?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var result = await _donViService.CreateAsync(dto, username);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [HttpPut("{id}")]
        [Authorize(Policy = Permissions.DonVi.Edit)]
        public async Task<IActionResult> Update(int id, [FromBody] CreateUpdateDonViDto dto)
        {
            var username = User.FindFirst(ClaimTypes.Name)?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var result = await _donViService.UpdateAsync(id, dto, username);
            if (result == null) return NotFound(new { message = "Không tìm thấy đơn vị để cập nhật." });
            return Ok(result);
        }

        [HttpPost("upload-image")]
        [Authorize]
        public async Task<IActionResult> UploadImage([FromForm] Microsoft.AspNetCore.Http.IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { message = "Vui lòng chọn tệp hình ảnh đơn vị." });
            }

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
            var extension = System.IO.Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension))
            {
                return BadRequest(new { message = "Chỉ chấp nhận các định dạng ảnh: .jpg, .jpeg, .png, .webp, .gif" });
            }

            // Tạo thư mục wwwroot/don-vi nếu chưa có
            var webRoot = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "wwwroot");
            var targetFolder = System.IO.Path.Combine(webRoot, "don-vi");
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

            var relativeUrl = $"/don-vi/{fileName}";
            return Ok(new { url = relativeUrl });
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = Permissions.DonVi.Delete)]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _donViService.DeleteAsync(id);
            if (!success) return NotFound(new { message = "Không tìm thấy đơn vị để xóa." });
            return Ok(new { message = "Đã xóa đơn vị thành công." });
        }
    }
}
