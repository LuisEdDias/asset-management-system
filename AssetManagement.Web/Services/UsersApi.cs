using AssetManagement.Shared.Users.Dtos;

namespace AssetManagement.Web.Services;

public sealed class UsersApi
{
    private readonly ApiClient _api;
    public UsersApi(ApiClient api) => _api = api;

    public Task<List<UserResponse>> GetAllAsync(CancellationToken ct)
        => _api.GetAsync<List<UserResponse>>("users", ct);

    public Task<UserResponse> GetByIdAsync(long id, CancellationToken ct)
        => _api.GetAsync<UserResponse>($"users/{id}", ct);

    public Task<UserResponse> CreateAsync(CreateUserRequest req, CancellationToken ct)
        => _api.PostAsync<UserResponse>("users", req, ct);

    public Task<UserResponse> UpdateAsync(long id, UpdateUserRequest req, CancellationToken ct)
        => _api.PutAsync<UserResponse>($"users/{id}", req, ct);
}