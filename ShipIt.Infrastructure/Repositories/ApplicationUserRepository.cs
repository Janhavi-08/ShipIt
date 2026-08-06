using ShipIt.Core.Models;
using ShipIt.Infrastructure.Persistence;

public class ApplicationUserRepository : IApplicationUserRepository
{
    private readonly ShipItDbContext _context;

    public ApplicationUserRepository(ShipItDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(ApplicationUser applicationUser)
    {
        await _context.ApplicationUsers.AddAsync(applicationUser);
    }
}