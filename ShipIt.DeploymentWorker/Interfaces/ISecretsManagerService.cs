public interface ISecretsManagerService
{
    Task<string> CreateOrUpdateSecretAsync(
        string secretName,
        string secretValue,
        CancellationToken cancellationToken);
}