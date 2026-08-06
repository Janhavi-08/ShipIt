namespace ShipIt.Core.DTOs.EnvironmentVariables;

public class EnvironmentVariableResponse
{
    public Guid EnvironmentVariableId { get; set; }

    public Guid DeploymentConfigurationId { get; set; }

    public string Key { get; set; } = string.Empty;

    public string? Value { get; set; }

    public bool IsEnabled { get; set; }
}