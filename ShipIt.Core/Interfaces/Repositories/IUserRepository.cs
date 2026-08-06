using ShipIt.Core.Models;

namespace ShipIt.Core.Interfaces.Repositories;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid userId);

    Task<User?> GetByEmailAsync(string email);

    Task<User?> GetByUsernameAsync(string username);

    Task<bool> ExistsByEmailAsync(string email);

    Task<bool> ExistsByUsernameAsync(string username);

    Task AddAsync(User user);

    Task SaveChangesAsync();
}