namespace AssetManagement.Application.Exceptions;

public sealed class AssetTypeInUseException : Exception
{
    public string AssetTypeName { get; }
    public AssetTypeInUseException(string assetTypeName)
        : base("AssetType.InUse")
    {
        AssetTypeName = assetTypeName;
    }
}