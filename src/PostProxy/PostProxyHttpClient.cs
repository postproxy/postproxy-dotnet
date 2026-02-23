using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using PostProxy.Exceptions;

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

    internal async Task<T> PostAsync<T>(string path, object? body = null, Dictionary<string, string>? queryParams = null, CancellationToken cancellationToken = default)
    {
        var url = BuildUrl(path, queryParams);
        using var content = body is not null
            ? new StringContent(JsonSerializer.Serialize(body, JsonOptions), Encoding.UTF8, "application/json")
            : null;
        using var response = await _httpClient.PostAsync(url, content, cancellationToken);
        return await HandleResponseAsync<T>(response, cancellationToken);
    }

    internal async Task<T> DeleteAsync<T>(string path, Dictionary<string, string>? queryParams = null, CancellationToken cancellationToken = default)
    {
        var url = BuildUrl(path, queryParams);
        using var response = await _httpClient.DeleteAsync(url, cancellationToken);
        return await HandleResponseAsync<T>(response, cancellationToken);
    }

    internal async Task<T> PostMultipartAsync<T>(
        string path,
        Dictionary<string, string>? queryParams,
        Dictionary<string, string> formFields,
        IEnumerable<string> filePaths,
        CancellationToken cancellationToken = default)
    {
        var url = BuildUrl(path, queryParams);
        using var content = new MultipartFormDataContent($"----PostProxy{Guid.NewGuid():N}");

        foreach (var (key, value) in formFields)
        {
            content.Add(new StringContent(value), key);
        }

        foreach (var filePath in filePaths)
        {
            var fileStream = File.OpenRead(filePath);
            var streamContent = new StreamContent(fileStream);
            streamContent.Headers.ContentType = new MediaTypeHeaderValue(GetMimeType(filePath));
            content.Add(streamContent, "media[]", Path.GetFileName(filePath));
        }

        using var response = await _httpClient.PostAsync(url, content, cancellationToken);
        return await HandleResponseAsync<T>(response, cancellationToken);
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
        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
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

            throw PostProxyException.FromStatusCode((int)response.StatusCode, message, responseDict);
        }

        return JsonSerializer.Deserialize<T>(body, JsonOptions)
            ?? throw new PostProxyException("Failed to deserialize response", (int)response.StatusCode);
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
