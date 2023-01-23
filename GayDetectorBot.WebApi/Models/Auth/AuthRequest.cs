using System.ComponentModel.DataAnnotations;

namespace GayDetectorBot.WebApi.Models.Auth;

public class AuthRequest
{
    [Required]
    public string Username { get; init; } = null!;
    [Required]
    public string Password { get; init; } = null!;
}