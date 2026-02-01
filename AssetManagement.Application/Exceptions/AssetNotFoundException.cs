namespace AssetManagement.Application.Exceptions;

public sealed class AssetNotFoundException : AppException
{
    public long AssetId { get; }
    public AssetNotFoundException(long id) 
        : base("Asset.NotFound")
    {
        AssetId = id;
    }
}