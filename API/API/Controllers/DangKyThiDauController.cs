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
    public class DangKyThiDauController : ControllerBase
    {
        private readonly IDangKyThiDauService _dangKyThiDauService;

        public DangKyThiDauController(IDangKyThiDauService dangKyThiDauService)
        {
            _dangKyThiDauService = dangKyThiDauService;
        }

        [HttpGet("paged")]
        [Authorize(Policy = Permissions.DangKyThiDau.View)]
        public async Task<IActionResult> GetPaged(
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? keyword = null,
            [FromQuery] int? giaiDauId = null,
            [FromQuery] int? giaiDauMonTheThaoId = null,
            [FromQuery] int? donViId = null,
            [FromQuery] string? trangThai = null)
        {
            var result = await _dangKyThiDauService.GetPagedAsync(pageIndex, pageSize, keyword, giaiDauId, giaiDauMonTheThaoId, donViId, trangThai);
            return Ok(result);
        }

        [HttpGet]
        [Authorize(Policy = Permissions.DangKyThiDau.View)]
        public async Task<IActionResult> GetAll(
            [FromQuery] int? giaiDauId = null,
            [FromQuery] int? giaiDauMonTheThaoId = null,
            [FromQuery] int? donViId = null)
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
            var result = await _dangKyThiDauService.GetAllAsync(giaiDauId, giaiDauMonTheThaoId, effectiveDonViId);
            return Ok(result);
        }

        [HttpGet("{id}")]
        [Authorize(Policy = Permissions.DangKyThiDau.View)]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _dangKyThiDauService.GetByIdAsync(id);
            if (result == null) return NotFound(new { message = "Không tìm thấy hồ sơ đăng ký thi đấu." });
            return Ok(result);
        }

        [HttpPost]
        [Authorize(Policy = Permissions.DangKyThiDau.Create)]
        public async Task<IActionResult> Create([FromBody] CreateUpdateDangKyThiDauDto dto)
        {
            try
            {
                var username = User.FindFirst(ClaimTypes.Name)?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                bool isPrivileged = User.IsInRole(AppRoles.Admin) || User.IsInRole(AppRoles.Manager);
                var result = await _dangKyThiDauService.CreateAsync(dto, username, isPrivileged);
                return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
            }
            catch (System.InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Policy = Permissions.DangKyThiDau.Edit)]
        public async Task<IActionResult> Update(int id, [FromBody] CreateUpdateDangKyThiDauDto dto)
        {
            try
            {
                var username = User.FindFirst(ClaimTypes.Name)?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                bool isPrivileged = User.IsInRole(AppRoles.Admin) || User.IsInRole(AppRoles.Manager);
                var result = await _dangKyThiDauService.UpdateAsync(id, dto, username, isPrivileged);
                if (result == null) return NotFound(new { message = "Không tìm thấy hồ sơ để cập nhật." });
                return Ok(result);
            }
            catch (System.InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = Permissions.DangKyThiDau.Delete)]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                bool isPrivileged = User.IsInRole(AppRoles.Admin) || User.IsInRole(AppRoles.Manager);
                if (!isPrivileged)
                {
                    var existing = await _dangKyThiDauService.GetByIdAsync(id);
                    if (existing == null) return NotFound(new { message = "Không tìm thấy hồ sơ để xóa." });
                    var claim = User.FindFirst("donViId")?.Value;
                    if (int.TryParse(claim, out var cid))
                    {
                        if (existing.DonViId.HasValue && existing.DonViId != cid)
                        {
                            return Forbid();
                        }
                    }
                }

                var success = await _dangKyThiDauService.DeleteAsync(id, isPrivileged);
                if (!success) return NotFound(new { message = "Không tìm thấy hồ sơ để xóa." });
                return Ok(new { message = "Xóa hồ sơ đăng ký thành công." });
            }
            catch (System.InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
