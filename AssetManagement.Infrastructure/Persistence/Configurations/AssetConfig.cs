using AssetManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AssetManagement.Infrastructure.Persistence.Configurations;

public sealed class AssetConfig : IEntityTypeConfiguration<Asset>
{
    public void Configure(EntityTypeBuilder<Asset> b)
    {
        b.ToTable("assets", t =>
        {
            t.HasCheckConstraint(
                "ck_assets_assignment_consistency",
                @"
                (
                (""Status"" = 'InUse' AND ""AssignedToUserId"" IS NOT NULL AND ""AssignedAtUtc"" IS NOT NULL)
                OR
                (""Status"" <> 'InUse' AND ""AssignedToUserId"" IS NULL AND ""AssignedAtUtc"" IS NULL)
                )
                ".Trim()
            );
        });

        b.HasKey(x => x.Id);

        b.Property(x => x.Name)
            .HasMaxLength(160)
            .IsRequired();

        b.Property(x => x.SerialNumber)
            .HasMaxLength(80)
            .IsRequired();

        b.HasIndex(x => x.SerialNumber)
            .IsUnique();

        b.Property(x => x.Value)
            .HasColumnType("numeric(18,2)")
            .IsRequired();

        b.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        b.HasOne(x => x.Type)
            .WithMany(t => t.Assets)
            .HasForeignKey(x => x.AssetTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        b.HasIndex(x => x.AssetTypeId);

        b.Property(x => x.AssignedAtUtc);

        b.HasOne(x => x.AssignedToUser)
            .WithMany(u => u.AssignedAssets)
            .HasForeignKey(x => x.AssignedToUserId)
            .OnDelete(DeleteBehavior.Restrict);

        b.HasIndex(x => x.AssignedToUserId);

        b.HasMany(x => x.AllocationLogs)
            .WithOne(l => l.Asset)
            .HasForeignKey(l => l.AssetId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}