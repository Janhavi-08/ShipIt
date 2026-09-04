using Microsoft.EntityFrameworkCore;
using ShipIt.Core.Enums;
using ShipIt.Core.Interfaces.Repositories;
using ShipIt.Infrastructure.Persistence;

namespace ShipIt.DeploymentWorker.Services;

public class DeploymentProcessor
{
    private readonly ShipItDbContext _context;
    private readonly ILogger<DeploymentProcessor> _logger;
    private readonly ISourceRepositoryRepository _sourceRepositoryRepository;
    private readonly IGitRepositoryService _gitRepositoryService;
    private readonly IDockerBuildService _dockerBuildService;
    private readonly IDeploymentWorkspaceService _workspaceService;
    private readonly IEcrService _ecrService;
    private readonly IEcsService _ecsService;
    private readonly IConfiguration _configuration;
    private readonly IEnvironmentVariableRepository _environmentVariableRepository;
    private readonly ISecretsManagerService _secretsManagerService;
    private readonly ISecretRepository _secretRepository;
    private readonly IDeploymentLockService _deploymentLockService;
    public DeploymentProcessor(
        ShipItDbContext context,
        IDockerBuildService dockerBuildService,
        IEcrService ecrService,
        IGitRepositoryService gitRepositoryService,
        IDeploymentWorkspaceService deploymentWorkspaceService,
        ISourceRepositoryRepository sourceRepositoryRepository,
        IConfiguration configuration,
        IEcsService ecsService,
        ISecretRepository secretRepository,
        ISecretsManagerService secretsManagerService,
        IEnvironmentVariableRepository environmentVariableRepository,
        IDeploymentLockService deploymentLockService,
        ILogger<DeploymentProcessor> logger)
    {
        _gitRepositoryService = gitRepositoryService;
        _sourceRepositoryRepository = sourceRepositoryRepository;
        _workspaceService = deploymentWorkspaceService;
        _context = context;   
        _dockerBuildService = dockerBuildService;
        _logger = logger;
        _ecrService = ecrService;
        _configuration = configuration;
        _ecsService = ecsService;
        _secretRepository = secretRepository;
        _secretsManagerService = secretsManagerService;
        _environmentVariableRepository = environmentVariableRepository;
        _deploymentLockService = deploymentLockService;

    }

