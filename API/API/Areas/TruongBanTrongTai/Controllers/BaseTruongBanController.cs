using Dms.Application.Common;
using Dms.Application.Interfaces;
using Dms.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace API.Areas.TruongBanTrongTai.Controllers
{
    [Area("TruongBanTrongTai")]
    [Authorize(Roles = "HeadReferee,Admin,Manager")]
    public abstract class BaseTruongBanController : Controller
    {
        protected readonly ITruongBanTrongTaiService _truongBanService;
        protected readonly UserManager<ApplicationUser> _userManager;

        public BaseTruongBanController(ITruongBanTrongTaiService truongBanService, UserManager<ApplicationUser> userManager)
        {
            _truongBanService = truongBanService;
            _userManager = userManager;
        }

        protected async Task<Dms.Domain.Entities.TrongTai?> GetCurrentRefereeAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return null;

            return await _truongBanService.GetRefereeByUserIdOrNameAsync(user.TrongTaiId, user.UserName, user.Email);
        }

        protected async Task<List<GiaiDau>> GetManagedTournamentsAsync()
        {
            bool isAdminOrManager = User.IsInRole(AppRoles.Admin) || User.IsInRole(AppRoles.Manager);
            var currentReferee = await GetCurrentRefereeAsync();

            return await _truongBanService.GetManagedTournamentsAsync(currentReferee?.Id, isAdminOrManager);
        }

        protected int? GetSelectedTournamentId(int? requestGiaiDauId, List<GiaiDau> managedTournaments)
        {
            if (requestGiaiDauId.HasValue && managedTournaments.Any(g => g.Id == requestGiaiDauId.Value))
            {
                Response.Cookies.Append("TruongBan_SelectedGiaiDauId", requestGiaiDauId.Value.ToString(), new Microsoft.AspNetCore.Http.CookieOptions
                {
                    Expires = DateTimeOffset.UtcNow.AddDays(7),
                    HttpOnly = true
                });
                return requestGiaiDauId.Value;
            }

            if (Request.Cookies.TryGetValue("TruongBan_SelectedGiaiDauId", out var cookieStr) && int.TryParse(cookieStr, out int cookieVal))
            {
                if (managedTournaments.Any(g => g.Id == cookieVal))
                {
                    return cookieVal;
                }
            }

            var first = managedTournaments.FirstOrDefault();
            if (first != null)
            {
                Response.Cookies.Append("TruongBan_SelectedGiaiDauId", first.Id.ToString(), new Microsoft.AspNetCore.Http.CookieOptions
                {
                    Expires = DateTimeOffset.UtcNow.AddDays(7),
                    HttpOnly = true
                });
                return first.Id;
            }

            return null;
        }
    }
}
