using Dms.Application.DTOs;

namespace Dms.Application.Interfaces
{
    public interface IGoogleAuthService
    {
        Task<GoogleUserDto?> VerifyGoogleTokenAsync(string idToken);
    }
}
