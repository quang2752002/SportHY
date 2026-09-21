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
    public class ThuKyController : ControllerBase
    {
        private readonly IThuKyService _thuKyService;

        public ThuKyController(IThuKyService thuKyService)
        {
            _thuKyService = thuKyService;
        }

        [HttpGet("paged")]
        [Authorize(Policy = Permissions.ThuKy.View)]
        public async Task<IActionResult> GetPaged(
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? keyword = null,
            [FromQuery] bool? trangThai = null)
        {
            var result = await _thuKyService.GetPagedAsync(pageIndex, pageSize, keyword, trangThai);
            return Ok(result);
        }

        [HttpGet]
        [Authorize(Policy = Permissions.ThuKy.View)]
        public async Task<IActionResult> GetAll()
        {
            var result = await _thuKyService.GetAllAsync();
            return Ok(result);
        }

        [HttpGet("{id}")]
        [Authorize(Policy = Permissions.ThuKy.View)]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _thuKyService.GetByIdAsync(id);
            if (result == null) return NotFound(new { message = "Không tìm thấy thư ký." });
            return Ok(result);
        }

        [HttpPost]
        [Authorize(Policy = Permissions.ThuKy.Create)]
        public async Task<IActionResult> Create([FromBody] CreateUpdateThuKyDto dto)
        {
            var username = User.FindFirst(ClaimTypes.Name)?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var result = await _thuKyService.CreateAsync(dto, username);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [HttpPut("{id}")]
        [Authorize(Policy = Permissions.ThuKy.Edit)]
        public async Task<IActionResult> Update(int id, [FromBody] CreateUpdateThuKyDto dto)
        {
            var username = User.FindFirst(ClaimTypes.Name)?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var result = await _thuKyService.UpdateAsync(id, dto, username);
            if (result == null) return NotFound(new { message = "Không tìm thấy thư ký để cập nhật." });
            return Ok(result);
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = Permissions.ThuKy.Delete)]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _thuKyService.DeleteAsync(id);
            if (!success) return NotFound(new { message = "Không tìm thấy thư ký để xóa." });
            return Ok(new { message = "Đã xóa thư ký thành công." });
        }
    }
}
