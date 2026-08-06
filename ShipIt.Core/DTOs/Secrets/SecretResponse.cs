public class SecretResponse
{
    public Guid SecretId { get; set; }

    public Guid DeploymentConfigurationId { get; set; }

    public string Key { get; set; } = string.Empty;

    public bool IsConfigured { get; set; }

    public bool IsEnabled { get; set; }
}