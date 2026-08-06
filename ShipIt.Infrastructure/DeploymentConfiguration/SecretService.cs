using ShipIt.Core.DTOs.Secrets;
using ShipIt.Core.Interfaces.Repositories;
using ShipIt.Core.Models;
using ShipIt.Core.Services;
using ShipIt.Infrastructure.Persistence;

namespace ShipIt.Infrastructure.Services;

public class SecretService : ISecretService
{
    private readonly ISecretRepository _secretRepository;
    private readonly IDeploymentConfigurationRepository _deploymentConfigurationRepository;
    private readonly IEncryptionService _encryptionService;
    private readonly ShipItDbContext _context;

    public SecretService(
        ISecretRepository secretRepository,
        IDeploymentConfigurationRepository deploymentConfigurationRepository,
        IEncryptionService encryptionService,
        ShipItDbContext context)
    {
        _secretRepository = secretRepository;
        _deploymentConfigurationRepository = deploymentConfigurationRepository;
        _encryptionService = encryptionService;
        _context = context;
    }

    public async Task<SecretResponse> CreateAsync(
        Guid deploymentConfigurationId,
        Guid userId,
        CreateSecretRequest request)
    {
        await ValidateDeploymentConfigurationAsync(
            deploymentConfigurationId,
            userId);

        var key = request.Key.Trim().ToUpperInvariant();

        if (await _secretRepository.ExistsAsync(deploymentConfigurationId, key))
            throw new Exception($"Secret '{key}' already exists.");

        var secret = new Secret
        {
            SecretId = Guid.NewGuid(),
            DeploymentConfigurationId = deploymentConfigurationId,

            Key = key,
            EncryptedValue = _encryptionService.Encrypt(request.Value),

            IsEnabled = request.IsEnabled,

            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _secretRepository.AddAsync(secret);

        await _context.SaveChangesAsync();

        return MapToResponse(secret);
    }

    public async Task<IReadOnlyList<SecretResponse>> GetAllAsync(
        Guid deploymentConfigurationId,
        Guid userId)
    {
        await ValidateDeploymentConfigurationAsync(
            deploymentConfigurationId,
            userId);

        var secrets = await _secretRepository
            .GetByDeploymentConfigurationIdAsync(deploymentConfigurationId);

        return secrets
            .Select(MapToResponse)
            .ToList();
    }

    public async Task<SecretResponse?> GetByIdAsync(
        Guid deploymentConfigurationId,
        Guid secretId,
        Guid userId)
    {
        await ValidateDeploymentConfigurationAsync(
            deploymentConfigurationId,
            userId);

        var secret = await _secretRepository.GetByIdAsync(
            deploymentConfigurationId,
            secretId);

        if (secret is null)
            return null;

        return MapToResponse(secret);
    }

    public async Task<SecretResponse> UpdateAsync(
        Guid deploymentConfigurationId,
        Guid secretId,
        Guid userId,
        UpdateSecretRequest request)
    {
        await ValidateDeploymentConfigurationAsync(
            deploymentConfigurationId,
            userId);

        var secret = await _secretRepository.GetByIdAsync(
            deploymentConfigurationId,
            secretId);

        if (secret is null)
            throw new Exception("Secret not found.");

        var key = request.Key.Trim().ToUpperInvariant();

        if (key != secret.Key &&
            await _secretRepository.ExistsAsync(deploymentConfigurationId, key))
        {
            throw new Exception($"Secret '{key}' already exists.");
        }

        secret.Key = key;
        secret.EncryptedValue = _encryptionService.Encrypt(request.Value);
        secret.IsEnabled = request.IsEnabled;
        secret.UpdatedAt = DateTime.UtcNow;

        await _secretRepository.UpdateAsync(secret);

        await _context.SaveChangesAsync();

        return MapToResponse(secret);
    }

    public async Task DeleteAsync(
        Guid deploymentConfigurationId,
        Guid secretId,
        Guid userId)
    {
        await ValidateDeploymentConfigurationAsync(
            deploymentConfigurationId,
            userId);

        var secret = await _secretRepository.GetByIdAsync(
            deploymentConfigurationId,
            secretId);

        if (secret is null)
            throw new Exception("Secret not found.");

        await _secretRepository.DeleteAsync(secret);

        await _context.SaveChangesAsync();
    }

    private async Task<DeploymentConfiguration> ValidateDeploymentConfigurationAsync(
        Guid deploymentConfigurationId,
        Guid userId)
    {
        var deploymentConfiguration =
            await _deploymentConfigurationRepository
                .GetWithApplicationAsync(deploymentConfigurationId);

        if (deploymentConfiguration is null)
            throw new Exception("Deployment configuration not found.");

        if (deploymentConfiguration.Application.OwnerId != userId)
            throw new Exception(
                "You do not have permission to manage this deployment configuration.");

        return deploymentConfiguration;
    }

    private static SecretResponse MapToResponse(Secret secret)
    {
        return new SecretResponse
        {
            SecretId = secret.SecretId,
            DeploymentConfigurationId = secret.DeploymentConfigurationId,
            Key = secret.Key,
            IsConfigured = !string.IsNullOrWhiteSpace(secret.EncryptedValue),
            IsEnabled = secret.IsEnabled
        };
    }
}