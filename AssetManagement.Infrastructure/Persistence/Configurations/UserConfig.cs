using AssetManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AssetManagement.Infrastructure.Persistence.Configurations;

public sealed class UserConfig : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> b)
    {
        b.ToTable("users");

        b.HasKey(x => x.Id);

        b.Property(x => x.Name)
            .HasMaxLength(120)
            .IsRequired();

        b.Property(x => x.Email)
            .HasMaxLength(200)
            .IsRequired();

        b.HasIndex(x => x.Email)
            .IsUnique();

        b.HasMany(x => x.AssignedAssets)
            .WithOne(a => a.AssignedToUser)
            .HasForeignKey(a => a.AssignedToUserId)
            .OnDelete(DeleteBehavior.Restrict);

        b.HasMany(x => x.AllocationLogs)
            .WithOne(l => l.User)
            .HasForeignKey(l => l.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}