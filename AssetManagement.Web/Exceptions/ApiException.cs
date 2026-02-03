namespace AssetManagement.Web.Exceptions;

public sealed class ApiException : Exception
{
    public int StatusCode { get; }
    public string? Title { get; }
    public string? Detail { get; }
    public Dictionary<string, string[]> ValidationErrors { get; }

    public ApiException(int statusCode, string? message, string? detail, Dictionary<string, string[]>? validationErrors = null)
        : base(message)
    {
        StatusCode = statusCode;
        Detail = detail;
        ValidationErrors = validationErrors ?? new Dictionary<string, string[]>();
    }
}