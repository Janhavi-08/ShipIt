using ShipIt.Core.Enums;

namespace ShipIt.Core.Models;

public class Application
{
    public Guid ApplicationId { get; set; }

    public Guid OwnerId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    // Navigation
    public User Owner { get; set; } = null!;

    public SourceRepository SourceRepository { get; set; } = null!;

    public ICollection<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();

    // public ICollection<DeploymentProfile> DeploymentProfiles { get; set; } = new List<DeploymentProfile>();

    // public ICollection<ApplicationVersion> Versions { get; set; } = new List<ApplicationVersion>();
}