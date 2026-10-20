using System.Text.Json;
using System.Text.Json.Serialization;
using PostProxy.Models;
using PostProxy.Parameters;

namespace PostProxy.Resources;

public class PostsResource
{
    private readonly PostProxyHttpClient _client;
    private readonly string? _defaultProfileGroupId;

    internal PostsResource(PostProxyHttpClient client, string? defaultProfileGroupId)
    {
        _client = client;
        _defaultProfileGroupId = defaultProfileGroupId;
    }

    public Task<PaginatedResponse<Post>> ListAsync(CancellationToken cancellationToken = default) =>
        ListAsync(null, cancellationToken);

    public Task<PaginatedResponse<Post>> ListAsync(ListPostsParams? parameters, CancellationToken cancellationToken = default)
    {
        var query = new Dictionary<string, string>();
        var profileGroupId = parameters?.ProfileGroupId ?? _defaultProfileGroupId;

        if (profileGroupId is not null)
            query["profile_group_id"] = profileGroupId;
        if (parameters?.Page is not null)
            query["page"] = parameters.Page.Value.ToString();
        if (parameters?.PerPage is not null)
            query["per_page"] = parameters.PerPage.Value.ToString();
        if (parameters?.Status is not null)
            query["status"] = JsonSerializer.Serialize(parameters.Status.Value, PostProxyHttpClient.JsonOptions).Trim('"');
        if (parameters?.Platforms is { Count: > 0 })
            query["platforms"] = string.Join(",", parameters.Platforms.Select(p =>
                JsonSerializer.Serialize(p, PostProxyHttpClient.JsonOptions).Trim('"')));
        if (parameters?.ScheduledAfter is not null)
            query["scheduled_at"] = parameters.ScheduledAfter.Value.ToString("O");

        return _client.GetAsync<PaginatedResponse<Post>>("/api/posts", query, cancellationToken);
    }

    public Task<Post> GetAsync(string id, CancellationToken cancellationToken = default) =>
        GetAsync(id, null, cancellationToken);

    public Task<Post> GetAsync(string id, string? profileGroupId, CancellationToken cancellationToken = default)
    {
        var query = new Dictionary<string, string>();
        var pgId = profileGroupId ?? _defaultProfileGroupId;
        if (pgId is not null)
            query["profile_group_id"] = pgId;

        return _client.GetAsync<Post>($"/api/posts/{Uri.EscapeDataString(id)}", query, cancellationToken);
    }

    public Task<Post> CreateAsync(CreatePostParams parameters, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(parameters);
        if (string.IsNullOrWhiteSpace(parameters.Body))
            throw new ArgumentException("Body is required.", nameof(parameters));
        if (parameters.Profiles is not { Count: > 0 })
            throw new ArgumentException("At least one profile is required.", nameof(parameters));

        var profileGroupId = parameters.ProfileGroupId ?? _defaultProfileGroupId;
        var headers = parameters.SkipDuplicateCheck
            ? new Dictionary<string, string> { ["X-Skip-Duplicate-Check"] = "true" }
            : null;

        var hasFileUploads = parameters.MediaFiles is { Count: > 0 }
            || parameters.Thread?.Any(t => t.MediaFiles is { Count: > 0 }) == true
            || HasLocalCoverFile(parameters.Platforms);

        if (hasFileUploads)
        {
            return CreateMultipartAsync(parameters, profileGroupId, headers, cancellationToken);
        }

        var body = new CreatePostBody
        {
            Post = new PostBody
            {
                Body = parameters.Body,
                ScheduledAt = parameters.ScheduledAt?.ToString("O"),
                Draft = parameters.Draft,
            },
            Profiles = parameters.Profiles,
            Media = parameters.Media,
            Platforms = parameters.Platforms,
            Thread = parameters.Thread,
            ProfileGroupId = profileGroupId,
            QueueId = parameters.QueueId,
            QueuePriority = parameters.QueuePriority,
        };

        return _client.PostAsync<Post>("/api/posts", body, cancellationToken: cancellationToken, idempotencyKey: parameters.IdempotencyKey, headers: headers);
    }

