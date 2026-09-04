public interface IDeploymentWorkspaceService
{
    Task<string> CreateAsync(
        Guid deploymentId,
        CancellationToken cancellationToken);

    Task CleanupAsync(
        string workspacePath,
        CancellationToken cancellationToken);
}