using AssetManagement.Application.Abstractions.Persistence;
using AssetManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AssetManagement.Infrastructure.Persistence.Repositories;

public sealed class AssetRepository : IAssetRepository
{
    private readonly AppDbContext _db;
    public AssetRepository(AppDbContext db) => _db = db;

    public async Task<Asset?> GetByIdAsync(long id, CancellationToken ct)
        => await _db.Assets.FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task<List<Asset>> GetAllAsyncNoTracking(CancellationToken ct)
        => await _db.Assets.Include(t => t.Type).AsNoTracking().OrderBy(x => x.Name).ToListAsync(ct);
    public async Task<List<Asset>> GetAllByAllocatedUserIdAsync(long userId, CancellationToken ct)
        => await _db.Assets
            .Where(a => a.AssignedToUserId == userId)
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .ToListAsync(ct);

    public async Task<bool> ExistsBySerialAsync(string normalizedSerial, CancellationToken ct)
        => await _db.Assets.AnyAsync(x => x.SerialNumber == normalizedSerial, ct);

    public void Add(Asset asset) => _db.Assets.Add(asset);
}