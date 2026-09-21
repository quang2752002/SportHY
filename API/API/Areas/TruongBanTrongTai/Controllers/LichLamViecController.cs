using Dms.Application.DTOs;
using Dms.Application.Interfaces;
using Dms.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace API.Areas.TruongBanTrongTai.Controllers
{
    public class LichLamViecController : BaseTruongBanController
    {
        private readonly IMonTheThaoService _monTheThaoService;
        private readonly ITrongTaiService _refereeService;

        public LichLamViecController(
            ITruongBanTrongTaiService truongBanService,
            IMonTheThaoService monTheThaoService,
            ITrongTaiService refereeService,
            UserManager<ApplicationUser> userManager)
            : base(truongBanService, userManager)
        {
            _monTheThaoService = monTheThaoService;
            _refereeService = refereeService;
        }

        public async Task<IActionResult> Index(int? giaiDauId, int? trongTaiId, DateTime? date, int? monTheThaoId)
        {
            var managedTournaments = await GetManagedTournamentsAsync();
            var currentReferee = await GetCurrentRefereeAsync();
            var selectedGiaiDauId = GetSelectedTournamentId(giaiDauId, managedTournaments);

            ViewBag.ManagedTournaments = managedTournaments;
            ViewBag.CurrentReferee = currentReferee;
            ViewBag.SelectedGiaiDauId = selectedGiaiDauId;
            ViewBag.SelectedTrongTaiId = trongTaiId;
            ViewBag.SelectedDate = date?.ToString("yyyy-MM-dd");
            ViewBag.SelectedMonId = monTheThaoId;

            if (!selectedGiaiDauId.HasValue)
            {
                ViewBag.Message = "Chưa có giải đấu nào được chọn.";
                return View(new List<RefereeScheduleGroupDto>());
            }

            var allMons = await _monTheThaoService.GetAllAsync();
            ViewBag.SportList = allMons.OrderBy(m => m.Ten).ToList();

            var allReferees = await _refereeService.GetAllAsync();
            ViewBag.AllReferees = allReferees.OrderBy(t => t.HoTen).ToList();

            var scheduleList = await _truongBanService.GetRefereeSchedulesAsync(selectedGiaiDauId.Value, trongTaiId, date, monTheThaoId);

            return View(scheduleList);
        }
    }
}
