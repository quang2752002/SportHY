using Dms.Application.Common;
using Dms.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace API.Controllers
{
    [AllowAnonymous]
    public class HomeController : Controller
    {
        private readonly IGiaiDauService _giaiDauService;
        private readonly IMonTheThaoService _monTheThaoService;
        private readonly ISanDauService _sanDauService;

        public HomeController(
            IGiaiDauService giaiDauService,
            IMonTheThaoService monTheThaoService,
            ISanDauService sanDauService)
        {
            _giaiDauService = giaiDauService;
            _monTheThaoService = monTheThaoService;
            _sanDauService = sanDauService;
        }

        /// <summary>
        /// Hiển thị trang chủ cổng thông tin giải đấu
        /// </summary>
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// API trả về danh sách toàn bộ giải đấu công khai và các môn thi đấu tổ chức
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetTournaments()
        {
            var list = await _giaiDauService.GetAllAsync();
            return Json(new { success = true, data = list });
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var item = await _giaiDauService.GetByIdAsync(id);
            if (item == null)
            {
                return NotFound("Không tìm thấy giải đấu yêu cầu.");
            }

            var allMonTheThaos = await _monTheThaoService.GetAllAsync();
            ViewBag.AllMonTheThaos = allMonTheThaos?.ToList() ?? new();

            return View(item);
        }
    }
}
