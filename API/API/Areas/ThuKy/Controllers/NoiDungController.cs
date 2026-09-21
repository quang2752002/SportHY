using Dms.Application.Interfaces;
using Dms.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace API.Areas.ThuKy.Controllers
{
    /// <summary>
    /// Controller theo dõi toàn bộ nội dung thi đấu trong giải
    /// </summary>
    public class NoiDungController : BaseThuKyController
    {
        private readonly IMonTheThaoService _monTheThaoService;

        public NoiDungController(
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
        /// AJAX endpoint lấy danh sách nội dung thi đấu
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetDanhSach(int? giaiDauId, int? monTheThaoId, string? loaiThiDau, string? gioiTinh, string? keyword)
        {
            var effectiveGiaiDauId = await GetSelectedGiaiDauIdAsync(giaiDauId);
            var list = await _thuKyService.GetDanhSachNoiDungAsync(effectiveGiaiDauId, monTheThaoId, loaiThiDau, gioiTinh, keyword);
            return Json(new { success = true, data = list });
        }

        /// <summary>
        /// AJAX endpoint lấy thông tin chi tiết một nội dung (danh sách VĐV, bảng đấu, vòng đấu)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetChiTiet(int id)
        {
            var data = await _thuKyService.GetChiTietNoiDungAsync(id);
            if (data == null)
            {
                return Json(new { success = false, message = "Không tìm thấy nội dung thi đấu." });
            }
            return Json(new { success = true, data });
        }
    }
}
