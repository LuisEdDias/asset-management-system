namespace AssetManagement.Shared.AssetTypes.Dtos;

public sealed record UpdateAssetTypeRequest{
    public string Name { get; set; } = string.Empty;
}