using Dms.Application.Common;
using Dms.Application.DTOs;
using Dms.Application.Interfaces;
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

    public class ChangePasswordViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập mật khẩu hiện tại.")]
        [DataType(DataType.Password)]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu mới.")]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng xác nhận mật khẩu mới.")]
        [DataType(DataType.Password)]
        [Compare(nameof(NewPassword), ErrorMessage = "Mật khẩu xác nhận không khớp.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public class UpdateAccountPhoneNumberViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập số điện thoại.")]
        [StringLength(32, ErrorMessage = "Số điện thoại không được dài quá 32 ký tự.")]
        [Phone(ErrorMessage = "Số điện thoại không đúng định dạng.")]
        public string? PhoneNumber { get; set; }
    }

    public class AccountProfilePageViewModel
    {
        public AccountProfileDto Profile { get; set; } = new();
        public ChangePasswordViewModel PasswordChange { get; set; } = new();
        public UpdateAccountPhoneNumberViewModel PhoneNumberChange { get; set; } = new();
    }

    public class AccountController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IAccountProfileService _accountProfileService;

        public AccountController(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            IAccountProfileService accountProfileService)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _accountProfileService = accountProfileService;
        }

        [HttpGet]
        [AllowAnonymous]
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
        [AllowAnonymous]
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
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var model = await BuildProfilePageAsync(user);
            return model == null ? NotFound() : View(model);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdatePhoneNumber(
            [Bind(Prefix = "PhoneNumberChange")] UpdateAccountPhoneNumberViewModel phoneModel)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            if (!ModelState.IsValid)
            {
                var invalidModel = await BuildProfilePageAsync(user);
                if (invalidModel == null) return NotFound();
                invalidModel.PhoneNumberChange = phoneModel;
                return View("Profile", invalidModel);
            }

            var updateResult = await _accountProfileService.UpdatePhoneNumberAsync(user.Id, phoneModel.PhoneNumber);
            if (!updateResult.success)
            {
                ModelState.AddModelError("PhoneNumberChange.PhoneNumber", updateResult.message);
                var failedModel = await BuildProfilePageAsync(user);
                if (failedModel == null) return NotFound();
                failedModel.PhoneNumberChange = phoneModel;
                return View("Profile", failedModel);
            }

            TempData["ProfileSuccess"] = updateResult.message;
            return RedirectToAction(nameof(Profile));
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword([Bind(Prefix = "PasswordChange")] ChangePasswordViewModel passwordModel)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            if (!ModelState.IsValid)
            {
                var invalidModel = await BuildProfilePageAsync(user);
                if (invalidModel == null) return NotFound();
                invalidModel.PasswordChange = passwordModel;
                return View("Profile", invalidModel);
            }

            var changeResult = await _userManager.ChangePasswordAsync(
                user,
                passwordModel.CurrentPassword,
                passwordModel.NewPassword);

            if (!changeResult.Succeeded)
            {
                foreach (var error in changeResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, GetVietnamesePasswordError(error));
                }

                var failedModel = await BuildProfilePageAsync(user);
                if (failedModel == null) return NotFound();
                failedModel.PasswordChange = passwordModel;
                return View("Profile", failedModel);
            }

            await _signInManager.RefreshSignInAsync(user);
            TempData["ProfileSuccess"] = "Đổi mật khẩu thành công.";
            return RedirectToAction(nameof(Profile));
        }

        private async Task<AccountProfilePageViewModel?> BuildProfilePageAsync(ApplicationUser user)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var profile = await _accountProfileService.GetProfileAsync(user.Id, roles.ToList());
            return profile == null
                ? null
                : new AccountProfilePageViewModel
                {
                    Profile = profile,
                    PhoneNumberChange = new UpdateAccountPhoneNumberViewModel { PhoneNumber = profile.PhoneNumber }
                };
        }

        private static string GetVietnamesePasswordError(IdentityError error)
        {
            return error.Code switch
            {
                "PasswordMismatch" => "Mật khẩu hiện tại không chính xác.",
                "PasswordTooShort" => "Mật khẩu mới chưa đạt độ dài tối thiểu theo quy định.",
                "PasswordRequiresDigit" => "Mật khẩu mới phải có ít nhất một chữ số.",
                "PasswordRequiresLower" => "Mật khẩu mới phải có ít nhất một chữ cái viết thường.",
                "PasswordRequiresUpper" => "Mật khẩu mới phải có ít nhất một chữ cái viết hoa.",
                "PasswordRequiresNonAlphanumeric" => "Mật khẩu mới phải có ít nhất một ký tự đặc biệt.",
                "PasswordRequiresUniqueChars" => "Mật khẩu mới cần có thêm các ký tự khác nhau.",
                "InvalidToken" => "Thông tin xác thực không hợp lệ hoặc đã hết hạn. Vui lòng thử lại.",
                _ => "Không thể đổi mật khẩu. Vui lòng kiểm tra lại thông tin và thử lại."
            };
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
