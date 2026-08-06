using ShipIt.Core.Deployment;
using ShipIt.Core.Deployment.Validation;
using ShipIt.Core.DTOs.DeploymentConfiguration;
using ShipIt.Core.Interfaces.Repositories;
using ShipIt.Core.Models;
using ShipIt.Core.Services;
using ShipIt.Infrastructure.Persistence;

public class DeploymentConfigurationService : IDeploymentConfigurationService
{

    private readonly IDeploymentConfigurationRepository _deploymentConfigurationRepository;
    private readonly IApplicationRepository _applicationRepository;
    private readonly ShipItDbContext _context;

    public DeploymentConfigurationService(
        IDeploymentConfigurationRepository deploymentConfigurationRepository,
        IApplicationRepository applicationRepository,
        ShipItDbContext context)
    {
        _deploymentConfigurationRepository = deploymentConfigurationRepository;
        _applicationRepository = applicationRepository;
        _context = context;
    }
    public async Task<DeploymentConfigurationResponse> CreateAsync(
        Guid applicationId,
        Guid userId,
        CreateDeploymentConfigurationRequest request)
    {
        var application = await ValidateApplicationAsync(applicationId, userId);

        if (await _deploymentConfigurationRepository.ExistsAsync(applicationId))
            throw new Exception("Deployment configuration already exists.");

        ValidateRuntimeConfiguration(request);

        var configuration = BuildDeploymentConfiguration(applicationId, request);

        await _deploymentConfigurationRepository.AddAsync(configuration);

        await _context.SaveChangesAsync();

        return MapToResponse(configuration);
    }
    private async Task<Application> ValidateApplicationAsync(
        Guid applicationId,
        Guid userId)
    {
        var application = await _applicationRepository.GetByIdAsync(applicationId);

        if (application is null)
            throw new Exception("Application not found.");

        if (application.OwnerId != userId)
            throw new Exception("You do not have permission to modify this application.");

        return application;
    }
    private static void ValidateRuntimeConfiguration(
    int containerPort,
    FargateCpu cpu,
    FargateMemory memory,
    int minimumInstances,
    int maximumInstances,
    string healthCheckPath)
    {
        if (!FargateValidator.IsValid(cpu, memory))
            throw new Exception(
                "The selected CPU and Memory combination is not supported.");

        if (minimumInstances < 1)
            throw new Exception(
                "Minimum instances must be at least 1.");

        if (maximumInstances < minimumInstances)
            throw new Exception(
                "Maximum instances must be greater than or equal to minimum instances.");

        if (containerPort < 1 || containerPort > 65535)
            throw new Exception(
                "Container port must be between 1 and 65535.");

        if (!healthCheckPath.StartsWith('/'))
            throw new Exception(
                "Health check path must start with '/'.");
    }
    private static void ValidateRuntimeConfiguration(
        CreateDeploymentConfigurationRequest request)
    {
        ValidateRuntimeConfiguration(
            request.ContainerPort,
            request.Cpu,
            request.Memory,
            request.MinimumInstances,
            request.MaximumInstances,
            request.HealthCheckPath);
    }
    private static void ValidateRuntimeConfiguration(
        UpdateDeploymentConfigurationRequest request)
    {
        ValidateRuntimeConfiguration(
            request.ContainerPort,
            request.Cpu,
            request.Memory,
            request.MinimumInstances,
            request.MaximumInstances,
            request.HealthCheckPath);
    }
    private static DeploymentConfiguration BuildDeploymentConfiguration(
        Guid applicationId,
        CreateDeploymentConfigurationRequest request)
    {
        var now = DateTime.UtcNow;

        return new DeploymentConfiguration
        {
            DeploymentConfigurationId = Guid.NewGuid(),
            ApplicationId = applicationId,

            ContainerPort = request.ContainerPort,
            Cpu = request.Cpu,
            Memory = request.Memory,

            MinimumInstances = request.MinimumInstances,
            MaximumInstances = request.MaximumInstances,

            HealthCheckPath = request.HealthCheckPath.Trim(),
            HealthCheckInterval = request.HealthCheckInterval,
            HealthCheckTimeout = request.HealthCheckTimeout,
            HealthyThreshold = request.HealthyThreshold,
            UnhealthyThreshold = request.UnhealthyThreshold,

            Subdomain = request.Subdomain.Trim(),
            EnableHttps = request.EnableHttps,

            CreatedAt = now,
            UpdatedAt = now
        };
    }
    private static DeploymentConfigurationResponse MapToResponse(
        DeploymentConfiguration configuration)
    {
        return new DeploymentConfigurationResponse
        {
            DeploymentConfigurationId = configuration.DeploymentConfigurationId,
            ApplicationId = configuration.ApplicationId,

            ContainerPort = configuration.ContainerPort,
            Cpu = configuration.Cpu,
            Memory = configuration.Memory,

            MinimumInstances = configuration.MinimumInstances,
            MaximumInstances = configuration.MaximumInstances,

            HealthCheckPath = configuration.HealthCheckPath,
            HealthCheckInterval = configuration.HealthCheckInterval,
            HealthCheckTimeout = configuration.HealthCheckTimeout,
            HealthyThreshold = configuration.HealthyThreshold,
            UnhealthyThreshold = configuration.UnhealthyThreshold,

            Subdomain = configuration.Subdomain,
            EnableHttps = configuration.EnableHttps
        };
    }
    public async Task<DeploymentConfigurationResponse?> GetAsync(
        Guid applicationId,
        Guid userId)
    {
        await ValidateApplicationAsync(applicationId, userId);

        var configuration = await _deploymentConfigurationRepository
            .GetByApplicationIdAsync(applicationId);

        if (configuration == null)
            return null;

        return MapToResponse(configuration);
    }
    public async Task<DeploymentConfigurationResponse> UpdateAsync(
        Guid applicationId,
        Guid userId,
        UpdateDeploymentConfigurationRequest request)
    {
        await ValidateApplicationAsync(applicationId, userId);

        var configuration = await _deploymentConfigurationRepository
            .GetByApplicationIdAsync(applicationId);

        if (configuration == null)
            throw new Exception("Deployment configuration not found.");

        ValidateRuntimeConfiguration(request);

        configuration.ContainerPort = request.ContainerPort;
        configuration.Cpu = request.Cpu;
        configuration.Memory = request.Memory;

        configuration.MinimumInstances = request.MinimumInstances;
        configuration.MaximumInstances = request.MaximumInstances;

        configuration.HealthCheckPath = request.HealthCheckPath.Trim();
        configuration.HealthCheckInterval = request.HealthCheckInterval;
        configuration.HealthCheckTimeout = request.HealthCheckTimeout;
        configuration.HealthyThreshold = request.HealthyThreshold;
        configuration.UnhealthyThreshold = request.UnhealthyThreshold;

        configuration.Subdomain = request.Subdomain.Trim();
        configuration.EnableHttps = request.EnableHttps;

        configuration.UpdatedAt = DateTime.UtcNow;

        await _deploymentConfigurationRepository.UpdateAsync(configuration);

        await _context.SaveChangesAsync();

        return MapToResponse(configuration);
    }

    }