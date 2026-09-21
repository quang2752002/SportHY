using Dms.Application.Common;
using Dms.Domain.Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                // Nếu ReturnUrl yêu cầu quyền Admin nhưng tài khoản hiện tại không phải Admin
                // Cho phép hiển thị form đăng nhập để người dùng có thể chuyển sang tài khoản Admin
                if (!string.IsNullOrEmpty(returnUrl) && returnUrl.StartsWith("/Admin", StringComparison.OrdinalIgnoreCase) && !User.IsInRole(AppRoles.Admin))
                {
                    var model = new LoginViewModel { ReturnUrl = returnUrl };
                    return View(model);
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
                        bool isAdminUrl = model.ReturnUrl.StartsWith("/Admin", StringComparison.OrdinalIgnoreCase);
                        if (isAdminUrl && !roles.Contains(AppRoles.Admin))
                        {
                            if (roles.Contains(AppRoles.Manager))
                            {
                                return RedirectToAction("Index", "GiaiDau", new { area = "Manager" });
                            }
                        }
                        else
                        {
                            return Redirect(model.ReturnUrl);
                        }
                    }

                    if (roles.Contains(AppRoles.Manager))
                    {
                        return RedirectToAction("Index", "GiaiDau", new { area = "Manager" });
                    }
                    if (roles.Contains(AppRoles.Admin))
                    {
                        return RedirectToAction("Index", "GiaiDau", new { area = "Admin" });
                    }
                    if (roles.Contains(AppRoles.Delegation))
                    {
                        return RedirectToAction("Index", "Home", new { area = "DonVi" });
                    }
                    if (roles.Contains(AppRoles.HeadReferee))
                    {
                        return RedirectToAction("Index", "Home", new { area = "TruongBanTrongTai" });
                    }
                    if (roles.Contains(AppRoles.SportCoordinator))
                    {
                        return RedirectToAction("Index", "Home", new { area = "DieuHanhMon" });
                    }
                    if (roles.Contains(AppRoles.Secretary))
                    {
                        return RedirectToAction("Index", "Home", new { area = "ThuKy" });
                    }
                    if (roles.Contains(AppRoles.Referee))
                    {
                        return RedirectToAction("Index", "Home", new { area = "TrongTai" });
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
            return RedirectToAction(nameof(Login));
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        private IActionResult RedirectToLocal(string? returnUrl)
        {
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                if (returnUrl.StartsWith("/Admin", StringComparison.OrdinalIgnoreCase) && !User.IsInRole(AppRoles.Admin))
                {
                    if (User.IsInRole(AppRoles.Manager))
                    {
                        return RedirectToAction("Index", "GiaiDau", new { area = "Manager" });
                    }
                }
                else
                {
                    return Redirect(returnUrl);
                }
            }

            if (User.IsInRole(AppRoles.Manager))
            {
                return RedirectToAction("Index", "GiaiDau", new { area = "Manager" });
            }
            if (User.IsInRole(AppRoles.Admin))
            {
                return RedirectToAction("Index", "GiaiDau", new { area = "Admin" });
            }
            if (User.IsInRole(AppRoles.Delegation))
            {
                return RedirectToAction("Index", "Home", new { area = "DonVi" });
            }
            if (User.IsInRole(AppRoles.Secretary))
            {
                return RedirectToAction("Index", "Home", new { area = "ThuKy" });
            }
            if (User.IsInRole(AppRoles.Referee) || User.IsInRole(AppRoles.HeadReferee))
            {
                return RedirectToAction("Index", "Home", new { area = "TrongTai" });
            }

            return RedirectToAction("Index", "GiaiDau", new { area = "Manager" });
        }
    }
}
