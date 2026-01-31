namespace AssetManagement.Application.Exceptions;

public sealed class AssetReturnInvalidException : AppException
{
    public long AssetId { get; }

    public AssetReturnInvalidException(long assetId)
        : base("Asset.ReturnInvalid")
    {
        AssetId = assetId;
    }
}