using AssetManagement.Application.Abstractions.Persistence;
using AssetManagement.Application.Assets.Dtos;
using AssetManagement.Application.Users.Dtos;
using AssetManagement.Application.Exceptions;
using AssetManagement.Domain.Entities;

namespace AssetManagement.Application.Users;

public sealed class UserService
{
    private readonly IUserRepository _users;
    private readonly IAssetAllocationLogRepository _logRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UserService(IUserRepository users, IAssetAllocationLogRepository logs, IUnitOfWork uow)
    {
        _users = users;
        _logRepository = logs;
        _unitOfWork = uow;
    }

    public async Task<List<UserResponse>> GetAllAsync(CancellationToken ct)
    {
        var users = await _users.GetAllAsyncNoTracking(ct);
        return users.Select(ToResponse).ToList();
    }

    public async Task<UserResponse> GetByIdAsync(long id, CancellationToken ct)
    {
        var user = await _users.GetByIdAsync(id, ct);
        if (user is null)
            throw new UserNotFoundException(id);

        return ToResponse(user);
    }

    public async Task<UserResponse> CreateAsync(CreateUserRequest request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);

        var normalizedEmail = NormalizeEmail(request.Email);

        if (await _users.ExistsByEmailAsync(normalizedEmail, ct))
            throw new UserDuplicateEmailException(request.Email);

        var user = new User(request.Name, request.Email);
        _users.Add(user);

        await _unitOfWork.SaveChangesAsync(ct);

        return ToResponse(user);
    }

    public async Task<UserResponse> UpdateAsync(long id, UpdateUserRequest request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);

        var user = await _users.GetByIdAsync(id, ct) 
            ?? throw new UserNotFoundException(id);

        var normalizedEmail = NormalizeEmail(request.Email);

        if (!string.Equals(user.Email, normalizedEmail, StringComparison.OrdinalIgnoreCase))
        {
            if (await _users.ExistsByEmailAsync(normalizedEmail, ct))
                throw new UserDuplicateEmailException(request.Email);
        }

        user.UpdateName(request.Name);
        user.UpdateEmail(request.Email);

        await _unitOfWork.SaveChangesAsync(ct);

        return ToResponse(user);
    }

    public async Task<List<AssetAllocationLogResponse>> GetAllocationLogAsync(long id, CancellationToken ct)
    {
        var logs = await _logRepository.GetHistoryAsync(null, id, ct);

        return logs.Select(l => new AssetAllocationLogResponse(
            l.Id,
            l.AssetId,
            l.Asset.Name,
            l.UserId,
            l.User.Name,
            l.Action.ToString(),
            l.AtUtc
        )).ToList();
    }

    private static string NormalizeEmail(string email)
        => email.Trim().ToLowerInvariant();

    private static UserResponse ToResponse(User u)
        => new(u.Id, u.Name, u.Email);
}
