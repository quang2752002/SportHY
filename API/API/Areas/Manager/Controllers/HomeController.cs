using Dms.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace API.Areas.Manager.Controllers
{
    public class HomeController : BaseManagerController
    {
        private readonly IGiaiDauService _giaiDauService;
        private readonly IDangKyThiDauService _dangKyService;
        private readonly ITranDauService _tranDauService;

        public HomeController(
            IGiaiDauService giaiDauService,
            IDangKyThiDauService dangKyService,
            ITranDauService tranDauService)
        {
            _giaiDauService = giaiDauService;
            _dangKyService = dangKyService;
            _tranDauService = tranDauService;
        }

        public async Task<IActionResult> Index()
        {
            var giaiDaus = (await _giaiDauService.GetAllAsync()).ToList();
            var dangKys = (await _dangKyService.GetAllAsync()).ToList();
            var tranDaus = (await _tranDauService.GetAllAsync()).ToList();

            var today = DateTime.Today;
            ViewBag.TotalGiaiDau = giaiDaus.Count;
            ViewBag.ActiveGiaiDau = giaiDaus.Count(g => g.TrangThai == Dms.Domain.Enums.TrangThaiGiaiDau.DangDienRa);
            ViewBag.TotalDangKy = dangKys.Count;
            ViewBag.TotalTranDau = tranDaus.Count;
            ViewBag.TodayTranDau = tranDaus.Count(t => t.ThoiGianBatDau.HasValue && t.ThoiGianBatDau.Value.Date == today);
            ViewBag.FinishedTranDau = tranDaus.Count(t => t.TrangThai == "KetThuc");

            ViewBag.RecentGiaiDaus = giaiDaus.Take(6).ToList();
            ViewBag.RecentDangKys = dangKys.OrderByDescending(d => d.NgayDangKy).Take(6).ToList();

            return View();
        }
    }
}
