using ShipIt.Core.Models;

namespace ShipIt.Core.Interfaces.Repositories;

public interface IApplicationRepository
{
    Task<bool> ExistsAsync(Guid ownerId, string name);

    Task AddAsync(Application application);

    Task<Application?> GetByIdAsync(Guid applicationId);

    Task<Application?> GetByNameAsync(Guid ownerId, string name);
}