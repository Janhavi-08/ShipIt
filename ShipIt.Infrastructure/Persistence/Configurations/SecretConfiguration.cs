using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShipIt.Core.Models;

namespace ShipIt.Infrastructure.Persistence.Configurations;

public class SecretConfiguration : IEntityTypeConfiguration<Secret>
{
    public void Configure(EntityTypeBuilder<Secret> builder)
    {
        builder.ToTable("Secrets");

        builder.HasKey(x => x.SecretId);

        builder.Property(x => x.Key)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.EncryptedValue)
            .HasColumnType("text")
            .IsRequired();

        builder.Property(x => x.IsEnabled)
            .HasDefaultValue(true);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .IsRequired();

        builder.HasOne(x => x.DeploymentConfiguration)
            .WithMany(x => x.Secrets)
            .HasForeignKey(x => x.DeploymentConfigurationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new
        {
            x.DeploymentConfigurationId,
            x.Key
        }).IsUnique();
    }
}