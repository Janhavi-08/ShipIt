public class DeploymentWorkspaceService
    : IDeploymentWorkspaceService
{
    private readonly ILogger<DeploymentWorkspaceService> _logger;

    private readonly string _rootPath =
        Path.Combine(
            Path.GetTempPath(),
            "shipit-deployments");

    public DeploymentWorkspaceService(
        ILogger<DeploymentWorkspaceService> logger)
    {
        _logger = logger;
    }

    public Task<string> CreateAsync(
        Guid deploymentId,
        CancellationToken cancellationToken)
    {
        var workspacePath = Path.Combine(
            _rootPath,
            $"deployment-{deploymentId}");

        Directory.CreateDirectory(workspacePath);

        _logger.LogInformation(
            "Created deployment workspace: {WorkspacePath}",
            workspacePath);

        return Task.FromResult(workspacePath);
    }

    public Task CleanupAsync(
        string workspacePath,
        CancellationToken cancellationToken)
    {
        if (Directory.Exists(workspacePath))
        {
            Directory.Delete(
                workspacePath,
                recursive: true);

            _logger.LogInformation(
                "Cleaned deployment workspace: {WorkspacePath}",
                workspacePath);
        }

        return Task.CompletedTask;
    }
}