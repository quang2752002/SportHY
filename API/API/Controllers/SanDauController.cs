using Dms.Application.Common;
using Dms.Application.DTOs;
using Dms.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Globalization;
using System.Security.Claims;
using System.Threading.Tasks;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SanDauController : ControllerBase
    {
        private readonly ISanDauService _sanDauService;

        public SanDauController(ISanDauService sanDauService)
        {
            _sanDauService = sanDauService;
        }

        [HttpGet("paged")]
        [Authorize(Policy = Permissions.SanDau.View)]
        public async Task<IActionResult> GetPaged(
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? keyword = null,
            [FromQuery] int? cumSanId = null,
            [FromQuery] int? monTheThaoId = null,
            [FromQuery] bool? trangThai = null,
            [FromQuery] int? donViId = null)
        {
            var result = await _sanDauService.GetPagedAsync(pageIndex, pageSize, keyword, cumSanId, monTheThaoId, trangThai, donViId);
            return Ok(result);
        }

        [HttpGet]
        [Authorize(Policy = Permissions.SanDau.View)]
        public async Task<IActionResult> GetAll(
            [FromQuery] int? cumSanId = null,
            [FromQuery] int? monTheThaoId = null,
            [FromQuery] int? donViId = null)
        {
            var result = await _sanDauService.GetAllAsync(cumSanId, monTheThaoId, donViId);
            return Ok(result);
        }

        [HttpGet("{id}")]
        [Authorize(Policy = Permissions.SanDau.View)]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _sanDauService.GetByIdAsync(id);
            if (result == null) return NotFound(new { message = "Không tìm thấy sân đấu." });
            return Ok(result);
        }

        [HttpPost]
        [Authorize(Roles = AppRoles.Admin + "," + AppRoles.Delegation)]
        public async Task<IActionResult> Create([FromBody] CreateUpdateSanDauDto dto)
        {
            if (dto == null) return BadRequest(new { message = "Dữ liệu sân đấu không hợp lệ." });

            var username = User.FindFirst(ClaimTypes.Name)?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            try
            {
                SanDauDto result;
                if (User.IsInRole(AppRoles.Delegation))
                {
                    var donViId = GetCurrentDonViId();
                    if (!donViId.HasValue) return Forbid();
                    result = await _sanDauService.CreateForDonViAsync(dto, donViId.Value, username);
                }
                else
                {
                    result = await _sanDauService.CreateAsync(dto, username);
                }

                return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = AppRoles.Admin + "," + AppRoles.Delegation)]
        public async Task<IActionResult> Update(int id, [FromBody] CreateUpdateSanDauDto dto)
        {
            if (dto == null) return BadRequest(new { message = "Dữ liệu sân đấu không hợp lệ." });

            var username = User.FindFirst(ClaimTypes.Name)?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            SanDauDto? result;
            if (User.IsInRole(AppRoles.Delegation))
            {
                var donViId = GetCurrentDonViId();
                if (!donViId.HasValue) return Forbid();
                result = await _sanDauService.UpdateForDonViAsync(id, dto, donViId.Value, username);
            }
            else
            {
                result = await _sanDauService.UpdateAsync(id, dto, username);
            }

            if (result == null) return NotFound(new { message = "Không tìm thấy sân đấu trong phạm vi được phép." });
            return Ok(result);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = AppRoles.Admin + "," + AppRoles.Delegation)]
        public async Task<IActionResult> Delete(int id)
        {
            bool success;
            if (User.IsInRole(AppRoles.Delegation))
            {
                var donViId = GetCurrentDonViId();
                if (!donViId.HasValue) return Forbid();
                success = await _sanDauService.DeleteForDonViAsync(id, donViId.Value);
            }
            else
            {
                success = await _sanDauService.DeleteAsync(id);
            }

            if (!success) return NotFound(new { message = "Không tìm thấy sân đấu trong phạm vi được phép." });
            return Ok(new { message = "Đã xóa mềm sân đấu thành công." });
        }

        private int? GetCurrentDonViId()
        {
            var value = User.FindFirst("donViId")?.Value;
            return int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var donViId) ? donViId : null;
        }
    }
}
