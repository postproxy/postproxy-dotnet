using System.Text.Json;
using PostProxy.Models;
using PostProxy.Parameters;

namespace PostProxy.Resources;

public class MessagesResource
{
    private readonly PostProxyHttpClient _client;
    private readonly string? _defaultProfileGroupId;

    internal MessagesResource(PostProxyHttpClient client, string? defaultProfileGroupId)
    {
        _client = client;
        _defaultProfileGroupId = defaultProfileGroupId;
    }

    public Task<PaginatedResponse<Message>> ListAsync(string chatId, CancellationToken cancellationToken = default) =>
        ListAsync(chatId, null, cancellationToken);

    public Task<PaginatedResponse<Message>> ListAsync(string chatId, ListMessagesParams? parameters, CancellationToken cancellationToken = default)
    {
        var query = new Dictionary<string, string>();
        var profileGroupId = parameters?.ProfileGroupId ?? _defaultProfileGroupId;
        if (profileGroupId is not null) query["profile_group_id"] = profileGroupId;
        if (parameters?.Page is not null) query["page"] = parameters.Page.Value.ToString();
        if (parameters?.PerPage is not null) query["per_page"] = parameters.PerPage.Value.ToString();
        if (parameters?.Direction is not null)
            query["direction"] = JsonSerializer.Serialize(parameters.Direction.Value, PostProxyHttpClient.JsonOptions).Trim('"');
        if (parameters?.Status is not null)
            query["status"] = JsonSerializer.Serialize(parameters.Status.Value, PostProxyHttpClient.JsonOptions).Trim('"');

        return _client.GetAsync<PaginatedResponse<Message>>($"/api/chats/{Uri.EscapeDataString(chatId)}/messages", query, cancellationToken);
    }

    public Task<Message> SendAsync(string chatId, string body, CancellationToken cancellationToken = default) =>
        SendAsync(chatId, new SendMessageParams { Body = body }, cancellationToken);

    public Task<Message> SendAsync(string chatId, SendMessageParams parameters, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(parameters);

        var profileGroupId = parameters.ProfileGroupId ?? _defaultProfileGroupId;
        var query = new Dictionary<string, string>();
        if (profileGroupId is not null) query["profile_group_id"] = profileGroupId;

        var path = $"/api/chats/{Uri.EscapeDataString(chatId)}/messages";

        if (parameters.MediaFiles is { Count: > 0 })
        {
            var fields = new List<KeyValuePair<string, string>>();
            if (parameters.Body is not null) fields.Add(new("body", parameters.Body));
            if (parameters.Tag is not null) fields.Add(new("tag", parameters.Tag));
            if (parameters.ReplyToExternalId is not null) fields.Add(new("reply_to_external_id", parameters.ReplyToExternalId));

            if (parameters.Media is { Count: > 0 })
            {
                foreach (var url in parameters.Media)
                    fields.Add(new("media[]", url));
            }

            var files = new List<(string FieldName, string FilePath)>();
            foreach (var filePath in parameters.MediaFiles)
                files.Add(("media[]", filePath));

            return _client.PostMultipartAsync<Message>(path, query, fields, files, cancellationToken, parameters.IdempotencyKey);
        }

        var jsonBody = new Dictionary<string, object?>();
        if (parameters.Body is not null) jsonBody["body"] = parameters.Body;
        if (parameters.Media is not null) jsonBody["media"] = parameters.Media;
        if (parameters.Tag is not null) jsonBody["tag"] = parameters.Tag;
        if (parameters.ReplyToExternalId is not null) jsonBody["reply_to_external_id"] = parameters.ReplyToExternalId;
        if (parameters.ReplyMarkup is not null) jsonBody["reply_markup"] = parameters.ReplyMarkup;

        return _client.PostAsync<Message>(path, jsonBody, query, cancellationToken, parameters.IdempotencyKey);
    }

    public Task<Message> GetAsync(string messageId, CancellationToken cancellationToken = default) =>
        GetAsync(messageId, null, cancellationToken);

    public Task<Message> GetAsync(string messageId, string? profileGroupId, CancellationToken cancellationToken = default)
    {
        return _client.GetAsync<Message>($"/api/messages/{Uri.EscapeDataString(messageId)}", BuildQuery(profileGroupId), cancellationToken);
    }

    public Task<Message> EditAsync(string messageId, EditMessageParams parameters, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(parameters);

        var jsonBody = new Dictionary<string, object?>();
        if (parameters.Body is not null) jsonBody["body"] = parameters.Body;
        if (parameters.ReplyMarkup is not null) jsonBody["reply_markup"] = parameters.ReplyMarkup;

        return _client.PatchAsync<Message>($"/api/messages/{Uri.EscapeDataString(messageId)}", jsonBody, BuildQuery(parameters.ProfileGroupId), cancellationToken, parameters.IdempotencyKey);
    }

    public Task<Message> ReactAsync(string messageId, CancellationToken cancellationToken = default) =>
        ReactAsync(messageId, null, cancellationToken);

    public Task<Message> ReactAsync(string messageId, ReactParams? parameters, CancellationToken cancellationToken = default)
    {
        var jsonBody = new Dictionary<string, object?>();
        if (parameters?.Reaction is not null) jsonBody["reaction"] = parameters.Reaction;
        if (parameters?.Emoji is not null) jsonBody["emoji"] = parameters.Emoji;

        return _client.PostAsync<Message>(
            $"/api/messages/{Uri.EscapeDataString(messageId)}/react",
            jsonBody.Count > 0 ? jsonBody : null,
            BuildQuery(parameters?.ProfileGroupId),
            cancellationToken,
            parameters?.IdempotencyKey);
    }

    public Task<Message> UnreactAsync(string messageId, CancellationToken cancellationToken = default) =>
        UnreactAsync(messageId, null, cancellationToken);

    public Task<Message> UnreactAsync(string messageId, string? profileGroupId, CancellationToken cancellationToken = default) =>
        UnreactAsync(messageId, profileGroupId, null, cancellationToken);

    public Task<Message> UnreactAsync(string messageId, string? profileGroupId, string? idempotencyKey, CancellationToken cancellationToken = default)
    {
        return _client.DeleteAsync<Message>($"/api/messages/{Uri.EscapeDataString(messageId)}/unreact", BuildQuery(profileGroupId), cancellationToken, idempotencyKey);
    }

    private Dictionary<string, string> BuildQuery(string? profileGroupId)
    {
        var query = new Dictionary<string, string>();
        var pgId = profileGroupId ?? _defaultProfileGroupId;
        if (pgId is not null) query["profile_group_id"] = pgId;
        return query;
    }
}
