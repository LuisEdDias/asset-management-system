using AssetManagement.Application.Abstractions.Persistence;
using AssetManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AssetManagement.Infrastructure.Persistence.Repositories;

public sealed class AssetAllocationLogRepository : IAssetAllocationLogRepository
{
    private readonly AppDbContext _db;
    public AssetAllocationLogRepository(AppDbContext db) => _db = db;

    public void Add(AssetAllocationLog log) => _db.AssetAllocationLogs.Add(log);

    public async Task<List<AssetAllocationLog>> GetHistoryAsync(long? assetId, long? userId, CancellationToken ct)
    {
        IQueryable<AssetAllocationLog> query = _db.AssetAllocationLogs
            .Include(l => l.Asset)
            .Include(l => l.User)
            .AsNoTracking();

        if (assetId.HasValue)
            query = query.Where(l => l.AssetId == assetId.Value);

        if (userId.HasValue)
            query = query.Where(l => l.UserId == userId.Value);

        return await query
            .OrderByDescending(l => l.AtUtc)
            .ToListAsync(ct);
    }
}