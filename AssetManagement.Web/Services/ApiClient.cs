using AssetManagement.Web.Models;
using AssetManagement.Web.Exceptions;

namespace AssetManagement.Web.Services;

public sealed class ApiClient
{
    private readonly HttpClient _http;
    public ApiClient(HttpClient http) => _http = http;

    public Task<T> GetAsync<T>(string url, CancellationToken ct = default)
        => SendAsync<T>(() => _http.GetAsync(url, ct), ct);

    public Task<T> PostAsync<T>(string url, object body, CancellationToken ct = default)
        => SendAsync<T>(() => _http.PostAsJsonAsync(url, body, ct), ct);

    public Task PostAsync(string url, object? body, CancellationToken ct = default)
        => SendAsync<object>(() => body is null ? _http.PostAsync(url, null, ct) : _http.PostAsJsonAsync(url, body, ct), ct);

    public Task<T> PutAsync<T>(string url, object body, CancellationToken ct = default)
        => SendAsync<T>(() => _http.PutAsJsonAsync(url, body, ct), ct);

    public Task DeleteAsync(string url, CancellationToken ct = default)
        => SendAsync<object>(() => _http.DeleteAsync(url, ct), ct);

    private async Task<T> SendAsync<T>(Func<Task<HttpResponseMessage>> send, CancellationToken ct)
    {
        var resp = await send();

        if (resp.IsSuccessStatusCode)
        {
            if (typeof(T) == typeof(object))
                return default!;

            var data = await resp.Content.ReadFromJsonAsync<T>(cancellationToken: ct);
            return data!;
        }

        throw await CreateApiException(resp, ct);
    }

    private static async Task<ApiException> CreateApiException(HttpResponseMessage resp, CancellationToken ct)
    {
        ApiProblemDetails? problem = null;

        try
        {
            problem = await resp.Content.ReadFromJsonAsync<ApiProblemDetails>(cancellationToken: ct);
        }
        catch { }

        return new ApiException(
            (int)resp.StatusCode, 
            problem?.Title ?? resp.ReasonPhrase, 
            problem?.Detail,
            problem?.Errors
        );
    }
}