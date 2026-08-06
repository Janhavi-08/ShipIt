using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShipIt.Core.Models;

namespace ShipIt.Infrastructure.Persistence.Configurations;

public class EnvironmentVariableConfiguration
    : IEntityTypeConfiguration<EnvironmentVariable>
{
    public void Configure(EntityTypeBuilder<EnvironmentVariable> builder)
    {
        builder.ToTable("EnvironmentVariables");

        builder.HasKey(x => x.EnvironmentVariableId);

        builder.Property(x => x.Key)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.Value)
            .HasMaxLength(4000);

        builder.Property(x => x.IsEnabled)
            .HasDefaultValue(true);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .IsRequired();

        builder.HasOne(x => x.DeploymentConfiguration)
            .WithMany(x => x.EnvironmentVariables)
            .HasForeignKey(x => x.DeploymentConfigurationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new
        {
            x.DeploymentConfigurationId,
            x.Key
        }).IsUnique();
    }
}