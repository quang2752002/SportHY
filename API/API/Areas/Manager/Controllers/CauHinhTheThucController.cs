using Dms.Application.DTOs;
using Dms.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace API.Areas.Manager.Controllers
{
    /// <summary>
    /// Controller quản lý cấu hình thể thức thi đấu và luật chấm điểm cho các môn thể thao
    /// Tuân thủ quy tắc Thin Controller: toàn bộ nghiệp vụ xử lý tại tầng Dms.Application
    /// </summary>
    public class CauHinhTheThucController : BaseManagerController
    {
        private readonly ICauHinhTheThucService _cauHinhTheThucService;

        public CauHinhTheThucController(ICauHinhTheThucService cauHinhTheThucService)
        {
            _cauHinhTheThucService = cauHinhTheThucService;
        }

        /// <summary>
        /// Lấy cấu hình thể thức áp dụng cho môn thể thao (hoặc theo giải đấu nếu có)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetConfig(int monTheThaoId, int? giaiDauMonTheThaoId = null)
        {
            try
            {
                var config = await _cauHinhTheThucService.GetEffectiveConfigAsync(monTheThaoId, giaiDauMonTheThaoId);
                return Json(new { success = true, data = config });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Lấy toàn bộ danh sách cấu hình thể thức thi đấu trong hệ thống
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var list = await _cauHinhTheThucService.GetAllAsync();
                return Json(new { success = true, data = list });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Lưu hoặc cập nhật cấu hình thể thức cho môn thể thao / nội dung thi đấu
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> SaveConfig([FromBody] CreateUpdateCauHinhTheThucDto dto)
        {
            if (dto == null || dto.MonTheThaoId <= 0)
            {
                return Json(new { success = false, message = "Dữ liệu cấu hình thể thức không hợp lệ." });
            }

            try
            {
                var username = User.FindFirst(ClaimTypes.Name)?.Value 
                    ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                    ?? "Manager";

                var result = await _cauHinhTheThucService.UpsertConfigAsync(dto, username);
                return Json(new { success = true, message = "Đã lưu cấu hình thể thức thi đấu thành công!", data = result });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}
