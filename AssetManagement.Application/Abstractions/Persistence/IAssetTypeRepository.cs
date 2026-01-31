using AssetManagement.Domain.Entities;

namespace AssetManagement.Application.Abstractions.Persistence;

public interface IAssetTypeRepository
{
    Task<List<AssetType>> GetAllAsyncNoTracking(CancellationToken ct);
    Task<AssetType?> GetByIdAsync(long id, CancellationToken ct);

    Task<bool> ExistsByNameAsync(string normalizedName, CancellationToken ct);

    Task<bool> IsInUseAsync(long assetTypeId, CancellationToken ct);
    
    void Add(AssetType assetType);
    void Remove(AssetType assetType);
}