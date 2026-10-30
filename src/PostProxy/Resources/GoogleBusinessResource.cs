using System.Text.Json;
using PostProxy.Models;

namespace PostProxy.Resources;

/// <summary>
/// Manages a Google Business Profile location's own data — hours, attributes,
/// services, menus, action links and photos — separate from posting to it.
/// </summary>
/// <remarks>
/// <c>locationId</c> is the placement id from <c>Profiles.PlacementsAsync(profileId)</c>.
/// Google's payloads are camelCased and passed through untouched in both directions, so
/// you can PATCH back exactly what you GET. Updates take a <c>fields</c> list naming
/// which top-level fields to change (Google's update mask).
/// </remarks>
public class GoogleBusinessResource
{
    private readonly PostProxyHttpClient _client;
    private readonly string? _defaultProfileGroupId;

    internal GoogleBusinessResource(PostProxyHttpClient client, string? defaultProfileGroupId)
    {
        _client = client;
        _defaultProfileGroupId = defaultProfileGroupId;
    }

    // --- Location ---

    public Task<JsonElement> GetLocationAsync(
        string profileId,
        string locationId,
        string? readMask = null,
        string? profileGroupId = null,
        CancellationToken cancellationToken = default) =>
        ReadAsync(profileId, "location", locationId, new() { ["read_mask"] = readMask }, profileGroupId, cancellationToken);

    /// <summary>
    /// <paramref name="location"/> holds the Google fields to change (e.g. <c>title</c>,
    /// <c>phoneNumbers</c>, <c>websiteUri</c>); <paramref name="fields"/> names them.
    /// </summary>
    public Task<JsonElement> UpdateLocationAsync(
        string profileId,
        string locationId,
        IEnumerable<string> fields,
        IReadOnlyDictionary<string, object?> location,
        string? profileGroupId = null,
        string? idempotencyKey = null,
        CancellationToken cancellationToken = default)
    {
        var body = new Dictionary<string, object?>(location) { ["fields"] = fields.ToList() };
        return WriteAsync(HttpMethod.Patch, profileId, "update_location", locationId, body, profileGroupId, idempotencyKey, cancellationToken);
    }

    public Task<JsonElement> ListAvailableLocationCategoriesAsync(
        string profileId,
        string locationId,
        string regionCode,
        string? languageCode = null,
        string? filter = null,
        string? view = null,
        int? pageSize = null,
        string? pageToken = null,
        string? profileGroupId = null,
        CancellationToken cancellationToken = default) =>
        ReadAsync(profileId, "available_location_categories", locationId, new()
        {
            ["region_code"] = regionCode,
            ["language_code"] = languageCode,
            ["filter"] = filter,
            ["view"] = view,
            ["page_size"] = pageSize?.ToString(),
            ["page_token"] = pageToken,
        }, profileGroupId, cancellationToken);

    public Task<JsonElement> UpdateHoursAsync(
        string profileId,
        string locationId,
        IEnumerable<string>? fields = null,
        object? regularHours = null,
        object? specialHours = null,
        IEnumerable<object>? moreHours = null,
        string? profileGroupId = null,
        string? idempotencyKey = null,
        CancellationToken cancellationToken = default) =>
        WriteAsync(HttpMethod.Patch, profileId, "update_hours", locationId, new()
        {
            ["fields"] = fields?.ToList(),
            ["regularHours"] = regularHours,
            ["specialHours"] = specialHours,
            ["moreHours"] = moreHours?.ToList(),
        }, profileGroupId, idempotencyKey, cancellationToken);

    // --- Attributes ---

    public Task<JsonElement> GetAttributesAsync(
        string profileId,
        string locationId,
        string? profileGroupId = null,
        CancellationToken cancellationToken = default) =>
        ReadAsync(profileId, "attributes", locationId, new(), profileGroupId, cancellationToken);

    public Task<JsonElement> ListAvailableAttributesAsync(
        string profileId,
        string locationId,
        string? categoryName = null,
        string? regionCode = null,
        string? languageCode = null,
        bool? showAll = null,
        int? pageSize = null,
        string? pageToken = null,
        string? profileGroupId = null,
        CancellationToken cancellationToken = default) =>
        ReadAsync(profileId, "available_attributes", locationId, new()
        {
            ["category_name"] = categoryName,
            ["region_code"] = regionCode,
            ["language_code"] = languageCode,
            ["show_all"] = showAll is null ? null : showAll.Value ? "true" : "false",
            ["page_size"] = pageSize?.ToString(),
            ["page_token"] = pageToken,
        }, profileGroupId, cancellationToken);

    public Task<JsonElement> UpdateAttributesAsync(
        string profileId,
        string locationId,
        IEnumerable<object> attributes,
        IEnumerable<string>? attributeMask = null,
        string? profileGroupId = null,
        string? idempotencyKey = null,
        CancellationToken cancellationToken = default) =>
        WriteAsync(HttpMethod.Patch, profileId, "update_attributes", locationId, new()
        {
            ["attributes"] = attributes.ToList(),
            ["attribute_mask"] = attributeMask?.ToList(),
        }, profileGroupId, idempotencyKey, cancellationToken);

