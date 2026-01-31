using AssetManagement.Domain.Enums;

namespace AssetManagement.Domain.Entities;

public class Asset
{
    public long Id { get; private set; }
    public string Name { get; private set; } = default!;
    public string SerialNumber { get; private set; } = default!;
    public decimal Value { get; private set; }
    public AssetStatus Status { get; private set; } = AssetStatus.Available;

    public long AssetTypeId { get; private set; }
    public AssetType Type { get; private set; } = default!;

    public long? AssignedToUserId { get; private set; }
    public User? AssignedToUser { get; private set; }
    public DateTimeOffset? AssignedAtUtc { get; private set; }

    public ICollection<AssetAllocationLog> AllocationLogs { get; private set; } = [];

    private Asset() { }

    public Asset(string name, string serialNumber, long assetTypeId, decimal value)
    {
        UpdateName(name);
        UpdateSerialNumber(serialNumber);
        ChangeType(assetTypeId);
        UpdateValue(value);

        Status = AssetStatus.Available;
        ClearAssignment();
    }

    public void UpdateName(string name)
    {
        Name = string.IsNullOrWhiteSpace(name)
            ? throw new ArgumentException("Name is required.", nameof(name))
            : name.Trim();
    }

    public void UpdateSerialNumber(string serialNumber)
    {
        SerialNumber = string.IsNullOrWhiteSpace(serialNumber)
            ? throw new ArgumentException("SerialNumber is required.", nameof(serialNumber))
            : serialNumber.Trim();
    }

    public void UpdateValue(decimal value)
    {
        if (value < 0)
            throw new ArgumentOutOfRangeException(nameof(value), "Value must be >= 0.");

        Value = value;
    }

    public void ChangeType(long assetTypeId)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(assetTypeId);

        AssetTypeId = assetTypeId;
    }

    public void AllocateTo(long userId, DateTimeOffset nowUtc)
    {
        if (Status != AssetStatus.Available)
            throw new InvalidOperationException("Asset is not available for allocation.");

        if (AssignedToUserId is not null)
            throw new InvalidOperationException("Asset already has an assigned user.");

        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(userId);

        AssignedToUserId = userId;
        AssignedAtUtc = nowUtc;
        Status = AssetStatus.InUse;

        EnsureConsistency();
    }

    public void Return()
    {
        if (Status != AssetStatus.InUse)
            throw new InvalidOperationException("Only in-use assets can be returned.");

        if (AssignedToUserId is null)
            throw new InvalidOperationException("Inconsistent state: InUse asset without assigned user.");

        ClearAssignment();
        Status = AssetStatus.Available;

        EnsureConsistency();
    }

    public void MarkMaintenance()
    {
        if (Status == AssetStatus.InUse)
            throw new InvalidOperationException("Cannot set maintenance while asset is in use.");

        ClearAssignment();
        Status = AssetStatus.Maintenance;

        EnsureConsistency();
    }

    public void CompleteMaintenance(bool isOperational)
    {
        if (Status != AssetStatus.Maintenance)
            throw new InvalidOperationException("Only assets under maintenance can complete maintenance.");

        Status = AssetStatus.Available;
        ClearAssignment();

        EnsureConsistency();
    }

    private void ClearAssignment()
    {
        AssignedToUserId = null;
        AssignedAtUtc = null;
    }

    private void EnsureConsistency()
    {
        if (Status == AssetStatus.InUse)
        {
            if (AssignedToUserId is null || AssignedAtUtc is null)
                throw new InvalidOperationException("Inconsistent state: InUse requires AssignedToUserId and AssignedAtUtc.");
        }
        else
        {
            if (AssignedToUserId is not null || AssignedAtUtc is not null)
                throw new InvalidOperationException("Inconsistent state: non-InUse requires no assigned user.");
        }
    }
}