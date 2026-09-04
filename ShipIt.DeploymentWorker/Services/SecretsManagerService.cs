using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;

namespace ShipIt.DeploymentWorker.Services;

public class SecretsManagerService : ISecretsManagerService
{
    private readonly IAmazonSecretsManager _client;
    private readonly ILogger<SecretsManagerService> _logger;

    public SecretsManagerService(
        IAmazonSecretsManager client,
        ILogger<SecretsManagerService> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<string> CreateOrUpdateSecretAsync(
        string secretName,
        string secretValue,
        CancellationToken cancellationToken)
    {
        try
        {
            var existing =
                await _client.DescribeSecretAsync(
                    new DescribeSecretRequest
                    {
                        SecretId = secretName
                    },
                    cancellationToken);

            await _client.PutSecretValueAsync(
                new PutSecretValueRequest
                {
                    SecretId = existing.ARN,
                    SecretString = secretValue
                },
                cancellationToken);

            _logger.LogInformation(
                "Updated Secrets Manager secret {SecretName}.",
                secretName);

            return existing.ARN;
        }
        catch (ResourceNotFoundException)
        {
            var response =
                await _client.CreateSecretAsync(
                    new CreateSecretRequest
                    {
                        Name = secretName,
                        SecretString = secretValue,
                        Tags =
                        [
                            new Tag
                            {
                                Key = "ManagedBy",
                                Value = "ShipIt"
                            }
                        ]
                    },
                    cancellationToken);

            _logger.LogInformation(
                "Created Secrets Manager secret {SecretName}.",
                secretName);

            return response.ARN;
        }
    }
}