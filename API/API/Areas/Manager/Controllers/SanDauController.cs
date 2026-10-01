using Dms.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace API.Areas.Manager.Controllers
{
    public class SanDauController : BaseManagerController
    {
        private readonly ISanDauService _sanDauService;
        private readonly ICumSanService _cumSanService;
        private readonly IMonTheThaoService _monTheThaoService;

        public SanDauController(
            ISanDauService sanDauService,
            ICumSanService cumSanService,
            IMonTheThaoService monTheThaoService)
        {
            _sanDauService = sanDauService;
            _cumSanService = cumSanService;
            _monTheThaoService = monTheThaoService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetPagedData(
            int pageIndex = 1,
            int pageSize = 10,
            string? keyword = null,
            int? cumSanId = null,
            int? monTheThaoId = null,
            bool? trangThai = null,
            int? donViId = null)
        {
            var result = await _sanDauService.GetPagedAsync(pageIndex, pageSize, keyword, cumSanId, monTheThaoId, trangThai, donViId);
            return Json(new { success = true, data = result });
        }

        [HttpGet]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _sanDauService.GetByIdAsync(id);
            if (result == null) return Json(new { success = false, message = "Không tìm thấy sân đấu." });
            return Json(new { success = true, data = result });
        }

        [HttpGet]
        public async Task<IActionResult> GetLookupData()
        {
            var cumSans = await _cumSanService.GetAllAsync();
            var monTheThaos = await _monTheThaoService.GetAllAsync();
            return Json(new { success = true, data = new { cumSans, monTheThaos } });
        }
    }
}
