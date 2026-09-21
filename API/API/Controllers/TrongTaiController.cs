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
    public class TrongTaiController : ControllerBase
    {
        private readonly ITrongTaiService _trongTaiService;

        public TrongTaiController(ITrongTaiService trongTaiService)
        {
            _trongTaiService = trongTaiService;
        }

        [HttpGet("paged")]
        [Authorize(Policy = Permissions.TrongTai.View)]
        public async Task<IActionResult> GetPaged(
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? keyword = null,
            [FromQuery] bool? trangThai = null)
        {
            var result = await _trongTaiService.GetPagedAsync(pageIndex, pageSize, keyword, trangThai);
            return Ok(result);
        }

        [HttpGet]
        [Authorize(Policy = Permissions.TrongTai.View)]
        public async Task<IActionResult> GetAll()
        {
            var result = await _trongTaiService.GetAllAsync();
            return Ok(result);
        }

        [HttpGet("{id}")]
        [Authorize(Policy = Permissions.TrongTai.View)]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _trongTaiService.GetByIdAsync(id);
            if (result == null) return NotFound(new { message = "Không tìm thấy trọng tài." });
            return Ok(result);
        }

        [HttpPost]
        [Authorize(Policy = Permissions.TrongTai.Create)]
        public async Task<IActionResult> Create([FromBody] CreateUpdateTrongTaiDto dto)
        {
            var username = User.FindFirst(ClaimTypes.Name)?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var result = await _trongTaiService.CreateAsync(dto, username);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [HttpPut("{id}")]
        [Authorize(Policy = Permissions.TrongTai.Edit)]
        public async Task<IActionResult> Update(int id, [FromBody] CreateUpdateTrongTaiDto dto)
        {
            var username = User.FindFirst(ClaimTypes.Name)?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var result = await _trongTaiService.UpdateAsync(id, dto, username);
            if (result == null) return NotFound(new { message = "Không tìm thấy trọng tài để cập nhật." });
            return Ok(result);
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = Permissions.TrongTai.Delete)]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _trongTaiService.DeleteAsync(id);
            if (!success) return NotFound(new { message = "Không tìm thấy trọng tài để xóa." });
            return Ok(new { message = "Đã xóa trọng tài thành công." });
        }
    }
}
