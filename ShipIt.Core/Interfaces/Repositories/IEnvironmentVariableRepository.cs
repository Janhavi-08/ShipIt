using ShipIt.Core.Models;

namespace ShipIt.Core.Interfaces.Repositories;

public interface IEnvironmentVariableRepository
{
    Task AddAsync(EnvironmentVariable environmentVariable);

    Task<EnvironmentVariable?> GetByIdAsync(Guid environmentVariableId);

    Task<IReadOnlyList<EnvironmentVariable>> GetByDeploymentConfigurationIdAsync(
        Guid deploymentConfigurationId);

    Task<bool> ExistsAsync(Guid deploymentConfigurationId, string key);

    Task UpdateAsync(EnvironmentVariable environmentVariable);

    Task DeleteAsync(EnvironmentVariable environmentVariable);
}