    private Task<Post> CreateMultipartAsync(CreatePostParams parameters, string? profileGroupId, IReadOnlyDictionary<string, string>? headers, CancellationToken cancellationToken)
    {
        var fields = new List<KeyValuePair<string, string>>
        {
            new("post[body]", parameters.Body),
        };

        if (parameters.ScheduledAt is not null)
            fields.Add(new("post[scheduled_at]", parameters.ScheduledAt.Value.ToString("O")));
        if (parameters.Draft is not null)
            fields.Add(new("post[draft]", parameters.Draft.Value.ToString().ToLowerInvariant()));
        if (parameters.QueueId is not null)
            fields.Add(new("queue_id", parameters.QueueId));
        if (parameters.QueuePriority is not null)
            fields.Add(new("queue_priority", parameters.QueuePriority));
        if (profileGroupId is not null)
            fields.Add(new("profile_group_id", profileGroupId));

        foreach (var profile in parameters.Profiles)
            fields.Add(new("profiles[]", profile));

        if (parameters.Media is not null)
        {
            foreach (var url in parameters.Media)
                fields.Add(new("media[]", url));
        }

        // Collect all file paths: cover files, parent media + thread child media
        var allFiles = new List<(string FieldName, string FilePath)>();

        AddPlatformFields(fields, allFiles, parameters.Platforms);

        if (parameters.MediaFiles is { Count: > 0 })
        {
            foreach (var filePath in parameters.MediaFiles)
                allFiles.Add(("media[]", filePath));
        }

        if (parameters.Thread is { Count: > 0 })
        {
            for (var i = 0; i < parameters.Thread.Count; i++)
            {
                var child = parameters.Thread[i];
                fields.Add(new($"thread[{i}][body]", child.Body));

                if (child.Media is { Count: > 0 })
                {
                    foreach (var url in child.Media)
                        fields.Add(new($"thread[{i}][media][]", url));
                }

                if (child.MediaFiles is { Count: > 0 })
                {
                    foreach (var filePath in child.MediaFiles)
                        allFiles.Add(($"thread[{i}][media][]", filePath));
                }
            }
        }

        return _client.PostMultipartAsync<Post>(
            "/api/posts",
            null,
            fields,
            allFiles,
            cancellationToken,
            parameters.IdempotencyKey,
            headers);
    }

    // `cover_file` is a local path unless it's a base64 data URL, which the API
    // takes as a plain string.
    private static bool IsLocalCoverFile(string? value) =>
        value is not null && !value.StartsWith("data:", StringComparison.Ordinal);

    private static bool HasLocalCoverFile(PlatformParams? platforms) =>
        platforms is not null && (
            IsLocalCoverFile(platforms.Instagram?.CoverFile)
            || IsLocalCoverFile(platforms.TikTok?.CoverFile)
            || IsLocalCoverFile(platforms.YouTube?.CoverFile)
            || IsLocalCoverFile(platforms.Pinterest?.CoverFile));

    // Platform params as flat form fields; a local `cover_file` is attached as a file.
    private static void AddPlatformFields(
        List<KeyValuePair<string, string>> fields,
        List<(string FieldName, string FilePath)> files,
        PlatformParams? platforms)
    {
        if (platforms is null) return;

        var root = JsonSerializer.SerializeToElement(platforms, PostProxyHttpClient.JsonOptions);
        foreach (var platform in root.EnumerateObject())
        {
            if (platform.Value.ValueKind != JsonValueKind.Object) continue;

            foreach (var param in platform.Value.EnumerateObject())
            {
                var key = $"platforms[{platform.Name}][{param.Name}]";
                if (param.Name == "cover_file" && param.Value.ValueKind == JsonValueKind.String
                    && IsLocalCoverFile(param.Value.GetString()))
                {
                    files.Add((key, param.Value.GetString()!));
                    continue;
                }

                AddFormValue(fields, key, param.Value);
            }
        }
    }

    private static void AddFormValue(List<KeyValuePair<string, string>> fields, string key, JsonElement value)
    {
        switch (value.ValueKind)
        {
            case JsonValueKind.Null:
            case JsonValueKind.Undefined:
                break;
            case JsonValueKind.Array:
                foreach (var item in value.EnumerateArray())
                    AddFormValue(fields, $"{key}[]", item);
                break;
            case JsonValueKind.Object:
                foreach (var prop in value.EnumerateObject())
                    AddFormValue(fields, $"{key}[{prop.Name}]", prop.Value);
                break;
            case JsonValueKind.String:
                fields.Add(new(key, value.GetString()!));
                break;
            case JsonValueKind.True:
                fields.Add(new(key, "true"));
                break;
            case JsonValueKind.False:
                fields.Add(new(key, "false"));
                break;
            default:
                fields.Add(new(key, value.GetRawText()));
                break;
        }
    }

