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
    public class CauHinhLichThiDauController : ControllerBase
    {
        private readonly ICauHinhLichThiDauService _service;

        public CauHinhLichThiDauController(ICauHinhLichThiDauService service)
        {
            _service = service;
        }

        [HttpGet("{monTheThaoId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetByMonTheThao(int monTheThaoId)
        {
            var result = await _service.GetByMonTheThaoAsync(monTheThaoId);
            if (result == null) return NotFound(new { message = "Chưa có cấu hình xếp lịch cho môn thể thao này." });
            return Ok(result);
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Upsert([FromBody] CreateUpdateCauHinhLichThiDauDto dto)
        {
            var username = User.FindFirst(ClaimTypes.Name)?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var result = await _service.UpsertAsync(dto, username);
            return Ok(result);
        }

        [HttpDelete("{monTheThaoId}")]
        [AllowAnonymous]
        public async Task<IActionResult> Delete(int monTheThaoId)
        {
            var success = await _service.DeleteAsync(monTheThaoId);
            if (!success) return NotFound(new { message = "Không tìm thấy cấu hình để xóa." });
            return Ok(new { message = "Đã đặt lại cấu hình xếp lịch về mặc định." });
        }
    }
}
