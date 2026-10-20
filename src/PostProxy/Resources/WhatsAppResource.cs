using System.Globalization;
using System.Text.Json;
using PostProxy.Models;
using PostProxy.Parameters;

namespace PostProxy.Resources;

/// <summary>
/// WhatsApp number administration: registration, business profile, display name,
/// username, blocked users, groups and conversion events.
/// </summary>
/// <remarks>
/// <c>phoneNumberId</c> is the placement id from <c>Profiles.PlacementsAsync(profileId)</c>.
/// Dataset and conversion calls are account-level and don't take one.
/// </remarks>
public class WhatsAppResource
{
    private readonly PostProxyHttpClient _client;
    private readonly string? _defaultProfileGroupId;

    internal WhatsAppResource(PostProxyHttpClient client, string? defaultProfileGroupId)
    {
        _client = client;
        _defaultProfileGroupId = defaultProfileGroupId;
    }

    // --- Phone number ---

    public Task<JsonElement> GetNumberInfoAsync(string profileId, string phoneNumberId, string? profileGroupId = null, CancellationToken cancellationToken = default) =>
        CallAsync(HttpMethod.Get, profileId, "number_info", new() { ["phone_number_id"] = phoneNumberId }, profileGroupId, null, cancellationToken);

    /// <summary><paramref name="pin"/> is the 6-digit two-step verification PIN.</summary>
    public Task<JsonElement> RegisterPhoneNumberAsync(string profileId, string phoneNumberId, string pin, string? profileGroupId = null, string? idempotencyKey = null, CancellationToken cancellationToken = default) =>
        CallAsync(HttpMethod.Post, profileId, "register_phone_number", new() { ["phone_number_id"] = phoneNumberId, ["pin"] = pin }, profileGroupId, idempotencyKey, cancellationToken);

    /// <summary><paramref name="method"/> is SMS or VOICE.</summary>
    public Task<JsonElement> RequestVerificationCodeAsync(string profileId, string phoneNumberId, string? method = null, string? language = null, string? profileGroupId = null, string? idempotencyKey = null, CancellationToken cancellationToken = default) =>
        CallAsync(HttpMethod.Post, profileId, "request_verification_code", new()
        {
            ["phone_number_id"] = phoneNumberId,
            ["method"] = method,
            ["language"] = language,
        }, profileGroupId, idempotencyKey, cancellationToken);

    public Task<JsonElement> VerifyPhoneNumberAsync(string profileId, string phoneNumberId, string code, string? profileGroupId = null, string? idempotencyKey = null, CancellationToken cancellationToken = default) =>
        CallAsync(HttpMethod.Post, profileId, "verify_phone_number", new() { ["phone_number_id"] = phoneNumberId, ["code"] = code }, profileGroupId, idempotencyKey, cancellationToken);

    // --- Business profile ---

    public Task<JsonElement> GetBusinessProfileAsync(string profileId, string phoneNumberId, string? profileGroupId = null, CancellationToken cancellationToken = default) =>
        CallAsync(HttpMethod.Get, profileId, "business_profile", new() { ["phone_number_id"] = phoneNumberId }, profileGroupId, null, cancellationToken);

    public Task<JsonElement> UpdateBusinessProfileAsync(
        string profileId,
        string phoneNumberId,
        string? about = null,
        string? address = null,
        string? description = null,
        string? email = null,
        IEnumerable<string>? websites = null,
        string? vertical = null,
        string? profileGroupId = null,
        string? idempotencyKey = null,
        CancellationToken cancellationToken = default) =>
        CallAsync(HttpMethod.Patch, profileId, "update_business_profile", new()
        {
            ["phone_number_id"] = phoneNumberId,
            ["about"] = about,
            ["address"] = address,
            ["description"] = description,
            ["email"] = email,
            ["websites"] = websites?.ToList(),
            ["vertical"] = vertical,
        }, profileGroupId, idempotencyKey, cancellationToken);

    /// <summary>
    /// Pass a public <paramref name="url"/>, or base64 <paramref name="data"/> with its
    /// <paramref name="contentType"/> (image/jpeg or image/png).
    /// </summary>
    public Task<JsonElement> UpdateBusinessProfilePhotoAsync(
        string profileId,
        string phoneNumberId,
        string? url = null,
        string? data = null,
        string? contentType = null,
        string? profileGroupId = null,
        string? idempotencyKey = null,
        CancellationToken cancellationToken = default) =>
        CallAsync(HttpMethod.Post, profileId, "update_business_profile_photo", new()
        {
            ["phone_number_id"] = phoneNumberId,
            ["url"] = url,
            ["data"] = data,
            ["content_type"] = contentType,
        }, profileGroupId, idempotencyKey, cancellationToken);

    // --- Display name ---

