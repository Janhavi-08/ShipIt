namespace ShipIt.Core.DTOs.EnvironmentVariables;

public class CreateEnvironmentVariableRequest
{
    public string Key { get; set; } = string.Empty;

    public string? Value { get; set; }

    public bool IsEnabled { get; set; } = true;
}