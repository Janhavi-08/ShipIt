using System.Diagnostics;

public class DockerBuildService : IDockerBuildService
{
    private readonly ILogger<DockerBuildService> _logger;

    public DockerBuildService(
        ILogger<DockerBuildService> logger)
    {
        _logger = logger;
    }

    public async Task<string> BuildAsync(
        string workspacePath,
        string imageTag,
        CancellationToken cancellationToken)
    {
        var dockerfilePath =
            Path.Combine(workspacePath, "Dockerfile");

        if (!File.Exists(dockerfilePath))
        {
            throw new InvalidOperationException(
                "Dockerfile was not found in the repository.");
        }

        var startInfo = new ProcessStartInfo
        {
            FileName = "docker",
            Arguments =
                $"build -t \"{imageTag}\" \"{workspacePath}\"",

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
            ReadOutputAsync(
                process.StandardOutput,
                cancellationToken);

        var errorTask =
            ReadOutputAsync(
                process.StandardError,
                cancellationToken);

        await process.WaitForExitAsync(
            cancellationToken);

        var output = await outputTask;
        var error = await errorTask;

        if (!string.IsNullOrWhiteSpace(output))
        {
            _logger.LogInformation(
                "Docker build output:\n{Output}",
                output);
        }

        if (process.ExitCode != 0)
        {
            _logger.LogError(
                "Docker build failed:\n{Error}",
                error);

            throw new InvalidOperationException(
                $"Docker build failed: {error}");
        }

        _logger.LogInformation(
            "Docker image {ImageTag} built successfully.",
            imageTag);

        return imageTag;
    }

    private static async Task<string> ReadOutputAsync(
        StreamReader reader,
        CancellationToken cancellationToken)
    {
        return await reader.ReadToEndAsync(
            cancellationToken);
    }
}