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
    public class DanhMucMonTheThaoController : ControllerBase
    {
        private readonly IDanhMucMonTheThaoService _service;

        public DanhMucMonTheThaoController(IDanhMucMonTheThaoService service)
        {
            _service = service;
        }

        [HttpGet("paged")]
        [Authorize(Policy = Permissions.DanhMucMonTheThao.View)]
        public async Task<IActionResult> GetPaged(
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? keyword = null,
            [FromQuery] bool? trangThai = null)
        {
            var result = await _service.GetPagedAsync(pageIndex, pageSize, keyword, trangThai);
            return Ok(result);
        }

        [HttpGet]
        [Authorize(Policy = Permissions.DanhMucMonTheThao.View)]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAllAsync();
            return Ok(result);
        }

        [HttpGet("{id}")]
        [Authorize(Policy = Permissions.DanhMucMonTheThao.View)]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _service.GetByIdAsync(id);
            if (result == null) return NotFound(new { message = "Không tìm thấy danh mục môn thể thao." });
            return Ok(result);
        }

        [HttpPost]
        [Authorize(Policy = Permissions.DanhMucMonTheThao.Create)]
        public async Task<IActionResult> Create([FromBody] CreateUpdateDanhMucMonTheThaoDto dto)
        {
            var username = User.FindFirst(ClaimTypes.Name)?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var result = await _service.CreateAsync(dto, username);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [HttpPut("{id}")]
        [Authorize(Policy = Permissions.DanhMucMonTheThao.Edit)]
        public async Task<IActionResult> Update(int id, [FromBody] CreateUpdateDanhMucMonTheThaoDto dto)
        {
            var username = User.FindFirst(ClaimTypes.Name)?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var result = await _service.UpdateAsync(id, dto, username);
            if (result == null) return NotFound(new { message = "Không tìm thấy danh mục môn thể thao để cập nhật." });
            return Ok(result);
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = Permissions.DanhMucMonTheThao.Delete)]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _service.DeleteAsync(id);
            if (!success) return NotFound(new { message = "Không tìm thấy danh mục môn thể thao để xóa." });
            return Ok(new { message = "Đã xóa danh mục môn thể thao thành công." });
        }
    }
}
