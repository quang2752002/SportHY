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
    public class LoaiHuyChuongController : ControllerBase
    {
        private readonly ILoaiHuyChuongService _loaiHuyChuongService;

        public LoaiHuyChuongController(ILoaiHuyChuongService loaiHuyChuongService)
        {
            _loaiHuyChuongService = loaiHuyChuongService;
        }

        [HttpGet("paged")]
        [Authorize(Policy = Permissions.HuyChuong.View)]
        public async Task<IActionResult> GetPaged(
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? keyword = null)
        {
            var result = await _loaiHuyChuongService.GetPagedAsync(pageIndex, pageSize, keyword);
            return Ok(result);
        }

        [HttpGet]
        [Authorize(Policy = Permissions.HuyChuong.View)]
        public async Task<IActionResult> GetAll()
        {
            var result = await _loaiHuyChuongService.GetAllAsync();
            return Ok(result);
        }

        [HttpGet("{id}")]
        [Authorize(Policy = Permissions.HuyChuong.View)]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _loaiHuyChuongService.GetByIdAsync(id);
            if (result == null) return NotFound(new { message = "Không tìm thấy loại huy chương." });
            return Ok(result);
        }

        [HttpPost]
        [Authorize(Policy = Permissions.HuyChuong.Create)]
        public async Task<IActionResult> Create([FromBody] CreateUpdateLoaiHuyChuongDto dto)
        {
            var username = User.FindFirst(ClaimTypes.Name)?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var result = await _loaiHuyChuongService.CreateAsync(dto, username);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [HttpPut("{id}")]
        [Authorize(Policy = Permissions.HuyChuong.Edit)]
        public async Task<IActionResult> Update(int id, [FromBody] CreateUpdateLoaiHuyChuongDto dto)
        {
            var username = User.FindFirst(ClaimTypes.Name)?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var result = await _loaiHuyChuongService.UpdateAsync(id, dto, username);
            if (result == null) return NotFound(new { message = "Không tìm thấy loại huy chương để cập nhật." });
            return Ok(result);
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = Permissions.HuyChuong.Delete)]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _loaiHuyChuongService.DeleteAsync(id);
            if (!success) return NotFound(new { message = "Không tìm thấy loại huy chương để xóa." });
            return Ok(new { message = "Đã xóa loại huy chương thành công." });
        }
    }
}
