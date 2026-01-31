using AssetManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AssetManagement.Infrastructure.Persistence.Configurations;

public sealed class AssetAllocationLogConfig : IEntityTypeConfiguration<AssetAllocationLog>
{
    public void Configure(EntityTypeBuilder<AssetAllocationLog> b)
    {
        b.ToTable("asset_allocation_logs");

        b.HasKey(x => x.Id);

        b.Property(x => x.Action)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        b.Property(x => x.AtUtc)
            .IsRequired();

        b.HasOne(x => x.Asset)
            .WithMany(a => a.AllocationLogs)
            .HasForeignKey(x => x.AssetId)
            .OnDelete(DeleteBehavior.Restrict);

        b.HasOne(x => x.User)
            .WithMany(u => u.AllocationLogs)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        b.HasIndex(x => x.AssetId);
        b.HasIndex(x => x.UserId);

        b.HasIndex(x => new { x.AssetId, x.AtUtc });
    }
}