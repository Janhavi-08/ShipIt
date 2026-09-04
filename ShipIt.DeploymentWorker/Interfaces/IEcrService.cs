public interface IEcrService
{
    Task<string> PushImageAsync(
        string localImageTag,
        string repositoryName,
        string imageTag,
        CancellationToken cancellationToken);
    Task EnsureRepositoryExistsAsync(
        string repositoryName, DeploymentMessage message,
        CancellationToken cancellationToken);
                
}