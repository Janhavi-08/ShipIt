using ShipIt.Core.Models;

public interface IJwtService
{
    string GenerateAccessToken(User user);

    string GenerateRefreshToken();
}