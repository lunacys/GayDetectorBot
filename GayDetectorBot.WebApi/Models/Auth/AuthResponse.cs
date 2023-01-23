using GayDetectorBot.WebApi.Models.Users;

namespace GayDetectorBot.WebApi.Models.Auth;

public class AuthResponse
{
    public int Id { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public string Token { get; set; }

    public AuthResponse(User user, string token)
    {
        Id = user.Id;
        Username = user.Username;
        Email = user.Email;
        Token = token;
    }
}