using Dms.Application.DTOs;
using Dms.Application.Interfaces;
using Dms.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace API.Areas.ThuKy.Controllers
{
    /// <summary>
    /// Controller kiểm tra kết quả thi đấu và theo dõi tình trạng xác nhận kết quả
    /// </summary>
    public class KetQuaController : BaseThuKyController
    {
        private readonly IMonTheThaoService _monTheThaoService;

        public KetQuaController(
            IThuKyGiaiService thuKyService,
            IGiaiDauService giaiDauService,
            IMonTheThaoService monTheThaoService,
            UserManager<ApplicationUser> userManager)
            : base(thuKyService, giaiDauService, userManager)
        {
            _monTheThaoService = monTheThaoService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int? giaiDauId)
        {
            var selectedGiaiDauId = await GetSelectedGiaiDauIdAsync(giaiDauId);
            ViewBag.SelectedGiaiDauId = selectedGiaiDauId;
            ViewBag.Tournaments = await GetAllTournamentsAsync();
            ViewBag.Sports = await _monTheThaoService.GetAllAsync();
            return View();
        }

        /// <summary>
        /// AJAX endpoint lấy danh sách kết quả trận đấu có bộ lọc
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetDanhSach(int? giaiDauId, int? monTheThaoId, string? trangThai, string? keyword)
        {
            var effectiveGiaiDauId = await GetSelectedGiaiDauIdAsync(giaiDauId);
            var list = await _thuKyService.GetDanhSachKetQuaAsync(effectiveGiaiDauId, monTheThaoId, trangThai, keyword);
            return Json(new { success = true, data = list });
        }

        /// <summary>
        /// AJAX Thư ký giải phê duyệt kết quả chính thức cho 1 trận đấu
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> XacNhan([FromBody] XacNhanKetQuaRequestDto dto)
        {
            if (dto == null || dto.TranDauId <= 0)
            {
                return Json(new { success = false, message = "Dữ liệu yêu cầu không hợp lệ." });
            }

            if (!await CanAccessMatchAsync(dto.TranDauId))
            {
                return Forbid();
            }

            var username = GetCurrentUsername();
            var (success, message) = await _thuKyService.XacNhanKetQuaAsync(dto.TranDauId, dto.GhiChu, username);
            return Json(new { success, message });
        }

        /// <summary>
        /// AJAX Thư ký giải gắn cờ yêu cầu kiểm tra lại kết quả
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> YeuCauKiemTraLai([FromBody] YeuCauKiemTraRequestDto dto)
        {
            if (dto == null || dto.TranDauId <= 0 || string.IsNullOrWhiteSpace(dto.LyDo))
            {
                return Json(new { success = false, message = "Vui lòng nhập lý do cụ thể cần kiểm tra lại kết quả." });
            }

            if (!await CanAccessMatchAsync(dto.TranDauId))
            {
                return Forbid();
            }

            var username = GetCurrentUsername();
            var (success, message) = await _thuKyService.YeuCauKiemTraLaiAsync(dto.TranDauId, dto.LyDo.Trim(), username);
            return Json(new { success, message });
        }

        /// <summary>
        /// AJAX Thư ký duyệt hàng loạt các trận đấu hợp lệ
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> XacNhanHangLoat([FromBody] BulkApproveRequestDto dto)
        {
            if (dto == null || dto.TranDauIds == null || dto.TranDauIds.Count == 0)
            {
                return Json(new { success = false, message = "Vui lòng chọn ít nhất một trận đấu để phê duyệt." });
            }

            if (!await CanAccessMatchesAsync(dto.TranDauIds))
            {
                return Forbid();
            }

            var username = GetCurrentUsername();
            var (success, message) = await _thuKyService.XacNhanHangLoatAsync(dto.TranDauIds, username);
            return Json(new { success, message });
        }
    }
}
