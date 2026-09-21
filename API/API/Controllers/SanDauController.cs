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
            [FromQuery] bool? trangThai = null)
        {
            var result = await _sanDauService.GetPagedAsync(pageIndex, pageSize, keyword, cumSanId, monTheThaoId, trangThai);
            return Ok(result);
        }

        [HttpGet]
        [Authorize(Policy = Permissions.SanDau.View)]
        public async Task<IActionResult> GetAll(
            [FromQuery] int? cumSanId = null,
            [FromQuery] int? monTheThaoId = null)
        {
            var result = await _sanDauService.GetAllAsync(cumSanId, monTheThaoId);
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
        [Authorize(Policy = Permissions.SanDau.Create)]
        public async Task<IActionResult> Create([FromBody] CreateUpdateSanDauDto dto)
        {
            var username = User.FindFirst(ClaimTypes.Name)?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var result = await _sanDauService.CreateAsync(dto, username);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [HttpPut("{id}")]
        [Authorize(Policy = Permissions.SanDau.Edit)]
        public async Task<IActionResult> Update(int id, [FromBody] CreateUpdateSanDauDto dto)
        {
            var username = User.FindFirst(ClaimTypes.Name)?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var result = await _sanDauService.UpdateAsync(id, dto, username);
            if (result == null) return NotFound(new { message = "Không tìm thấy sân đấu để cập nhật." });
            return Ok(result);
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = Permissions.SanDau.Delete)]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _sanDauService.DeleteAsync(id);
            if (!success) return NotFound(new { message = "Không tìm thấy sân đấu để xóa." });
            return Ok(new { message = "Đã xóa sân đấu thành công." });
        }
    }
}
