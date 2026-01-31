namespace AssetManagement.Application.Exceptions;

public sealed class AllocationConflictException : AppException
{
    public long AllocationId { get; }

    public AllocationConflictException(long allocationId)
        : base("Allocation.Conflict")
    {
        AllocationId = allocationId;
    }
}