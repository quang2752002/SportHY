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
            ViewBag.Keyword = keyword;
            ViewBag.CapBac = capBac;
            ViewBag.TrangThai = trangThai;

            var result = await _truongBanService.GetRefereeListAsync(selectedGiaiDauId, keyword, capBac, trangThai);
            ViewBag.MatchCountMap = result.MatchCountMap;

            return View(result.Referees);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id, int? giaiDauId)
        {
            var details = await _truongBanService.GetRefereeDetailsAsync(id);
            if (details == null)
            {
                return NotFound();
            }

            ViewBag.History = details.History;
            return View(details.Referee);
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrUpdate(TrongTaiEntity model)
        {
            var (success, message) = await _truongBanService.CreateOrUpdateRefereeAsync(model);
            return Json(new { success = success, message = message });
        }
    }
}