    public Task<JsonElement> GetDisplayNameAsync(string profileId, string phoneNumberId, string? profileGroupId = null, CancellationToken cancellationToken = default) =>
        CallAsync(HttpMethod.Get, profileId, "display_name", new() { ["phone_number_id"] = phoneNumberId }, profileGroupId, null, cancellationToken);

    /// <summary>
    /// Meta reviews the new name; the outcome arrives as the
    /// <c>placement.name_status_updated</c> webhook.
    /// </summary>
    public Task<JsonElement> RequestDisplayNameChangeAsync(string profileId, string phoneNumberId, string displayName, string? profileGroupId = null, string? idempotencyKey = null, CancellationToken cancellationToken = default) =>
        CallAsync(HttpMethod.Post, profileId, "request_display_name_change", new()
        {
            ["phone_number_id"] = phoneNumberId,
            ["display_name"] = displayName,
        }, profileGroupId, idempotencyKey, cancellationToken);

    // --- Username ---

    public Task<JsonElement> GetUsernameAsync(string profileId, string phoneNumberId, string? profileGroupId = null, CancellationToken cancellationToken = default) =>
        CallAsync(HttpMethod.Get, profileId, "username", new() { ["phone_number_id"] = phoneNumberId }, profileGroupId, null, cancellationToken);

    /// <summary><paramref name="transferAction"/> is <c>none</c> or <c>force_transfer</c>.</summary>
    public Task<JsonElement> SetUsernameAsync(string profileId, string phoneNumberId, string username, string? transferAction = null, string? profileGroupId = null, string? idempotencyKey = null, CancellationToken cancellationToken = default) =>
        CallAsync(HttpMethod.Put, profileId, "set_username", new()
        {
            ["phone_number_id"] = phoneNumberId,
            ["username"] = username,
            ["transfer_action"] = transferAction,
        }, profileGroupId, idempotencyKey, cancellationToken);

    public Task<JsonElement> DeleteUsernameAsync(string profileId, string phoneNumberId, string? profileGroupId = null, string? idempotencyKey = null, CancellationToken cancellationToken = default) =>
        CallAsync(HttpMethod.Delete, profileId, "delete_username", new() { ["phone_number_id"] = phoneNumberId }, profileGroupId, idempotencyKey, cancellationToken);

    public Task<JsonElement> GetUsernameSuggestionsAsync(string profileId, string phoneNumberId, string? profileGroupId = null, CancellationToken cancellationToken = default) =>
        CallAsync(HttpMethod.Get, profileId, "username_suggestions", new() { ["phone_number_id"] = phoneNumberId }, profileGroupId, null, cancellationToken);

    // --- Blocked users ---

    public Task<JsonElement> ListBlockedUsersAsync(string profileId, string phoneNumberId, int? limit = null, string? after = null, string? profileGroupId = null, CancellationToken cancellationToken = default) =>
        CallAsync(HttpMethod.Get, profileId, "blocked_users", new()
        {
            ["phone_number_id"] = phoneNumberId,
            ["limit"] = limit,
            ["after"] = after,
        }, profileGroupId, null, cancellationToken);

    public Task<JsonElement> GetBlockedUserStatusAsync(string profileId, string phoneNumberId, string user, string? profileGroupId = null, CancellationToken cancellationToken = default) =>
        CallAsync(HttpMethod.Get, profileId, "blocked_user_status", new() { ["phone_number_id"] = phoneNumberId, ["user"] = user }, profileGroupId, null, cancellationToken);

    /// <summary><paramref name="users"/> are phone numbers or WhatsApp ids.</summary>
    public Task<JsonElement> BlockUsersAsync(string profileId, string phoneNumberId, IEnumerable<string> users, string? profileGroupId = null, string? idempotencyKey = null, CancellationToken cancellationToken = default) =>
        CallAsync(HttpMethod.Post, profileId, "block_users", new() { ["phone_number_id"] = phoneNumberId, ["users"] = users.ToList() }, profileGroupId, idempotencyKey, cancellationToken);

    public Task<JsonElement> UnblockUsersAsync(string profileId, string phoneNumberId, IEnumerable<string> users, string? profileGroupId = null, string? idempotencyKey = null, CancellationToken cancellationToken = default) =>
        CallAsync(HttpMethod.Post, profileId, "unblock_users", new() { ["phone_number_id"] = phoneNumberId, ["users"] = users.ToList() }, profileGroupId, idempotencyKey, cancellationToken);

    // --- Groups ---

    public Task<JsonElement> ListGroupsAsync(string profileId, string phoneNumberId, int? limit = null, string? after = null, string? profileGroupId = null, CancellationToken cancellationToken = default) =>
        CallAsync(HttpMethod.Get, profileId, "groups", new()
        {
            ["phone_number_id"] = phoneNumberId,
            ["limit"] = limit,
            ["after"] = after,
        }, profileGroupId, null, cancellationToken);

