namespace PostProxy.Parameters;

public record CreateWhatsAppTemplateParams
{
    public required string Name { get; init; }

    /// <summary>MARKETING, UTILITY or AUTHENTICATION.</summary>
    public required string Category { get; init; }

    public required string Language { get; init; }

    /// <summary>POSITIONAL or NAMED.</summary>
    public string? ParameterFormat { get; init; }

    /// <summary>Meta's component objects (HEADER, BODY, FOOTER, BUTTONS), sent as-is.</summary>
    public IReadOnlyList<object>? Components { get; init; }

    /// <summary>Create from Meta's template library instead of <see cref="Components"/>.</summary>
    public string? LibraryTemplateName { get; init; }

    public object? LibraryTemplateBodyInputs { get; init; }
    public IReadOnlyList<object>? LibraryTemplateButtonInputs { get; init; }
    public int? MessageSendTtlSeconds { get; init; }
    public string? ProfileGroupId { get; init; }
    public string? IdempotencyKey { get; init; }
}

public record UpdateWhatsAppTemplateParams
{
    /// <summary>Meta's component objects, sent as-is.</summary>
    public IReadOnlyList<object>? Components { get; init; }

    public int? MessageSendTtlSeconds { get; init; }

    /// <summary>Picks the language when the template is addressed by a name that exists in several.</summary>
    public string? Language { get; init; }

    public string? ProfileGroupId { get; init; }
    public string? IdempotencyKey { get; init; }
}
