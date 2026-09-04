using System.Diagnostics;

public class GitRepositoryService : IGitRepositoryService
{
    private readonly ILogger<GitRepositoryService> _logger;

    public GitRepositoryService(
        ILogger<GitRepositoryService> logger)
    {
        _logger = logger;
    }

    public async Task CloneAsync(
        string repositoryUrl,
        string branch,
        string workspacePath,
        CancellationToken cancellationToken)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "git",
            Arguments =
                $"clone --branch \"{branch}\" " +
                $"--single-branch " +
                $"\"{repositoryUrl}\" " +
                $"\"{workspacePath}\"",

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

        if (!string.IsNullOrWhiteSpace(output))
        {
            _logger.LogInformation(
                "Git output: {Output}",
                output);
        }

        if (process.ExitCode != 0)
        {
            _logger.LogError(
                "Git clone failed: {Error}",
                error);

            throw new InvalidOperationException(
                $"Git clone failed: {error}");
        }

        _logger.LogInformation(
            "Repository cloned successfully.");
    }
}