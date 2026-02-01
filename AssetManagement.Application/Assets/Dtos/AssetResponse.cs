namespace AssetManagement.Application.Assets.Dtos;

public sealed record AssetResponse(
    long Id,
    string Name,
    string SerialNumber,
    long AssetTypeId,
    string? AssetTypeName,
    string Status,
    decimal Value,
    long? AssignedToUserId,
    DateTimeOffset? AssignedAt
);