using Dms.Application.Interfaces;
using Dms.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace API.Areas.TruongBanTrongTai.Controllers
{
    public class HomeController : BaseTruongBanController
    {
        public HomeController(ITruongBanTrongTaiService truongBanService, UserManager<ApplicationUser> userManager)
            : base(truongBanService, userManager)
        {
        }

        [HttpGet]
        public async Task<IActionResult> Index(int? giaiDauId)
        {
            var managedTournaments = await GetManagedTournamentsAsync();
            var currentReferee = await GetCurrentRefereeAsync();
            var selectedGiaiDauId = GetSelectedTournamentId(giaiDauId, managedTournaments);

            ViewBag.ManagedTournaments = managedTournaments;
            ViewBag.CurrentReferee = currentReferee;
            ViewBag.SelectedGiaiDauId = selectedGiaiDauId;

            return View();
        }

        /// <summary>
        /// API endpoint AJAX lấy thông tin Dashboard tổng quan cho Trưởng ban trọng tài
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetDashboardData(int? giaiDauId)
        {
            var managedTournaments = await GetManagedTournamentsAsync();
            var selectedGiaiDauId = GetSelectedTournamentId(giaiDauId, managedTournaments);

            if (!selectedGiaiDauId.HasValue)
            {
                return Json(new
                {
                    success = false,
                    message = "Bạn chưa được phân công làm Trưởng ban trọng tài cho giải đấu nào."
                });
            }

            var dashboard = await _truongBanService.GetDashboardAsync(selectedGiaiDauId.Value);

            return Json(new
            {
                success = true,
                data = dashboard
            });
        }
    }
}
