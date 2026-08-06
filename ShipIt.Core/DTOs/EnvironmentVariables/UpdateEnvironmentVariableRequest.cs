namespace ShipIt.Core.DTOs.EnvironmentVariables;

public class UpdateEnvironmentVariableRequest
{
    public string Key { get; set; } = string.Empty;

    public string? Value { get; set; }

    public bool IsEnabled { get; set; }
}