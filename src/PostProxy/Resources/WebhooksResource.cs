using PostProxy.Models;

namespace PostProxy.Resources;

public class WebhooksResource
{
    private readonly PostProxyHttpClient _client;

    internal WebhooksResource(PostProxyHttpClient client)
    {
        _client = client;
    }

    public Task<ListResponse<Webhook>> ListAsync(CancellationToken cancellationToken = default) =>
        _client.GetAsync<ListResponse<Webhook>>("/api/webhooks", cancellationToken: cancellationToken);

    public Task<Webhook> GetAsync(string id, CancellationToken cancellationToken = default) =>
        _client.GetAsync<Webhook>($"/api/webhooks/{Uri.EscapeDataString(id)}", cancellationToken: cancellationToken);

    public Task<Webhook> CreateAsync(string url, IEnumerable<string> events, string? description = null, CancellationToken cancellationToken = default)
    {
        var body = new Dictionary<string, object>
        {
            ["url"] = url,
            ["events"] = events.ToList(),
        };
        if (description is not null)
            body["description"] = description;

        return _client.PostAsync<Webhook>("/api/webhooks", body, cancellationToken: cancellationToken);
    }

    public Task<Webhook> UpdateAsync(string id, string? url = null, IEnumerable<string>? events = null, bool? enabled = null, string? description = null, CancellationToken cancellationToken = default)
    {
        var body = new Dictionary<string, object>();
        if (url is not null) body["url"] = url;
        if (events is not null) body["events"] = events.ToList();
        if (enabled is not null) body["enabled"] = enabled.Value;
        if (description is not null) body["description"] = description;

        return _client.PatchAsync<Webhook>($"/api/webhooks/{Uri.EscapeDataString(id)}", body, cancellationToken: cancellationToken);
    }

    public Task<DeleteResponse> DeleteAsync(string id, CancellationToken cancellationToken = default) =>
        _client.DeleteAsync<DeleteResponse>($"/api/webhooks/{Uri.EscapeDataString(id)}", cancellationToken: cancellationToken);

    public Task<PaginatedResponse<WebhookDelivery>> DeliveriesAsync(string id, int? page = null, int? perPage = null, CancellationToken cancellationToken = default)
    {
        var query = new Dictionary<string, string>();
        if (page is not null) query["page"] = page.Value.ToString();
        if (perPage is not null) query["per_page"] = perPage.Value.ToString();

        return _client.GetAsync<PaginatedResponse<WebhookDelivery>>($"/api/webhooks/{Uri.EscapeDataString(id)}/deliveries", query, cancellationToken);
    }
}
