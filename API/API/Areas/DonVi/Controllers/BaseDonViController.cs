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
    [Authorize(Roles = AppRoles.Delegation)]
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

            // 1. Lấy thông tin user hiện tại đang đăng nhập
            var user = await _userManager.GetUserAsync(User);
            if (user == null && !string.IsNullOrEmpty(User.Identity?.Name))
            {
                user = await _userManager.FindByNameAsync(User.Identity.Name);
            }

            int? targetId = null;

            // 2. Nếu là tài khoản Đoàn/Đơn vị (không có quyền switch), ưu tiên tuyệt đối DonViId của tài khoản
            if (!canSwitch)
            {
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
            else
            {
                // Với Admin / Manager: cho phép chuyển đơn vị thông qua tham số hoặc Cookie
                if (overrideDonViId.HasValue)
                {
                    targetId = overrideDonViId.Value;
                    Response.Cookies.Append("DonVi_SelectedId", targetId.Value.ToString(), new CookieOptions
                    {
                        Expires = DateTimeOffset.UtcNow.AddDays(7),
                        HttpOnly = true,
                        IsEssential = true
                    });
                }
                else if (Request.Cookies.TryGetValue("DonVi_SelectedId", out string? cookieVal) && int.TryParse(cookieVal, out int cId))
                {
                    targetId = cId;
                }
                else if (user?.DonViId.HasValue == true)
                {
                    targetId = user.DonViId.Value;
                }
            }

            // 3. Tìm đơn vị trong danh sách
            DonViDto? current = null;
            if (targetId.HasValue)
            {
                current = allDonVis.FirstOrDefault(d => d.Id == targetId.Value);
                if (current == null)
                {
                    current = await _donViService.GetByIdAsync(targetId.Value);
                }
            }

            // Fallback chỉ dành cho Admin/Manager khi chưa có đơn vị nào được chọn
            if (current == null && canSwitch && allDonVis.Count > 0)
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
