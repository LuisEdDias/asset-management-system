using Microsoft.EntityFrameworkCore;
using AssetManagement.Domain.Entities;
using AssetManagement.Domain.Enums;

namespace AssetManagement.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Asset> Assets => Set<Asset>();
    public DbSet<AssetType> AssetTypes => Set<AssetType>();
    public DbSet<User> Users => Set<User>();
    public DbSet<AssetAllocationLog> AssetAllocationLogs => Set<AssetAllocationLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        base.OnModelCreating(modelBuilder);

        // --- SEED DATA ---

        modelBuilder.Entity<AssetType>().HasData(
            new { Id = 1L, Name = "NOTEBOOK" },
            new { Id = 2L, Name = "MONITOR" },
            new { Id = 3L, Name = "DESKTOP" },
            new { Id = 4L, Name = "PERIFÉRICOS" }
        );

        modelBuilder.Entity<User>().HasData(
            new { Id = 1L, Name = "Ana", Email = "ana@mail.com" },
            new { Id = 2L, Name = "Luís", Email = "luis@mail.com" },
            new { Id = 3L, Name = "José", Email = "jose@mail.com" }
        );

        modelBuilder.Entity<Asset>().HasData(
        new
        {
            Id = 1L,
            Name = "MacBook Pro M3",
            SerialNumber = "SN123",
            Value = 15000.00m,
            AssetTypeId = 1L,
            Status = AssetStatus.Available
        },
        new
        {
            Id = 2L,
            Name = "Acer Aspire 5",
            SerialNumber = "SN456",
            Value = 3500.00m,
            AssetTypeId = 2L,
            Status = AssetStatus.Available
        },
        new
        {
            Id = 3L,
            Name = "Dell UltraSharp 27",
            SerialNumber = "SN789",
            Value = 3500.00m,
            AssetTypeId = 2L,
            Status = AssetStatus.Maintenance
        },
        new
        {
            Id = 4L,
            Name = "Dell Tower Plus Intel Core Ultra 5",
            SerialNumber = "SN987",
            Value = 9999.99m,
            AssetTypeId = 3L,
            Status = AssetStatus.Available
        },
        new
        {
            Id = 5L,
            Name = "Mouse Logitech",
            SerialNumber = "SN654",
            Value = 30.00m,
            AssetTypeId = 4L,
            Status = AssetStatus.Available
        },
        new
        {
            Id = 6L,
            Name = "Multifuncional HP",
            SerialNumber = "SN321",
            Value = 1500.00m,
            AssetTypeId = 4L,
            Status = AssetStatus.Maintenance
        }
        );
    }
}