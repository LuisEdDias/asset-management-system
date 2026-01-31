using AssetManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AssetManagement.Infrastructure.Persistence.Configurations;

public sealed class AssetTypeConfig : IEntityTypeConfiguration<AssetType>
{
    public void Configure(EntityTypeBuilder<AssetType> b)
    {
        b.ToTable("asset_types");

        b.HasKey(x => x.Id);

        b.Property(x => x.Name)
            .HasMaxLength(60)
            .IsRequired();

        b.HasIndex(x => x.Name)
            .IsUnique();

        b.HasMany(x => x.Assets)
            .WithOne(a => a.Type)
            .HasForeignKey(a => a.AssetTypeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}