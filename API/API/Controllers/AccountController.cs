using Dms.Application.Common;
using Dms.Domain.Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace API.Controllers
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        public bool RememberMe { get; set; } = true;

        public string? ReturnUrl { get; set; }
    }

    [AllowAnonymous]
    public class AccountController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public AccountController(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Login(string? returnUrl = null, bool switchAccount = false)
        {
            if (switchAccount)
            {
                await _signInManager.SignOutAsync();
                ClearAllCustomCookies();
                var switchModel = new LoginViewModel { ReturnUrl = returnUrl };
                return View(switchModel);
            }

            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                // Nếu ReturnUrl không phù hợp với quyền của tài khoản hiện tại,
                // Cho phép hiển thị form đăng nhập để người dùng có thể đổi sang tài khoản có quyền phù hợp
                if (!string.IsNullOrEmpty(returnUrl))
                {
                    bool isAdminUrl = returnUrl.StartsWith("/Admin", StringComparison.OrdinalIgnoreCase);
                    bool isManagerUrl = returnUrl.StartsWith("/Manager", StringComparison.OrdinalIgnoreCase);
                    bool isTrongTaiUrl = returnUrl.StartsWith("/TrongTai", StringComparison.OrdinalIgnoreCase);
                    bool isTruongBanUrl = returnUrl.StartsWith("/TruongBanTrongTai", StringComparison.OrdinalIgnoreCase);
                    bool isDonViUrl = returnUrl.StartsWith("/DonVi", StringComparison.OrdinalIgnoreCase);

                    bool userCanAccess = (isAdminUrl && User.IsInRole(AppRoles.Admin))
                        || (isManagerUrl && (User.IsInRole(AppRoles.Manager) || User.IsInRole(AppRoles.Admin)))
                        || (isTrongTaiUrl && (User.IsInRole(AppRoles.Referee) || User.IsInRole(AppRoles.HeadReferee) || User.IsInRole(AppRoles.Admin) || User.IsInRole(AppRoles.Manager)))
                        || (isTruongBanUrl && (User.IsInRole(AppRoles.HeadReferee) || User.IsInRole(AppRoles.Admin) || User.IsInRole(AppRoles.Manager)))
                        || (isDonViUrl && (User.IsInRole(AppRoles.Delegation) || User.IsInRole(AppRoles.Admin) || User.IsInRole(AppRoles.Manager)));

                    if (!userCanAccess)
                    {
                        var model = new LoginViewModel { ReturnUrl = returnUrl };
                        return View(model);
                    }
                }

                return RedirectToLocal(returnUrl);
            }

            var loginModel = new LoginViewModel { ReturnUrl = returnUrl };
            return View(loginModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Hủy phiên đăng nhập cũ và xóa cookie phiên cũ trước khi đăng nhập tài khoản mới
            await _signInManager.SignOutAsync();
            ClearAllCustomCookies();

            var result = await _signInManager.PasswordSignInAsync(
                model.Username,
                model.Password,
                model.RememberMe,
                lockoutOnFailure: false);

            if (result.Succeeded)
            {
                var user = await _userManager.FindByNameAsync(model.Username);
                if (user != null)
                {
                    var roles = await _userManager.GetRolesAsync(user);

                    if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                    {
                        var safeUrl = ResolveSafeReturnUrl(model.ReturnUrl, roles);
                        if (!string.IsNullOrEmpty(safeUrl))
                        {
                            return Redirect(safeUrl);
                        }
                    }

                    if (roles.Contains(AppRoles.Admin))
                    {
                        return RedirectToAction("Index", "GiaiDau", new { area = "Admin" });
                    }
                    if (roles.Contains(AppRoles.Manager))
                    {
                        return RedirectToAction("Index", "GiaiDau", new { area = "Manager" });
                    }
                    if (roles.Contains(AppRoles.HeadReferee))
                    {
                        return RedirectToAction("Index", "Home", new { area = "TruongBanTrongTai" });
                    }
                    if (roles.Contains(AppRoles.Referee))
                    {
                        return RedirectToAction("Index", "Home", new { area = "TrongTai" });
                    }
                    if (roles.Contains(AppRoles.Delegation))
                    {
                        return RedirectToAction("Index", "Home", new { area = "DonVi" });
                    }
                    if (roles.Contains(AppRoles.SportCoordinator))
                    {
                        return RedirectToAction("Index", "Home", new { area = "DieuHanhMon" });
                    }
                    if (roles.Contains(AppRoles.Secretary))
                    {
                        return RedirectToAction("Index", "Home", new { area = "ThuKy" });
                    }
                }

                return RedirectToAction("Index", "GiaiDau", new { area = "Manager" });
            }

            ModelState.AddModelError(string.Empty, "Tên đăng nhập hoặc mật khẩu không chính xác.");
            return View(model);
        }

        [HttpPost]
        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            ClearAllCustomCookies();
            return RedirectToAction(nameof(Login));
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        private IActionResult RedirectToLocal(string? returnUrl)
        {
            var roles = new List<string>();
            if (User.IsInRole(AppRoles.Admin)) roles.Add(AppRoles.Admin);
            if (User.IsInRole(AppRoles.Manager)) roles.Add(AppRoles.Manager);
            if (User.IsInRole(AppRoles.HeadReferee)) roles.Add(AppRoles.HeadReferee);
            if (User.IsInRole(AppRoles.Referee)) roles.Add(AppRoles.Referee);
            if (User.IsInRole(AppRoles.Delegation)) roles.Add(AppRoles.Delegation);
            if (User.IsInRole(AppRoles.Secretary)) roles.Add(AppRoles.Secretary);
            if (User.IsInRole(AppRoles.SportCoordinator)) roles.Add(AppRoles.SportCoordinator);

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                var safeUrl = ResolveSafeReturnUrl(returnUrl, roles);
                if (!string.IsNullOrEmpty(safeUrl))
                {
                    return Redirect(safeUrl);
                }
            }

            if (roles.Contains(AppRoles.Admin))
            {
                return RedirectToAction("Index", "GiaiDau", new { area = "Admin" });
            }
            if (roles.Contains(AppRoles.Manager))
            {
                return RedirectToAction("Index", "GiaiDau", new { area = "Manager" });
            }
            if (roles.Contains(AppRoles.HeadReferee))
            {
                return RedirectToAction("Index", "Home", new { area = "TruongBanTrongTai" });
            }
            if (roles.Contains(AppRoles.Referee))
            {
                return RedirectToAction("Index", "Home", new { area = "TrongTai" });
            }
            if (roles.Contains(AppRoles.Delegation))
            {
                return RedirectToAction("Index", "Home", new { area = "DonVi" });
            }
            if (roles.Contains(AppRoles.Secretary))
            {
                return RedirectToAction("Index", "Home", new { area = "ThuKy" });
            }
            if (roles.Contains(AppRoles.SportCoordinator))
            {
                return RedirectToAction("Index", "Home", new { area = "DieuHanhMon" });
            }

            return RedirectToAction("Index", "GiaiDau", new { area = "Manager" });
        }

        /// <summary>
        /// Thẩm định và chuẩn hóa ReturnUrl an toàn dựa trên tập quyền thực tế của tài khoản,
        /// tránh trường hợp redirect vào link trận đấu/biên bản cũ của tài khoản trước gây lỗi 403 / Forbid.
        /// </summary>
        private string? ResolveSafeReturnUrl(string returnUrl, IList<string> roles)
        {
            if (string.IsNullOrWhiteSpace(returnUrl)) return null;

            // Nếu ReturnUrl chứa thông số trận đấu cụ thể (tranDauId), không nên redirect trực tiếp
            // vì tài khoản trọng tài mới có thể không được phân công trận này. Thay vào đó chuyển về Dashboard.
            if (returnUrl.Contains("tranDauId=", StringComparison.OrdinalIgnoreCase))
            {
                if (roles.Contains(AppRoles.Referee)) return "/TrongTai/Home";
                if (roles.Contains(AppRoles.HeadReferee)) return "/TruongBanTrongTai/Home";
            }

            if (returnUrl.StartsWith("/Admin", StringComparison.OrdinalIgnoreCase) && roles.Contains(AppRoles.Admin))
                return returnUrl;

            if (returnUrl.StartsWith("/Manager", StringComparison.OrdinalIgnoreCase) && (roles.Contains(AppRoles.Manager) || roles.Contains(AppRoles.Admin)))
                return returnUrl;

            if (returnUrl.StartsWith("/TrongTai", StringComparison.OrdinalIgnoreCase) && (roles.Contains(AppRoles.Referee) || roles.Contains(AppRoles.HeadReferee) || roles.Contains(AppRoles.Admin) || roles.Contains(AppRoles.Manager)))
                return returnUrl;

            if (returnUrl.StartsWith("/TruongBanTrongTai", StringComparison.OrdinalIgnoreCase) && (roles.Contains(AppRoles.HeadReferee) || roles.Contains(AppRoles.Admin) || roles.Contains(AppRoles.Manager)))
                return returnUrl;

            if (returnUrl.StartsWith("/DonVi", StringComparison.OrdinalIgnoreCase) && (roles.Contains(AppRoles.Delegation) || roles.Contains(AppRoles.Admin) || roles.Contains(AppRoles.Manager)))
                return returnUrl;

            if (returnUrl.StartsWith("/DieuHanhMon", StringComparison.OrdinalIgnoreCase) && (roles.Contains(AppRoles.SportCoordinator) || roles.Contains(AppRoles.Admin) || roles.Contains(AppRoles.Manager)))
                return returnUrl;

            if (returnUrl.StartsWith("/ThuKy", StringComparison.OrdinalIgnoreCase) && (roles.Contains(AppRoles.Secretary) || roles.Contains(AppRoles.Admin) || roles.Contains(AppRoles.Manager)))
                return returnUrl;

            return null;
        }

        /// <summary>
        /// Xóa sạch các cookie lưu trạng thái lựa chọn riêng biệt của tài khoản cũ
        /// </summary>
        private void ClearAllCustomCookies()
        {
            Response.Cookies.Delete("DonVi_SelectedId");
            Response.Cookies.Delete("TrongTai_SelectedId");
            Response.Cookies.Delete("TruongBan_SelectedGiaiDauId");
            Response.Cookies.Delete("DieuHanh_SelectedGiaiDauId");
            Response.Cookies.Delete("DieuHanh_SelectedDanhMucId");
            Response.Cookies.Delete("ThuKy_SelectedGiaiDauId");
        }
    }
}
