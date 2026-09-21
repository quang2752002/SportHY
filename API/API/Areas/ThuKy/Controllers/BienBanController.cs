using Dms.Application.DTOs;
using Dms.Application.Interfaces;
using Dms.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace API.Areas.ThuKy.Controllers
{
    public class SignBienBanRequest
    {
        public int TranDauId { get; set; }
        public string SignerName { get; set; } = string.Empty;
    }

    /// <summary>
    /// Controller kiểm tra biên bản, in ấn và xuất biên bản PDF
    /// </summary>
    public class BienBanController : BaseThuKyController
    {
        private readonly IMonTheThaoService _monTheThaoService;

        public BienBanController(
            IThuKyGiaiService thuKyService,
            IGiaiDauService giaiDauService,
            IMonTheThaoService monTheThaoService,
            UserManager<ApplicationUser> userManager)
            : base(thuKyService, giaiDauService, userManager)
        {
            _monTheThaoService = monTheThaoService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int? tranDauId, int? giaiDauId)
        {
            var selectedGiaiDauId = await GetSelectedGiaiDauIdAsync(giaiDauId);
            ViewBag.SelectedGiaiDauId = selectedGiaiDauId;
            ViewBag.SelectedTranDauId = tranDauId;
            ViewBag.Tournaments = await GetAllTournamentsAsync();
            ViewBag.Sports = await _monTheThaoService.GetAllAsync();
            return View();
        }

        /// <summary>
        /// AJAX endpoint lấy danh sách biên bản kiểm tra
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetDanhSach(int? giaiDauId, int? monTheThaoId, string? trangThaiKy, string? keyword)
        {
            var effectiveGiaiDauId = await GetSelectedGiaiDauIdAsync(giaiDauId);
            var list = await _thuKyService.GetDanhSachBienBanAsync(effectiveGiaiDauId, monTheThaoId, trangThaiKy, keyword);
            return Json(new { success = true, data = list });
        }

        /// <summary>
        /// AJAX endpoint lấy chi tiết 1 biên bản thi đấu
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetChiTiet(int tranDauId)
        {
            var data = await _thuKyService.GetChiTietBienBanAsync(tranDauId);
            if (data == null)
            {
                return Json(new { success = false, message = "Không tìm thấy biên bản trận đấu." });
            }
            return Json(new { success = true, data });
        }

        /// <summary>
        /// AJAX Thư ký giải ký xác nhận biên bản điện tử
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> KyXacNhan([FromBody] SignBienBanRequest request)
        {
            if (request == null || request.TranDauId <= 0 || string.IsNullOrWhiteSpace(request.SignerName))
            {
                return Json(new { success = false, message = "Vui lòng nhập họ tên người ký xác nhận." });
            }

            var username = GetCurrentUsername();
            var (success, message) = await _thuKyService.KyXacNhanBienBanAsync(request.TranDauId, request.SignerName.Trim(), username);
            return Json(new { success, message });
        }

        /// <summary>
        /// Trang hiển thị mẫu in biên bản trận đấu chuẩn A4 (Quốc hiệu tiêu ngữ)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> In(int id)
        {
            var data = await _thuKyService.GetChiTietBienBanAsync(id);
            if (data == null)
            {
                return NotFound("Không tìm thấy biên bản trận đấu.");
            }
            return View(data);
        }
    }
}
