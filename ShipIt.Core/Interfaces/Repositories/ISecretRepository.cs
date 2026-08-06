using ShipIt.Core.Models;

namespace ShipIt.Core.Interfaces.Repositories;

public interface ISecretRepository
{
    Task AddAsync(Secret secret);

    Task<Secret?> GetByIdAsync(
        Guid deploymentConfigurationId,
        Guid secretId);

    Task<List<Secret>> GetByDeploymentConfigurationIdAsync(
        Guid deploymentConfigurationId);

    Task<bool> ExistsAsync(
        Guid deploymentConfigurationId,
        string key);

    Task UpdateAsync(Secret secret);

    Task DeleteAsync(Secret secret);
}