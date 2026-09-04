using Amazon.ApplicationAutoScaling;
using Amazon.ECR;
using Amazon.ECS;
using Amazon.ElasticLoadBalancingV2;
using Amazon.Route53;
using Amazon.SecretsManager;
using Amazon.SQS;
using Microsoft.EntityFrameworkCore;
using ShipIt.Core.Interfaces.Repositories;
using ShipIt.DeploymentWorker;
using ShipIt.DeploymentWorker.Services;
using ShipIt.Infrastructure.Persistence;
using ShipIt.Infrastructure.Repositories;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();

builder.Services.AddSingleton<IAmazonSQS>(
    new AmazonSQSClient());

builder.Services.AddHostedService<Worker>();
builder.Services.AddScoped<DeploymentProcessor>();
builder.Services.AddScoped<
    ISourceRepositoryRepository,
    SourceRepositoryRepository>();
var connectionString =
    builder.Configuration.GetConnectionString(
        "DefaultConnection");

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "DefaultConnection is not configured.");
}

builder.Services.AddDbContext<ShipItDbContext>(
    options =>
        options.UseNpgsql(connectionString));


builder.Services.AddScoped<IDeploymentWorkspaceService,
DeploymentWorkspaceService>();
builder.Services.AddScoped<
    IEcrService,
    EcrService>();

builder.Services.AddScoped<
    IGitRepositoryService,
    GitRepositoryService>();
builder.Services.AddScoped<
    IDockerBuildService,
    DockerBuildService>();
builder.Services.AddSingleton<IAmazonECR>(
    new AmazonECRClient());
builder.Services.AddSingleton<IAmazonECS>(
    new AmazonECSClient());
builder.Services.AddScoped<
IEcsService,
EcsService>();
builder.Services.AddScoped<
IEnvironmentVariableRepository,
EnvironmentVariableRepository>();
builder.Services.AddSingleton<IAmazonSecretsManager>(
new AmazonSecretsManagerClient());
builder.Services.AddSingleton<IAmazonSecretsManager>(
new AmazonSecretsManagerClient());
builder.Services.AddScoped<
ISecretsManagerService,
SecretsManagerService>();
builder.Services.AddScoped<
ISecretRepository,
SecretRepository>();

builder.Services.AddSingleton<IAmazonElasticLoadBalancingV2>(
    new AmazonElasticLoadBalancingV2Client());
builder.Services.AddSingleton<IAmazonApplicationAutoScaling>(
new AmazonApplicationAutoScalingClient());
builder.Services.AddSingleton<IAmazonRoute53>(new AmazonRoute53Client());
builder.Services.AddScoped<IDeploymentLockService,DeploymentLockService>();
var host = builder.Build();
host.Run();
