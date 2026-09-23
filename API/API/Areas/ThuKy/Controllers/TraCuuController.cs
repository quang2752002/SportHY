using Dms.Application.Interfaces;
using Dms.Application.DTOs;
using Dms.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace API.Areas.ThuKy.Controllers
{
    /// <summary>
    /// Controller tra cứu thông minh dữ liệu giải đấu (VĐV, Đoàn, Trận đấu, Biên bản)
    /// </summary>
    public class TraCuuController : BaseThuKyController
    {
        public TraCuuController(
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
        /// AJAX endpoint tìm kiếm thông minh đa tiêu chí
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> TimKiem(string keyword, int? giaiDauId, string? loaiDoiTuong = "All")
        {
            var effectiveGiaiDauId = await GetSelectedGiaiDauIdAsync(giaiDauId);
            if (User.IsInRole(Dms.Application.Common.AppRoles.Secretary) && !effectiveGiaiDauId.HasValue)
            {
                return Json(new
                {
                    success = true,
                    data = new ThuKyTraCuuResultDto { Keyword = keyword ?? string.Empty }
                });
            }

            var result = await _thuKyService.TraCuuTongHopAsync(keyword, effectiveGiaiDauId, loaiDoiTuong);
            return Json(new { success = true, data = result });
        }
    }
}