    /// <summary>
    /// Updates a draft or scheduled post. On a <b>published</b> post only the body,
    /// the YouTube title and the Pinterest title / destination link can change; the
    /// edit is pushed to the platforms in the background, reported by the
    /// <c>platform_post.edited</c> / <c>platform_post.edit_failed</c> webhooks.
    /// </summary>
    public Task<Post> UpdateAsync(string id, UpdatePostParams parameters, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(parameters);

        var profileGroupId = parameters.ProfileGroupId ?? _defaultProfileGroupId;

        var hasFileUploads = parameters.MediaFiles is { Count: > 0 }
            || parameters.Thread?.Any(t => t.MediaFiles is { Count: > 0 }) == true
            || HasLocalCoverFile(parameters.Platforms);

        if (hasFileUploads)
        {
            return UpdateMultipartAsync(id, parameters, profileGroupId, cancellationToken);
        }

        var postBody = new Dictionary<string, object?>();
        if (parameters.Body is not null)
            postBody["body"] = parameters.Body;
        if (parameters.ScheduledAt is not null)
            postBody["scheduled_at"] = parameters.ScheduledAt.Value.ToString("O");
        if (parameters.Draft is not null)
            postBody["draft"] = parameters.Draft.Value;

        var body = new Dictionary<string, object?>();
        if (postBody.Count > 0)
            body["post"] = postBody;
        if (parameters.Profiles is not null)
            body["profiles"] = parameters.Profiles;
        if (parameters.Media is not null)
            body["media"] = parameters.Media;
        if (parameters.Platforms is not null)
            body["platforms"] = parameters.Platforms;
        if (parameters.Thread is not null)
            body["thread"] = parameters.Thread;
        if (parameters.QueueId is not null)
            body["queue_id"] = parameters.QueueId;
        if (parameters.QueuePriority is not null)
            body["queue_priority"] = parameters.QueuePriority;

        var query = new Dictionary<string, string>();
        if (profileGroupId is not null)
            query["profile_group_id"] = profileGroupId;

        return _client.PatchAsync<Post>($"/api/posts/{Uri.EscapeDataString(id)}", body, query, cancellationToken, parameters.IdempotencyKey);
    }

    private Task<Post> UpdateMultipartAsync(string id, UpdatePostParams parameters, string? profileGroupId, CancellationToken cancellationToken)
    {
        var fields = new List<KeyValuePair<string, string>>();

        if (parameters.Body is not null)
            fields.Add(new("post[body]", parameters.Body));
        if (parameters.ScheduledAt is not null)
            fields.Add(new("post[scheduled_at]", parameters.ScheduledAt.Value.ToString("O")));
        if (parameters.Draft is not null)
            fields.Add(new("post[draft]", parameters.Draft.Value.ToString().ToLowerInvariant()));
        if (parameters.QueueId is not null)
            fields.Add(new("queue_id", parameters.QueueId));
        if (parameters.QueuePriority is not null)
            fields.Add(new("queue_priority", parameters.QueuePriority));
        if (profileGroupId is not null)
            fields.Add(new("profile_group_id", profileGroupId));

        if (parameters.Profiles is not null)
        {
            foreach (var profile in parameters.Profiles)
                fields.Add(new("profiles[]", profile));
        }

        if (parameters.Media is not null)
        {
            foreach (var url in parameters.Media)
                fields.Add(new("media[]", url));
        }

        var allFiles = new List<(string FieldName, string FilePath)>();

        AddPlatformFields(fields, allFiles, parameters.Platforms);

        if (parameters.MediaFiles is { Count: > 0 })
        {
            foreach (var filePath in parameters.MediaFiles)
                allFiles.Add(("media[]", filePath));
        }

        if (parameters.Thread is { Count: > 0 })
        {
            for (var i = 0; i < parameters.Thread.Count; i++)
            {
                var child = parameters.Thread[i];
                fields.Add(new($"thread[{i}][body]", child.Body));

                if (child.Media is { Count: > 0 })
                {
                    foreach (var url in child.Media)
                        fields.Add(new($"thread[{i}][media][]", url));
                }

                if (child.MediaFiles is { Count: > 0 })
                {
                    foreach (var filePath in child.MediaFiles)
                        allFiles.Add(($"thread[{i}][media][]", filePath));
                }
            }
        }

        return _client.PatchMultipartAsync<Post>(
            $"/api/posts/{Uri.EscapeDataString(id)}",
            null,
            fields,
            allFiles,
            cancellationToken,
            parameters.IdempotencyKey);
    }

    public Task<Post> PublishDraftAsync(string id, CancellationToken cancellationToken = default) =>
        PublishDraftAsync(id, null, cancellationToken);

    public Task<Post> PublishDraftAsync(string id, string? profileGroupId, CancellationToken cancellationToken = default) =>
        PublishDraftAsync(id, profileGroupId, null, cancellationToken);

