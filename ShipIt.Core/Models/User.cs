using ShipIt.Core.Enums;

namespace ShipIt.Core.Models;

public class User
{
    public Guid UserId { get; set; }

    public string Username { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public PlatformRole Role { get; set; } = PlatformRole.User;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    // Navigation Properties
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();

    // public ICollection<ApplicationUser> Applications { get; set; } = new List<ApplicationUser>();
}