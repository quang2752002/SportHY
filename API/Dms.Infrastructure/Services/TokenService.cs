using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Dms.Application.DTOs;
using Dms.Application.Interfaces;
using Dms.Domain.Entities;
using Dms.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Dms.Infrastructure.Services
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole<int>> _roleManager;

        public TokenService(
            IConfiguration configuration,
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole<int>> roleManager)
        {
            _configuration = configuration;
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public string GenerateAccessToken(ApplicationUser user, IList<string> roles, IEnumerable<string>? permissions = null)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"] ?? "SuperSecretKeyForDienLanhProject_2026_Minimum_32Bytes!";
            var issuer    = jwtSettings["Issuer"] ?? "DmsBackend";
            var audience  = jwtSettings["Audience"] ?? "DmsFrontend";
            var expiryMin = int.TryParse(jwtSettings["AccessTokenExpiryMinutes"], out var exp) ? exp : 60;

            var key   = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub,   user.Id.ToString()),
                new Claim(ClaimTypes.NameIdentifier,     user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
                new Claim(JwtRegisteredClaimNames.Jti,   Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Name,               user.UserName ?? string.Empty),
                new Claim("username",                    user.UserName ?? string.Empty),
                new Claim("fullName",                    user.FullName),
            };

            if (user.DonViId.HasValue)
            {
                claims.Add(new Claim("donViId", user.DonViId.Value.ToString()));
            }

            if (user.TrongTaiId.HasValue)
            {
                claims.Add(new Claim("trongTaiId", user.TrongTaiId.Value.ToString()));
            }

            if (user.ThuKyId.HasValue)
            {
                claims.Add(new Claim("thuKyId", user.ThuKyId.Value.ToString()));
            }

            foreach (var role in roles)
                claims.Add(new Claim(ClaimTypes.Role, role));

            if (permissions != null)
            {
                foreach (var permission in permissions)
                    claims.Add(new Claim("permission", permission));
            }

            var token = new JwtSecurityToken(
                issuer:             issuer,
                audience:           audience,
                claims:             claims,
                expires:            DateTime.UtcNow.AddMinutes(expiryMin),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public string GenerateRefreshToken()
        {
            var randomBytes = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            return Convert.ToBase64String(randomBytes);
        }

        public async Task<AuthResponseDto> CreateAuthResponse(ApplicationUser user)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var permissionsList = new HashSet<string>();

            foreach (var roleName in roles)
            {
                var role = await _roleManager.FindByNameAsync(roleName);
                if (role != null)
                {
                    var roleClaims = await _roleManager.GetClaimsAsync(role);
                    foreach (var claim in roleClaims.Where(c => c.Type == "permission"))
                    {
                        permissionsList.Add(claim.Value);
                    }
                }
            }

            var permissions = permissionsList.ToList();
            var accessToken = GenerateAccessToken(user, roles, permissions);
            var refreshTokenStr = GenerateRefreshToken();

            // Thu hồi tất cả refresh token cũ còn active
            var oldTokens = await _context.RefreshTokens
                .Where(r => r.UserId == user.Id && r.Revoked == null)
                .ToListAsync();
            oldTokens.ForEach(t => t.Revoked = DateTime.UtcNow);

            var expiryDays = int.TryParse(_configuration["JwtSettings:RefreshTokenExpiryDays"], out var expD) ? expD : 7;

            // Lưu refresh token mới vào DB
            var refreshToken = new RefreshToken
            {
                Token   = refreshTokenStr,
                UserId  = user.Id,
                Created = DateTime.UtcNow,
                Expires = DateTime.UtcNow.AddDays(expiryDays),
            };
            _context.RefreshTokens.Add(refreshToken);
            await _context.SaveChangesAsync();

            var expiryMin = int.TryParse(_configuration["JwtSettings:AccessTokenExpiryMinutes"], out var expM) ? expM : 60;

            return new AuthResponseDto
            {
                AccessToken       = accessToken,
                RefreshToken      = refreshTokenStr,
                AccessTokenExpiry = DateTime.UtcNow.AddMinutes(expiryMin),
                UserId            = user.Id,
                Username          = user.UserName ?? string.Empty,
                FullName          = user.FullName,
                Role              = roles.FirstOrDefault() ?? "User",
                DonViId           = user.DonViId,
                TrongTaiId        = user.TrongTaiId,
                ThuKyId           = user.ThuKyId,
                Permissions       = permissions,
            };
        }

        public async Task<AuthResponseDto> RefreshAsync(string accessToken, string refreshToken)
        {
            // Lấy principal từ access token hết hạn (không kiểm tra expiry)
            var principal = GetPrincipalFromExpiredToken(accessToken);
            var userIdStr = principal?.FindFirstValue(ClaimTypes.NameIdentifier)
                          ?? principal?.FindFirstValue(JwtRegisteredClaimNames.Sub);

            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out var userId))
                throw new UnauthorizedAccessException("Access token không hợp lệ.");

            var user = await _userManager.FindByIdAsync(userIdStr);
            if (user == null)
                throw new UnauthorizedAccessException("Người dùng không tồn tại.");

            // Kiểm tra refresh token trong DB
            var storedToken = await _context.RefreshTokens
                .FirstOrDefaultAsync(r => r.Token == refreshToken && r.UserId == userId);

            if (storedToken == null || !storedToken.IsActive)
                throw new UnauthorizedAccessException("Refresh token không hợp lệ hoặc đã hết hạn.");

            // Thu hồi refresh token cũ
            storedToken.Revoked = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return await CreateAuthResponse(user);
        }

        public async Task RevokeAsync(int userId)
        {
            var tokens = await _context.RefreshTokens
                .Where(r => r.UserId == userId && r.Revoked == null)
                .ToListAsync();
            tokens.ForEach(t => t.Revoked = DateTime.UtcNow);
            await _context.SaveChangesAsync();
        }

        private ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secretKey   = jwtSettings["SecretKey"] ?? "SuperSecretKeyForDienLanhProject_2026_Minimum_32Bytes!";

            var tokenValidationParams = new TokenValidationParameters
            {
                ValidateAudience         = false,
                ValidateIssuer           = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                ValidateLifetime         = false, // Bỏ qua expiry khi refresh
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var principal    = tokenHandler.ValidateToken(token, tokenValidationParams, out var securityToken);

            if (securityToken is not JwtSecurityToken jwtSecurityToken ||
                !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                throw new SecurityTokenException("Token không hợp lệ.");

            return principal;
        }
    }
}
