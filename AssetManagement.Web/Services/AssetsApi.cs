using AssetManagement.Shared.Assets.Dtos;

namespace AssetManagement.Web.Services;

public sealed class AssetsApi
{
    private readonly ApiClient _api;
    public AssetsApi(ApiClient api) => _api = api;

    public Task<List<AssetResponse>> GetAllAsync(CancellationToken ct)
        => _api.GetAsync<List<AssetResponse>>("assets", ct);

    public Task<AssetResponse> GetByIdAsync(long id, CancellationToken ct)
        => _api.GetAsync<AssetResponse>($"assets/{id}", ct);

    public Task<AssetResponse> GetByAssetSerialAsync(string assetSerial, CancellationToken ct)
        => _api.GetAsync<AssetResponse>($"assets/by-serial/{assetSerial}", ct);

    public Task<List<AssetResponse>> GetByUserAsync(long userId, CancellationToken ct)
        => _api.GetAsync<List<AssetResponse>>($"assets/by-user/{userId}", ct);

    public Task<AssetResponse> CreateAsync(CreateAssetRequest req, CancellationToken ct)
        => _api.PostAsync<AssetResponse>("assets", req, ct);

    public Task<AssetResponse> UpdateAsync(long id, UpdateAssetRequest req, CancellationToken ct)
        => _api.PutAsync<AssetResponse>($"assets/{id}", req, ct);

    public Task AllocateAsync(long assetId, long userId, CancellationToken ct)
        => _api.PostAsync($"assets/{assetId}/allocate", new AllocateAssetRequest { UserId = userId }, ct);

    public Task ReturnAsync(long assetId, CancellationToken ct)
        => _api.PostAsync($"assets/{assetId}/return", body: null, ct);

    public Task MarkMaintenanceAsync(long assetId, CancellationToken ct)
        => _api.PostAsync($"assets/{assetId}/maintenance", body: null, ct);

    public Task CompleteMaintenanceAsync(long assetId, CancellationToken ct)
        => _api.PostAsync($"assets/{assetId}/maintenance/complete", body: null, ct);

    public Task<List<AssetAllocationLogResponse>> GetHistoryAsync(CancellationToken ct)
        => _api.GetAsync<List<AssetAllocationLogResponse>>("assets/history", ct);

    public Task<List<AssetAllocationLogResponse>> GetHistoryByAssetAsync(long assetId, CancellationToken ct)
        => _api.GetAsync<List<AssetAllocationLogResponse>>($"assets/{assetId}/history", ct);
}