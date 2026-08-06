using ShipIt.Core.Models;

namespace ShipIt.Core.Interfaces.Repositories;

public interface IDeploymentConfigurationRepository
{
    Task<bool> ExistsAsync(Guid applicationId);

    Task<DeploymentConfiguration?> GetByApplicationIdAsync(Guid applicationId);

    Task AddAsync(DeploymentConfiguration configuration);

    Task UpdateAsync(DeploymentConfiguration configuration);
}