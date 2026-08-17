using System.Text.Json;
using System.Text.Json.Serialization;

namespace PostProxy.Models;

public record Message
{
    [JsonPropertyName("id")]
    public required string Id { get; init; }

    [JsonPropertyName("chat_id")]
    public required string ChatId { get; init; }

    [JsonPropertyName("external_id")]
    public string? ExternalId { get; init; }

    [JsonPropertyName("direction")]
    public MessageDirection Direction { get; init; }

    [JsonPropertyName("body")]
    public string? Body { get; init; }

    [JsonPropertyName("status")]
    public MessageStatus Status { get; init; }

    [JsonPropertyName("tag")]
    public string? Tag { get; init; }

    [JsonPropertyName("external_comment_id")]
    public string? ExternalCommentId { get; init; }

    [JsonPropertyName("error_message")]
    public string? ErrorMessage { get; init; }

    [JsonPropertyName("platform_data")]
    public IReadOnlyDictionary<string, JsonElement>? PlatformData { get; init; }

    [JsonPropertyName("external_posted_at")]
    public DateTimeOffset? ExternalPostedAt { get; init; }

    [JsonPropertyName("external_delivered_at")]
    public DateTimeOffset? ExternalDeliveredAt { get; init; }

    [JsonPropertyName("external_read_at")]
    public DateTimeOffset? ExternalReadAt { get; init; }

    [JsonPropertyName("external_edited_at")]
    public DateTimeOffset? ExternalEditedAt { get; init; }

    [JsonPropertyName("reply_to_external_id")]
    public string? ReplyToExternalId { get; init; }

    [JsonPropertyName("reply_markup")]
    public IReadOnlyDictionary<string, JsonElement>? ReplyMarkup { get; init; }

    [JsonPropertyName("quick_replies")]
    public IReadOnlyList<QuickReply>? QuickReplies { get; init; }

    [JsonPropertyName("buttons")]
    public IReadOnlyList<MessageButton>? Buttons { get; init; }

    [JsonPropertyName("card")]
    public MessageCard? Card { get; init; }

    [JsonPropertyName("tapped_action")]
    public TappedAction? TappedAction { get; init; }

    [JsonPropertyName("external_deleted_at")]
    public DateTimeOffset? ExternalDeletedAt { get; init; }

    [JsonPropertyName("reactions")]
    public IReadOnlyList<Reaction> Reactions { get; init; } = Array.Empty<Reaction>();

    [JsonPropertyName("attachments")]
    public IReadOnlyList<Attachment> Attachments { get; init; } = Array.Empty<Attachment>();

    [JsonPropertyName("is_unsupported")]
    public bool IsUnsupported { get; init; }

    [JsonPropertyName("created_at")]
    public DateTimeOffset? CreatedAt { get; init; }
}
