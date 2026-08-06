using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShipIt.Core.Models;

namespace ShipIt.Infrastructure.Persistence.Configurations;

public class RepositoryConfiguration : IEntityTypeConfiguration<SourceRepository>
{
    public void Configure(EntityTypeBuilder<SourceRepository> builder)
    {
        builder.ToTable("SourceRepository");

        builder.HasKey(x => x.RepositoryId);

        builder.Property(x => x.RepositoryName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.DefaultBranch)
            .HasMaxLength(100)
            .HasDefaultValue("main");

        builder.HasOne(x => x.Application)
            .WithOne(x => x.SourceRepository)
            .HasForeignKey<SourceRepository>(x => x.ApplicationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.ApplicationId)
            .IsUnique();
        builder.Property(x => x.RepositoryOwner)
            .HasMaxLength(200)
            .IsRequired();
    }
}