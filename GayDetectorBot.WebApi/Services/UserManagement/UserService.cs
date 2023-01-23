using GayDetectorBot.WebApi.Data;
using GayDetectorBot.WebApi.Models.Users;
using GayDetectorBot.WebApi.Services.Auth;
using Microsoft.EntityFrameworkCore;

namespace GayDetectorBot.WebApi.Services.UserManagement;

public class UserService : IUserService
{
    private readonly GayDetectorBotContext _context;
    private readonly IPasswordHasherService _hasher;

    public UserService(GayDetectorBotContext context, IPasswordHasherService passwordHasher)
    {
        _context = context;
        _hasher = passwordHasher;
    }

    public async Task<IEnumerable<User>> GetAllAsync()
    {
        return await _context.Users.ToListAsync();
    }

    public async Task<User?> GetByIdAsync(int id)
    {
        return await _context.Users.FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        return await _context.Users.FirstOrDefaultAsync(x => x.Username == username);
    }

    public async Task ChangePasswordByIdAsync(int id, string oldPassword, string newPassword)
    {
        if (string.IsNullOrEmpty(oldPassword))
            throw new Exception("Old Password is empty");
        if (string.IsNullOrEmpty(newPassword))
            throw new Exception("New Password is empty");
        if (oldPassword == newPassword)
            throw new Exception("Passwords are the same");

        var existing = await _context.Users.FindAsync(id);

        if (existing == null)
            throw new Exception("Cannot find user with id " + id);

        if (!_hasher.Check(existing.PasswordHash, oldPassword).Verified)
            throw new Exception("Incorrect old password");

        var hashedPwd = _hasher.Hash(newPassword);
        existing.PasswordHash = hashedPwd;

        _context.Update(existing);
        await _context.SaveChangesAsync();
    }

    public string HashPassword(string password)
    {
        return _hasher.Hash(password);
    }
}