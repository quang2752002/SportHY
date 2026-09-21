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
    public class BangDauController : ControllerBase
    {
        private readonly IBangDauService _bangDauService;

        public BangDauController(IBangDauService bangDauService)
        {
            _bangDauService = bangDauService;
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAll([FromQuery] int? giaiDauMonTheThaoId = null)
        {
            var result = await _bangDauService.GetAllAsync(giaiDauMonTheThaoId);
            return Ok(result);
        }

        [HttpGet("{id:int}")]
        [Authorize]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _bangDauService.GetByIdAsync(id);
            if (result == null) return NotFound(new { message = "Không tìm thấy bảng đấu." });
            return Ok(result);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create([FromBody] CreateUpdateBangDauDto dto)
        {
            var username = User.FindFirst(ClaimTypes.Name)?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var result = await _bangDauService.CreateAsync(dto, username);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [HttpPut("{id:int}")]
        [Authorize]
        public async Task<IActionResult> Update(int id, [FromBody] CreateUpdateBangDauDto dto)
        {
            var username = User.FindFirst(ClaimTypes.Name)?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var result = await _bangDauService.UpdateAsync(id, dto, username);
            if (result == null) return NotFound(new { message = "Không tìm thấy bảng đấu để cập nhật." });
            return Ok(result);
        }

        [HttpDelete("{id:int}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _bangDauService.DeleteAsync(id);
            if (!success) return NotFound(new { message = "Không tìm thấy bảng đấu để xóa." });
            return Ok(new { message = "Đã xóa bảng đấu thành công." });
        }

        [HttpPost("assign-teams")]
        [Authorize]
        public async Task<IActionResult> AssignTeams([FromBody] AssignTeamsToBangDto dto)
        {
            var username = User.FindFirst(ClaimTypes.Name)?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var success = await _bangDauService.AssignTeamsAsync(dto, username);
            if (!success) return BadRequest(new { message = "Không thể gán đội vào bảng đấu." });
            return Ok(new { message = "Đã gán đội vào bảng đấu thành công." });
        }

        [HttpPost("auto-distribute")]
        [Authorize]
        public async Task<IActionResult> AutoDistribute([FromBody] AutoDistributeBangDto dto)
        {
            var username = User.FindFirst(ClaimTypes.Name)?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var result = await _bangDauService.AutoDistributeAsync(dto, username);
            return Ok(result);
        }
    }
}
