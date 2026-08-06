using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShipIt.Core.Constants;
using ShipIt.Core.Models;

namespace ShipIt.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(x => x.UserId);

        builder.Property(x => x.Username)
               .HasMaxLength(DatabaseConstants.UsernameMaxLength)
               .IsRequired();

        builder.Property(x => x.Email)
               .HasMaxLength(DatabaseConstants.EmailMaxLength)
               .IsRequired();

        builder.Property(x => x.PasswordHash)
               .IsRequired();

        builder.HasIndex(x => x.Username)
               .IsUnique();

        builder.HasIndex(x => x.Email)
               .IsUnique();
    }
}