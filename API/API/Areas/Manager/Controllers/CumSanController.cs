using Dms.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace API.Areas.Manager.Controllers
{
    public class CumSanController : BaseManagerController
    {
        private readonly ICumSanService _cumSanService;
        private readonly ISanDauService _sanDauService;

        public CumSanController(ICumSanService cumSanService, ISanDauService sanDauService)
        {
            _cumSanService = cumSanService;
            _sanDauService = sanDauService;
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
            bool? trangThai = null,
            int? donViId = null)
        {
            var result = await _cumSanService.GetPagedAsync(pageIndex, pageSize, keyword, trangThai, donViId);
            return Json(new { success = true, data = result });
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _cumSanService.GetAllAsync();
            return Json(new { success = true, data = result });
        }

        [HttpGet]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _cumSanService.GetByIdAsync(id);
            if (result == null) return Json(new { success = false, message = "Không tìm thấy cụm sân." });
            return Json(new { success = true, data = result });
        }

        [HttpGet]
        public async Task<IActionResult> GetCourts(int cumSanId)
        {
            var result = await _sanDauService.GetPagedAsync(1, 1000, null, cumSanId);
            return Json(new { success = true, data = result });
        }
    }
}
