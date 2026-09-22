using Dms.Application.Common;
using Dms.Application.DTOs;
using Dms.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
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
        private readonly ITranDauService _tranDauService;
        private readonly IBangDauService _bangDauService;
        private readonly IThuKyGiaiService _thuKyGiaiService;
        private readonly IDonViService _donViService;

        public HomeController(
            IGiaiDauService giaiDauService,
            IMonTheThaoService monTheThaoService,
            ISanDauService sanDauService,
            ITranDauService tranDauService,
            IBangDauService bangDauService,
            IThuKyGiaiService thuKyGiaiService,
            IDonViService donViService)
        {
            _giaiDauService = giaiDauService;
            _monTheThaoService = monTheThaoService;
            _sanDauService = sanDauService;
            _tranDauService = tranDauService;
            _bangDauService = bangDauService;
            _thuKyGiaiService = thuKyGiaiService;
            _donViService = donViService;
        }

        /// <summary>
        /// Hiển thị trang chủ cổng thông tin giải đấu
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var tournaments = await _giaiDauService.GetAllAsync();
            ViewBag.Tournaments = tournaments?.ToList() ?? new();
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

        /// <summary>
        /// Trang hiển thị Bảng Xếp Hạng Huy Chương các đơn vị công khai ngang cấp trang chủ
        /// </summary>
        [HttpGet]
        [Route("BangXepHang")]
        [Route("Home/BangXepHang")]
        public async Task<IActionResult> BangXepHang(int? giaiDauId = null)
        {
            var tournaments = (await _giaiDauService.GetAllAsync())?.ToList() ?? new();
            ViewBag.Tournaments = tournaments;
            ViewBag.SelectedGiaiDauId = giaiDauId ?? 0;
            return View();
        }

        /// <summary>
        /// Lấy bảng tổng sắp huy chương các đơn vị theo giải đấu hoặc toàn bộ giải đấu
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetMedalRankings(int? giaiDauId = null)
        {
            try
            {
                if (giaiDauId.HasValue && giaiDauId.Value > 0)
                {
                    var result = await _thuKyGiaiService.GetBangTongSapHuyChuongAsync(giaiDauId.Value);
                    return Json(new { success = true, data = result });
                }

                // Tổng hợp toàn bộ các giải đấu
                var allTournaments = (await _giaiDauService.GetAllAsync())?.ToList() ?? new();
                var allDonVis = (await _donViService.GetAllAsync())?.ToList() ?? new();
                var donViMap = allDonVis.ToDictionary(d => d.Id);
                var tally = new Dictionary<int, (int vang, int bac, int dong)>();

                foreach (var dv in allDonVis)
                {
                    tally[dv.Id] = (0, 0, 0);
                }

                int vangTotal = 0, bacTotal = 0, dongTotal = 0;

                foreach (var t in allTournaments)
                {
                    var bts = await _thuKyGiaiService.GetBangTongSapHuyChuongAsync(t.Id);
                    if (bts?.BangXepHang != null)
                    {
                        foreach (var item in bts.BangXepHang)
                        {
                            if (!tally.ContainsKey(item.DonViId))
                            {
                                tally[item.DonViId] = (0, 0, 0);
                            }
                            var cur = tally[item.DonViId];
                            tally[item.DonViId] = (cur.vang + item.SoHuyChuongVang, cur.bac + item.SoHuyChuongBac, cur.dong + item.SoHuyChuongDong);
                        }
                        vangTotal += bts.TongSoHuyChuongVang;
                        bacTotal += bts.TongSoHuyChuongBac;
                        dongTotal += bts.TongSoHuyChuongDong;
                    }
                }

                var list = new List<HuyChuongDoanDto>();
                foreach (var kvp in tally)
                {
                    donViMap.TryGetValue(kvp.Key, out var dv);
                    list.Add(new HuyChuongDoanDto
                    {
                        DonViId = kvp.Key,
                        MaDonVi = dv?.Ma ?? "",
                        TenDonVi = dv?.Ten ?? "Đơn vị",
                        SoHuyChuongVang = kvp.Value.vang,
                        SoHuyChuongBac = kvp.Value.bac,
                        SoHuyChuongDong = kvp.Value.dong
                    });
                }

                var sorted = list.OrderByDescending(x => x.SoHuyChuongVang)
                                 .ThenByDescending(x => x.SoHuyChuongBac)
                                 .ThenByDescending(x => x.SoHuyChuongDong)
                                 .ThenByDescending(x => x.TongSoHuyChuong)
                                 .ThenBy(x => x.TenDonVi)
                                 .ToList();

                for (int i = 0; i < sorted.Count; i++)
                {
                    sorted[i].XepHang = i + 1;
                }

                var consolidated = new BangTongSapHuyChuongDto
                {
                    GiaiDauId = 0,
                    TenGiaiDau = "Tất cả các giải đấu",
                    TongSoHuyChuongVang = vangTotal,
                    TongSoHuyChuongBac = bacTotal,
                    TongSoHuyChuongDong = dongTotal,
                    BangXepHang = sorted,
                    NgayXuatBaoCao = DateTime.Now
                };

                return Json(new { success = true, data = consolidated });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
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

        /// <summary>
        /// Trang hiển thị Lịch thi đấu và Bảng đấu công khai cho khán giả & VĐV ngang cấp trang chủ
        /// </summary>
        [HttpGet]
        [Route("LichThiDau")]
        [Route("Home/LichThiDau")]
        public async Task<IActionResult> LichThiDau(int? giaiDauId = null, int? giaiDauMonTheThaoId = null)
        {
            var tournaments = (await _giaiDauService.GetAllAsync())?.ToList() ?? new();
            ViewBag.Tournaments = tournaments;

            int selectedGiaiDauId = giaiDauId ?? tournaments.FirstOrDefault()?.Id ?? 0;
            ViewBag.SelectedGiaiDauId = selectedGiaiDauId;

            var selectedTournament = selectedGiaiDauId > 0 ? await _giaiDauService.GetByIdAsync(selectedGiaiDauId) : null;
            ViewBag.SelectedTournament = selectedTournament;

            var sports = selectedTournament?.MonTheThaos ?? new();
            ViewBag.Sports = sports;

            int selectedGmtId = giaiDauMonTheThaoId ?? sports.FirstOrDefault()?.Id ?? 0;
            ViewBag.SelectedGmtId = selectedGmtId;

            return View();
        }

        /// <summary>
        /// Lấy danh sách các môn thi đấu thuộc một giải đấu cụ thể
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetSportsByTournament(int giaiDauId)
        {
            var tournament = await _giaiDauService.GetByIdAsync(giaiDauId);
            if (tournament == null) return Json(new { success = false, message = "Không tìm thấy giải đấu." });
            return Json(new { success = true, data = tournament.MonTheThaos });
        }

        /// <summary>
        /// Lấy toàn bộ danh sách trận đấu và bảng đấu theo giải đấu & môn thi đấu để hiển thị
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetScheduleAndBrackets(
            int giaiDauId,
            int? giaiDauMonTheThaoId = null,
            int? vongDauId = null,
            int? bangDauId = null,
            string? keyword = null,
            DateTime? ngay = null,
            string? trangThai = null)
        {
            try
            {
                var matches = await _tranDauService.GetAllAsync(
                    giaiDauId: giaiDauId,
                    giaiDauMonTheThaoId: (giaiDauMonTheThaoId.HasValue && giaiDauMonTheThaoId.Value > 0) ? giaiDauMonTheThaoId : null,
                    vongDauId: (vongDauId.HasValue && vongDauId.Value > 0) ? vongDauId : null,
                    bangDauId: (bangDauId.HasValue && bangDauId.Value > 0) ? bangDauId : null,
                    sanDauId: null,
                    ngay: ngay,
                    danhMucMonTheThaoId: null
                );

                if (!string.IsNullOrWhiteSpace(keyword))
                {
                    var kw = keyword.Trim().ToLower();
                    matches = matches.Where(t =>
                        (!string.IsNullOrEmpty(t.TenTran) && t.TenTran.ToLower().Contains(kw)) ||
                        (!string.IsNullOrEmpty(t.TenDoi1) && t.TenDoi1.ToLower().Contains(kw)) ||
                        (!string.IsNullOrEmpty(t.TenDoi2) && t.TenDoi2.ToLower().Contains(kw)) ||
                        (!string.IsNullOrEmpty(t.DonViDoi1) && t.DonViDoi1.ToLower().Contains(kw)) ||
                        (!string.IsNullOrEmpty(t.DonViDoi2) && t.DonViDoi2.ToLower().Contains(kw)) ||
                        (!string.IsNullOrEmpty(t.TenSanDau) && t.TenSanDau.ToLower().Contains(kw)) ||
                        (!string.IsNullOrEmpty(t.TenMonTheThao) && t.TenMonTheThao.ToLower().Contains(kw))
                    );
                }

                if (!string.IsNullOrWhiteSpace(trangThai))
                {
                    matches = matches.Where(t => t.TrangThai == trangThai);
                }

                // Lấy các bảng đấu nếu có chọn môn cụ thể
                IEnumerable<BangDauDto> groups = new List<BangDauDto>();
                if (giaiDauMonTheThaoId.HasValue && giaiDauMonTheThaoId.Value > 0)
                {
                    groups = await _bangDauService.GetAllAsync(giaiDauMonTheThaoId.Value);
                }
                else
                {
                    // Lấy tất cả bảng đấu thuộc các môn trong giải
                    var tournament = await _giaiDauService.GetByIdAsync(giaiDauId);
                    if (tournament != null)
                    {
                        var groupList = new List<BangDauDto>();
                        foreach (var m in tournament.MonTheThaos)
                        {
                            var gList = await _bangDauService.GetAllAsync(m.Id);
                            if (gList != null) groupList.AddRange(gList);
                        }
                        groups = groupList;
                    }
                }

                return Json(new
                {
                    success = true,
                    matches = matches.OrderBy(t => t.ThoiGianDuKien).ThenBy(t => t.SoTran).ToList(),
                    groups = groups.ToList()
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}
