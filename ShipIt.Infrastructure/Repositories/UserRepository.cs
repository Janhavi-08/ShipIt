using Microsoft.EntityFrameworkCore;
using ShipIt.Core.Interfaces.Repositories;
using ShipIt.Core.Models;
using ShipIt.Infrastructure.Persistence;

namespace ShipIt.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly ShipItDbContext _context;

    public UserRepository(ShipItDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByIdAsync(Guid userId)
    {
        return await _context.Users
            .FirstOrDefaultAsync(x => x.UserId == userId);
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _context.Users
            .FirstOrDefaultAsync(x => x.Email == email);
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        return await _context.Users
            .FirstOrDefaultAsync(x => x.Username == username);
    }

    public async Task<bool> ExistsByEmailAsync(string email)
    {
        return await _context.Users
            .AnyAsync(x => x.Email.ToLower() == email.ToLower());
    }

    public async Task<bool> ExistsByUsernameAsync(string username)
    {
        return await _context.Users
            .AnyAsync(x => x.Username == username);
    }

    public async Task AddAsync(User user)
    {
        await _context.Users.AddAsync(user);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}