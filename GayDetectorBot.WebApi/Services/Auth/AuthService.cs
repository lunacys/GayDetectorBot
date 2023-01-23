using GayDetectorBot.WebApi.Models.Auth;
using GayDetectorBot.WebApi.Models.Users;
using GayDetectorBot.WebApi.Services.UserManagement;

namespace GayDetectorBot.WebApi.Services.Auth;

public class AuthService : IAuthService
{
    private readonly IUserService _userService;
    private readonly IPasswordHasherService _hasher;
    private readonly ITokenGeneratorService _tokenGenerator;


    public AuthService(IUserService userService, IPasswordHasherService hasher, ITokenGeneratorService tokenGenerator)
    {
        _userService = userService;
        _hasher = hasher;
        _tokenGenerator = tokenGenerator;
    }

    public async Task<AuthResponse?> AuthAsync(AuthRequest request)
    {
        User? user = null;

        try
        {
            user = await _userService.GetByUsernameAsync(request.Username);
        }
        catch (Exception e)
        {
            return null;
        }

        if (user == null)
            return null;

        if (!_hasher.Check(user.PasswordHash, request.Password).Verified)
            return null; 

        var token = _tokenGenerator.Generate(user);

        return new AuthResponse(user, token);
    }
}