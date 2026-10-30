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
        if (parameters.PlacementId is not null) body["placement_id"] = parameters.PlacementId;

        return _client.PostAsync<Chat>($"/api/profiles/{Uri.EscapeDataString(profileId)}/chats", body, query, cancellationToken, parameters.IdempotencyKey);
    }

    public Task<Chat> GetAsync(string chatId, CancellationToken cancellationToken = default) =>
        GetAsync(chatId, null, cancellationToken);

    public Task<Chat> GetAsync(string chatId, string? profileGroupId, CancellationToken cancellationToken = default)
    {
        return _client.GetAsync<Chat>($"/api/chats/{Uri.EscapeDataString(chatId)}", BuildQuery(profileGroupId), cancellationToken);
    }

    public Task<Chat> ArchiveAsync(string chatId, CancellationToken cancellationToken = default) =>
        ArchiveAsync(chatId, null, cancellationToken);

    public Task<Chat> ArchiveAsync(string chatId, string? profileGroupId, CancellationToken cancellationToken = default) =>
        ArchiveAsync(chatId, profileGroupId, null, cancellationToken);

    public Task<Chat> ArchiveAsync(string chatId, string? profileGroupId, string? idempotencyKey, CancellationToken cancellationToken = default)
    {
        return _client.PostAsync<Chat>($"/api/chats/{Uri.EscapeDataString(chatId)}/archive", null, BuildQuery(profileGroupId), cancellationToken, idempotencyKey);
    }

    public Task<Chat> UnarchiveAsync(string chatId, CancellationToken cancellationToken = default) =>
        UnarchiveAsync(chatId, null, cancellationToken);

    public Task<Chat> UnarchiveAsync(string chatId, string? profileGroupId, CancellationToken cancellationToken = default) =>
        UnarchiveAsync(chatId, profileGroupId, null, cancellationToken);

    public Task<Chat> UnarchiveAsync(string chatId, string? profileGroupId, string? idempotencyKey, CancellationToken cancellationToken = default)
    {
        return _client.DeleteAsync<Chat>($"/api/chats/{Uri.EscapeDataString(chatId)}/archive", BuildQuery(profileGroupId), cancellationToken, idempotencyKey);
    }

    /// <summary>Marks the latest inbound message as read (shows the read receipt).</summary>
    public Task<Chat> MarkReadAsync(string chatId, string? profileGroupId = null, string? idempotencyKey = null, CancellationToken cancellationToken = default)
    {
        return _client.PostAsync<Chat>($"/api/chats/{Uri.EscapeDataString(chatId)}/mark_read", null, BuildQuery(profileGroupId), cancellationToken, idempotencyKey);
    }

    /// <summary>Shows the typing indicator. Facebook, Instagram and WhatsApp only.</summary>
    public Task<AcceptedResponse> StartTypingAsync(string chatId, string? profileGroupId = null, string? idempotencyKey = null, CancellationToken cancellationToken = default)
    {
        return _client.PostAsync<AcceptedResponse>($"/api/chats/{Uri.EscapeDataString(chatId)}/start_typing", null, BuildQuery(profileGroupId), cancellationToken, idempotencyKey);
    }

    public Task<AcceptedResponse> StopTypingAsync(string chatId, string? profileGroupId = null, string? idempotencyKey = null, CancellationToken cancellationToken = default)
    {
        return _client.PostAsync<AcceptedResponse>($"/api/chats/{Uri.EscapeDataString(chatId)}/stop_typing", null, BuildQuery(profileGroupId), cancellationToken, idempotencyKey);
    }

    // --- Thread control (Facebook & Instagram) ---
    //
    // Meta gives one app at a time control of a conversation when several share a
    // Page or Instagram inbox. These return the chat, whose Metadata["thread_control"]
    // holds the owner Meta reports back.

    /// <summary>
    /// Hands the conversation to another Meta app, e.g. Page Inbox <c>263902037430900</c>
    /// or Instagram Inbox <c>1217981644879628</c>.
    /// </summary>
    public Task<Chat> PassThreadControlAsync(string chatId, string targetAppId, string? metadata = null, string? profileGroupId = null, string? idempotencyKey = null, CancellationToken cancellationToken = default) =>
        ThreadControlAsync(chatId, "pass_thread_control", targetAppId, metadata, profileGroupId, idempotencyKey, cancellationToken);

    /// <summary>
    /// Takes the conversation from whichever app holds it. Needs Conversation Routing;
    /// Instagram must be connected via Facebook.
    /// </summary>
    public Task<Chat> TakeThreadControlAsync(string chatId, string? metadata = null, string? profileGroupId = null, string? idempotencyKey = null, CancellationToken cancellationToken = default) =>
        ThreadControlAsync(chatId, "take_thread_control", null, metadata, profileGroupId, idempotencyKey, cancellationToken);

    /// <summary>Gives up control; the conversation goes idle.</summary>
    public Task<Chat> ReleaseThreadControlAsync(string chatId, string? metadata = null, string? profileGroupId = null, string? idempotencyKey = null, CancellationToken cancellationToken = default) =>
        ThreadControlAsync(chatId, "release_thread_control", null, metadata, profileGroupId, idempotencyKey, cancellationToken);

    /// <summary>
    /// Asks the current owner to pass the conversation. Nothing changes until it
    /// does — watch for <c>chat.thread_control_changed</c>.
    /// </summary>
    public Task<Chat> RequestThreadControlAsync(string chatId, string? metadata = null, string? profileGroupId = null, string? idempotencyKey = null, CancellationToken cancellationToken = default) =>
        ThreadControlAsync(chatId, "request_thread_control", null, metadata, profileGroupId, idempotencyKey, cancellationToken);

    /// <summary>Asks Meta which app controls the conversation right now.</summary>
    public Task<ThreadOwner> GetThreadOwnerAsync(string chatId, string? profileGroupId = null, CancellationToken cancellationToken = default)
    {
        return _client.GetAsync<ThreadOwner>($"/api/chats/{Uri.EscapeDataString(chatId)}/thread_owner", BuildQuery(profileGroupId), cancellationToken);
    }

    private Task<Chat> ThreadControlAsync(string chatId, string action, string? targetAppId, string? metadata, string? profileGroupId, string? idempotencyKey, CancellationToken cancellationToken)
    {
        var body = new Dictionary<string, object?>();
        if (targetAppId is not null) body["target_app_id"] = targetAppId;
        if (metadata is not null) body["metadata"] = metadata;

        return _client.PostAsync<Chat>($"/api/chats/{Uri.EscapeDataString(chatId)}/{action}", body, BuildQuery(profileGroupId), cancellationToken, idempotencyKey);
    }

    private Dictionary<string, string> BuildQuery(string? profileGroupId)
    {
        var query = new Dictionary<string, string>();
        var pgId = profileGroupId ?? _defaultProfileGroupId;
        if (pgId is not null) query["profile_group_id"] = pgId;
        return query;
    }
}
