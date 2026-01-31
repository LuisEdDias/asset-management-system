using AssetManagement.Application.Abstractions.Persistence;
using AssetManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AssetManagement.Infrastructure.Persistence.Repositories;

public sealed class AssetTypeRepository : IAssetTypeRepository
{
    private readonly AppDbContext _db;

    public AssetTypeRepository(AppDbContext db) => _db = db;

    public Task<List<AssetType>> GetAllAsyncNoTracking(CancellationToken ct)
        => _db.AssetTypes.AsNoTracking().OrderBy(x => x.Name).ToListAsync(ct);

    public Task<AssetType?> GetByIdAsync(long id, CancellationToken ct)
        => _db.AssetTypes.FirstOrDefaultAsync(x => x.Id == id, ct);

    public Task<bool> ExistsByNameAsync(string normalizedName, CancellationToken ct)
        => _db.AssetTypes.AnyAsync(x => x.Name == normalizedName, ct);

    public Task<bool> IsInUseAsync(long assetTypeId, CancellationToken ct)
        => _db.Assets.AnyAsync(x => x.AssetTypeId == assetTypeId, ct);
        
    public void Add(AssetType assetType) => _db.AssetTypes.Add(assetType);

    public void Remove(AssetType assetType) => _db.AssetTypes.Remove(assetType);
}