    /// <summary><paramref name="joinApprovalMode"/> is <c>approval_required</c> or <c>auto_approve</c>.</summary>
    public Task<JsonElement> CreateGroupAsync(
        string profileId,
        string phoneNumberId,
        string subject,
        string? description = null,
        string? joinApprovalMode = null,
        string? profileGroupId = null,
        string? idempotencyKey = null,
        CancellationToken cancellationToken = default) =>
        CallAsync(HttpMethod.Post, profileId, "create_group", new()
        {
            ["phone_number_id"] = phoneNumberId,
            ["subject"] = subject,
            ["description"] = description,
            ["join_approval_mode"] = joinApprovalMode,
        }, profileGroupId, idempotencyKey, cancellationToken);

    public Task<JsonElement> GetGroupAsync(string profileId, string phoneNumberId, string groupId, string? profileGroupId = null, CancellationToken cancellationToken = default) =>
        CallAsync(HttpMethod.Get, profileId, "group", new() { ["phone_number_id"] = phoneNumberId, ["group_id"] = groupId }, profileGroupId, null, cancellationToken);

    public Task<JsonElement> UpdateGroupAsync(
        string profileId,
        string phoneNumberId,
        string groupId,
        string? subject = null,
        string? description = null,
        string? joinApprovalMode = null,
        string? profileGroupId = null,
        string? idempotencyKey = null,
        CancellationToken cancellationToken = default) =>
        CallAsync(HttpMethod.Patch, profileId, "update_group", new()
        {
            ["phone_number_id"] = phoneNumberId,
            ["group_id"] = groupId,
            ["subject"] = subject,
            ["description"] = description,
            ["join_approval_mode"] = joinApprovalMode,
        }, profileGroupId, idempotencyKey, cancellationToken);

    public Task<JsonElement> DeleteGroupAsync(string profileId, string phoneNumberId, string groupId, string? profileGroupId = null, string? idempotencyKey = null, CancellationToken cancellationToken = default) =>
        CallAsync(HttpMethod.Delete, profileId, "delete_group", new() { ["phone_number_id"] = phoneNumberId, ["group_id"] = groupId }, profileGroupId, idempotencyKey, cancellationToken);

    public Task<JsonElement> AddGroupParticipantsAsync(string profileId, string phoneNumberId, string groupId, IEnumerable<string> phoneNumbers, string? profileGroupId = null, string? idempotencyKey = null, CancellationToken cancellationToken = default) =>
        GroupMembersAsync("add_group_participants", profileId, phoneNumberId, groupId, phoneNumbers, profileGroupId, idempotencyKey, cancellationToken);

    public Task<JsonElement> RemoveGroupParticipantsAsync(string profileId, string phoneNumberId, string groupId, IEnumerable<string> phoneNumbers, string? profileGroupId = null, string? idempotencyKey = null, CancellationToken cancellationToken = default) =>
        GroupMembersAsync("remove_group_participants", profileId, phoneNumberId, groupId, phoneNumbers, profileGroupId, idempotencyKey, cancellationToken);

    public Task<JsonElement> CreateGroupInviteLinkAsync(string profileId, string phoneNumberId, string groupId, string? profileGroupId = null, string? idempotencyKey = null, CancellationToken cancellationToken = default) =>
        CallAsync(HttpMethod.Post, profileId, "create_group_invite_link", new() { ["phone_number_id"] = phoneNumberId, ["group_id"] = groupId }, profileGroupId, idempotencyKey, cancellationToken);

    public Task<JsonElement> ListGroupJoinRequestsAsync(string profileId, string phoneNumberId, string groupId, string? profileGroupId = null, CancellationToken cancellationToken = default) =>
        CallAsync(HttpMethod.Get, profileId, "group_join_requests", new() { ["phone_number_id"] = phoneNumberId, ["group_id"] = groupId }, profileGroupId, null, cancellationToken);

    public Task<JsonElement> ApproveGroupJoinRequestsAsync(string profileId, string phoneNumberId, string groupId, IEnumerable<string> phoneNumbers, string? profileGroupId = null, string? idempotencyKey = null, CancellationToken cancellationToken = default) =>
        GroupMembersAsync("approve_group_join_requests", profileId, phoneNumberId, groupId, phoneNumbers, profileGroupId, idempotencyKey, cancellationToken);

    public Task<JsonElement> RejectGroupJoinRequestsAsync(string profileId, string phoneNumberId, string groupId, IEnumerable<string> phoneNumbers, string? profileGroupId = null, string? idempotencyKey = null, CancellationToken cancellationToken = default) =>
        GroupMembersAsync("reject_group_join_requests", profileId, phoneNumberId, groupId, phoneNumbers, profileGroupId, idempotencyKey, cancellationToken);