    // --- Services & food menus ---

    public Task<JsonElement> GetServiceListAsync(
        string profileId,
        string locationId,
        string? profileGroupId = null,
        CancellationToken cancellationToken = default) =>
        ReadAsync(profileId, "service_list", locationId, new(), profileGroupId, cancellationToken);

    public Task<JsonElement> UpdateServiceListAsync(
        string profileId,
        string locationId,
        IEnumerable<object> serviceItems,
        IEnumerable<string>? fields = null,
        string? profileGroupId = null,
        string? idempotencyKey = null,
        CancellationToken cancellationToken = default) =>
        WriteAsync(HttpMethod.Patch, profileId, "update_service_list", locationId, new()
        {
            ["serviceItems"] = serviceItems.ToList(),
            ["fields"] = fields?.ToList(),
        }, profileGroupId, idempotencyKey, cancellationToken);

    public Task<JsonElement> GetFoodMenusAsync(
        string profileId,
        string locationId,
        IEnumerable<string>? fields = null,
        string? profileGroupId = null,
        CancellationToken cancellationToken = default) =>
        ReadAsync(profileId, "food_menus", locationId, new()
        {
            ["fields"] = fields is null ? null : string.Join(",", fields),
        }, profileGroupId, cancellationToken);

    public Task<JsonElement> UpdateFoodMenusAsync(
        string profileId,
        string locationId,
        IEnumerable<object> menus,
        IEnumerable<string>? fields = null,
        string? profileGroupId = null,
        string? idempotencyKey = null,
        CancellationToken cancellationToken = default) =>
        WriteAsync(HttpMethod.Patch, profileId, "update_food_menus", locationId, new()
        {
            ["menus"] = menus.ToList(),
            ["fields"] = fields?.ToList(),
        }, profileGroupId, idempotencyKey, cancellationToken);

    // --- Place action links (booking, ordering, ...) ---

    public Task<JsonElement> ListPlaceActionLinksAsync(
        string profileId,
        string locationId,
        int? pageSize = null,
        string? pageToken = null,
        string? profileGroupId = null,
        CancellationToken cancellationToken = default) =>
        ReadAsync(profileId, "place_action_links", locationId, new()
        {
            ["page_size"] = pageSize?.ToString(),
            ["page_token"] = pageToken,
        }, profileGroupId, cancellationToken);

    public Task<JsonElement> ListAvailablePlaceActionTypesAsync(
        string profileId,
        string locationId,
        string? languageCode = null,
        int? pageSize = null,
        string? pageToken = null,
        string? profileGroupId = null,
        CancellationToken cancellationToken = default) =>
        ReadAsync(profileId, "available_place_action_types", locationId, new()
        {
            ["language_code"] = languageCode,
            ["page_size"] = pageSize?.ToString(),
            ["page_token"] = pageToken,
        }, profileGroupId, cancellationToken);

    /// <summary>
    /// <paramref name="placeActionType"/> is e.g. APPOINTMENT, ONLINE_APPOINTMENT,
    /// DINING_RESERVATION, FOOD_ORDERING, FOOD_DELIVERY, FOOD_TAKEOUT, SHOP_ONLINE.
    /// </summary>
    public Task<JsonElement> CreatePlaceActionLinkAsync(
        string profileId,
        string locationId,
        string uri,
        string placeActionType,
        bool? isPreferred = null,
        string? profileGroupId = null,
        string? idempotencyKey = null,
        CancellationToken cancellationToken = default) =>
        WriteAsync(HttpMethod.Post, profileId, "create_place_action_link", locationId, new()
        {
            ["uri"] = uri,
            ["place_action_type"] = placeActionType,
            ["is_preferred"] = isPreferred,
        }, profileGroupId, idempotencyKey, cancellationToken);

    /// <summary><paramref name="name"/> is the link's resource name from <see cref="ListPlaceActionLinksAsync"/>.</summary>
    public Task<JsonElement> UpdatePlaceActionLinkAsync(
        string profileId,
        string locationId,
        string name,
        IEnumerable<string> fields,
        string? uri = null,
        string? placeActionType = null,
        bool? isPreferred = null,
        string? profileGroupId = null,
        string? idempotencyKey = null,
        CancellationToken cancellationToken = default) =>
        WriteAsync(HttpMethod.Patch, profileId, "update_place_action_link", locationId, new()
        {
            ["name"] = name,
            ["fields"] = fields.ToList(),
            ["uri"] = uri,
            ["place_action_type"] = placeActionType,
            ["is_preferred"] = isPreferred,
        }, profileGroupId, idempotencyKey, cancellationToken);

    public Task<JsonElement> DeletePlaceActionLinkAsync(
        string profileId,
        string locationId,
        string name,
        string? profileGroupId = null,
        string? idempotencyKey = null,
        CancellationToken cancellationToken = default) =>
        RemoveAsync(profileId, "delete_place_action_link", locationId, "name", name, profileGroupId, idempotencyKey, cancellationToken);

