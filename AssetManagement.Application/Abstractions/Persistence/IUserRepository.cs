using AssetManagement.Domain.Entities;

namespace AssetManagement.Application.Abstractions.Persistence;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(long id, CancellationToken ct);
    Task<List<User>> GetAllAsyncNoTracking(CancellationToken ct);

    Task<bool> ExistsAsync(long id, CancellationToken ct);
    Task<bool> ExistsByEmailAsync(string normalizedEmail, CancellationToken ct);

    void Add(User user);
    void Remove(User user);
}