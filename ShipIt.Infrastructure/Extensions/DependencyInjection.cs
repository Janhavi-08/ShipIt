using Microsoft.Extensions.DependencyInjection;
using ShipIt.Core.Interfaces.Repositories;
using ShipIt.Infrastructure.Repositories;

namespace ShipIt.Infrastructure.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        services.AddScoped<IUserRepository, UserRepository>();

        return services;
    }
}