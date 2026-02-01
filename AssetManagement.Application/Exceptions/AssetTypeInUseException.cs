namespace AssetManagement.Application.Exceptions;

public sealed class AssetTypeInUseException : AppException
{
    public string AssetTypeName { get; }
    public AssetTypeInUseException(string assetTypeName)
        : base("AssetType.InUse")
    {
        AssetTypeName = assetTypeName;
    }
}