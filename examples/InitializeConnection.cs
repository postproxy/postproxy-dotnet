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
