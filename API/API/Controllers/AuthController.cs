using System.Security.Claims;
using Dms.Application.DTOs;
using Dms.Application.Interfaces;
using Dms.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole<int>> _roleManager;
        private readonly ITokenService _tokenService;
        private readonly IGoogleAuthService _googleAuthService;
        private readonly IRecaptchaService _recaptchaService;

        public AuthController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole<int>> roleManager,
            ITokenService tokenService,
            IGoogleAuthService googleAuthService,
            IRecaptchaService recaptchaService)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _tokenService = tokenService;
            _googleAuthService = googleAuthService;
            _recaptchaService = recaptchaService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            var user = await _userManager.FindByNameAsync(request.Username);
            if (user == null || !await _userManager.CheckPasswordAsync(user, request.Password))
            {
                return Unauthorized("Tên đăng nhập hoặc mật khẩu không chính xác.");
            }

            var authResponse = await _tokenService.CreateAuthResponse(user);
            return Ok(authResponse);
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest("Tên đăng nhập và mật khẩu không được để trống.");
            }

            var existingUser = await _userManager.FindByNameAsync(request.Username);
            if (existingUser != null)
            {
                return BadRequest("Tên đăng nhập đã tồn tại trong hệ thống.");
            }

            if (!string.IsNullOrEmpty(request.Email))
            {
                var existingEmail = await _userManager.FindByEmailAsync(request.Email);
                if (existingEmail != null)
                {
                    return BadRequest("Email đã được sử dụng.");
                }
            }

            var user = new ApplicationUser
            {
                UserName = request.Username,
                Email = request.Email,
                FullName = string.IsNullOrWhiteSpace(request.FullName) ? request.Username : request.FullName,
                PhoneNumber = request.PhoneNumber,
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors.Select(e => e.Description));
            }

            // Gán Role mặc định là User
            if (await _roleManager.RoleExistsAsync("User"))
            {
                await _userManager.AddToRoleAsync(user, "User");
            }

            var authResponse = await _tokenService.CreateAuthResponse(user);
            return Ok(authResponse);
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetCurrentUser()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                           ?? User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized("Không thể xác thực thông tin tài khoản.");
            }

            var user = await _userManager.FindByIdAsync(userIdClaim);
            if (user == null)
            {
                return NotFound("Người dùng không tồn tại.");
            }

            var roles = await _userManager.GetRolesAsync(user);
            var permissionsList = new HashSet<string>();

            foreach (var roleName in roles)
            {
                var role = await _roleManager.FindByNameAsync(roleName);
                if (role != null)
                {
                    var claims = await _roleManager.GetClaimsAsync(role);
                    foreach (var claim in claims.Where(c => c.Type == "permission"))
                    {
                        permissionsList.Add(claim.Value);
                    }
                }
            }

            var userInfo = new UserInfoDto
            {
                Id = user.Id,
                Username = user.UserName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                FullName = user.FullName,
                PhoneNumber = user.PhoneNumber,
                Roles = roles.ToList(),
                Permissions = permissionsList.ToList()
            };

            return Ok(userInfo);
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                           ?? User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value;

            if (int.TryParse(userIdClaim, out var userId))
            {
                await _tokenService.RevokeAsync(userId);
            }

            return Ok(new { message = "Đăng xuất thành công." });
        }

        [HttpPost("google-login")]
        public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginRequestDto request)
        {
            var googleUser = await _googleAuthService.VerifyGoogleTokenAsync(request.IdToken);
            if (googleUser == null)
            {
                return Unauthorized("Google Token không hợp lệ.");
            }

            var user = await _userManager.FindByEmailAsync(googleUser.Email);
            if (user == null)
            {
                user = new ApplicationUser
                {
                    UserName = googleUser.Email,
                    Email = googleUser.Email,
                    FullName = googleUser.Name,
                    EmailConfirmed = true,
                    CreatedAt = DateTime.UtcNow
                };

                var createResult = await _userManager.CreateAsync(user);
                if (!createResult.Succeeded)
                {
                    return BadRequest("Không thể khởi tạo tài khoản từ Google.");
                }

                if (await _roleManager.RoleExistsAsync("User"))
                {
                    await _userManager.AddToRoleAsync(user, "User");
                }
            }

            var authResponse = await _tokenService.CreateAuthResponse(user);
            return Ok(authResponse);
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDto request)
        {
            try
            {
                var response = await _tokenService.RefreshAsync(request.AccessToken, request.RefreshToken);
                return Ok(response);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
        }
    }
}
