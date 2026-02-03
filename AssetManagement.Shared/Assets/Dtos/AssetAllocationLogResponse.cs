namespace AssetManagement.Shared.Assets.Dtos;

public sealed record AssetAllocationLogResponse
{
    
    public long Id { get; set; }
    public long AssetId { get; set; }
    public string AssetName { get; set; } = string.Empty;
    public long UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public DateTimeOffset AllocatedAtUtc { get; set; }
}