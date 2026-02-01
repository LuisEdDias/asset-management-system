using AssetManagement.Domain.Entities;

namespace AssetManagement.Application.Abstractions.Persistence;

public interface IAssetAllocationLogRepository
{
    void Add(AssetAllocationLog log);

    Task<List<AssetAllocationLog>> GetHistoryAsync(long? assetId, long? userId, CancellationToken ct);
}