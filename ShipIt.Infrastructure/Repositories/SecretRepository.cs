using Microsoft.EntityFrameworkCore;
using ShipIt.Core.Interfaces.Repositories;
using ShipIt.Core.Models;
using ShipIt.Infrastructure.Persistence;

namespace ShipIt.Infrastructure.Repositories;

public class SecretRepository : ISecretRepository
{
    private readonly ShipItDbContext _context;

    public SecretRepository(ShipItDbContext context)
    {
        _context = context;
    }

    public async Task<bool> ExistsAsync(
        Guid deploymentConfigurationId,
        string key)
    {
        return await _context.Secrets
            .AnyAsync(x =>
                x.DeploymentConfigurationId == deploymentConfigurationId &&
                x.Key == key);
    }

    public async Task<Secret?> GetByIdAsync(
        Guid deploymentConfigurationId,
        Guid secretId)
    {
        return await _context.Secrets
            .FirstOrDefaultAsync(x =>
                x.SecretId == secretId &&
                x.DeploymentConfigurationId == deploymentConfigurationId);
    }

    public async Task<List<Secret>> GetByDeploymentConfigurationIdAsync(
        Guid deploymentConfigurationId)
    {
        return await _context.Secrets
            .Where(x => x.DeploymentConfigurationId == deploymentConfigurationId)
            .OrderBy(x => x.Key)
            .ToListAsync();
    }

    public async Task AddAsync(Secret secret)
    {
        await _context.Secrets.AddAsync(secret);
    }

    public Task UpdateAsync(Secret secret)
    {
        _context.Secrets.Update(secret);

        return Task.CompletedTask;
    }

    public Task DeleteAsync(Secret secret)
    {
        _context.Secrets.Remove(secret);

        return Task.CompletedTask;
    }
}