    // --- Media ---

    public Task<JsonElement> ListMediaAsync(
        string profileId,
        string locationId,
        int? pageSize = null,
        string? pageToken = null,
        string? profileGroupId = null,
        CancellationToken cancellationToken = default) =>
        ReadAsync(profileId, "media", locationId, new()
        {
            ["page_size"] = pageSize?.ToString(),
            ["page_token"] = pageToken,
        }, profileGroupId, cancellationToken);

    /// <summary>
    /// <paramref name="category"/> is e.g. COVER, PROFILE, LOGO, EXTERIOR, INTERIOR, PRODUCT,
    /// ADDITIONAL; <paramref name="mediaFormat"/> is PHOTO or VIDEO.
    /// </summary>
    public Task<JsonElement> CreateMediaAsync(
        string profileId,
        string locationId,
        string mediaUrl,
        string? category = null,
        string? mediaFormat = null,
        string? description = null,
        string? profileGroupId = null,
        string? idempotencyKey = null,
        CancellationToken cancellationToken = default) =>
        WriteAsync(HttpMethod.Post, profileId, "create_media", locationId, new()
        {
            ["media_url"] = mediaUrl,
            ["category"] = category,
            ["media_format"] = mediaFormat,
            ["description"] = description,
        }, profileGroupId, idempotencyKey, cancellationToken);

    /// <summary><paramref name="mediaName"/> is the item's resource name from <see cref="ListMediaAsync"/>.</summary>
    public Task<JsonElement> DeleteMediaAsync(
        string profileId,
        string locationId,
        string mediaName,
        string? profileGroupId = null,
        string? idempotencyKey = null,
        CancellationToken cancellationToken = default) =>
        RemoveAsync(profileId, "delete_media", locationId, "media_name", mediaName, profileGroupId, idempotencyKey, cancellationToken);

    // --- Search keywords ---

    /// <summary>
    /// The searches that showed the location in Google Search and Maps, month by month.
    /// Months are <c>YYYY-MM</c>; the range is at most 12 months. Unlike the rest of this
    /// resource, the response is PostProxy-shaped (snake_case) and typed.
    /// </summary>
    public Task<SearchKeywordsResponse> GetSearchKeywordsAsync(
        string profileId,
        string locationId,
        string? fromMonth = null,
        string? toMonth = null,
        string? profileGroupId = null,
        CancellationToken cancellationToken = default)
    {
        var query = BuildQuery(profileGroupId);
        query["location_id"] = locationId;
        if (fromMonth is not null) query["from_month"] = fromMonth;
        if (toMonth is not null) query["to_month"] = toMonth;

        return _client.GetAsync<SearchKeywordsResponse>(ActionPath(profileId, "search_keywords"), query, cancellationToken);
    }

    private static string ActionPath(string profileId, string action) =>
        $"/api/profiles/{Uri.EscapeDataString(profileId)}/google_business/{action}";

    private Task<JsonElement> ReadAsync(
        string profileId,
        string action,
        string locationId,
        Dictionary<string, string?> parameters,
        string? profileGroupId,
        CancellationToken cancellationToken)
    {
        var query = BuildQuery(profileGroupId);
        query["location_id"] = locationId;
        foreach (var (key, value) in parameters)
        {
            if (value is not null) query[key] = value;
        }

        return _client.GetAsync<JsonElement>(ActionPath(profileId, action), query, cancellationToken);
    }

    private Task<JsonElement> WriteAsync(
        HttpMethod method,
        string profileId,
        string action,
        string locationId,
        Dictionary<string, object?> parameters,
        string? profileGroupId,
        string? idempotencyKey,
        CancellationToken cancellationToken)
    {
        // Google's own fields go through untouched, without snake_casing.
        var body = new Dictionary<string, object?> { ["location_id"] = locationId };
        foreach (var (key, value) in parameters)
        {
            if (value is not null) body[key] = PostProxyHttpClient.Raw(value);
        }

        var path = ActionPath(profileId, action);
        var query = BuildQuery(profileGroupId);
        return method == HttpMethod.Post
            ? _client.PostAsync<JsonElement>(path, body, query, cancellationToken, idempotencyKey)
            : _client.PatchAsync<JsonElement>(path, body, query, cancellationToken, idempotencyKey);
    }

    private Task<JsonElement> RemoveAsync(
        string profileId,
        string action,
        string locationId,
        string key,
        string value,
        string? profileGroupId,
        string? idempotencyKey,
        CancellationToken cancellationToken)
    {
        var query = BuildQuery(profileGroupId);
        query["location_id"] = locationId;
        query[key] = value;

        return _client.DeleteAsync<JsonElement>(ActionPath(profileId, action), query, cancellationToken, idempotencyKey);
    }

    private Dictionary<string, string> BuildQuery(string? profileGroupId)
    {
        var query = new Dictionary<string, string>();
        var pgId = profileGroupId ?? _defaultProfileGroupId;
        if (pgId is not null) query["profile_group_id"] = pgId;
        return query;
    }
}
