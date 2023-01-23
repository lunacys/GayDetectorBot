using GayDetectorBot.WebApi.Configuration;
using GayDetectorBot.WebApi.Models.Users;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace GayDetectorBot.WebApi.Services.Auth;

public class TokenGeneratorService : ITokenGeneratorService
{
    private readonly AppSettings _appSettings;

    public TokenGeneratorService(IOptions<AppSettings> appSettings)
    {
        _appSettings = appSettings.Value;
    }

    public string Generate(User user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
        var claims = new[]
        {
            new Claim(ClaimsIdentity.DefaultNameClaimType, user.Username)
        };
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims, "Token", ClaimsIdentity.DefaultNameClaimType, ClaimsIdentity.DefaultRoleClaimType),
            // NotBefore = DateTime.UtcNow,
            Expires = DateTime.UtcNow.AddDays(7),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
            Issuer = AppVersionInfo.AppName + " " + AppVersionInfo.BuildVersion,
            Audience = "GayDetectorBot"
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }
}