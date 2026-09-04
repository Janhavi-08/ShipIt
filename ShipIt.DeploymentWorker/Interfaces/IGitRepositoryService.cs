public interface IGitRepositoryService
{
    Task CloneAsync(
        string repositoryUrl,
        string branch,
        string workspacePath,
        CancellationToken cancellationToken);
}