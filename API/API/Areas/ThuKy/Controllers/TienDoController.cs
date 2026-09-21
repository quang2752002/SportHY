using Dms.Application.Interfaces;
using Dms.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace API.Areas.ThuKy.Controllers
{
    /// <summary>
    /// Controller theo dõi tiến độ từng môn thể thao của giải đấu
    /// </summary>
    public class TienDoController : BaseThuKyController
    {
        public TienDoController(
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
        /// AJAX endpoint lấy danh sách tiến độ các môn thể thao
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetTienDo(int? giaiDauId)
        {
            var effectiveGiaiDauId = await GetSelectedGiaiDauIdAsync(giaiDauId);
            var list = await _thuKyService.GetTienDoCacMonAsync(effectiveGiaiDauId);
            return Json(new { success = true, data = list });
        }
    }
}
