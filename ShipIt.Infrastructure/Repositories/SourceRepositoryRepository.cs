using ShipIt.Core.Interfaces.Repositories;
using ShipIt.Core.Models;
using ShipIt.Infrastructure.Persistence;

namespace ShipIt.Infrastructure.Repositories;

public class SourceRepositoryRepository : ISourceRepositoryRepository
{
    private readonly ShipItDbContext _context;

    public SourceRepositoryRepository(ShipItDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(SourceRepository repository)
    {
        await _context.SourceRepositories.AddAsync(repository);
    }
}