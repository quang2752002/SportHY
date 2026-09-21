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
            [FromQuery] bool? trangThai = null)
        {
            var result = await _cumSanService.GetPagedAsync(pageIndex, pageSize, keyword, trangThai);
            return Ok(result);
        }

        [HttpGet]
        [Authorize(Policy = Permissions.SanDau.View)]
        public async Task<IActionResult> GetAll()
        {
            var result = await _cumSanService.GetAllAsync();
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
        [Authorize(Policy = Permissions.SanDau.Create)]
        public async Task<IActionResult> Create([FromBody] CreateUpdateCumSanDto dto)
        {
            var username = User.FindFirst(ClaimTypes.Name)?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var result = await _cumSanService.CreateAsync(dto, username);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [HttpPut("{id}")]
        [Authorize(Policy = Permissions.SanDau.Edit)]
        public async Task<IActionResult> Update(int id, [FromBody] CreateUpdateCumSanDto dto)
        {
            var username = User.FindFirst(ClaimTypes.Name)?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var result = await _cumSanService.UpdateAsync(id, dto, username);
            if (result == null) return NotFound(new { message = "Không tìm thấy cụm sân để cập nhật." });
            return Ok(result);
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = Permissions.SanDau.Delete)]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _cumSanService.DeleteAsync(id);
            if (!success) return NotFound(new { message = "Không tìm thấy cụm sân để xóa." });
            return Ok(new { message = "Đã xóa cụm sân thành công." });
        }
    }
}
