using ShipIt.Core.Models;

namespace ShipIt.Core.Interfaces.Repositories;

public interface ISourceRepositoryRepository
{
    Task AddAsync(SourceRepository repository);
}