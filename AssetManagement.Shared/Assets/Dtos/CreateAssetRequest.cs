namespace AssetManagement.Shared.Assets.Dtos;

public sealed record CreateAssetRequest{
    public string Name { get; set; } = string.Empty;
    public string SerialNumber { get; set; } = string.Empty;
    public long AssetTypeId { get; set; }
    public decimal Value { get; set; }
}