using Dms.Application.Common;
using Dms.Application.DTOs;
using Dms.Application.Interfaces;
using Dms.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace API.Areas.TrongTai.Controllers
{
    [Area("TrongTai")]
    [Authorize(Roles = AppRoles.Referee + "," + AppRoles.HeadReferee + "," + AppRoles.Secretary + "," + AppRoles.Admin + "," + AppRoles.Manager)]
    public abstract class BaseTrongTaiController : Controller
    {
        protected readonly UserManager<ApplicationUser> _userManager;
        protected readonly ITrongTaiService _trongTaiService;
        protected readonly ITranDauService _tranDauService;

        public BaseTrongTaiController(
            UserManager<ApplicationUser> userManager,
            ITrongTaiService trongTaiService,
            ITranDauService tranDauService)
        {
            _userManager = userManager;
            _trongTaiService = trongTaiService;
            _tranDauService = tranDauService;
        }

        /// <summary>
        /// Xác định Trọng tài / Thư ký hiện tại của người dùng (hoặc trọng tài được chọn bởi Admin/Manager)
        /// </summary>
        protected async Task<TrongTaiDto?> GetCurrentRefereeAsync(int? overrideRefereeId = null)
        {
            var allReferees = (await _trongTaiService.GetAllAsync())?.ToList() ?? new();
            ViewBag.AllReferees = allReferees;
            bool canSwitch = User.IsInRole(AppRoles.Admin) || User.IsInRole(AppRoles.Manager);
            ViewBag.CanSwitchReferee = canSwitch;

            int? targetId = null;

            if (canSwitch && overrideRefereeId.HasValue)
            {
                targetId = overrideRefereeId.Value;
                Response.Cookies.Append("TrongTai_SelectedId", targetId.Value.ToString(), new CookieOptions
                {
                    Expires = DateTimeOffset.UtcNow.AddDays(7),
                    HttpOnly = true,
                    IsEssential = true
                });
            }
            else if (canSwitch && Request.Cookies.TryGetValue("TrongTai_SelectedId", out string? cookieVal) && int.TryParse(cookieVal, out int cId))
            {
                targetId = cId;
            }

            if (!targetId.HasValue)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user?.TrongTaiId.HasValue == true)
                {
                    targetId = user.TrongTaiId.Value;
                }
                else if (user != null)
                {
                    var match = allReferees.FirstOrDefault(r =>
                        !string.IsNullOrEmpty(user.UserName) && string.Equals(r.Ma, user.UserName, StringComparison.OrdinalIgnoreCase));

                    if (match != null)
                    {
                        targetId = match.Id;
                    }
                }
            }

            TrongTaiDto? current = null;
            if (targetId.HasValue)
            {
                current = allReferees.FirstOrDefault(r => r.Id == targetId.Value);
            }

            ViewBag.CurrentReferee = current;
            return current;
        }

        /// <summary>
        /// Determines whether the current user may access a match. Referees must have an explicit assignment;
        /// administrators and managers can access all matches for support and oversight.
        /// </summary>
        /// <param name="match">The match whose access should be checked.</param>
        /// <returns>True when the user is elevated or assigned to the match; otherwise false.</returns>
        protected async Task<bool> CanAccessMatchAsync(TranDauDto? match)
        {
            if (match == null) return false;
            var currentReferee = await GetCurrentRefereeAsync();
            return await _tranDauService.CanAccessMatchAsync(match.Id, currentReferee?.Id, CanBrowseAllMatches());
        }

        /// <summary>
        /// Indicates whether the current user may browse every match in a tournament.
        /// </summary>
        /// <returns>True for administrators and managers; otherwise false.</returns>
        protected bool CanBrowseAllMatches()
        {
            return User.IsInRole(AppRoles.Admin) || User.IsInRole(AppRoles.Manager);
        }

        [HttpPost]
        public IActionResult SwitchReferee(int trongTaiId, string? returnUrl = null)
        {
            if (User.IsInRole(AppRoles.Admin) || User.IsInRole(AppRoles.Manager))
            {
                Response.Cookies.Append("TrongTai_SelectedId", trongTaiId.ToString(), new CookieOptions
                {
                    Expires = DateTimeOffset.UtcNow.AddDays(7),
                    HttpOnly = true,
                    IsEssential = true
                });
            }

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "Home", new { area = "TrongTai" });
        }
    }
}
