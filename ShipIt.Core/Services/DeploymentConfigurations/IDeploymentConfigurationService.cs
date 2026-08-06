using ShipIt.Core.DTOs.DeploymentConfiguration;

namespace ShipIt.Core.Services;

public interface IDeploymentConfigurationService
{
    Task<DeploymentConfigurationResponse> CreateAsync(
        Guid applicationId,
        Guid userId,
        CreateDeploymentConfigurationRequest request);

    Task<DeploymentConfigurationResponse?> GetAsync(
        Guid applicationId,
        Guid userId);

    Task<DeploymentConfigurationResponse> UpdateAsync(
        Guid applicationId,
        Guid userId,
        UpdateDeploymentConfigurationRequest request);
}