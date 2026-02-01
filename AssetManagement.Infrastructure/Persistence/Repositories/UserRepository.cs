using AssetManagement.Application.Abstractions.Persistence;
using AssetManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AssetManagement.Infrastructure.Persistence.Repositories;

public sealed class UserRepository : IUserRepository
{
    private readonly AppDbContext _db;

    public UserRepository(AppDbContext db) => _db = db;

    public async Task<User?> GetByIdAsync(long id, CancellationToken ct)
        => await _db.Users.FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task<List<User>> GetAllAsyncNoTracking(CancellationToken ct)
        => await _db.Users.AsNoTracking().OrderBy(x => x.Name).ToListAsync(ct);

    public async Task<bool> ExistsAsync(long id, CancellationToken ct)
        => await _db.Users.AnyAsync(x => x.Id == id, ct);
    public async Task<bool> ExistsByEmailAsync(string normalizedEmail, CancellationToken ct)
        => await _db.Users.AnyAsync(x => x.Email == normalizedEmail, ct);

    public void Add(User user) => _db.Users.Add(user);

    public void Remove(User user) => _db.Users.Remove(user);
}