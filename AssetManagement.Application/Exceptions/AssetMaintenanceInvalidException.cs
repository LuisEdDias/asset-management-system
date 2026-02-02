namespace AssetManagement.Application.Exceptions;

public sealed class AssetMaintenanceValidationException : AppException
{
    public long AssetId { get; }

    public AssetMaintenanceValidationException(long assetId)
        : base("Asset.MaintenanceInvalid")
    {
        AssetId = assetId;
    }
}