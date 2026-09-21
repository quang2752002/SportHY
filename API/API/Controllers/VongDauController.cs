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
    public class VongDauController : ControllerBase
    {
        private readonly IVongDauService _vongDauService;

        public VongDauController(IVongDauService vongDauService)
        {
            _vongDauService = vongDauService;
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAll([FromQuery] int? giaiDauMonTheThaoId = null)
        {
            var result = await _vongDauService.GetAllAsync(giaiDauMonTheThaoId);
            return Ok(result);
        }

        [HttpGet("{id:int}")]
        [Authorize]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _vongDauService.GetByIdAsync(id);
            if (result == null) return NotFound(new { message = "Không tìm thấy vòng đấu." });
            return Ok(result);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create([FromBody] CreateUpdateVongDauDto dto)
        {
            var username = User.FindFirst(ClaimTypes.Name)?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var result = await _vongDauService.CreateAsync(dto, username);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [HttpPut("{id:int}")]
        [Authorize]
        public async Task<IActionResult> Update(int id, [FromBody] CreateUpdateVongDauDto dto)
        {
            var username = User.FindFirst(ClaimTypes.Name)?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var result = await _vongDauService.UpdateAsync(id, dto, username);
            if (result == null) return NotFound(new { message = "Không tìm thấy vòng đấu để cập nhật." });
            return Ok(result);
        }

        [HttpDelete("{id:int}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _vongDauService.DeleteAsync(id);
            if (!success) return NotFound(new { message = "Không tìm thấy vòng đấu để xóa." });
            return Ok(new { message = "Đã xóa vòng đấu thành công." });
        }
    }
}
