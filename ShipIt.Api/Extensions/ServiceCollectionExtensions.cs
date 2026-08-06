using Microsoft.EntityFrameworkCore;
using ShipIt.Infrastructure.Persistence;
using ShipIt.Infrastructure.Extensions;
using ShipIt.Infrastructure.Authentication;
using ShipIt.Core.Interfaces.Repositories;
using ShipIt.Infrastructure.Repositories;
using ShipIt.Core.Services.Authentication;
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
            
           
        return services;
    }
}