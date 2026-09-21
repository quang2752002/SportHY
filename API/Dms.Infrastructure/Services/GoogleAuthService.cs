using Dms.Application.DTOs;
using Dms.Application.Interfaces;
using Google.Apis.Auth;
using Microsoft.Extensions.Configuration;

namespace Dms.Infrastructure.Services
{
    public class GoogleAuthService : IGoogleAuthService
    {
        private readonly IConfiguration _configuration;

        public GoogleAuthService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<GoogleUserDto?> VerifyGoogleTokenAsync(string idToken)
        {
            try
            {
                var settings = new GoogleJsonWebSignature.ValidationSettings()
                {
                    Audience = new List<string>() { _configuration["Authentication:Google:ClientId"]! }
                };

                var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, settings);
                if (payload == null) return null;

                return new GoogleUserDto
                {
                    Email = payload.Email,
                    Name = payload.Name,
                    FirstName = payload.GivenName,
                    LastName = payload.FamilyName
                };
            }
            catch
            {
                // Xử lý lỗi token không hợp lệ hoặc hết hạn
                return null;
            }
        }
    }
}
