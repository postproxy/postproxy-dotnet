using PostProxy.Models;

namespace PostProxy.Resources;

public class QueuesResource
{
    private readonly PostProxyHttpClient _client;

    internal QueuesResource(PostProxyHttpClient client)
    {
        _client = client;
    }

    public Task<ListResponse<Queue>> ListAsync(CancellationToken cancellationToken = default) =>
        _client.GetAsync<ListResponse<Queue>>("/api/post_queues", cancellationToken: cancellationToken);

    public Task<Queue> GetAsync(string id, CancellationToken cancellationToken = default) =>
        _client.GetAsync<Queue>($"/api/post_queues/{Uri.EscapeDataString(id)}", cancellationToken: cancellationToken);

    public Task<NextSlotResponse> NextSlotAsync(string id, CancellationToken cancellationToken = default) =>
        _client.GetAsync<NextSlotResponse>($"/api/post_queues/{Uri.EscapeDataString(id)}/next_slot", cancellationToken: cancellationToken);

    public Task<Queue> CreateAsync(
        string name,
        string profileGroupId,
        string? description = null,
        string? timezone = null,
        int? jitter = null,
        IEnumerable<object>? timeslots = null,
        CancellationToken cancellationToken = default)
    {
        var postQueue = new Dictionary<string, object> { ["name"] = name };
        if (description is not null) postQueue["description"] = description;
        if (timezone is not null) postQueue["timezone"] = timezone;
        if (jitter is not null) postQueue["jitter"] = jitter.Value;
        if (timeslots is not null) postQueue["queue_timeslots_attributes"] = timeslots.ToList();

        var body = new Dictionary<string, object>
        {
            ["profile_group_id"] = profileGroupId,
            ["post_queue"] = postQueue,
        };

        return _client.PostAsync<Queue>("/api/post_queues", body, cancellationToken: cancellationToken);
    }

    public Task<Queue> UpdateAsync(
        string id,
        string? name = null,
        string? description = null,
        string? timezone = null,
        bool? enabled = null,
        int? jitter = null,
        IEnumerable<object>? timeslots = null,
        CancellationToken cancellationToken = default)
    {
        var postQueue = new Dictionary<string, object>();
        if (name is not null) postQueue["name"] = name;
        if (description is not null) postQueue["description"] = description;
        if (timezone is not null) postQueue["timezone"] = timezone;
        if (enabled is not null) postQueue["enabled"] = enabled.Value;
        if (jitter is not null) postQueue["jitter"] = jitter.Value;
        if (timeslots is not null) postQueue["queue_timeslots_attributes"] = timeslots.ToList();

        var body = new Dictionary<string, object> { ["post_queue"] = postQueue };

        return _client.PatchAsync<Queue>($"/api/post_queues/{Uri.EscapeDataString(id)}", body, cancellationToken: cancellationToken);
    }

    public Task<DeleteResponse> DeleteAsync(string id, CancellationToken cancellationToken = default) =>
        _client.DeleteAsync<DeleteResponse>($"/api/post_queues/{Uri.EscapeDataString(id)}", cancellationToken: cancellationToken);
}
