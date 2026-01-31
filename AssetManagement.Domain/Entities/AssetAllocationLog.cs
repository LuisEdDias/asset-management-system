using AssetManagement.Domain.Enums;

namespace AssetManagement.Domain.Entities;

public class AssetAllocationLog
{
    public long Id { get; private set; }

    public long AssetId { get; private set; }
    public Asset Asset { get; private set; } = default!;

    public long UserId { get; private set; }
    public User User { get; private set; } = default!;

    public AllocationAction Action { get; private set; }
    public DateTimeOffset AtUtc { get; private set; }

    private AssetAllocationLog() { }

    public AssetAllocationLog(long assetId, long userId, AllocationAction action, DateTimeOffset atUtc)
    {
        AssetId = assetId;
        UserId = userId;
        Action = action;
        AtUtc = atUtc;
    }
}