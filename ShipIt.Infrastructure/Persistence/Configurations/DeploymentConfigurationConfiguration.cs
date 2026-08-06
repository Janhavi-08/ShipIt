using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShipIt.Core.Models;

namespace ShipIt.Infrastructure.Persistence.Configurations;

public class DeploymentConfigurationConfiguration : IEntityTypeConfiguration<DeploymentConfiguration>
{
    public void Configure(EntityTypeBuilder<DeploymentConfiguration> builder)
    {
        builder.ToTable("DeploymentConfigurations");

        builder.HasKey(x => x.DeploymentConfigurationId);

        builder.Property(x => x.ContainerPort)
            .IsRequired();

        builder.Property(x => x.Cpu)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.Memory)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.MinimumInstances)
            .IsRequired();

        builder.Property(x => x.MaximumInstances)
            .IsRequired();

        builder.Property(x => x.HealthCheckPath)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.HealthCheckInterval)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.HealthCheckTimeout)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.HealthyThreshold)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.UnhealthyThreshold)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.Subdomain)
            .HasMaxLength(100);

        builder.Property(x => x.EnableHttps)
            .HasDefaultValue(true);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .IsRequired();

        builder.HasOne(x => x.Application)
            .WithOne(x => x.DeploymentConfiguration)
            .HasForeignKey<DeploymentConfiguration>(x => x.ApplicationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.ApplicationId)
            .IsUnique();
    }
}