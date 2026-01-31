using AssetManagement.Domain.Enums;

namespace AssetManagement.Domain.Entities;

public class Asset
{
    public long Id { get; private set; }
    public string Name { get; private set; } = default!;
    public string SerialNumber { get; private set; } = default!;
    public AssetType Type { get; private set; } = default!;
    public decimal Value { get; private set; }
    public AssetStatus Status { get; private set; } = AssetStatus.Available;

    public long? AssignedToUserId { get; private set; }
    public User? AssignedToUser { get; private set; }
    public DateTimeOffset? AssignedAt { get; private set; }

    public ICollection<AssetAllocationLog> AllocationLogs { get; private set; } = [];

    private Asset() { }

    public Asset(string name, string serialNumber, AssetType type, decimal value)
    {
        Name = string.IsNullOrWhiteSpace(name) 
        ? throw new ArgumentException("Name is required.") 
        : name.Trim();

        SerialNumber = string.IsNullOrWhiteSpace(serialNumber) 
        ? throw new ArgumentException("SerialNumber is required.") 
        : serialNumber.Trim();

        Value = value < 0 
        ? throw new ArgumentOutOfRangeException("Value must be >= 0.", nameof(value)) 
        : value;

        Type = type ?? throw new ArgumentNullException("Type is required.", nameof(type));

        Status = AssetStatus.Available;
    }

    public void AllocateTo(long userId)
    {
        if (Status != AssetStatus.Available)
            throw new InvalidOperationException("Asset is not available for allocation.");

        if (AssignedToUserId is not null)
            throw new InvalidOperationException("Asset already has an assigned user.");

        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(userId);

        AssignedToUserId = userId;
        AssignedAt = DateTimeOffset.UtcNow;
        Status = AssetStatus.InUse;
    }

    public void Return()
    {
        if (Status != AssetStatus.InUse)
            throw new InvalidOperationException("Only in-use assets can be returned.");

        if (AssignedToUserId is null)
            throw new InvalidOperationException("Inconsistent state: InUse asset without assigned user.");

        AssignedToUserId = null;
        AssignedAt = null;
        Status = AssetStatus.Available;
    }

    public void MarkMaintenance()
    {
        if (Status == AssetStatus.InUse)
            throw new InvalidOperationException("Cannot set maintenance while asset is in use.");

        Status = AssetStatus.Maintenance;
    }

    public void CompleteMaintenance(bool isOperational)
    {
        if (Status != AssetStatus.Maintenance)
            throw new InvalidOperationException("Only assets under maintenance can complete maintenance.");

        Status = isOperational ? AssetStatus.Available : AssetStatus.Irrecoverable;
    }
}