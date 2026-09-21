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
    public class KhoiController : ControllerBase
    {
        private readonly IKhoiService _khoiService;

        public KhoiController(IKhoiService khoiService)
        {
            _khoiService = khoiService;
        }

        [HttpGet("paged")]
        [Authorize(Policy = Permissions.Khoi.View)]
        public async Task<IActionResult> GetPaged(
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? keyword = null,
            [FromQuery] bool? trangThai = null)
        {
            var result = await _khoiService.GetPagedAsync(pageIndex, pageSize, keyword, trangThai);
            return Ok(result);
        }

        [HttpGet]
        [Authorize(Policy = Permissions.Khoi.View)]
        public async Task<IActionResult> GetAll()
        {
            var result = await _khoiService.GetAllAsync();
            return Ok(result);
        }

        [HttpGet("{id}")]
        [Authorize(Policy = Permissions.Khoi.View)]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _khoiService.GetByIdAsync(id);
            if (result == null) return NotFound(new { message = "Không tìm thấy khối." });
            return Ok(result);
        }

        [HttpPost]
        [Authorize(Policy = Permissions.Khoi.Create)]
        public async Task<IActionResult> Create([FromBody] CreateUpdateKhoiDto dto)
        {
            var username = User.FindFirst(ClaimTypes.Name)?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var result = await _khoiService.CreateAsync(dto, username);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [HttpPut("{id}")]
        [Authorize(Policy = Permissions.Khoi.Edit)]
        public async Task<IActionResult> Update(int id, [FromBody] CreateUpdateKhoiDto dto)
        {
            var username = User.FindFirst(ClaimTypes.Name)?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var result = await _khoiService.UpdateAsync(id, dto, username);
            if (result == null) return NotFound(new { message = "Không tìm thấy khối để cập nhật." });
            return Ok(result);
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = Permissions.Khoi.Delete)]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _khoiService.DeleteAsync(id);
            if (!success) return NotFound(new { message = "Không tìm thấy khối để xóa." });
            return Ok(new { message = "Đã xóa khối thành công." });
        }
    }
}
