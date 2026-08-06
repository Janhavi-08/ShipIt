using ShipIt.Core.Deployment;
using ShipIt.Core.Deployment.Enums;

namespace ShipIt.Core.DTOs.DeploymentConfiguration;

public class CreateDeploymentConfigurationRequest
{
    // Runtime
    public int ContainerPort { get; set; }

    public FargateCpu Cpu { get; set; }

    public FargateMemory Memory { get; set; }

    // Scaling
    public int MinimumInstances { get; set; }

    public int MaximumInstances { get; set; }

    // Health Check
    public string HealthCheckPath { get; set; } = "/";

    public HealthCheckInterval HealthCheckInterval { get; set; }

    public HealthCheckTimeout HealthCheckTimeout { get; set; }

    public HealthCheckThreshold HealthyThreshold { get; set; }

    public HealthCheckThreshold UnhealthyThreshold { get; set; }

    // Networking
    public string Subdomain { get; set; } = string.Empty;

    public bool EnableHttps { get; set; }
}