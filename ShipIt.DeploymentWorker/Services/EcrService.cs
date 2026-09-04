
using System.Diagnostics;
using Amazon.ECR;
using Amazon.ECR.Model;

public class EcrService : IEcrService
{
    private readonly IAmazonECR _ecrClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<EcrService> _logger;

    private readonly string _region;

    public EcrService(
        IAmazonECR ecrClient,
        IConfiguration configuration,
        ILogger<EcrService> logger)
    {
        _ecrClient = ecrClient;
        _configuration = configuration;
        _logger = logger;

        _region =
            configuration["AWS:Region"]
            ?? throw new InvalidOperationException(
                "AWS region is not configured.");
    }

    public async Task<string> PushImageAsync(
        string localImageTag,
        string repositoryName,
        string imageTag,
        CancellationToken cancellationToken)
    {
        var authResponse =
            await _ecrClient.GetAuthorizationTokenAsync(
                new GetAuthorizationTokenRequest(),
                cancellationToken);

        var authorizationData =
            authResponse.AuthorizationData.FirstOrDefault();

        if (authorizationData == null)
        {
            throw new InvalidOperationException(
                "Unable to obtain ECR authorization data.");
        }

        var token =
            authorizationData.AuthorizationToken;

        var proxyEndpoint =
            authorizationData.ProxyEndpoint;

        if (string.IsNullOrWhiteSpace(token) ||
            string.IsNullOrWhiteSpace(proxyEndpoint))
        {
            throw new InvalidOperationException(
                "Invalid ECR authorization response.");
        }

        await DockerLoginAsync(
            token,
            proxyEndpoint,
            cancellationToken);

        var registry =
            proxyEndpoint.Replace(
                "https://",
                string.Empty);

        var ecrImage =
            $"{registry}/{repositoryName}:{imageTag}";

        await RunDockerCommandAsync(
            $"tag \"{localImageTag}\" \"{ecrImage}\"",
            cancellationToken);

        _logger.LogInformation(
            "Docker image tagged as {EcrImage}.",
            ecrImage);

        await RunDockerCommandAsync(
            $"push \"{ecrImage}\"",
            cancellationToken);

        _logger.LogInformation(
            "Docker image pushed successfully to ECR: {EcrImage}.",
            ecrImage);

        return ecrImage;
    }

    private async Task DockerLoginAsync(
        string authorizationToken,
        string proxyEndpoint,
        CancellationToken cancellationToken)
    {
        var decodedToken =
            Convert.FromBase64String(
                authorizationToken);

        var credentials =
            System.Text.Encoding.UTF8.GetString(
                decodedToken);

        var separator =
            credentials.IndexOf(':');

        if (separator <= 0)
        {
            throw new InvalidOperationException(
                "Invalid ECR authorization token.");
        }

        var username =
            credentials[..separator];

        var password =
            credentials[(separator + 1)..];

        var startInfo = new ProcessStartInfo
        {
            FileName = "docker",
            Arguments =
                $"login \"{proxyEndpoint}\" " +
                $"--username \"{username}\" " +
                "--password-stdin",

            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,

            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = new Process
        {
            StartInfo = startInfo
        };

        process.Start();

        await process.StandardInput.WriteAsync(
            password.AsMemory(),
            cancellationToken);

        await process.StandardInput.WriteLineAsync();

        process.StandardInput.Close();

        var output =
            await process.StandardOutput.ReadToEndAsync(
                cancellationToken);

        var error =
            await process.StandardError.ReadToEndAsync(
                cancellationToken);

        await process.WaitForExitAsync(
            cancellationToken);

        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException(
                $"Docker login to ECR failed: {error}");
        }

        _logger.LogInformation(
            "Docker authenticated successfully with ECR.");
    }

    private static async Task RunDockerCommandAsync(
        string arguments,
        CancellationToken cancellationToken)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "docker",
            Arguments = arguments,

            RedirectStandardOutput = true,
            RedirectStandardError = true,

            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = new Process
        {
            StartInfo = startInfo
        };

        process.Start();

        var outputTask =
            process.StandardOutput.ReadToEndAsync(
                cancellationToken);

        var errorTask =
            process.StandardError.ReadToEndAsync(
                cancellationToken);

        await process.WaitForExitAsync(
            cancellationToken);

        var output = await outputTask;
        var error = await errorTask;

        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException(
                $"Docker command failed: {error}");
        }
    }
    public async Task EnsureRepositoryExistsAsync(
    string repositoryName, DeploymentMessage  message,
    CancellationToken cancellationToken)
{
    try
    {
        var response =
            await _ecrClient.DescribeRepositoriesAsync(
                new DescribeRepositoriesRequest
                {
                    RepositoryNames =
                        new List<string>
                        {
                            repositoryName
                        }
                },
                cancellationToken);

        if (response.Repositories.Count > 0)
        {
            _logger.LogInformation(
                "ECR repository {RepositoryName} already exists.",
                repositoryName);

            return;
        }
    }
    catch (RepositoryNotFoundException)
    {
        // Repository doesn't exist.
    }

    _logger.LogInformation(
        "Creating ECR repository {RepositoryName}.",
        repositoryName);

    await _ecrClient.CreateRepositoryAsync(
    new CreateRepositoryRequest
    {
        RepositoryName = repositoryName,
        Tags =
        [
            new Tag
            {
                Key = "ManagedBy",
                Value = "ShipIt"
            },
            new Tag
            {
                Key = "ApplicationId",
                Value = message.ApplicationId.ToString()
            }
        ]
    },
    cancellationToken);

    _logger.LogInformation(
        "ECR repository {RepositoryName} created successfully.",
        repositoryName);
}
}