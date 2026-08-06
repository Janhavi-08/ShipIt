using Microsoft.EntityFrameworkCore;
using ShipIt.Core.Interfaces.Repositories;
using ShipIt.Core.Models;
using ShipIt.Infrastructure.Persistence;

namespace ShipIt.Infrastructure.Repositories;

public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly ShipItDbContext _context;

    public RefreshTokenRepository(ShipItDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(RefreshToken refreshToken)
    {
        await _context.RefreshTokens.AddAsync(refreshToken);
    }

    public async Task<RefreshToken?> GetByTokenAsync(string token)
    {
        return await _context.RefreshTokens
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.Token == token);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}