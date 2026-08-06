using Microsoft.EntityFrameworkCore;
using ShipIt.Core.DTOs.EnvironmentVariables;
using ShipIt.Core.Interfaces.Repositories;
using ShipIt.Core.Models;
using ShipIt.Core.Services;
using ShipIt.Infrastructure.Persistence;

public class EnvironmentVariableService: IEnvironmentVariableService{
private readonly IEnvironmentVariableRepository _environmentVariableRepository;
private readonly IDeploymentConfigurationRepository _deploymentConfigurationRepository;
private readonly ShipItDbContext _context;

public EnvironmentVariableService(
    IEnvironmentVariableRepository environmentVariableRepository,
    IDeploymentConfigurationRepository deploymentConfigurationRepository,
    ShipItDbContext context)
{
    _environmentVariableRepository = environmentVariableRepository;
    _deploymentConfigurationRepository = deploymentConfigurationRepository;
    _context = context;
}
private async Task<DeploymentConfiguration> ValidateDeploymentConfigurationAsync(
    Guid deploymentConfigurationId,
    Guid userId)
{
    var deploymentConfiguration =
        await _deploymentConfigurationRepository
            .GetWithApplicationAsync(deploymentConfigurationId);

    if (deploymentConfiguration is null)
        throw new Exception(
            "Deployment configuration not found.");

    if (deploymentConfiguration.Application.OwnerId != userId)
        throw new Exception(
            "You do not have permission to manage this deployment configuration.");

    return deploymentConfiguration;
}
    public async Task<EnvironmentVariableResponse> CreateAsync(
        Guid deploymentConfigurationId,
        Guid userId,
        CreateEnvironmentVariableRequest request)
    {
        await ValidateDeploymentConfigurationAsync(
            deploymentConfigurationId,
            userId);

        var key = request.Key.Trim().ToUpperInvariant();

        if (await _environmentVariableRepository.ExistsAsync(
                deploymentConfigurationId,
                key))
        {
            throw new Exception(
                $"Environment variable '{key}' already exists.");
        }

        var environmentVariable = new EnvironmentVariable
        {
            EnvironmentVariableId = Guid.NewGuid(),
            DeploymentConfigurationId = deploymentConfigurationId,

            Key = key,
            Value = request.Value?.Trim(),
            IsEnabled = request.IsEnabled,

            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _environmentVariableRepository.AddAsync(environmentVariable);

        await _context.SaveChangesAsync();

        return MapToResponse(environmentVariable);
    }
    public async Task<IReadOnlyList<EnvironmentVariableResponse>> GetAllAsync(
        Guid deploymentConfigurationId,
        Guid userId)
    {
        await ValidateDeploymentConfigurationAsync(
            deploymentConfigurationId,
            userId);

        var environmentVariables =
            await _environmentVariableRepository
                .GetByDeploymentConfigurationIdAsync(deploymentConfigurationId);

        return environmentVariables
            .Select(MapToResponse)
            .ToList();
    }
    public async Task DeleteAsync(
    Guid deploymentConfigurationId,
    Guid environmentVariableId,
    Guid userId)
{
    await ValidateDeploymentConfigurationAsync(
        deploymentConfigurationId,
        userId);

    var environmentVariable =
        await _environmentVariableRepository
            .GetByIdAsync(environmentVariableId);

    if (environmentVariable is null ||
        environmentVariable.DeploymentConfigurationId != deploymentConfigurationId)
    {
        throw new Exception(
            "Environment variable not found.");
    }

    await _environmentVariableRepository
        .DeleteAsync(environmentVariable);

    await _context.SaveChangesAsync();
}
    public async Task<EnvironmentVariableResponse?> GetByIdAsync(
        Guid deploymentConfigurationId,
        Guid environmentVariableId,
        Guid userId)
    {
        await ValidateDeploymentConfigurationAsync(
            deploymentConfigurationId,
            userId);

        var environmentVariable =
            await _environmentVariableRepository
                .GetByIdAsync(environmentVariableId);

        if (environmentVariable is null ||
            environmentVariable.DeploymentConfigurationId != deploymentConfigurationId)
        {
            return null;
        }

        return MapToResponse(environmentVariable);
    }
    public async Task<EnvironmentVariableResponse> UpdateAsync(
        Guid deploymentConfigurationId,
        Guid environmentVariableId,
        Guid userId,
        UpdateEnvironmentVariableRequest request)
    {
        await ValidateDeploymentConfigurationAsync(
            deploymentConfigurationId,
            userId);

        var environmentVariable =
            await _environmentVariableRepository
                .GetByIdAsync(environmentVariableId);

        if (environmentVariable is null ||
            environmentVariable.DeploymentConfigurationId != deploymentConfigurationId)
        {
            throw new Exception(
                "Environment variable not found.");
        }

        var key = request.Key.Trim().ToUpperInvariant();

        if (key != environmentVariable.Key &&
            await _environmentVariableRepository.ExistsAsync(
                deploymentConfigurationId,
                key))
        {
            throw new Exception(
                $"Environment variable '{key}' already exists.");
        }

        environmentVariable.Key = key;
        environmentVariable.Value = request.Value?.Trim();
        environmentVariable.IsEnabled = request.IsEnabled;
        environmentVariable.UpdatedAt = DateTime.UtcNow;

        await _environmentVariableRepository.UpdateAsync(environmentVariable);

        await _context.SaveChangesAsync();

        return MapToResponse(environmentVariable);
    }
    public async Task<EnvironmentVariable?> GetByIdAsync(
        Guid deploymentConfigurationId,
        Guid environmentVariableId)
    {
        return await _context.EnvironmentVariables.AsNoTracking()
            .FirstOrDefaultAsync(x =>
                x.EnvironmentVariableId == environmentVariableId &&
                x.DeploymentConfigurationId == deploymentConfigurationId);
    }
private static EnvironmentVariableResponse MapToResponse(
    EnvironmentVariable environmentVariable)
{
    return new EnvironmentVariableResponse
    {
        EnvironmentVariableId = environmentVariable.EnvironmentVariableId,
        DeploymentConfigurationId = environmentVariable.DeploymentConfigurationId,
        Key = environmentVariable.Key,
        Value = environmentVariable.Value,
        IsEnabled = environmentVariable.IsEnabled
    };
}
}