using GayDetectorBot.WebApi.Models.Auth;

namespace GayDetectorBot.WebApi.Services.Auth;

public interface IAuthService
{
    Task<AuthResponse?> AuthAsync(AuthRequest request);
}