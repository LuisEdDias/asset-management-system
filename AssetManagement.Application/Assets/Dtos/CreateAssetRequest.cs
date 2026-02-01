namespace AssetManagement.Application.Assets.Dtos;

public sealed record CreateAssetRequest(string Name, string SerialNumber, long AssetTypeId, decimal Value);