public interface IDockerBuildService
{
    Task<string> BuildAsync(
        string workspacePath,
        string imageTag,
        CancellationToken cancellationToken);
}