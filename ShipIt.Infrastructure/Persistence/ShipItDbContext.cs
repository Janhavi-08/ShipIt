using Microsoft.EntityFrameworkCore;
using ShipIt.Core.Models;

namespace ShipIt.Infrastructure.Persistence;

public class ShipItDbContext : DbContext
{
    public ShipItDbContext(DbContextOptions<ShipItDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();

    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Application> Applications => Set<Application>();

    public DbSet<SourceRepository> SourceRepositories => Set<SourceRepository>();
    public DbSet<DeploymentConfiguration> DeploymentConfigurations => Set<DeploymentConfiguration>();
    public DbSet<ApplicationUser> ApplicationUsers => Set<ApplicationUser>();
    public DbSet<EnvironmentVariable> EnvironmentVariables => Set<EnvironmentVariable>();
    public DbSet<Secret> Secrets => Set<Secret>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ShipItDbContext).Assembly);
    }
}