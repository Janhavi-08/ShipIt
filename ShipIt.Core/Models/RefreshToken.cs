namespace ShipIt.Core.Models;

public class RefreshToken
{
    public Guid RefreshTokenId { get; set; }

    public Guid UserId { get; set; }

    public string Token { get; set; } = string.Empty;

    public DateTime ExpiresAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? RevokedAt { get; set; }

    public bool IsRevoked { get; set; }

    // Navigation Property
    public User User { get; set; } = null!;
}