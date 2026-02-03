using AssetManagement.Shared.AssetTypes.Dtos;

namespace AssetManagement.Web.Services;

public sealed class AssetTypesApi
{
    private readonly ApiClient _api;
    public AssetTypesApi(ApiClient api) => _api = api;

    public Task<List<AssetTypeResponse>> GetAllAsync(CancellationToken ct)
        => _api.GetAsync<List<AssetTypeResponse>>("asset-types", ct);

    public Task<AssetTypeResponse> CreateAsync(CreateAssetTypeRequest req, CancellationToken ct)
        => _api.PostAsync<AssetTypeResponse>("asset-types", req, ct);

    public Task<AssetTypeResponse> UpdateAsync(long id, UpdateAssetTypeRequest req, CancellationToken ct)
        => _api.PutAsync<AssetTypeResponse>($"asset-types/{id}", req, ct);

    public Task DeleteAsync(long id, CancellationToken ct)
        => _api.DeleteAsync($"asset-types/{id}", ct);
}
