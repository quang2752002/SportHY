using Dms.Application.DTOs;
using Dms.Application.Interfaces;
using Dms.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Areas.DonVi.Controllers
{
    public class LichThiDauController : BaseDonViController
    {
        private readonly ITranDauService _tranDauService;
        private readonly IGiaiDauService _giaiDauService;
        private readonly IDangKyThiDauService _dangKyThiDauService;

        public LichThiDauController(
            UserManager<ApplicationUser> userManager,
            IDonViService donViService,
            ITranDauService tranDauService,
            IGiaiDauService giaiDauService,
            IDangKyThiDauService dangKyThiDauService)
            : base(userManager, donViService)
        {
            _tranDauService = tranDauService;
            _giaiDauService = giaiDauService;
            _dangKyThiDauService = dangKyThiDauService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int? giaiDauId = null, string? filterScope = "my_unit")
        {
            var currentUnit = await GetCurrentDonViAsync();
            var tournaments = (await _giaiDauService.GetAllAsync())?.ToList() ?? new();
            ViewBag.Tournaments = tournaments;

            int? selectedGiaiDauId = giaiDauId;
            if (!selectedGiaiDauId.HasValue && tournaments.Count > 0)
            {
                selectedGiaiDauId = tournaments[0].Id;
            }
            ViewBag.SelectedGiaiDauId = selectedGiaiDauId;
            ViewBag.FilterScope = filterScope ?? "my_unit";

            var allMatches = (await _tranDauService.GetAllAsync(giaiDauId: selectedGiaiDauId))?.ToList() ?? new();

            // Lấy danh sách ID đăng ký của đơn vị trong giải này
            var unitRegIds = new HashSet<int>();
            if (currentUnit != null)
            {
                var regs = await _dangKyThiDauService.GetAllAsync(giaiDauId: selectedGiaiDauId, donViId: currentUnit.Id);
                foreach (var r in regs)
                {
                    unitRegIds.Add(r.Id);
                }
            }
            ViewBag.UnitRegIds = unitRegIds;

            // Kiểm tra xem trận đấu có đội của đoàn tham gia không
            bool MatchesUnit(TranDauDto m)
            {
                if (currentUnit == null) return false;
                if (!string.IsNullOrEmpty(m.DonViDoi1) && m.DonViDoi1.Contains(currentUnit.Ten, StringComparison.OrdinalIgnoreCase)) return true;
                if (!string.IsNullOrEmpty(m.DonViDoi2) && m.DonViDoi2.Contains(currentUnit.Ten, StringComparison.OrdinalIgnoreCase)) return true;
                if (m.Doi1DangKyId.HasValue && unitRegIds.Contains(m.Doi1DangKyId.Value)) return true;
                if (m.Doi2DangKyId.HasValue && unitRegIds.Contains(m.Doi2DangKyId.Value)) return true;
                if (m.ThanhPhanTranDaus.Any(tp => unitRegIds.Contains(tp.DangKyThiDauId) ||
                    (!string.IsNullOrEmpty(tp.TenDonVi) && tp.TenDonVi.Contains(currentUnit.Ten, StringComparison.OrdinalIgnoreCase))))
                {
                    return true;
                }
                return false;
            }

            var displayMatches = allMatches;
            if (filterScope == "my_unit")
            {
                displayMatches = allMatches.Where(MatchesUnit).ToList();
            }

            ViewBag.AllMatchesCount = allMatches.Count;
            ViewBag.UnitMatchesCount = allMatches.Count(MatchesUnit);

            return View(displayMatches);
        }
    }
}
