namespace AssetManagement.Shared.Assets.Dtos;

public sealed record AssetResponse{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string SerialNumber { get; set; } = string.Empty;
    public long AssetTypeId { get; set; }
    public string? AssetTypeName { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal Value { get; set; }
    public long? AssignedToUserId { get; set; }
    public string? AssignedToUserName { get; set; }
    public DateTimeOffset? AssignedAt { get; set; }
}