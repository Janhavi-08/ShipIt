using ShipIt.Core.Models;

public interface IApplicationUserRepository
{
    Task AddAsync(ApplicationUser applicationUser);
}