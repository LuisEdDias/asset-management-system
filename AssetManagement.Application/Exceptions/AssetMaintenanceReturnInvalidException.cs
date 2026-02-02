namespace AssetManagement.Application.Exceptions;

public sealed class AssetMaintenanceReturnValidationException : AppException
{
    public long AssetId { get; }

    public AssetMaintenanceReturnValidationException(long assetId)
        : base("Asset.MaintenanceReturnInvalid")
    {
        AssetId = assetId;
    }
}