using PostProxy;

var client = PostProxyClient.Builder("your-api-key")
    .Build();
var gbp = client.GoogleBusiness;

var profileId = "your-google-business-profile-id";

// locationId is the placement id of the location
var placements = await client.Profiles.PlacementsAsync(profileId);
var locationId = placements.Data[0].Id;

// Read and update the location. Google's fields are camelCased and passed through as-is.
var location = await gbp.GetLocationAsync(profileId, locationId,
    readMask: "title,phoneNumbers,websiteUri,regularHours");
Console.WriteLine($"Location: {location}");

await gbp.UpdateLocationAsync(profileId, locationId, ["websiteUri"],
    new Dictionary<string, object?> { ["websiteUri"] = "https://example.com" });

// Opening hours: Mon-Fri 9-17
string[] weekdays = ["MONDAY", "TUESDAY", "WEDNESDAY", "THURSDAY", "FRIDAY"];
await gbp.UpdateHoursAsync(profileId, locationId,
    fields: ["regularHours"],
    regularHours: new
    {
        periods = weekdays.Select(day => new
        {
            openDay = day,
            openTime = new { hours = 9 },
            closeDay = day,
            closeTime = new { hours = 17 },
        }),
    });

// Booking link
var types = await gbp.ListAvailablePlaceActionTypesAsync(profileId, locationId);
Console.WriteLine($"Available action types: {types}");
await gbp.CreatePlaceActionLinkAsync(profileId, locationId,
    "https://book.example.com", "APPOINTMENT", isPreferred: true);

// Add a storefront photo
await gbp.CreateMediaAsync(profileId, locationId, "https://example.com/storefront.jpg",
    category: "EXTERIOR", mediaFormat: "PHOTO");
Console.WriteLine($"Media: {await gbp.ListMediaAsync(profileId, locationId)}");

// Searches that showed the location, month by month (at most 12 months)
var keywords = await gbp.GetSearchKeywordsAsync(profileId, locationId,
    fromMonth: "2026-07", toMonth: "2026-09");
foreach (var month in keywords.Months)
{
    foreach (var k in month.Keywords)
    {
        // Low-volume keywords report only an upper bound
        var count = k.Impressions?.ToString() ?? $"<{k.ImpressionsThreshold}";
        Console.WriteLine($"{month.Month} {k.Keyword}: {count}");
    }
}