    public Task<Post> PublishDraftAsync(string id, string? profileGroupId, string? idempotencyKey, CancellationToken cancellationToken = default)
    {
        var query = new Dictionary<string, string>();
        var pgId = profileGroupId ?? _defaultProfileGroupId;
        if (pgId is not null)
            query["profile_group_id"] = pgId;

        return _client.PostAsync<Post>($"/api/posts/{Uri.EscapeDataString(id)}/publish", queryParams: query, cancellationToken: cancellationToken, idempotencyKey: idempotencyKey);
    }

    public Task<DeleteResponse> DeleteAsync(string id, CancellationToken cancellationToken = default) =>
        DeleteAsync(id, null, null, cancellationToken);

    public Task<DeleteResponse> DeleteAsync(string id, string? profileGroupId, CancellationToken cancellationToken = default) =>
        DeleteAsync(id, null, profileGroupId, cancellationToken);

    public Task<DeleteResponse> DeleteAsync(string id, bool? deleteOnPlatform, string? profileGroupId, CancellationToken cancellationToken = default) =>
        DeleteAsync(id, deleteOnPlatform, profileGroupId, null, cancellationToken);

    public Task<DeleteResponse> DeleteAsync(string id, bool? deleteOnPlatform, string? profileGroupId, string? idempotencyKey, CancellationToken cancellationToken = default)
    {
        var query = new Dictionary<string, string>();
        var pgId = profileGroupId ?? _defaultProfileGroupId;
        if (pgId is not null)
            query["profile_group_id"] = pgId;
        if (deleteOnPlatform is not null)
            query["delete_on_platform"] = deleteOnPlatform.Value ? "true" : "false";

        return _client.DeleteAsync<DeleteResponse>($"/api/posts/{Uri.EscapeDataString(id)}", query, cancellationToken, idempotencyKey);
    }

    public Task<DeleteOnPlatformResponse> DeleteOnPlatformAsync(string id, CancellationToken cancellationToken = default) =>
        DeleteOnPlatformAsync(id, null, cancellationToken);

    public Task<DeleteOnPlatformResponse> DeleteOnPlatformAsync(string id, DeleteOnPlatformParams? parameters, CancellationToken cancellationToken = default)
    {
        var query = new Dictionary<string, string>();
        var pgId = parameters?.ProfileGroupId ?? _defaultProfileGroupId;
        if (pgId is not null)
            query["profile_group_id"] = pgId;

        var body = new Dictionary<string, object?>();
        if (parameters?.PostProfileId is not null)
            body["post_profile_id"] = parameters.PostProfileId;
        if (parameters?.ProfileId is not null)
            body["profile_id"] = parameters.ProfileId;
        if (parameters?.Network is not null)
            body["network"] = parameters.Network;

        return _client.PostAsync<DeleteOnPlatformResponse>(
            $"/api/posts/{Uri.EscapeDataString(id)}/delete_on_platform",
            body.Count > 0 ? body : null,
            query,
            cancellationToken,
            parameters?.IdempotencyKey);
    }

    public Task<StatsResponse> StatsAsync(PostStatsParams parameters, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(parameters);
        if (parameters.PostIds is not { Count: > 0 })
            throw new ArgumentException("At least one post ID is required.", nameof(parameters));

        var query = new Dictionary<string, string>
        {
            ["post_ids"] = string.Join(",", parameters.PostIds),
        };

        if (parameters.Profiles is { Count: > 0 })
            query["profiles"] = string.Join(",", parameters.Profiles);
        if (parameters.From is not null)
            query["from"] = parameters.From.Value.ToString("O");
        if (parameters.To is not null)
            query["to"] = parameters.To.Value.ToString("O");

        return _client.GetAsync<StatsResponse>("/api/posts/stats", query, cancellationToken);
    }

    private record CreatePostBody
    {
        [JsonPropertyName("post")]
        public required PostBody Post { get; init; }

        [JsonPropertyName("profiles")]
        public required IReadOnlyList<string> Profiles { get; init; }

        [JsonPropertyName("media")]
        public IReadOnlyList<string>? Media { get; init; }

        [JsonPropertyName("platforms")]
        public PlatformParams? Platforms { get; init; }

        [JsonPropertyName("thread")]
        public IReadOnlyList<ThreadChildInput>? Thread { get; init; }

        [JsonPropertyName("profile_group_id")]
        public string? ProfileGroupId { get; init; }

        [JsonPropertyName("queue_id")]
        public string? QueueId { get; init; }

        [JsonPropertyName("queue_priority")]
        public string? QueuePriority { get; init; }
    }

    private record PostBody
    {
        [JsonPropertyName("body")]
        public required string Body { get; init; }

        [JsonPropertyName("scheduled_at")]
        public string? ScheduledAt { get; init; }

        [JsonPropertyName("draft")]
        public bool? Draft { get; init; }
    }
}
