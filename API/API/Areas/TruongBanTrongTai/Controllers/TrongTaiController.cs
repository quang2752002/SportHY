using Dms.Application.Interfaces;
using Dms.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TrongTaiEntity = Dms.Domain.Entities.TrongTai;

namespace API.Areas.TruongBanTrongTai.Controllers
{
    public class TrongTaiController : BaseTruongBanController
    {
        public TrongTaiController(ITruongBanTrongTaiService truongBanService, UserManager<ApplicationUser> userManager)
            : base(truongBanService, userManager)
        {
        }

        public async Task<IActionResult> Index(int? giaiDauId, string? keyword, string? capBac, bool? trangThai)
        {
            var managedTournaments = await GetManagedTournamentsAsync();
            var currentReferee = await GetCurrentRefereeAsync();
            var selectedGiaiDauId = GetSelectedTournamentId(giaiDauId, managedTournaments);

            ViewBag.ManagedTournaments = managedTournaments;
            ViewBag.CurrentReferee = currentReferee;
            ViewBag.SelectedGiaiDauId = selectedGiaiDauId;
            ViewBag.ManagedTournaments = managedTournaments;
            ViewBag.Keyword = keyword;
            ViewBag.CapBac = capBac;
            ViewBag.TrangThai = trangThai;

            if (!selectedGiaiDauId.HasValue && !IsAdminOrManager())
            {
                ViewBag.MatchCountMap = new Dictionary<int, int>();
                return View(new List<TrongTaiEntity>());
            }

            var result = await _truongBanService.GetRefereeListAsync(selectedGiaiDauId, keyword, capBac, trangThai);
            ViewBag.MatchCountMap = result.MatchCountMap;

            return View(result.Referees);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id, int? giaiDauId)
        {
            var managedTournaments = await GetManagedTournamentsAsync();
            ViewBag.ManagedTournaments = managedTournaments;

            int? effectiveGiaiDauId = giaiDauId;
            if (!IsAdminOrManager())
            {
                effectiveGiaiDauId = GetSelectedTournamentId(giaiDauId, managedTournaments);
                if (!effectiveGiaiDauId.HasValue) return Forbid();
            }

            var details = await _truongBanService.GetRefereeDetailsAsync(id, effectiveGiaiDauId);
            if (details == null)
            {
                return NotFound();
            }

            ViewBag.SelectedGiaiDauId = effectiveGiaiDauId;
            ViewBag.History = details.History;
            return View(details.Referee);
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrUpdate(TrongTaiEntity model)
        {
            if (!await CanManageAnyTournamentAsync()) return Forbid();

            var (success, message) = await _truongBanService.CreateOrUpdateRefereeAsync(model);
            return Json(new { success = success, message = message });
        }
    }
}
