namespace AssetManagement.Shared.AssetTypes.Dtos;

public sealed record AssetTypeResponse{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
}