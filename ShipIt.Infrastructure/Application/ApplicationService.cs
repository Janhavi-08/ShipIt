using ShipIt.Core.Enums;
using ShipIt.Core.Interfaces.Repositories;
using ShipIt.Core.Models;
using ShipIt.Core.Services;
using ShipIt.Infrastructure.Persistence;

public class ApplicationService : IApplicationService
{

    private readonly IApplicationRepository _applicationRepository;
    private readonly ISourceRepositoryRepository _sourceRepositoryRepository;
    private readonly IApplicationUserRepository _applicationUserRepository;
    private readonly ShipItDbContext _context;


    public ApplicationService(
        IApplicationRepository applicationRepository,
        ISourceRepositoryRepository sourceRepositoryRepository,
        IApplicationUserRepository applicationUserRepository,
        ShipItDbContext context)
    {
        _applicationRepository = applicationRepository;
        _sourceRepositoryRepository = sourceRepositoryRepository;
        _applicationUserRepository = applicationUserRepository;
        _context = context;
    }

    public async Task<ApplicationResponse> CreateAsync(
        Guid ownerId,
        CreateApplicationRequest request)
    {
        var applicationName = request.Application.Name.Trim();

        if (await _applicationRepository.ExistsAsync(ownerId, applicationName))
            throw new Exception("Application already exists.");

        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var now = DateTime.UtcNow;

            var application = new Application
            {
                ApplicationId = Guid.NewGuid(),
                OwnerId = ownerId,
                Name = applicationName,
                Description = request.Application.Description.Trim(),
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            };

            await _applicationRepository.AddAsync(application);

            var repository = new SourceRepository
            {
                RepositoryId = Guid.NewGuid(),
                ApplicationId = application.ApplicationId,
                Provider = request.SourceRepository.Provider,
                RepositoryOwner = request.SourceRepository.Owner.Trim(),
                RepositoryName = request.SourceRepository.RepositoryName.Trim(),
                DefaultBranch = request.SourceRepository.DefaultBranch.Trim(),
                IsPrivate = request.SourceRepository.IsPrivate,
                CreatedAt = now
            };

            await _sourceRepositoryRepository.AddAsync(repository);

            var applicationUser = new ApplicationUser
            {
                ApplicationUserId = Guid.NewGuid(),
                ApplicationId = application.ApplicationId,
                UserId = ownerId,
                Role = ApplicationRole.Owner,
                CreatedAt = now
            };

            await _applicationUserRepository.AddAsync(applicationUser);

            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            return new ApplicationResponse
            {
                ApplicationId = application.ApplicationId,
                Name = application.Name,
                Description = application.Description,
                RepositoryName = repository.RepositoryName,
                Owner = repository.RepositoryOwner,
                CreatedAt = application.CreatedAt
            };
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
public async Task<ApplicationResponse?> GetByIdAsync(Guid applicationId)
{
    var application = await _applicationRepository.GetByIdAsync(applicationId);

    if (application == null)
        return null;

    return new ApplicationResponse
    {
        ApplicationId = application.ApplicationId,
        Name = application.Name,
        Description = application.Description,
        RepositoryName = application.SourceRepository.RepositoryName,
        Owner = application.SourceRepository.RepositoryOwner,
        CreatedAt = application.CreatedAt
    };
}
    
}