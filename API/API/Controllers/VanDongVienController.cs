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
    public class VanDongVienController : ControllerBase
    {
        private readonly IVanDongVienService _vanDongVienService;

        public VanDongVienController(IVanDongVienService vanDongVienService)
        {
            _vanDongVienService = vanDongVienService;
        }

        [HttpGet("paged")]
        [Authorize(Policy = Permissions.VanDongVien.View)]
        public async Task<IActionResult> GetPaged(
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? keyword = null,
            [FromQuery] int? donViId = null,
            [FromQuery] bool? trangThai = null)
        {
            var result = await _vanDongVienService.GetPagedAsync(pageIndex, pageSize, keyword, donViId, trangThai);
            return Ok(result);
        }

        [HttpGet]
        [Authorize(Policy = Permissions.VanDongVien.View)]
        public async Task<IActionResult> GetAll([FromQuery] int? donViId = null)
        {
            int? effectiveDonViId = donViId;
            if (!effectiveDonViId.HasValue && !User.IsInRole(AppRoles.Admin) && !User.IsInRole(AppRoles.Manager))
            {
                var claim = User.FindFirst("donViId")?.Value;
                if (int.TryParse(claim, out var cid))
                {
                    effectiveDonViId = cid;
                }
            }
            var result = await _vanDongVienService.GetAllAsync(effectiveDonViId);
            return Ok(result);
        }

        /// <summary>
        /// Lấy danh sách vận động viên của đoàn hiện tại (theo tài khoản đăng nhập hoặc donViId)
        /// Tham khảo nghiệp vụ tương tự trang đăng ký thi đấu
        /// </summary>
        [HttpGet("doan")]
        [Authorize]
        public async Task<IActionResult> GetByDoan([FromQuery] int? donViId = null)
        {
            int? effectiveDonViId = donViId;
            if (!effectiveDonViId.HasValue)
            {
                var claim = User.FindFirst("donViId")?.Value;
                if (int.TryParse(claim, out var cid))
                {
                    effectiveDonViId = cid;
                }
            }
            var result = await _vanDongVienService.GetAllAsync(effectiveDonViId);
            return Ok(result);
        }

        [HttpGet("{id}")]
        [Authorize(Policy = Permissions.VanDongVien.View)]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _vanDongVienService.GetByIdAsync(id);
            if (result == null) return NotFound(new { message = "Không tìm thấy vận động viên." });
            return Ok(result);
        }

        [HttpPost]
        [Authorize(Policy = Permissions.VanDongVien.Create)]
        public async Task<IActionResult> Create([FromBody] CreateUpdateVanDongVienDto dto)
        {
            var username = User.FindFirst(ClaimTypes.Name)?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var result = await _vanDongVienService.CreateAsync(dto, username);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [HttpPut("{id}")]
        [Authorize(Policy = Permissions.VanDongVien.Edit)]
        public async Task<IActionResult> Update(int id, [FromBody] CreateUpdateVanDongVienDto dto)
        {
            var username = User.FindFirst(ClaimTypes.Name)?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var result = await _vanDongVienService.UpdateAsync(id, dto, username);
            if (result == null) return NotFound(new { message = "Không tìm thấy vận động viên để cập nhật." });
            return Ok(result);
        }

        [HttpPost("upload-avatar")]
        [Authorize]
        public async Task<IActionResult> UploadAvatar([FromForm] Microsoft.AspNetCore.Http.IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { message = "Vui lòng chọn tệp hình ảnh của vận động viên." });
            }

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
            var extension = System.IO.Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension))
            {
                return BadRequest(new { message = "Chỉ chấp nhận các định dạng ảnh: .jpg, .jpeg, .png, .webp, .gif" });
            }

            // Tạo thư mục wwwroot/vdv nếu chưa có
            var webRoot = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "wwwroot");
            var targetFolder = System.IO.Path.Combine(webRoot, "vdv");
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

            var relativeUrl = $"/vdv/{fileName}";
            return Ok(new { url = relativeUrl });
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            var isAdminOrManager = User.IsInRole(AppRoles.Admin) || User.IsInRole(AppRoles.Manager);
            var isDelegation = User.IsInRole(AppRoles.Delegation);
            var hasPermission = User.Claims.Any(c => c.Type == "permission" && 
                (c.Value == Permissions.VanDongVien.Delete || c.Value == Permissions.DonVi.ManageAthletes));

            if (!isAdminOrManager && !hasPermission && !isDelegation)
            {
                return Forbid();
            }

            // Nếu không phải Admin/Manager, kiểm tra xem VĐV có thuộc đơn vị của user hay không
            if (!isAdminOrManager)
            {
                var claimDonVi = User.FindFirst("donViId")?.Value;
                if (int.TryParse(claimDonVi, out var userDonViId))
                {
                    var vdv = await _vanDongVienService.GetByIdAsync(id);
                    if (vdv != null && vdv.DonViId.HasValue && vdv.DonViId.Value != userDonViId)
                    {
                        return BadRequest(new { message = "Bạn không có quyền xóa vận động viên của đơn vị khác." });
                    }
                }
            }

            var success = await _vanDongVienService.DeleteAsync(id);
            if (!success) return NotFound(new { message = "Không tìm thấy vận động viên để xóa hoặc VĐV đã bị xóa." });
            return Ok(new { message = "Đã xóa vận động viên thành công." });
        }
    }
}
