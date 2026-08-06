using Microsoft.EntityFrameworkCore;
using ShipIt.Core.Interfaces.Repositories;
using ShipIt.Core.Models;
using ShipIt.Infrastructure.Persistence;

namespace ShipIt.Infrastructure.Repositories;

public class ApplicationRepository : IApplicationRepository
{
    private readonly ShipItDbContext _context;

    public ApplicationRepository(ShipItDbContext context)
    {
        _context = context;
    }

    public async Task<bool> ExistsAsync(Guid ownerId, string name)
    {
        return await _context.Applications
            .AnyAsync(x => x.OwnerId == ownerId &&
                           x.Name == name);
    }

    public async Task<Application?> GetByIdAsync(Guid applicationId)
    {
        return await _context.Applications
            .Include(x => x.SourceRepository)
            .Include(x => x.Users)
            .FirstOrDefaultAsync(x => x.ApplicationId == applicationId);
    }

    public async Task<Application?> GetByNameAsync(Guid ownerId, string name)
    {
        return await _context.Applications
            .FirstOrDefaultAsync(x => x.OwnerId == ownerId &&
                                      x.Name == name);
    }

    public async Task AddAsync(Application application)
    {
        await _context.Applications.AddAsync(application);
    }
}