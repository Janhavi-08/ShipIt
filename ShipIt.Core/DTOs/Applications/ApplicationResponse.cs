public class ApplicationResponse
{
    public Guid ApplicationId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string RepositoryName { get; set; } = string.Empty;

    public string Owner { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
}