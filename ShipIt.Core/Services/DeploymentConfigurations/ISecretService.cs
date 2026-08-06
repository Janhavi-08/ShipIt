using ShipIt.Core.DTOs.Secrets;

namespace ShipIt.Core.Services;

public interface ISecretService
{
    Task<SecretResponse> CreateAsync(
        Guid deploymentConfigurationId,
        Guid userId,
        CreateSecretRequest request);

    Task<IReadOnlyList<SecretResponse>> GetAllAsync(
        Guid deploymentConfigurationId,
        Guid userId);

    Task<SecretResponse?> GetByIdAsync(
        Guid deploymentConfigurationId,
        Guid secretId,
        Guid userId);

    Task<SecretResponse> UpdateAsync(
        Guid deploymentConfigurationId,
        Guid secretId,
        Guid userId,
        UpdateSecretRequest request);

    Task DeleteAsync(
        Guid deploymentConfigurationId,
        Guid secretId,
        Guid userId);
}