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
        private readonly IDanhMucMonTheThaoService _danhMucMonTheThaoService;

        public HuyChuongController(
            IHuyChuongService huyChuongService,
            IThuKyGiaiService thuKyGiaiService,
            IGiaiDauService giaiDauService,
            IDonViService donViService,
            IMonTheThaoService monTheThaoService,
            IDanhMucMonTheThaoService danhMucMonTheThaoService)
        {
            _huyChuongService = huyChuongService;
            _thuKyGiaiService = thuKyGiaiService;
            _giaiDauService = giaiDauService;
            _donViService = donViService;
            _monTheThaoService = monTheThaoService;
            _danhMucMonTheThaoService = danhMucMonTheThaoService;
        }

        /// <summary>
        /// Trang hiển thị bảng tổng sắp và quản lý huy chương giải đấu
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Index([FromQuery] int? giaiDauId = null, [FromQuery] int? danhMucId = null, [FromQuery] int? monTheThaoId = null)
        {
            var tournaments = (await _giaiDauService.GetAllAsync())?.ToList() ?? new();
            var categories = (await _danhMucMonTheThaoService.GetAllAsync())?.ToList() ?? new();

            ViewBag.Tournaments = tournaments;
            ViewBag.Categories = categories;
            ViewBag.SelectedGiaiDauId = giaiDauId ?? 0;
            ViewBag.SelectedDanhMucId = danhMucId ?? 0;
            ViewBag.SelectedMonTheThaoId = monTheThaoId ?? 0;

            ViewBag.GiaiDaus = tournaments;
            ViewBag.DonVis = await _donViService.GetAllAsync();
            return View();
        }

        /// <summary>
        /// Lấy bảng tổng sắp huy chương toàn đoàn sử dụng chung backend với trang /BangXepHang, hỗ trợ lọc theo môn và danh mục
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetMedalRankings([FromQuery] int? giaiDauId = null, [FromQuery] int? danhMucId = null, [FromQuery] int? monTheThaoId = null)
        {
            try
            {
                var result = await _thuKyGiaiService.GetBangTongSapHuyChuongAsync(giaiDauId, danhMucId, monTheThaoId);
                return Json(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Lấy danh sách bảng xếp hạng huy chương gom nhóm theo từng danh mục môn thể thao
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetRankingsByDanhMuc([FromQuery] int? giaiDauId = null)
        {
            try
            {
                var result = await _thuKyGiaiService.GetBangXepHangTheoDanhMucAsync(giaiDauId);
                return Json(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Lấy kết quả huy chương theo từng môn thi đấu trong giải
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetRankingsByMonTheThao([FromQuery] int? giaiDauId = null, [FromQuery] int? danhMucId = null)
        {
            try
            {
                var result = await _thuKyGiaiService.GetBangXepHangTheoMonAsync(giaiDauId, danhMucId);
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
    }
}
