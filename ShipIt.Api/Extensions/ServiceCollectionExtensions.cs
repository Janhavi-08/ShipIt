using Microsoft.EntityFrameworkCore;
using ShipIt.Infrastructure.Persistence;
using ShipIt.Infrastructure.Extensions;
using ShipIt.Infrastructure.Authentication;
using ShipIt.Core.Interfaces.Repositories;
using ShipIt.Infrastructure.Repositories;
using ShipIt.Core.Services.Authentication;
using ShipIt.Core.Services;
using ShipIt.Infrastructure.Services;
namespace ShipIt.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<ShipItDbContext>(options =>
        {
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"));
        });
        services.AddInfrastructureServices();

        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.Configure<JwtSettings>(
            configuration.GetSection(JwtSettings.SectionName));

        services.AddScoped<IJwtService, JwtService>();

        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

        services.AddScoped<IUserRepository, UserRepository>();

        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

        services.AddScoped<IPasswordHasher, PasswordHasher>();

        services.AddScoped<IJwtService, JwtService>();

        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddScoped<IApplicationService, ApplicationService>();
        services.AddScoped<IApplicationRepository, ApplicationRepository>();

        services.AddScoped<ISourceRepositoryRepository, SourceRepositoryRepository>();

        services.AddScoped<IApplicationUserRepository, ApplicationUserRepository>();
        services.AddScoped<IDeploymentOptionsService, DeploymentOptionsService>();
        services.AddScoped<IDeploymentConfigurationRepository, DeploymentConfigurationRepository>();
        services.AddScoped<IDeploymentConfigurationService, DeploymentConfigurationService>();
       
        services.AddScoped<IEnvironmentVariableService, EnvironmentVariableService>();

        services.AddScoped<IEnvironmentVariableRepository, EnvironmentVariableRepository>();
        services.AddDataProtection();

        services.AddScoped<IEncryptionService, EncryptionService>();
        services.AddScoped<ISecretService, SecretService>();

        services.AddScoped<ISecretRepository, SecretRepository>();
        return services;
    }
}
