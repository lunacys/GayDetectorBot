using GayDetectorBot.WebApi.Models.Users;

namespace GayDetectorBot.WebApi.Services.Auth;

public interface ITokenGeneratorService
{
    string Generate(User user);
}