namespace AssetManagement.Shared.AssetTypes.Dtos;

public sealed record CreateAssetTypeRequest{
    public string Name { get; set; } = string.Empty;
}