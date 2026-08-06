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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ShipItDbContext).Assembly);
    }
}