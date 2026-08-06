using Microsoft.EntityFrameworkCore;
using ShipIt.Core.Interfaces.Repositories;
using ShipIt.Core.Models;
using ShipIt.Infrastructure.Persistence;

namespace ShipIt.Infrastructure.Repositories;

public class EnvironmentVariableRepository : IEnvironmentVariableRepository
{
    private readonly ShipItDbContext _context;

    public EnvironmentVariableRepository(ShipItDbContext context)
    {
        _context = context;
    }

    public async Task<bool> ExistsAsync(Guid deploymentConfigurationId, string key)
    {
        return await _context.EnvironmentVariables
            .AnyAsync(x => x.DeploymentConfigurationId == deploymentConfigurationId &&
                        x.Key == key);
    }

    public async Task<EnvironmentVariable?> GetByIdAsync(Guid environmentVariableId)
    {
        return await _context.EnvironmentVariables
            .FirstOrDefaultAsync(x => x.EnvironmentVariableId == environmentVariableId);
    }

    public async Task<IReadOnlyList<EnvironmentVariable>> GetByDeploymentConfigurationIdAsync(Guid deploymentConfigurationId)
    {
        return await _context.EnvironmentVariables
        .Where(x => x.DeploymentConfigurationId == deploymentConfigurationId)
        .OrderBy(x => x.Key)
        .ToListAsync();
    }

    public async Task AddAsync(EnvironmentVariable environmentVariable)
    {
        await _context.EnvironmentVariables.AddAsync(environmentVariable);
    }

    public Task DeleteAsync(EnvironmentVariable environmentVariable)
    {
        _context.EnvironmentVariables.Remove(environmentVariable);

        return Task.CompletedTask;
    }
        
    public Task UpdateAsync(EnvironmentVariable environmentVariable)
{
    _context.EnvironmentVariables.Update(environmentVariable);

    return Task.CompletedTask;
}
}