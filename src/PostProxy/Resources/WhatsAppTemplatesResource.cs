using System.Text.Json;
using PostProxy.Models;
using PostProxy.Parameters;

namespace PostProxy.Resources;

/// <summary>
/// WhatsApp message templates, which live on the WhatsApp Business account (the
/// profile). A template is addressed by its PostProxy id, Meta's id, or its name —
/// pass <c>language</c> with a name that exists in several languages.
/// </summary>
public class WhatsAppTemplatesResource
{
    private readonly PostProxyHttpClient _client;
    private readonly string? _defaultProfileGroupId;

    internal WhatsAppTemplatesResource(PostProxyHttpClient client, string? defaultProfileGroupId)
    {
        _client = client;
        _defaultProfileGroupId = defaultProfileGroupId;
    }

    /// <summary><paramref name="refresh"/> re-reads the templates from Meta first.</summary>
    public Task<ListResponse<WhatsAppTemplate>> ListAsync(
        string profileId,
        string? name = null,
        string? language = null,
        string? status = null,
        bool? refresh = null,
        string? profileGroupId = null,
        CancellationToken cancellationToken = default)
    {
        var query = BuildQuery(profileGroupId);
        if (name is not null) query["name"] = name;
        if (language is not null) query["language"] = language;
        if (status is not null) query["status"] = status;
        if (refresh is not null) query["refresh"] = refresh.Value ? "true" : "false";

        return _client.GetAsync<ListResponse<WhatsAppTemplate>>(TemplatesPath(profileId), query, cancellationToken);
    }

    public Task<WhatsAppTemplate> GetAsync(
        string profileId,
        string id,
        string? language = null,
        string? profileGroupId = null,
        CancellationToken cancellationToken = default)
    {
        var query = BuildQuery(profileGroupId);
        if (language is not null) query["language"] = language;

        return _client.GetAsync<WhatsAppTemplate>(TemplatesPath(profileId, id), query, cancellationToken);
    }

    /// <summary>
    /// Submits a template to Meta for review; follow its status via <see cref="GetAsync"/>
    /// or the <c>message_template.status_updated</c> webhook.
    /// </summary>
    public Task<WhatsAppTemplate> CreateAsync(string profileId, CreateWhatsAppTemplateParams parameters, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(parameters);

        var body = new Dictionary<string, object?>
        {
            ["name"] = parameters.Name,
            ["category"] = parameters.Category,
            ["language"] = parameters.Language,
        };
        if (parameters.ParameterFormat is not null) body["parameter_format"] = parameters.ParameterFormat;
        if (parameters.Components is not null) body["components"] = parameters.Components.Select(PostProxyHttpClient.Raw).ToList();
        if (parameters.LibraryTemplateName is not null) body["library_template_name"] = parameters.LibraryTemplateName;
        if (parameters.LibraryTemplateBodyInputs is not null) body["library_template_body_inputs"] = PostProxyHttpClient.Raw(parameters.LibraryTemplateBodyInputs);
        if (parameters.LibraryTemplateButtonInputs is not null) body["library_template_button_inputs"] = parameters.LibraryTemplateButtonInputs.Select(PostProxyHttpClient.Raw).ToList();
        if (parameters.MessageSendTtlSeconds is not null) body["message_send_ttl_seconds"] = parameters.MessageSendTtlSeconds;

        return _client.PostAsync<WhatsAppTemplate>(TemplatesPath(profileId), body, BuildQuery(parameters.ProfileGroupId), cancellationToken, parameters.IdempotencyKey);
    }

    public Task<WhatsAppTemplate> UpdateAsync(string profileId, string id, UpdateWhatsAppTemplateParams parameters, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(parameters);

        var query = BuildQuery(parameters.ProfileGroupId);
        if (parameters.Language is not null) query["language"] = parameters.Language;

        var body = new Dictionary<string, object?>();
        if (parameters.Components is not null) body["components"] = parameters.Components.Select(PostProxyHttpClient.Raw).ToList();
        if (parameters.MessageSendTtlSeconds is not null) body["message_send_ttl_seconds"] = parameters.MessageSendTtlSeconds;

        return _client.PatchAsync<WhatsAppTemplate>(TemplatesPath(profileId, id), body, query, cancellationToken, parameters.IdempotencyKey);
    }

    /// <summary>Deleting by name without <paramref name="language"/> removes every language of the template.</summary>
    public Task<JsonElement> DeleteAsync(
        string profileId,
        string id,
        string? language = null,
        string? profileGroupId = null,
        string? idempotencyKey = null,
        CancellationToken cancellationToken = default)
    {
        var query = BuildQuery(profileGroupId);
        if (language is not null) query["language"] = language;

        return _client.DeleteAsync<JsonElement>(TemplatesPath(profileId, id), query, cancellationToken, idempotencyKey);
    }

    /// <summary>Meta's ready-made template library.</summary>
    public Task<JsonElement> LibraryAsync(
        string profileId,
        string? name = null,
        string? language = null,
        string? profileGroupId = null,
        CancellationToken cancellationToken = default)
    {
        var query = BuildQuery(profileGroupId);
        if (name is not null) query["name"] = name;
        if (language is not null) query["language"] = language;

        return _client.GetAsync<JsonElement>($"{TemplatesPath(profileId)}/library", query, cancellationToken);
    }

    private static string TemplatesPath(string profileId, string? id = null)
    {
        var path = $"/api/profiles/{Uri.EscapeDataString(profileId)}/templates";
        return id is null ? path : $"{path}/{Uri.EscapeDataString(id)}";
    }

    private Dictionary<string, string> BuildQuery(string? profileGroupId)
    {
        var query = new Dictionary<string, string>();
        var pgId = profileGroupId ?? _defaultProfileGroupId;
        if (pgId is not null) query["profile_group_id"] = pgId;
        return query;
    }
}