    public async Task ProcessAsync(
        DeploymentMessage message,
        CancellationToken cancellationToken)
    {
       
            var deployment = await _context.Deployments
                .FirstOrDefaultAsync(
                    x => x.DeploymentId == message.DeploymentId,
                    cancellationToken);

            if (deployment == null)
            {
                _logger.LogWarning(
                    "Deployment {DeploymentId} was not found.",
                    message.DeploymentId);

                throw new InvalidOperationException(
                    $"Deployment {message.DeploymentId} was not found.");
            }
        try
        {
            if (deployment.Status != DeploymentStatus.Pending)
            {
                _logger.LogWarning(
                    "Deployment {DeploymentId} has status {Status}.",
                    deployment.DeploymentId,
                    deployment.Status);

                return;
            }

            var sourceRepository =
                await _sourceRepositoryRepository
                    .GetByApplicationIdAsync(
                        message.ApplicationId);

            if (sourceRepository == null)
                throw new InvalidOperationException(
                    "Source repository was not found.");

            _logger.LogInformation(
                "Source repository found. Provider: {Provider}, Repository: {Owner}/{Repository}, Branch: {Branch}, Private: {IsPrivate}",
                sourceRepository.Provider,
                sourceRepository.RepositoryOwner,
                sourceRepository.RepositoryName,
                sourceRepository.DefaultBranch,
                sourceRepository.IsPrivate);

            var workspacePath =
        await _workspaceService.CreateAsync(
            message.DeploymentId,
            cancellationToken);

            deployment.Status = DeploymentStatus.Building;
            deployment.StartedAt ??= DateTime.UtcNow;
            deployment.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(
                cancellationToken);
            _logger.LogInformation(
         "Deployment {DeploymentId} moved to Building.",
         deployment.DeploymentId);
            var repositoryUrl =
                $"https://github.com/" +
                $"{sourceRepository.RepositoryOwner}/" +
                $"{sourceRepository.RepositoryName}.git";

            await _gitRepositoryService.CloneAsync(
                repositoryUrl,
                sourceRepository.DefaultBranch,
                workspacePath,
                cancellationToken);

            var dockerfilePath =
            Path.Combine(
                workspacePath,
                "Dockerfile");

            if (!File.Exists(dockerfilePath))
            {
                throw new InvalidOperationException(
                    "Dockerfile was not found in the repository.");
            }

            var imageTag = $"shipit/{message.ApplicationId}:{message.DeploymentId}";

            await _dockerBuildService.BuildAsync(
                workspacePath,
                imageTag,
                cancellationToken);
            deployment.Status = DeploymentStatus.ImageBuilt;
            deployment.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            var repositoryName =
        $"shipit/{message.ApplicationId}";

            await _ecrService.EnsureRepositoryExistsAsync(
                repositoryName, message,
                cancellationToken);

            var ecrImage =
                await _ecrService.PushImageAsync(
                    imageTag,
                    repositoryName,
                    message.DeploymentId.ToString(),
                    cancellationToken);

            deployment.EcrImageUri = ecrImage;
            deployment.Status = DeploymentStatus.ImagePushed;
            deployment.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            var clusterName =
          _configuration["AWS:ECS:ClusterName"]
          ?? throw new InvalidOperationException(
              "ECS cluster name is not configured.");

            var clusterArn =
                await _ecsService.EnsureClusterExistsAsync(
                    clusterName,
                    cancellationToken);

            _logger.LogInformation(
                "Using ECS cluster {ClusterArn}.",
                clusterArn);


            var configuration =
                await _context.DeploymentConfigurations
                    .FirstOrDefaultAsync(
                        x => x.DeploymentConfigurationId ==
                             message.DeploymentConfigurationId,
                        cancellationToken);

            if (configuration == null)
            {
                throw new InvalidOperationException(
                    "Deployment configuration was not found.");
            }

            var environmentVariables = await _environmentVariableRepository.GetByDeploymentConfigurationIdAsync(message.DeploymentConfigurationId);

            var ecsEnvironmentVariables = environmentVariables
            .Where(x => x.IsEnabled)
            .Select(x => new EcsEnvironmentVariableInput
            {
                Name = x.Key,
                Value = x.Value ?? ""
            })
            .ToList();
            var secrets =
        await _secretRepository
            .GetByDeploymentConfigurationIdAsync(
                message.DeploymentConfigurationId);

            var enabledSecrets = secrets
        .Where(x => x.IsEnabled)
        .ToList();

            var secretArns = new List<EcsSecretInput>();

            foreach (var secret in enabledSecrets)
            {
                var secretName =
                    $"shipit/{message.ApplicationId}/{secret.Key}";

                var arn =
                    await _secretsManagerService
                        .CreateOrUpdateSecretAsync(
                            secretName,
                            secret.EncryptedValue ?? "",
                            cancellationToken);

                secretArns.Add(
                    new EcsSecretInput
                    {
                        Name = secret.Key,
                        ValueFrom = arn
                    });
            }

            var executionRoleArn = _configuration["AWS:ECS:TaskExecutionRoleArn"];
              if (string.IsNullOrWhiteSpace(executionRoleArn))
            {
                throw new InvalidOperationException(
                    "AWS Task Execution Role ID is not configured.");
            }
            var taskDefinitionInput = new EcsTaskDefinitionInput
                {
                    FamilyName =
                        $"shipit-{message.ApplicationId}",

                    ImageUri = ecrImage,

                    Cpu = (int)configuration.Cpu,

                    Memory = (int)configuration.Memory,

                    ContainerPort = configuration.ContainerPort,

                    ExecutionRoleArn = executionRoleArn,
                       
                    EnvironmentVariables =
                        ecsEnvironmentVariables,

                    Secrets = secretArns
                };
            var taskDefinitionArn =
            await _ecsService.RegisterTaskDefinitionAsync(
                taskDefinitionInput,
                cancellationToken);
            deployment.EcsTaskDefinitionArn = taskDefinitionArn;

            deployment.EcsTaskDefinitionRevision = ExtractRevision(taskDefinitionArn);
            deployment.Status = DeploymentStatus.TaskDefinitionCreated;

            deployment.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            var subnetIds =
                _configuration
                    .GetSection("AWS:ECS:PrivateSubnetIds")
                    .Get<string[]>()
                ?? throw new InvalidOperationException(
                    "ECS private subnets are not configured.");

            var taskSecurityGroupId =
                _configuration["AWS:ECS:TaskSecurityGroupId"]
                ?? throw new InvalidOperationException(
                    "ECS task security group is not configured.");

                        var desiredCount = configuration.MinimumInstances;
            var serviceName = $"shipit-{message.ApplicationId}";


            var vpcId = _configuration["Aws:VpcId"];
            if (string.IsNullOrWhiteSpace(vpcId))
            {
                throw new InvalidOperationException(
                    "AWS VPC ID is not configured.");
            }
            var baseDomain = _configuration["Routing:BaseDomain"];
            if (string.IsNullOrWhiteSpace(baseDomain))
            {
                throw new InvalidOperationException(
                    "Base domain is not configured.");
            }
            var hostName = configuration.Subdomain +"."+ baseDomain;
            var hostedZoneId = _configuration["Route53:HostedZoneId"];
            var albDnsName = _configuration["Aws:ALB:DnsName"];
            var albZoneId = _configuration["Aws:ALB:ZoneId"];
            if (string.IsNullOrWhiteSpace(hostedZoneId))
            {
                throw new InvalidOperationException(
                    "Route 53 hosted zone ID is not configured.");
            }

            if (string.IsNullOrWhiteSpace(albDnsName))
            {
                throw new InvalidOperationException(
                    "ALB DNS name is not configured.");
            }

            if (string.IsNullOrWhiteSpace(albZoneId))
            {
                throw new InvalidOperationException(
                    "ALB hosted zone ID is not configured.");
            }
            await _ecsService.EnsureDnsRecordAsync(
                hostedZoneId,
                hostName,
                albDnsName,
                albZoneId,
                cancellationToken);
            var listenerArn = _configuration["Aws:ALB:HttpsListenerArn"];
            if (string.IsNullOrWhiteSpace(listenerArn))
                {
                    throw new InvalidOperationException(
                        "ALB HTTPS listener ARN is not configured.");
                }
            var targetGroupName = $"shipit-{message.ApplicationId:N}"[..32];
            
            
            var targetGroupArn = await _ecsService.EnsureTargetGroupAsync(
                targetGroupName,
                vpcId,
                configuration.ContainerPort,
                cancellationToken);
            await _ecsService.EnsureListenerRuleAsync(
                listenerArn,
                hostName,
                targetGroupArn,
                cancellationToken);

            var serviceArn = await _ecsService.EnsureServiceAsync(
                clusterName,
                serviceName,
                taskDefinitionArn,
                desiredCount,
                targetGroupArn,
                "application",
                configuration.ContainerPort,
                subnetIds,
                taskSecurityGroupId,
                cancellationToken);

            deployment.Status = DeploymentStatus.EcsServiceCreated;

            deployment.UpdatedAt = DateTime.UtcNow;
            deployment.EcsClusterName = clusterName;
            deployment.EcsServiceName = serviceName;
            deployment.TargetGroupArn = targetGroupArn;

            await _context.SaveChangesAsync(cancellationToken);



            deployment.Status = DeploymentStatus.Deploying;
            deployment.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);


            var minimumInstances =
                configuration.MinimumInstances;

            var maximumInstances =
                configuration.MaximumInstances;


            await _ecsService.ConfigureAutoScalingAsync(
                clusterName,
                serviceName,
                minimumInstances,
                maximumInstances,
                cancellationToken);

            await _ecsService.ConfigureCpuScalingPolicyAsync(
                clusterName,
                serviceName,
                70,
                cancellationToken);
    





            var healthResult =
                await _ecsService.WaitForServiceHealthyAsync(
                    clusterName,
                    serviceName,
                    targetGroupArn,
                    desiredCount,
                    cancellationToken);

            if (healthResult.IsHealthy)
            {
                var loadBalancerArn =
                    _configuration["AWS:ALB:LoadBalancerArn"]
                    ?? throw new InvalidOperationException(
                        "ALB load balancer ARN is not configured.");

                var loadBalancerDns =
                    await _ecsService.GetLoadBalancerDnsNameAsync(
                        loadBalancerArn,
                        cancellationToken);

                deployment.DeploymentUrl =
                    $"http://{loadBalancerDns}";
                deployment.Status =
                    DeploymentStatus.Successful;

                deployment.ErrorMessage = null;

                deployment.UpdatedAt =
                    DateTime.UtcNow;

                await _context.SaveChangesAsync(
                    cancellationToken);

                _logger.LogInformation(
                    "Deployment {DeploymentId} completed successfully.",
                    deployment.DeploymentId);
            }
            else
            {
                deployment.Status =
                    DeploymentStatus.Failed;

                deployment.ErrorMessage =
                    healthResult.ErrorMessage;

                deployment.UpdatedAt =
                    DateTime.UtcNow;

                await _context.SaveChangesAsync(
                    cancellationToken);

                _logger.LogError(
                    "Deployment {DeploymentId} failed: {ErrorMessage}",
                    deployment.DeploymentId,
                    healthResult.ErrorMessage);
            }
        }
        catch (Exception ex)
{
    deployment.Status =
        DeploymentStatus.Failed;

    deployment.ErrorMessage =
        ex.Message;

    deployment.UpdatedAt =
        DateTime.UtcNow;

    await _context.SaveChangesAsync(
        cancellationToken);

    throw;
}
finally
{
    await _deploymentLockService.ReleaseAsync(
        deployment.ApplicationId);
}

    }
    private static int ExtractRevision(
        string taskDefinitionArn)
    {
        var revision = taskDefinitionArn.Split(':').Last();

        return int.Parse(revision);
    }
    
}