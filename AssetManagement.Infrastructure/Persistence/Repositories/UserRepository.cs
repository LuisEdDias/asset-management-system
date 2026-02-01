using AssetManagement.Application.Abstractions.Persistence;
using AssetManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AssetManagement.Infrastructure.Persistence.Repositories;

public sealed class UserRepository : IUserRepository
{
    private readonly AppDbContext _db;

    public UserRepository(AppDbContext db) => _db = db;

    public Task<User?> GetByIdAsync(long id, CancellationToken ct)
        => _db.Users.FirstOrDefaultAsync(x => x.Id == id, ct);

    public Task<List<User>> GetAllAsyncNoTracking(CancellationToken ct)
        => _db.Users.AsNoTracking().OrderBy(x => x.Name).ToListAsync(ct);

    public Task<bool> ExistsByEmailAsync(string normalizedEmail, CancellationToken ct)
        => _db.Users.AnyAsync(x => x.Email == normalizedEmail, ct);

    public void Add(User user) => _db.Users.Add(user);

    public void Remove(User user) => _db.Users.Remove(user);
}