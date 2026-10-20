namespace PostProxy.Parameters;

/// <summary>
/// A conversion (e.g. Purchase, LeadSubmitted) reported back to Meta for a
/// click-to-WhatsApp ad. Identify the person by <see cref="ChatId"/> or <see cref="Phone"/>.
/// </summary>
public record WhatsAppConversionEvent
{
    public required string EventName { get; init; }
    public required string EventId { get; init; }
    public string? ChatId { get; init; }
    public string? Phone { get; init; }
    public string? DatasetId { get; init; }

    /// <summary>Unix timestamp or ISO 8601 time; defaults to now.</summary>
    public string? EventTime { get; init; }

    public string? Email { get; init; }
    public string? ExternalId { get; init; }
    public string? Currency { get; init; }
    public decimal? Value { get; init; }
    public IReadOnlyList<string>? ContentIds { get; init; }
    public string? TestCode { get; init; }
}
