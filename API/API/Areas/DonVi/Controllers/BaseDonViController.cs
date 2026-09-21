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

namespace API.Areas.DonVi.Controllers
{
    [Area("DonVi")]
    [Authorize(Roles = AppRoles.Delegation + "," + AppRoles.Admin + "," + AppRoles.Manager)]
    public abstract class BaseDonViController : Controller
    {
        protected readonly UserManager<ApplicationUser> _userManager;
        protected readonly IDonViService _donViService;

        public BaseDonViController(
            UserManager<ApplicationUser> userManager,
            IDonViService donViService)
        {
            _userManager = userManager;
            _donViService = donViService;
        }

        /// <summary>
        /// Xác định Đơn vị thi đấu hiện tại của người dùng (hoặc đơn vị được chọn bởi Admin/Manager)
        /// </summary>
        protected async Task<DonViDto?> GetCurrentDonViAsync(int? overrideDonViId = null)
        {
            var allDonVis = (await _donViService.GetAllAsync())?.ToList() ?? new();
            ViewBag.AllDonVis = allDonVis;
            bool canSwitch = User.IsInRole(AppRoles.Admin) || User.IsInRole(AppRoles.Manager);
            ViewBag.CanSwitchUnit = canSwitch;

            int? targetId = null;

            if (canSwitch && overrideDonViId.HasValue)
            {
                targetId = overrideDonViId.Value;
                Response.Cookies.Append("DonVi_SelectedId", targetId.Value.ToString(), new CookieOptions
                {
                    Expires = DateTimeOffset.UtcNow.AddDays(7),
                    HttpOnly = true,
                    IsEssential = true
                });
            }
            else if (canSwitch && Request.Cookies.TryGetValue("DonVi_SelectedId", out string? cookieVal) && int.TryParse(cookieVal, out int cId))
            {
                targetId = cId;
            }

            if (!targetId.HasValue)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user?.DonViId.HasValue == true)
                {
                    targetId = user.DonViId.Value;
                }
                else if (user != null)
                {
                    var match = allDonVis.FirstOrDefault(d =>
                        (!string.IsNullOrEmpty(user.FullName) && d.Ten.Contains(user.FullName, StringComparison.OrdinalIgnoreCase)) ||
                        (!string.IsNullOrEmpty(user.UserName) && d.Ma.Equals(user.UserName, StringComparison.OrdinalIgnoreCase)));

                    if (match != null)
                    {
                        targetId = match.Id;
                    }
                }
            }

            // Fallback nếu không xác định được
            DonViDto? current = null;
            if (targetId.HasValue)
            {
                current = allDonVis.FirstOrDefault(d => d.Id == targetId.Value);
            }

            if (current == null && allDonVis.Count > 0)
            {
                current = allDonVis[0];
            }

            ViewBag.CurrentDonVi = current;
            return current;
        }

        [HttpPost]
        public IActionResult SwitchDonVi(int donViId, string? returnUrl = null)
        {
            if (User.IsInRole(AppRoles.Admin) || User.IsInRole(AppRoles.Manager))
            {
                Response.Cookies.Append("DonVi_SelectedId", donViId.ToString(), new CookieOptions
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

            return RedirectToAction("Index", "Home", new { area = "DonVi" });
        }
    }
}
