namespace AssetManagement.Application.Assets.Dtos;

public sealed record AssetAllocationLogResponse(
    long Id,
    long AssetId,
    string AssetName,
    long UserId,
    string UserName,
    string Action,
    DateTimeOffset AllocatedAtUtc);