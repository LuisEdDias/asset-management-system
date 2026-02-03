using AssetManagement.Domain.Entities;

namespace AssetManagement.Application.Abstractions.Persistence;

public interface IAssetRepository
{
    Task<Asset?> GetByIdAsync(long id, CancellationToken ct);
    Task<Asset?> GetBySerialAsync(string normalizedSerial, CancellationToken ct);
    Task<List<Asset>> GetAllAsyncNoTracking(CancellationToken ct);
    Task<List<Asset>> GetAllByAllocatedUserIdAsync(long userId, CancellationToken ct);

    Task<bool> ExistsBySerialAsync(string normalizedSerial, CancellationToken ct);

    void Add(Asset asset);
}