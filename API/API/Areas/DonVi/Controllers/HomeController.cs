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

            // Dùng cùng truy vấn phân trang với màn Hồ sơ VĐV để dashboard hiển thị đúng
            // toàn bộ hồ sơ (bao gồm cả VĐV tạm ngừng), không chỉ VĐV đang hoạt động.
            var athletePage = await _vanDongVienService.GetPagedAsync(
                pageIndex: 1,
                pageSize: 5,
                donViId: donViId);
            var recentVdvs = athletePage.Items.ToList();
            var allRegs = (await _dangKyThiDauService.GetAllAsync(donViId: donViId))?.ToList() ?? new();
            var allTournaments = (await _giaiDauService.GetAllAsync())?.ToList() ?? new();

            ViewBag.TotalVdvs = athletePage.TotalCount;
            ViewBag.ApprovedRegs = allRegs.Count(r => r.TrangThai == "DaDuyet");
            ViewBag.PendingRegs = allRegs.Count(r => r.TrangThai != "DaDuyet" && r.TrangThai != "TuChoi");
            ViewBag.ActiveTournamentsCount = allTournaments.Count(t => t.TrangThai == TrangThaiGiaiDau.SapDienRa || t.TrangThai == TrangThaiGiaiDau.DangDienRa);

            ViewBag.RecentAthletes = recentVdvs;
            ViewBag.AvailableTournaments = allTournaments
                .Where(t => t.TrangThai == TrangThaiGiaiDau.SapDienRa || t.TrangThai == TrangThaiGiaiDau.DangDienRa)
                .Take(4)
                .ToList();

            return View(recentVdvs);
        }

        [HttpPost]
        public async Task<IActionResult> QuickAddAthlete([FromBody] CreateUpdateVanDongVienDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.HoTen))
            {
                return Json(new { success = false, message = "Vui lòng nhập họ và tên vận động viên." });
            }

            if (!ModelState.IsValid)
            {
                var validationMessage = ModelState.Values
                    .SelectMany(entry => entry.Errors)
                    .Select(error => error.ErrorMessage)
                    .FirstOrDefault(message => !string.IsNullOrWhiteSpace(message));
                return Json(new { success = false, message = validationMessage ?? "Thông tin vận động viên chưa hợp lệ." });
            }

            var currentDonVi = await GetCurrentDonViAsync();
            if (currentDonVi == null)
            {
                return Json(new { success = false, message = "Chưa xác định được đơn vị quản lý." });
            }

            dto.DonViId = currentDonVi.Id;
            if (string.IsNullOrWhiteSpace(dto.Ma))
            {
                var unitCode = string.IsNullOrWhiteSpace(currentDonVi.Ma) ? "DV" : currentDonVi.Ma.Trim();
                if (unitCode.Length > 13)
                {
                    unitCode = unitCode.Substring(0, 13);
                }

                dto.Ma = $"VDV-{unitCode}-{Guid.NewGuid():N}";
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
