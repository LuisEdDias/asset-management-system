namespace AssetManagement.Application.Exceptions;

public sealed class AssetTypeDuplicateNameException : AppException
{
    public string AssetTypeName { get; }

    public AssetTypeDuplicateNameException(string assetTypeName)
        : base("AssetType.DuplicateName")
    {
        AssetTypeName = assetTypeName;
    }
}