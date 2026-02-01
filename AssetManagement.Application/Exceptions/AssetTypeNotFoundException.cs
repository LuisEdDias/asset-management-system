namespace AssetManagement.Application.Exceptions;

public sealed class AssetTypeNotFoundException : AppException
{
    public long AssetTypeId { get; }
    public AssetTypeNotFoundException(long id)
        : base("AssetType.NotFound")
    {
        AssetTypeId = id;
    }
}