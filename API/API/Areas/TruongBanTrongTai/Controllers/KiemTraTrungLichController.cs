using Dms.Application.DTOs;
using Dms.Application.Interfaces;
using Dms.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace API.Areas.TruongBanTrongTai.Controllers
{
    public class KiemTraTrungLichController : BaseTruongBanController
    {
        public KiemTraTrungLichController(
            ITruongBanTrongTaiService truongBanService,
            UserManager<ApplicationUser> userManager)
            : base(truongBanService, userManager)
        {
        }

        public async Task<IActionResult> Index(int? giaiDauId)
        {
            var managedTournaments = await GetManagedTournamentsAsync();
            var currentReferee = await GetCurrentRefereeAsync();
            var selectedGiaiDauId = GetSelectedTournamentId(giaiDauId, managedTournaments);

            ViewBag.ManagedTournaments = managedTournaments;
            ViewBag.CurrentReferee = currentReferee;
            ViewBag.SelectedGiaiDauId = selectedGiaiDauId;

            if (!selectedGiaiDauId.HasValue)
            {
                ViewBag.Message = "Chưa có giải đấu nào được chọn.";
                return View(new List<ScanConflictItemDto>());
            }

            var conflicts = await _truongBanService.ScanConflictsAsync(selectedGiaiDauId.Value);
            ViewBag.ConflictCount = conflicts.Count;

            return View(conflicts);
        }
    }
}
