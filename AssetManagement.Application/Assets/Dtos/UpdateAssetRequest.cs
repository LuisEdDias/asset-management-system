namespace AssetManagement.Application.Assets.Dtos;

public sealed record UpdateAssetRequest(string Name, long AssetTypeId, decimal Value);