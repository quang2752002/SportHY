using Dms.Application.Common;
using Dms.Application.Interfaces;
using Dms.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace API.Areas.TruongBanTrongTai.Controllers
{
    [Area("TruongBanTrongTai")]
    [Authorize(Roles = AppRoles.Referee)]
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
            bool isAdminOrManager = IsAdminOrManager();
            var currentReferee = await GetCurrentRefereeAsync();

            return await _truongBanService.GetManagedTournamentsAsync(currentReferee?.Id, isAdminOrManager);
        }

        protected bool IsAdminOrManager()
        {
            return User.IsInRole(AppRoles.Admin) || User.IsInRole(AppRoles.Manager);
        }

        protected async Task<bool> CanManageTournamentAsync(int giaiDauId)
        {
            if (IsAdminOrManager()) return true;
            var managedTournaments = await GetManagedTournamentsAsync();
            return managedTournaments.Any(tournament => tournament.Id == giaiDauId);
        }

        protected async Task<bool> CanManageAnyTournamentAsync()
        {
            return IsAdminOrManager() || (await GetManagedTournamentsAsync()).Count > 0;
        }

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (IsAdminOrManager())
            {
                await next();
                return;
            }

            var managedTournaments = await GetManagedTournamentsAsync();
            ViewBag.ManagedTournaments = managedTournaments;
            ViewBag.HasManagedTournament = managedTournaments.Count > 0;

            // Cho phép trọng tài bình thường vào nếu được phân công làm Trưởng ban (TruongBanTrongTaiId) trong ít nhất 1 giải.
            // Không yêu cầu role HeadReferee — chỉ cần có giải được giao.
            if (managedTournaments.Count == 0)
            {
                context.Result = Forbid();
                return;
            }

            if (context.ActionArguments.TryGetValue("giaiDauId", out var requestedTournament) &&
                requestedTournament is int requestedTournamentId &&
                managedTournaments.All(tournament => tournament.Id != requestedTournamentId))
            {
                context.Result = Forbid();
                return;
            }

            await next();
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
