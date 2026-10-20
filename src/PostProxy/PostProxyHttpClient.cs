using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using PostProxy.Exceptions;
using PostProxy.Models;

namespace PostProxy;

public class PostProxyHttpClient
{
    private readonly HttpClient _httpClient;

    internal static readonly JsonSerializerOptions JsonOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        NumberHandling = JsonNumberHandling.AllowReadingFromString,
    };

    // No naming policy: for payloads passed through to a platform untouched,
    // e.g. Google's camelCased Business Profile fields.
    private static readonly JsonSerializerOptions RawJsonOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    internal PostProxyHttpClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    internal async Task<T> GetAsync<T>(string path, Dictionary<string, string>? queryParams = null, CancellationToken cancellationToken = default)
    {
        var url = BuildUrl(path, queryParams);
        using var response = await _httpClient.GetAsync(url, cancellationToken);
        return await HandleResponseAsync<T>(response, cancellationToken);
    }

    internal Task<T> PostAsync<T>(string path, object? body = null, Dictionary<string, string>? queryParams = null, CancellationToken cancellationToken = default, string? idempotencyKey = null, IReadOnlyDictionary<string, string>? headers = null) =>
        SendJsonAsync<T>(HttpMethod.Post, path, body, queryParams, cancellationToken, idempotencyKey, headers);

    internal Task<T> PatchAsync<T>(string path, object? body = null, Dictionary<string, string>? queryParams = null, CancellationToken cancellationToken = default, string? idempotencyKey = null) =>
        SendJsonAsync<T>(HttpMethod.Patch, path, body, queryParams, cancellationToken, idempotencyKey, null);

    internal Task<T> PutAsync<T>(string path, object? body = null, Dictionary<string, string>? queryParams = null, CancellationToken cancellationToken = default, string? idempotencyKey = null) =>
        SendJsonAsync<T>(HttpMethod.Put, path, body, queryParams, cancellationToken, idempotencyKey, null);

    internal async Task<T> DeleteAsync<T>(string path, Dictionary<string, string>? queryParams = null, CancellationToken cancellationToken = default, string? idempotencyKey = null)
    {
        var url = BuildUrl(path, queryParams);
        using var request = new HttpRequestMessage(HttpMethod.Delete, url);
        AddIdempotencyKey(request, idempotencyKey);
        using var response = await _httpClient.SendAsync(request, cancellationToken);
        return await HandleResponseAsync<T>(response, cancellationToken);
    }

    /// <summary>GET that returns the raw response body instead of parsing JSON.</summary>
    internal async Task<BinaryResponse> GetBytesAsync(string path, Dictionary<string, string>? queryParams = null, CancellationToken cancellationToken = default)
    {
        var url = BuildUrl(path, queryParams);
        using var response = await _httpClient.GetAsync(url, cancellationToken);
        if (!response.IsSuccessStatusCode)
            await ThrowForStatusAsync(response, cancellationToken);

        var data = await response.Content.ReadAsByteArrayAsync(cancellationToken);
        return new BinaryResponse
        {
            Data = data,
            ContentType = response.Content.Headers.ContentType?.MediaType,
        };
    }

    internal Task<T> PostMultipartAsync<T>(
        string path,
        Dictionary<string, string>? queryParams,
        IEnumerable<KeyValuePair<string, string>> formFields,
        IEnumerable<(string FieldName, string FilePath)> files,
        CancellationToken cancellationToken = default,
        string? idempotencyKey = null,
        IReadOnlyDictionary<string, string>? headers = null) =>
        SendMultipartAsync<T>(HttpMethod.Post, path, queryParams, formFields, files, cancellationToken, idempotencyKey, headers);

    internal Task<T> PatchMultipartAsync<T>(
        string path,
        Dictionary<string, string>? queryParams,
        IEnumerable<KeyValuePair<string, string>> formFields,
        IEnumerable<(string FieldName, string FilePath)> files,
        CancellationToken cancellationToken = default,
        string? idempotencyKey = null) =>
        SendMultipartAsync<T>(HttpMethod.Patch, path, queryParams, formFields, files, cancellationToken, idempotencyKey, null);

    private async Task<T> SendJsonAsync<T>(
        HttpMethod method,
        string path,
        object? body,
        Dictionary<string, string>? queryParams,
        CancellationToken cancellationToken,
        string? idempotencyKey,
        IReadOnlyDictionary<string, string>? headers)
    {
        var url = BuildUrl(path, queryParams);
        using var content = body is not null
            ? new StringContent(JsonSerializer.Serialize(body, JsonOptions), Encoding.UTF8, "application/json")
            : null;
        using var request = new HttpRequestMessage(method, url) { Content = content };
        AddIdempotencyKey(request, idempotencyKey);
        AddHeaders(request, headers);
        using var response = await _httpClient.SendAsync(request, cancellationToken);
        return await HandleResponseAsync<T>(response, cancellationToken);
    }

    private async Task<T> SendMultipartAsync<T>(
        HttpMethod method,
        string path,
        Dictionary<string, string>? queryParams,
        IEnumerable<KeyValuePair<string, string>> formFields,
        IEnumerable<(string FieldName, string FilePath)> files,
        CancellationToken cancellationToken,
        string? idempotencyKey,
        IReadOnlyDictionary<string, string>? headers)
    {
        var url = BuildUrl(path, queryParams);
        using var content = new MultipartFormDataContent($"----PostProxy{Guid.NewGuid():N}");

        foreach (var (key, value) in formFields)
        {
            content.Add(new StringContent(value), key);
        }

        foreach (var (fieldName, filePath) in files)
        {
            var fileStream = File.OpenRead(filePath);
            var streamContent = new StreamContent(fileStream);
            streamContent.Headers.ContentType = new MediaTypeHeaderValue(GetMimeType(filePath));
            content.Add(streamContent, fieldName, Path.GetFileName(filePath));
        }

        using var request = new HttpRequestMessage(method, url) { Content = content };
        AddIdempotencyKey(request, idempotencyKey);
        AddHeaders(request, headers);
        using var response = await _httpClient.SendAsync(request, cancellationToken);
        return await HandleResponseAsync<T>(response, cancellationToken);
    }

    private static void AddHeaders(HttpRequestMessage request, IReadOnlyDictionary<string, string>? headers)
    {
        if (headers is null) return;
        foreach (var (name, value) in headers)
            request.Headers.TryAddWithoutValidation(name, value);
    }

    internal static string EnumValue<TEnum>(TEnum value) where TEnum : struct, Enum =>
        JsonSerializer.Serialize(value, JsonOptions).Trim('"');

    internal static JsonElement Raw(object value) =>
        value is JsonElement element ? element : JsonSerializer.SerializeToElement(value, RawJsonOptions);

    private static void AddIdempotencyKey(HttpRequestMessage request, string? idempotencyKey)
    {
        if (idempotencyKey is not null)
            request.Headers.Add("Idempotency-Key", idempotencyKey);
    }

    private static string BuildUrl(string path, Dictionary<string, string>? queryParams)
    {
        if (queryParams is null or { Count: 0 })
            return path;

        var query = string.Join("&", queryParams
            .Where(kv => kv.Value is not null)
            .Select(kv => $"{Uri.EscapeDataString(kv.Key)}={Uri.EscapeDataString(kv.Value)}"));

        return $"{path}?{query}";
    }

    private static async Task<T> HandleResponseAsync<T>(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (!response.IsSuccessStatusCode)
            await ThrowForStatusAsync(response, cancellationToken);

        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<T>(body, JsonOptions)
            ?? throw new PostProxyException("Failed to deserialize response", (int)response.StatusCode);
    }

    private static async Task ThrowForStatusAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        var message = ExtractErrorMessage(body);
        Dictionary<string, object>? responseDict = null;
        try
        {
            responseDict = JsonSerializer.Deserialize<Dictionary<string, object>>(body, JsonOptions);
        }
        catch
        {
            // ignore parse errors for error response
        }

        throw PostProxyException.FromStatusCode((int)response.StatusCode, message, responseDict, ParseRetryAfter(response));
    }

    // Retry-After in seconds; null when absent, non-positive, or an HTTP date.
    private static int? ParseRetryAfter(HttpResponseMessage response)
    {
        var seconds = response.Headers.RetryAfter?.Delta?.TotalSeconds;
        return seconds is > 0 ? (int)seconds.Value : null;
    }

    private static string ExtractErrorMessage(string body)
    {
        try
        {
            using var doc = JsonDocument.Parse(body);
            if (doc.RootElement.TryGetProperty("error", out var error))
                return error.GetString() ?? body;
            if (doc.RootElement.TryGetProperty("message", out var message))
                return message.GetString() ?? body;
        }
        catch
        {
            // not JSON
        }

        return body;
    }

    private static string GetMimeType(string filePath) =>
        Path.GetExtension(filePath).ToLowerInvariant() switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".webp" => "image/webp",
            ".mp4" => "video/mp4",
            ".mov" => "video/quicktime",
            ".avi" => "video/x-msvideo",
            ".webm" => "video/webm",
            _ => "application/octet-stream",
        };
}
