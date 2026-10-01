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
    public class CumSanController : ControllerBase
    {
        private readonly ICumSanService _cumSanService;

        public CumSanController(ICumSanService cumSanService)
        {
            _cumSanService = cumSanService;
        }

        [HttpGet("paged")]
        [Authorize(Policy = Permissions.SanDau.View)]
        public async Task<IActionResult> GetPaged(
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? keyword = null,
            [FromQuery] bool? trangThai = null,
            [FromQuery] int? donViId = null)
        {
            var result = await _cumSanService.GetPagedAsync(pageIndex, pageSize, keyword, trangThai, donViId);
            return Ok(result);
        }

        [HttpGet]
        [Authorize(Policy = Permissions.SanDau.View)]
        public async Task<IActionResult> GetAll([FromQuery] int? donViId = null)
        {
            var result = await _cumSanService.GetAllAsync(donViId);
            return Ok(result);
        }

        [HttpGet("{id}")]
        [Authorize(Policy = Permissions.SanDau.View)]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _cumSanService.GetByIdAsync(id);
            if (result == null) return NotFound(new { message = "Không tìm thấy cụm sân." });
            return Ok(result);
        }

        [HttpPost]
        [Authorize(Roles = AppRoles.Admin + "," + AppRoles.Delegation)]
        public async Task<IActionResult> Create([FromBody] CreateUpdateCumSanDto dto)
        {
            if (dto == null) return BadRequest(new { message = "Dữ liệu cụm sân không hợp lệ." });

            var username = User.FindFirst(ClaimTypes.Name)?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            try
            {
                CumSanDto result;
                if (User.IsInRole(AppRoles.Delegation))
                {
                    var donViId = GetCurrentDonViId();
                    if (!donViId.HasValue) return Forbid();
                    result = await _cumSanService.CreateForDonViAsync(dto, donViId.Value, username);
                }
                else
                {
                    result = await _cumSanService.CreateAsync(dto, username);
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
        public async Task<IActionResult> Update(int id, [FromBody] CreateUpdateCumSanDto dto)
        {
            if (dto == null) return BadRequest(new { message = "Dữ liệu cụm sân không hợp lệ." });

            var username = User.FindFirst(ClaimTypes.Name)?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            CumSanDto? result;
            if (User.IsInRole(AppRoles.Delegation))
            {
                var donViId = GetCurrentDonViId();
                if (!donViId.HasValue) return Forbid();
                result = await _cumSanService.UpdateForDonViAsync(id, dto, donViId.Value, username);
            }
            else
            {
                result = await _cumSanService.UpdateAsync(id, dto, username);
            }

            if (result == null) return NotFound(new { message = "Không tìm thấy cụm sân trong phạm vi được phép." });
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
                success = await _cumSanService.DeleteForDonViAsync(id, donViId.Value);
            }
            else
            {
                success = await _cumSanService.DeleteAsync(id);
            }

            if (!success) return NotFound(new { message = "Không tìm thấy cụm sân trong phạm vi được phép." });
            return Ok(new { message = "Đã xóa mềm cụm sân thành công." });
        }

        private int? GetCurrentDonViId()
        {
            var value = User.FindFirst("donViId")?.Value;
            return int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var donViId) ? donViId : null;
        }
    }
}