    // --- Conversions (account-level) ---

    public Task<JsonElement> GetDatasetAsync(string profileId, string? profileGroupId = null, CancellationToken cancellationToken = default) =>
        CallAsync(HttpMethod.Get, profileId, "dataset", new(), profileGroupId, null, cancellationToken);

    public Task<JsonElement> CreateDatasetAsync(string profileId, string? profileGroupId = null, string? idempotencyKey = null, CancellationToken cancellationToken = default) =>
        CallAsync(HttpMethod.Post, profileId, "create_dataset", new(), profileGroupId, idempotencyKey, cancellationToken);

    /// <summary>Reports a conversion back to Meta for a click-to-WhatsApp ad.</summary>
    public Task<JsonElement> SendConversionEventAsync(string profileId, WhatsAppConversionEvent conversionEvent, string? profileGroupId = null, string? idempotencyKey = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(conversionEvent);

        return CallAsync(HttpMethod.Post, profileId, "send_conversion_event", new()
        {
            ["event_name"] = conversionEvent.EventName,
            ["event_id"] = conversionEvent.EventId,
            ["chat_id"] = conversionEvent.ChatId,
            ["phone"] = conversionEvent.Phone,
            ["dataset_id"] = conversionEvent.DatasetId,
            ["event_time"] = conversionEvent.EventTime,
            ["email"] = conversionEvent.Email,
            ["external_id"] = conversionEvent.ExternalId,
            ["currency"] = conversionEvent.Currency,
            ["value"] = conversionEvent.Value,
            ["content_ids"] = conversionEvent.ContentIds,
            ["test_code"] = conversionEvent.TestCode,
        }, profileGroupId, idempotencyKey, cancellationToken);
    }

    // --- Media ---

    /// <summary>
    /// Downloads an inbound media file (e.g. from a message attachment's
    /// <c>ExternalId</c>). Returns the raw bytes and their content type.
    /// </summary>
    public Task<BinaryResponse> DownloadMediaAsync(string profileId, string phoneNumberId, string mediaId, string? profileGroupId = null, CancellationToken cancellationToken = default)
    {
        var query = BuildQuery(profileGroupId);
        query["phone_number_id"] = phoneNumberId;
        query["media_id"] = mediaId;

        return _client.GetBytesAsync($"/api/profiles/{Uri.EscapeDataString(profileId)}/whatsapp/media", query, cancellationToken);
    }

    private Task<JsonElement> GroupMembersAsync(string action, string profileId, string phoneNumberId, string groupId, IEnumerable<string> phoneNumbers, string? profileGroupId, string? idempotencyKey, CancellationToken cancellationToken) =>
        CallAsync(HttpMethod.Post, profileId, action, new()
        {
            ["phone_number_id"] = phoneNumberId,
            ["group_id"] = groupId,
            ["phone_numbers"] = phoneNumbers.ToList(),
        }, profileGroupId, idempotencyKey, cancellationToken);

    // GET and DELETE send `data` as query params; everything else as a JSON body.
    private Task<JsonElement> CallAsync(
        HttpMethod method,
        string profileId,
        string action,
        Dictionary<string, object?> data,
        string? profileGroupId,
        string? idempotencyKey,
        CancellationToken cancellationToken)
    {
        var path = $"/api/profiles/{Uri.EscapeDataString(profileId)}/whatsapp/{action}";
        var query = BuildQuery(profileGroupId);
        var compact = data.Where(kv => kv.Value is not null).ToDictionary(kv => kv.Key, kv => kv.Value);

        if (method == HttpMethod.Get || method == HttpMethod.Delete)
        {
            foreach (var (key, value) in compact)
                query[key] = Convert.ToString(value, CultureInfo.InvariantCulture)!;

            return method == HttpMethod.Get
                ? _client.GetAsync<JsonElement>(path, query, cancellationToken)
                : _client.DeleteAsync<JsonElement>(path, query, cancellationToken, idempotencyKey);
        }

        if (method == HttpMethod.Put)
            return _client.PutAsync<JsonElement>(path, compact, query, cancellationToken, idempotencyKey);
        if (method == HttpMethod.Patch)
            return _client.PatchAsync<JsonElement>(path, compact, query, cancellationToken, idempotencyKey);
        return _client.PostAsync<JsonElement>(path, compact, query, cancellationToken, idempotencyKey);
    }

    private Dictionary<string, string> BuildQuery(string? profileGroupId)
    {
        var query = new Dictionary<string, string>();
        var pgId = profileGroupId ?? _defaultProfileGroupId;
        if (pgId is not null) query["profile_group_id"] = pgId;
        return query;
    }
}
