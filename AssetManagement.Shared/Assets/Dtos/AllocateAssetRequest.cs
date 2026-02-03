namespace AssetManagement.Shared.Assets.Dtos;

public sealed record AllocateAssetRequest
{
    public long UserId { get; set; }
}