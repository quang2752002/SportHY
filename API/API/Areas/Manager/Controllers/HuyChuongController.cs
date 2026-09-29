using Dms.Application.DTOs;
using Dms.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace API.Areas.Manager.Controllers
{
    public class HuyChuongController : BaseManagerController
    {
        private readonly IHuyChuongService _huyChuongService;
        private readonly IThuKyGiaiService _thuKyGiaiService;
        private readonly IGiaiDauService _giaiDauService;
        private readonly IDonViService _donViService;
        private readonly IMonTheThaoService _monTheThaoService;

        public HuyChuongController(
            IHuyChuongService huyChuongService,
            IThuKyGiaiService thuKyGiaiService,
            IGiaiDauService giaiDauService,
            IDonViService donViService,
            IMonTheThaoService monTheThaoService)
        {
            _huyChuongService = huyChuongService;
            _thuKyGiaiService = thuKyGiaiService;
            _giaiDauService = giaiDauService;
            _donViService = donViService;
            _monTheThaoService = monTheThaoService;
        }

        [HttpGet]
        public async Task<IActionResult> Index([FromQuery] int? giaiDauId = null)
        {
            ViewBag.GiaiDaus = await _giaiDauService.GetAllAsync();
            ViewBag.DonVis = await _donViService.GetAllAsync();
            ViewBag.SelectedGiaiDauId = giaiDauId;
            return View();
        }

        /// <summary>
        /// Lấy bảng tổng sắp huy chương toàn đoàn sử dụng chung backend với trang /BangXepHang, hỗ trợ lọc theo môn
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetMedalRankings([FromQuery] int? giaiDauId = null, [FromQuery] int? monTheThaoId = null)
        {
            try
            {
                var result = await _thuKyGiaiService.GetBangTongSapHuyChuongAsync(giaiDauId, null, monTheThaoId);
                return Json(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Lấy danh sách môn thể thao theo giải đấu hoặc toàn bộ môn thi đấu
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetSportsByGiaiDau([FromQuery] int? giaiDauId = null)
        {
            try
            {
                if (giaiDauId.HasValue && giaiDauId.Value > 0)
                {
                    var giaiDau = await _giaiDauService.GetByIdAsync(giaiDauId.Value);
                    if (giaiDau == null) return Json(new { success = true, data = new List<object>() });

                    var sports = (giaiDau.MonTheThaos ?? new List<GiaiDauMonTheThaoDto>())
                        .Select(m => new { id = m.MonTheThaoId, ten = m.Ten, hinhThucThiDau = m.HinhThucThiDau })
                        .ToList();
                    return Json(new { success = true, data = sports });
                }
                else
                {
                    var allMons = await _monTheThaoService.GetAllAsync();
                    var sports = allMons
                        .Select(m => new { id = m.Id, ten = m.Ten, hinhThucThiDau = m.HinhThucThiDau })
                        .ToList();
                    return Json(new { success = true, data = sports });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Lấy danh sách chi tiết các huy chương đã trao, hỗ trợ lọc theo môn
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int? giaiDauId = null, [FromQuery] int? monTheThaoId = null)
        {
            var result = await _huyChuongService.GetAllAsync(giaiDauId, monTheThaoId);
            return Json(new { success = true, data = result });
        }

        /// <summary>
        /// Tự động quét kết quả các trận đấu và bảng đấu để trao và đồng bộ huy chương
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> SyncMedalsFromResults([FromQuery] int? giaiDauId = null, [FromQuery] int? monTheThaoId = null)
        {
            var username = User.FindFirst(ClaimTypes.Name)?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "Manager";
            try
            {
                var (success, message, awarded) = await _huyChuongService.SyncMedalsFromResultsAsync(giaiDauId, monTheThaoId, username);
                return Json(new { success, message, awarded });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Save([FromBody] CreateUpdateHuyChuongDto dto)
        {
            var username = User.FindFirst(ClaimTypes.Name)?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "Manager";

            try
            {
                var created = await _huyChuongService.CreateAsync(dto, username);
                return Json(new { success = true, message = "Trao huy chương thành công!", data = created });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _huyChuongService.DeleteAsync(id);
            if (!success) return Json(new { success = false, message = "Không tìm thấy huy chương để xóa." });
            return Json(new { success = true, message = "Đã thu hồi huy chương thành công!" });
        }
    }
}
