using PostProxy.Models;

namespace PostProxy.Resources;

public class CommentsResource
{
    private readonly PostProxyHttpClient _client;

    internal CommentsResource(PostProxyHttpClient client)
    {
        _client = client;
    }

    public Task<PaginatedResponse<Comment>> ListAsync(string postId, string profileId, int? page = null, int? perPage = null, CancellationToken cancellationToken = default)
    {
        var query = new Dictionary<string, string> { ["profile_id"] = profileId };
        if (page is not null) query["page"] = page.Value.ToString();
        if (perPage is not null) query["per_page"] = perPage.Value.ToString();

        return _client.GetAsync<PaginatedResponse<Comment>>($"/api/posts/{Uri.EscapeDataString(postId)}/comments", query, cancellationToken);
    }

    public Task<Comment> GetAsync(string postId, string commentId, string profileId, CancellationToken cancellationToken = default)
    {
        var query = new Dictionary<string, string> { ["profile_id"] = profileId };

        return _client.GetAsync<Comment>($"/api/posts/{Uri.EscapeDataString(postId)}/comments/{Uri.EscapeDataString(commentId)}", query, cancellationToken);
    }

    public Task<Comment> CreateAsync(string postId, string profileId, string text, string? parentId = null, CancellationToken cancellationToken = default)
    {
        var query = new Dictionary<string, string> { ["profile_id"] = profileId };
        var body = new Dictionary<string, object> { ["text"] = text };
        if (parentId is not null) body["parent_id"] = parentId;

        return _client.PostAsync<Comment>($"/api/posts/{Uri.EscapeDataString(postId)}/comments", body, query, cancellationToken);
    }

    public Task<AcceptedResponse> DeleteAsync(string postId, string commentId, string profileId, CancellationToken cancellationToken = default)
    {
        var query = new Dictionary<string, string> { ["profile_id"] = profileId };

        return _client.DeleteAsync<AcceptedResponse>($"/api/posts/{Uri.EscapeDataString(postId)}/comments/{Uri.EscapeDataString(commentId)}", query, cancellationToken);
    }

    public Task<AcceptedResponse> HideAsync(string postId, string commentId, string profileId, CancellationToken cancellationToken = default) =>
        CommentActionAsync(postId, commentId, profileId, "hide", cancellationToken);

    public Task<AcceptedResponse> UnhideAsync(string postId, string commentId, string profileId, CancellationToken cancellationToken = default) =>
        CommentActionAsync(postId, commentId, profileId, "unhide", cancellationToken);

    public Task<AcceptedResponse> LikeAsync(string postId, string commentId, string profileId, CancellationToken cancellationToken = default) =>
        CommentActionAsync(postId, commentId, profileId, "like", cancellationToken);

    public Task<AcceptedResponse> UnlikeAsync(string postId, string commentId, string profileId, CancellationToken cancellationToken = default) =>
        CommentActionAsync(postId, commentId, profileId, "unlike", cancellationToken);

    private Task<AcceptedResponse> CommentActionAsync(string postId, string commentId, string profileId, string action, CancellationToken cancellationToken)
    {
        var query = new Dictionary<string, string> { ["profile_id"] = profileId };
        var path = $"/api/posts/{Uri.EscapeDataString(postId)}/comments/{Uri.EscapeDataString(commentId)}/{action}";

        return _client.PostAsync<AcceptedResponse>(path, null, query, cancellationToken);
    }
}
