using Dms.Application.Interfaces;
using Dms.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace API.Areas.ThuKy.Controllers
{
    /// <summary>
    /// Controller trang chủ / Dashboard tổng quan Thư ký giải
    /// </summary>
    public class HomeController : BaseThuKyController
    {
        public HomeController(
            IThuKyGiaiService thuKyService,
            IGiaiDauService giaiDauService,
            UserManager<ApplicationUser> userManager)
            : base(thuKyService, giaiDauService, userManager)
        {
        }

        [HttpGet]
        public async Task<IActionResult> Index(int? giaiDauId)
        {
            var selectedGiaiDauId = await GetSelectedGiaiDauIdAsync(giaiDauId);
            ViewBag.SelectedGiaiDauId = selectedGiaiDauId;
            ViewBag.Tournaments = await GetAllTournamentsAsync();
            return View();
        }

        /// <summary>
        /// AJAX endpoint lấy số liệu Dashboard tổng quan
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetDashboardData(int? giaiDauId)
        {
            var effectiveGiaiDauId = await GetSelectedGiaiDauIdAsync(giaiDauId);
            var data = await _thuKyService.GetDashboardAsync(effectiveGiaiDauId);
            return Json(new { success = true, data });
        }
    }
}
