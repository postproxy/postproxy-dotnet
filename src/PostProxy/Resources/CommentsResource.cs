using PostProxy.Models;

namespace PostProxy.Resources;

public class CommentsResource
{
    private readonly PostProxyHttpClient _client;

    internal CommentsResource(PostProxyHttpClient client)
    {
        _client = client;
    }

    /// <summary>
    /// Lists a post's comments. <paramref name="from"/> and <paramref name="to"/>
    /// filter on when PostProxy received the comment (created_at), not the
    /// platform's posted_at, and apply to top-level comments — one in range
    /// brings its full replies list with it.
    /// </summary>
    public Task<PaginatedResponse<Comment>> ListAsync(string postId, string profileId, int? page = null, int? perPage = null, string? from = null, string? to = null, CancellationToken cancellationToken = default)
    {
        var query = new Dictionary<string, string> { ["profile_id"] = profileId };
        if (page is not null) query["page"] = page.Value.ToString();
        if (perPage is not null) query["per_page"] = perPage.Value.ToString();
        if (from is not null) query["from"] = from;
        if (to is not null) query["to"] = to;

        return _client.GetAsync<PaginatedResponse<Comment>>($"/api/posts/{Uri.EscapeDataString(postId)}/comments", query, cancellationToken);
    }

    /// <summary>
    /// Lists comments across every post in the profile group. Flat: replies come
    /// back as their own entries linked by <c>ParentExternalId</c>, so
    /// <c>Total</c> counts every comment. <paramref name="profiles"/> takes
    /// profile IDs or network names, mixed. Every filter is optional; unknown or
    /// out-of-scope IDs are ignored rather than erroring.
    /// </summary>
    public Task<PaginatedResponse<BulkComment>> ListAllAsync(
        IEnumerable<string>? postIds = null,
        IEnumerable<string>? profiles = null,
        string? from = null,
        string? to = null,
        int? page = null,
        int? perPage = null,
        string? profileGroupId = null,
        CancellationToken cancellationToken = default)
    {
        var query = new Dictionary<string, string>();
        if (postIds is not null) query["post_ids"] = string.Join(",", postIds);
        if (profiles is not null) query["profiles"] = string.Join(",", profiles);
        if (from is not null) query["from"] = from;
        if (to is not null) query["to"] = to;
        if (page is not null) query["page"] = page.Value.ToString();
        if (perPage is not null) query["per_page"] = perPage.Value.ToString();
        if (profileGroupId is not null) query["profile_group_id"] = profileGroupId;

        return _client.GetAsync<PaginatedResponse<BulkComment>>("/api/comments", query, cancellationToken);
    }

    public Task<Comment> GetAsync(string postId, string commentId, string profileId, CancellationToken cancellationToken = default)
    {
        var query = new Dictionary<string, string> { ["profile_id"] = profileId };

        return _client.GetAsync<Comment>($"/api/posts/{Uri.EscapeDataString(postId)}/comments/{Uri.EscapeDataString(commentId)}", query, cancellationToken);
    }

    public Task<Comment> CreateAsync(string postId, string profileId, string text, string? parentId = null, string? idempotencyKey = null, CancellationToken cancellationToken = default)
    {
        var query = new Dictionary<string, string> { ["profile_id"] = profileId };
        var body = new Dictionary<string, object> { ["text"] = text };
        if (parentId is not null) body["parent_id"] = parentId;

        return _client.PostAsync<Comment>($"/api/posts/{Uri.EscapeDataString(postId)}/comments", body, query, cancellationToken, idempotencyKey);
    }

    public Task<AcceptedResponse> DeleteAsync(string postId, string commentId, string profileId, string? idempotencyKey = null, CancellationToken cancellationToken = default)
    {
        var query = new Dictionary<string, string> { ["profile_id"] = profileId };

        return _client.DeleteAsync<AcceptedResponse>($"/api/posts/{Uri.EscapeDataString(postId)}/comments/{Uri.EscapeDataString(commentId)}", query, cancellationToken, idempotencyKey);
    }

    public Task<AcceptedResponse> HideAsync(string postId, string commentId, string profileId, string? idempotencyKey = null, CancellationToken cancellationToken = default) =>
        CommentActionAsync(postId, commentId, profileId, "hide", idempotencyKey, cancellationToken);

    public Task<AcceptedResponse> UnhideAsync(string postId, string commentId, string profileId, string? idempotencyKey = null, CancellationToken cancellationToken = default) =>
        CommentActionAsync(postId, commentId, profileId, "unhide", idempotencyKey, cancellationToken);

    public Task<AcceptedResponse> LikeAsync(string postId, string commentId, string profileId, string? idempotencyKey = null, CancellationToken cancellationToken = default) =>
        CommentActionAsync(postId, commentId, profileId, "like", idempotencyKey, cancellationToken);

    public Task<AcceptedResponse> UnlikeAsync(string postId, string commentId, string profileId, string? idempotencyKey = null, CancellationToken cancellationToken = default) =>
        CommentActionAsync(postId, commentId, profileId, "unlike", idempotencyKey, cancellationToken);

    public Task<Message> PrivateReplyAsync(string postId, string commentId, string profileId, string text, string? idempotencyKey = null, CancellationToken cancellationToken = default)
    {
        var query = new Dictionary<string, string> { ["profile_id"] = profileId };
        var body = new Dictionary<string, object> { ["text"] = text };
        var path = $"/api/posts/{Uri.EscapeDataString(postId)}/comments/{Uri.EscapeDataString(commentId)}/private_reply";

        return _client.PostAsync<Message>(path, body, query, cancellationToken, idempotencyKey);
    }

    private Task<AcceptedResponse> CommentActionAsync(string postId, string commentId, string profileId, string action, string? idempotencyKey, CancellationToken cancellationToken)
    {
        var query = new Dictionary<string, string> { ["profile_id"] = profileId };
        var path = $"/api/posts/{Uri.EscapeDataString(postId)}/comments/{Uri.EscapeDataString(commentId)}/{action}";

        return _client.PostAsync<AcceptedResponse>(path, null, query, cancellationToken, idempotencyKey);
    }
}
