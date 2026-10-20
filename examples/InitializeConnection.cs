using PostProxy;
using PostProxy.Models;

var client = PostProxyClient.Builder("your-api-key")
    .Build();

// List profile groups
var groups = await client.ProfileGroups.ListAsync();
Console.WriteLine($"Profile Groups: {string.Join(", ", groups.Data.Select(g => g.Name))}");

// Initialize a connection
var connection = await client.ProfileGroups.InitializeConnectionAsync(
    "your-profile-group-id",
    Platform.Instagram,
    "https://your-app.com/callback");
Console.WriteLine($"Connection URL: {connection.Url}");

// Instagram via Facebook login (needed to attach music to reels)
var igViaFacebook = await client.ProfileGroups.InitializeConnectionAsync(
    "your-profile-group-id",
    Platform.Instagram,
    "https://your-app.com/callback",
    onboarding: ConnectionOnboarding.Facebook);
Console.WriteLine($"Instagram (Facebook login) URL: {igViaFacebook.Url}");

// WhatsApp through the WhatsApp Business app (ConnectionOnboarding.Api for the Cloud API)
var whatsapp = await client.ProfileGroups.InitializeConnectionAsync(
    "your-profile-group-id",
    Platform.WhatsApp,
    "https://your-app.com/callback",
    onboarding: ConnectionOnboarding.BusinessApp);
Console.WriteLine($"WhatsApp URL: {whatsapp.Url}");

// After connecting, list a profile's placements (Pages, channels, locations)
var placements = await client.Profiles.PlacementsAsync("profile-id");
foreach (var p in placements.Data)
    Console.WriteLine($"Placement: {p.Name} ({p.Id})");

// Move one placement to a different profile group
if (placements.Data.Count > 0)
{
    await client.Profiles.AssignPlacementToGroupAsync(
        "profile-id", placements.Data[0].Id, "other-group-id");
}
