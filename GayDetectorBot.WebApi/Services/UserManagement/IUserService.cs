using GayDetectorBot.WebApi.Models.Users;

namespace GayDetectorBot.WebApi.Services.UserManagement;

public interface IUserService
{
    Task<IEnumerable<User>> GetAllAsync();
    Task<User?> GetByIdAsync(int id);
    Task<User?> GetByUsernameAsync(string username);
    Task ChangePasswordByIdAsync(int id, string oldPassword, string newPassword);
    string HashPassword(string password);
}