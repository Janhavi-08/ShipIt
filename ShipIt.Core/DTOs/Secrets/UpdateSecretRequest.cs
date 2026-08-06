namespace ShipIt.Core.DTOs.Secrets;

public class UpdateSecretRequest
{
    public string Key { get; set; } = string.Empty;

    public string Value { get; set; } = string.Empty;

    public bool IsEnabled { get; set; }
}