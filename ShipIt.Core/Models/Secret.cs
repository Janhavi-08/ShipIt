namespace ShipIt.Core.Models;

public class Secret
{
    public Guid SecretId { get; set; }

    public Guid DeploymentConfigurationId { get; set; }

    public string Key { get; set; } = string.Empty;

    public string EncryptedValue { get; set; } = string.Empty;

    public bool IsEnabled { get; set; } = true;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public DeploymentConfiguration DeploymentConfiguration { get; set; } = null!;
}