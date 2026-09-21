using Dms.Application.DTOs;
using Dms.Application.Interfaces;
using Dms.Domain.Entities;
using Dms.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace API.Areas.DonVi.Controllers
{
    public class HomeController : BaseDonViController
    {
        private readonly IVanDongVienService _vanDongVienService;
        private readonly IDangKyThiDauService _dangKyThiDauService;
        private readonly IGiaiDauService _giaiDauService;
        private readonly ITranDauService _tranDauService;

        public HomeController(
            UserManager<ApplicationUser> userManager,
            IDonViService donViService,
            IVanDongVienService vanDongVienService,
            IDangKyThiDauService dangKyThiDauService,
            IGiaiDauService giaiDauService,
            ITranDauService tranDauService)
            : base(userManager, donViService)
        {
            _vanDongVienService = vanDongVienService;
            _dangKyThiDauService = dangKyThiDauService;
            _giaiDauService = giaiDauService;
            _tranDauService = tranDauService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var currentDonVi = await GetCurrentDonViAsync();
            int? donViId = currentDonVi?.Id;

            var allVdvs = (await _vanDongVienService.GetAllAsync(donViId))?.ToList() ?? new();
            var allRegs = (await _dangKyThiDauService.GetAllAsync(donViId: donViId))?.ToList() ?? new();
            var allTournaments = (await _giaiDauService.GetAllAsync())?.ToList() ?? new();

            ViewBag.TotalVdvs = allVdvs.Count;
            ViewBag.ApprovedRegs = allRegs.Count(r => r.TrangThai == "DaDuyet");
            ViewBag.PendingRegs = allRegs.Count(r => r.TrangThai != "DaDuyet" && r.TrangThai != "TuChoi");
            ViewBag.ActiveTournamentsCount = allTournaments.Count(t => t.TrangThai == TrangThaiGiaiDau.SapDienRa || t.TrangThai == TrangThaiGiaiDau.DangDienRa);

            ViewBag.RecentAthletes = allVdvs.OrderByDescending(v => v.Id).Take(5).ToList();
            ViewBag.AvailableTournaments = allTournaments
                .Where(t => t.TrangThai == TrangThaiGiaiDau.SapDienRa || t.TrangThai == TrangThaiGiaiDau.DangDienRa)
                .Take(4)
                .ToList();

            return View(allVdvs);
        }

        [HttpPost]
        public async Task<IActionResult> QuickAddAthlete([FromBody] CreateUpdateVanDongVienDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.HoTen))
            {
                return Json(new { success = false, message = "Vui lòng nhập họ và tên vận động viên." });
            }

            var currentDonVi = await GetCurrentDonViAsync();
            if (currentDonVi == null)
            {
                return Json(new { success = false, message = "Chưa xác định được đơn vị quản lý." });
            }

            dto.DonViId = currentDonVi.Id;
            if (string.IsNullOrWhiteSpace(dto.Ma))
            {
                dto.Ma = $"VDV-{currentDonVi.Ma}-{DateTime.Now:fffss}";
            }

            var username = User.FindFirst(ClaimTypes.Name)?.Value ?? "DonVi";

            try
            {
                var created = await _vanDongVienService.CreateAsync(dto, username);
                return Json(new { success = true, message = $"Đã thêm vận động viên [{created.HoTen}] thành công!", data = created });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}
