namespace AssetManagement.Shared.Assets.Dtos;

public sealed record UpdateAssetRequest{
    public string Name { get; set; } = string.Empty;
    public long AssetTypeId { get; set; }
    public decimal Value { get; set; }
}