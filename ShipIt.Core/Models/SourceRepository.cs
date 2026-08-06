using ShipIt.Core.Enums;

namespace ShipIt.Core.Models;

public class SourceRepository
{
    public Guid RepositoryId { get; set; }

    public Guid ApplicationId { get; set; }

    public SourceRepositoryProvider Provider { get; set; }

    public string RepositoryName { get; set; } = string.Empty;
    
    public string RepositoryOwner { get; set; } = string.Empty;
    
    public string DefaultBranch { get; set; } = "main";

    public bool IsPrivate { get; set; }

    public DateTime CreatedAt { get; set; }

    // Navigation
    public Application Application { get; set; } = null!;
}