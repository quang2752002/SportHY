using Dms.Application.Common;
using Dms.Application.DTOs;
using Dms.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TranDauController : ControllerBase
    {
        private readonly ITranDauService _tranDauService;

        public TranDauController(ITranDauService tranDauService)
        {
            _tranDauService = tranDauService;
        }

        [HttpGet("paged")]
        [Authorize]
        public async Task<IActionResult> GetPaged(
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? keyword = null,
            [FromQuery] int? giaiDauId = null,
            [FromQuery] int? giaiDauMonTheThaoId = null,
            [FromQuery] int? vongDauId = null,
            [FromQuery] int? bangDauId = null,
            [FromQuery] int? sanDauId = null,
            [FromQuery] DateTime? ngay = null,
            [FromQuery] string? trangThai = null)
        {
            var result = await _tranDauService.GetPagedAsync(
                pageIndex, pageSize, keyword, giaiDauId, giaiDauMonTheThaoId, vongDauId, bangDauId, sanDauId, ngay, trangThai);
            return Ok(result);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAll(
            [FromQuery] int? giaiDauId = null,
            [FromQuery] int? giaiDauMonTheThaoId = null,
            [FromQuery] int? vongDauId = null,
            [FromQuery] int? bangDauId = null,
            [FromQuery] int? sanDauId = null,
            [FromQuery] DateTime? ngay = null)
        {
            var result = await _tranDauService.GetAllAsync(giaiDauId, giaiDauMonTheThaoId, vongDauId, bangDauId, sanDauId, ngay);
            return Ok(result);
        }

        [HttpGet("{id:int}")]
        [Authorize]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _tranDauService.GetByIdAsync(id);
            if (result == null) return NotFound(new { message = "Không tìm thấy thông tin trận đấu." });
            return Ok(result);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create([FromBody] CreateUpdateTranDauDto dto)
        {
            var username = User.FindFirst(ClaimTypes.Name)?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var result = await _tranDauService.CreateAsync(dto, username);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [HttpPut("{id:int}")]
        [Authorize]
        public async Task<IActionResult> Update(int id, [FromBody] CreateUpdateTranDauDto dto)
        {
            var username = User.FindFirst(ClaimTypes.Name)?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var result = await _tranDauService.UpdateAsync(id, dto, username);
            if (result == null) return NotFound(new { message = "Không tìm thấy trận đấu để cập nhật." });
            return Ok(result);
        }

        [HttpDelete("{id:int}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _tranDauService.DeleteAsync(id);
            if (!success) return NotFound(new { message = "Không tìm thấy trận đấu để xóa." });
            return Ok(new { message = "Đã xóa trận đấu thành công." });
        }

        [HttpDelete("clear-by-giai-dau-mon/{giaiDauMonTheThaoId:int}")]
        [Authorize]
        public async Task<IActionResult> ClearByGiaiDauMon(int giaiDauMonTheThaoId)
        {
            await _tranDauService.ClearByGiaiDauMonTheThaoAsync(giaiDauMonTheThaoId);
            return Ok(new { message = "Đã xóa toàn bộ lịch thi đấu của môn này." });
        }

        [HttpPost("auto-generate")]
        [Authorize]
        public async Task<IActionResult> AutoSchedule([FromBody] AutoScheduleRequestDto request)
        {
            var username = User.FindFirst(ClaimTypes.Name)?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var result = await _tranDauService.AutoScheduleAsync(request, username);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpPost("check-conflict")]
        [Authorize]
        public async Task<IActionResult> CheckConflict([FromBody] ConflictCheckRequestDto request)
        {
            var result = await _tranDauService.CheckConflictAsync(request);
            return Ok(result);
        }

        [HttpGet("check-all-conflicts/{giaiDauId:int}")]
        [Authorize]
        public async Task<IActionResult> CheckAllConflicts(int giaiDauId)
        {
            var result = await _tranDauService.CheckAllConflictsAsync(giaiDauId);
            return Ok(result);
        }
    }
}
