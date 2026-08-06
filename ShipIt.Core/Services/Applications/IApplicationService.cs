namespace ShipIt.Core.Services;

public interface IApplicationService
{
    Task<ApplicationResponse> CreateAsync(
        Guid ownerId,
        CreateApplicationRequest request);

    Task<ApplicationResponse?> GetByIdAsync(
        Guid applicationId);
}