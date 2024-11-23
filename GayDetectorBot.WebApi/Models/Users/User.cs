using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Identity;

namespace GayDetectorBot.WebApi.Models.Users;

public class User
{
    [Key]
    public int Id { get; set; }
    [Required]
    [ProtectedPersonalData]
    [StringLength(256)]
    public string Username { get; set; }
    [Required]
    [ProtectedPersonalData]
    [EmailAddress]
    [StringLength(256)]
    public string Email { get; set; }
    [Required]
    [DataType(DataType.Password)]
    [JsonIgnore]
    public string PasswordHash { get; set; }
    [JsonIgnore]
    [NotMapped]
    public string Password { get; set; }
}