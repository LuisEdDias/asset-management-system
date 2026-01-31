namespace AssetManagement.Application.Exceptions;

public sealed class AssetNotAvailableException : AppException
{
    public long AssetId { get; }

    public AssetNotAvailableException(long assetId)
        : base("Asset.NotAvailable")
    {
        AssetId = assetId;
    }
}