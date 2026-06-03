using PostProxy.Models;
using PostProxy.Parameters;

namespace PostProxy.Resources;

public class ChatsResource
{
    private readonly PostProxyHttpClient _client;
    private readonly string? _defaultProfileGroupId;

    internal ChatsResource(PostProxyHttpClient client, string? defaultProfileGroupId)
    {
        _client = client;
        _defaultProfileGroupId = defaultProfileGroupId;
    }

    public Task<PaginatedResponse<Chat>> ListAsync(string profileId, CancellationToken cancellationToken = default) =>
        ListAsync(profileId, null, cancellationToken);

    public Task<PaginatedResponse<Chat>> ListAsync(string profileId, ListChatsParams? parameters, CancellationToken cancellationToken = default)
    {
        var query = new Dictionary<string, string>();
        var profileGroupId = parameters?.ProfileGroupId ?? _defaultProfileGroupId;
        if (profileGroupId is not null) query["profile_group_id"] = profileGroupId;
        if (parameters?.Page is not null) query["page"] = parameters.Page.Value.ToString();
        if (parameters?.PerPage is not null) query["per_page"] = parameters.PerPage.Value.ToString();
        if (parameters?.Before is not null) query["before"] = parameters.Before.Value.ToString("O");
        if (parameters?.After is not null) query["after"] = parameters.After.Value.ToString("O");

        return _client.GetAsync<PaginatedResponse<Chat>>($"/api/profiles/{Uri.EscapeDataString(profileId)}/chats", query, cancellationToken);
    }

    public Task<Chat> CreateAsync(string profileId, string participantExternalId, CancellationToken cancellationToken = default) =>
        CreateAsync(profileId, new CreateChatParams { ParticipantExternalId = participantExternalId }, cancellationToken);

    public Task<Chat> CreateAsync(string profileId, CreateChatParams parameters, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(parameters);

        var query = new Dictionary<string, string>();
        var profileGroupId = parameters.ProfileGroupId ?? _defaultProfileGroupId;
        if (profileGroupId is not null) query["profile_group_id"] = profileGroupId;

        var body = new Dictionary<string, object> { ["participant_external_id"] = parameters.ParticipantExternalId };
        if (parameters.ParticipantUsername is not null) body["participant_username"] = parameters.ParticipantUsername;
        if (parameters.ParticipantName is not null) body["participant_name"] = parameters.ParticipantName;

        return _client.PostAsync<Chat>($"/api/profiles/{Uri.EscapeDataString(profileId)}/chats", body, query, cancellationToken);
    }

    public Task<Chat> GetAsync(string chatId, CancellationToken cancellationToken = default) =>
        GetAsync(chatId, null, cancellationToken);

    public Task<Chat> GetAsync(string chatId, string? profileGroupId, CancellationToken cancellationToken = default)
    {
        return _client.GetAsync<Chat>($"/api/chats/{Uri.EscapeDataString(chatId)}", BuildQuery(profileGroupId), cancellationToken);
    }

    public Task<Chat> ArchiveAsync(string chatId, CancellationToken cancellationToken = default) =>
        ArchiveAsync(chatId, null, cancellationToken);

    public Task<Chat> ArchiveAsync(string chatId, string? profileGroupId, CancellationToken cancellationToken = default)
    {
        return _client.PostAsync<Chat>($"/api/chats/{Uri.EscapeDataString(chatId)}/archive", null, BuildQuery(profileGroupId), cancellationToken);
    }

    public Task<Chat> UnarchiveAsync(string chatId, CancellationToken cancellationToken = default) =>
        UnarchiveAsync(chatId, null, cancellationToken);

    public Task<Chat> UnarchiveAsync(string chatId, string? profileGroupId, CancellationToken cancellationToken = default)
    {
        return _client.DeleteAsync<Chat>($"/api/chats/{Uri.EscapeDataString(chatId)}/archive", BuildQuery(profileGroupId), cancellationToken);
    }

    private Dictionary<string, string> BuildQuery(string? profileGroupId)
    {
        var query = new Dictionary<string, string>();
        var pgId = profileGroupId ?? _defaultProfileGroupId;
        if (pgId is not null) query["profile_group_id"] = pgId;
        return query;
    }
}
