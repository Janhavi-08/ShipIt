using ShipIt.Core.Models;

namespace ShipIt.Core.Interfaces.Repositories;

public interface IRefreshTokenRepository
{
    Task AddAsync(RefreshToken refreshToken);

    Task<RefreshToken?> GetByTokenAsync(string token);

    Task SaveChangesAsync();
}