namespace AssetManagement.Application.Exceptions;

public sealed class EntityNotFoundException : AppException
{
    public long EntityId { get; }
    public EntityNotFoundException(long id)
        : base("Common.NotFound")
    {
        EntityId = id;
    }
}