using Microsoft.EntityFrameworkCore;
using ShipIt.Core.Interfaces.Repositories;
using ShipIt.Core.Models;
using ShipIt.Infrastructure.Persistence;

namespace ShipIt.Infrastructure.Repositories;

public class DeploymentConfigurationRepository
    : IDeploymentConfigurationRepository
{
    private readonly ShipItDbContext _context;

    public DeploymentConfigurationRepository(ShipItDbContext context)
    {
        _context = context;
    }

    public async Task<bool> ExistsAsync(Guid applicationId)
    {
        return await _context.DeploymentConfigurations
            .AnyAsync(x => x.ApplicationId == applicationId);
    }

    public async Task<DeploymentConfiguration?> GetByApplicationIdAsync(Guid applicationId)
    {
        return await _context.DeploymentConfigurations
            .FirstOrDefaultAsync(x => x.ApplicationId == applicationId);
    }

    public async Task AddAsync(DeploymentConfiguration configuration)
    {
        await _context.DeploymentConfigurations.AddAsync(configuration);
    }

    public Task UpdateAsync(DeploymentConfiguration configuration)
    {
        _context.DeploymentConfigurations.Update(configuration);

        return Task.CompletedTask;
    }
}