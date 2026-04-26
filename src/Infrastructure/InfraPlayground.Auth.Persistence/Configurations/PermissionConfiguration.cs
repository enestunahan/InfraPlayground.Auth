using InfraPlayground.Auth.Domain.Entities.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InfraPlayground.Auth.Persistence.Configurations;

public sealed class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        builder.ToTable("Permissions");

        builder.HasKey(permission => permission.Id);

        builder.Property(permission => permission.Code)
            .HasMaxLength(200)
            .IsRequired();

        builder.HasIndex(permission => permission.Code)
            .IsUnique();

        builder.Property(permission => permission.Description)
            .HasMaxLength(500)
            .IsRequired();
    }
}
