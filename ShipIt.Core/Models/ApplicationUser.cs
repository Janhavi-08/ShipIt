using ShipIt.Core.Enums;

namespace ShipIt.Core.Models;

public class ApplicationUser
{
    public Guid ApplicationUserId { get; set; }

    public Guid ApplicationId { get; set; }

    public Guid UserId { get; set; }

    public ApplicationRole Role { get; set; }

    public DateTime CreatedAt { get; set; }

    // Navigation
    public Application Application { get; set; } = null!;

    public User User { get; set; } = null!;
}