using PostProxy;
using PostProxy.Models;
using PostProxy.Parameters;
using PostProxy.Webhooks;

var client = PostProxyClient.Builder("your-api-key")
    .Build();

// Only the body, the YouTube title and the Pinterest title / destination link
// can change once a post is published.
var post = await client.Posts.UpdateAsync("published-post-id", new UpdatePostParams
{
    Body = "Launch day! (now with the right link)",
    Platforms = new PlatformParams
    {
        YouTube = new YouTubeParams { Title = "Launch day — full walkthrough" },
        Pinterest = new PinterestParams { DestinationLink = "https://example.com/launch" },
    },
});
foreach (var p in post.Platforms ?? [])
    Console.WriteLine($"{p.Platform}: {p.Permalink}");

// The edit is pushed to each platform in the background. Your webhook handler
// learns how it went:
static void HandleWebhook(string rawBody)
{
    var ev = WebhookEvents.Parse(rawBody);
    if (ev.Type == WebhookEventType.PlatformPostEdited)
    {
        Console.WriteLine($"Edited on {WebhookEvents.AsPlatformPost(ev).Platform}");
    }
    else if (ev.Type == WebhookEventType.PlatformPostEditFailed)
    {
        var data = WebhookEvents.AsPlatformPost(ev);
        Console.WriteLine($"Edit failed on {data.Platform}: {data.Error}");
    }
}
