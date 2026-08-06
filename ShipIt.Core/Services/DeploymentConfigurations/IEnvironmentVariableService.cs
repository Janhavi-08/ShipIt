using ShipIt.Core.DTOs.EnvironmentVariables;
using ShipIt.Core.Models;

namespace ShipIt.Core.Services;

public interface IEnvironmentVariableService
{
    Task<EnvironmentVariableResponse> CreateAsync(
        Guid deploymentConfigurationId,
        Guid userId,
        CreateEnvironmentVariableRequest request);

    Task<IReadOnlyList<EnvironmentVariableResponse>> GetAllAsync(
        Guid deploymentConfigurationId,
        Guid userId);

    Task<EnvironmentVariableResponse?> GetByIdAsync(
        Guid deploymentConfigurationId,
        Guid environmentVariableId,
        Guid userId);

    Task<EnvironmentVariableResponse> UpdateAsync(
        Guid deploymentConfigurationId,
        Guid environmentVariableId,
        Guid userId,
        UpdateEnvironmentVariableRequest request);

    Task DeleteAsync(
        Guid deploymentConfigurationId,
        Guid environmentVariableId,
        Guid userId);
        Task<EnvironmentVariable?> GetByIdAsync(
    Guid deploymentConfigurationId,
    Guid environmentVariableId);
        
}