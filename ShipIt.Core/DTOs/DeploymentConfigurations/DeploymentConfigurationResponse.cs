using ShipIt.Core.Deployment;
using ShipIt.Core.Deployment.Enums;

namespace ShipIt.Core.DTOs.DeploymentConfiguration;

public class DeploymentConfigurationResponse
{
    public Guid DeploymentConfigurationId { get; set; }

    public Guid ApplicationId { get; set; }

    public int ContainerPort { get; set; }

    public FargateCpu Cpu { get; set; }

    public FargateMemory Memory { get; set; }

    public int MinimumInstances { get; set; }

    public int MaximumInstances { get; set; }

    public string HealthCheckPath { get; set; } = string.Empty;

    public HealthCheckInterval HealthCheckInterval { get; set; }

    public HealthCheckTimeout HealthCheckTimeout { get; set; }

    public HealthCheckThreshold HealthyThreshold { get; set; }

    public HealthCheckThreshold UnhealthyThreshold { get; set; }

    public string Subdomain { get; set; } = string.Empty;

    public bool EnableHttps { get; set; }
}