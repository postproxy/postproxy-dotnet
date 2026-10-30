using PostProxy;

var client = PostProxyClient.Builder(Environment.GetEnvironmentVariable("POSTPROXY_API_KEY")!)
    .Build();

// Meta's own inbox apps
const string PageInboxAppId = "263902037430900";
const string InstagramInboxAppId = "1217981644879628"; // use this one for Instagram chats

// A Facebook or Instagram chat
var chatId = "your-chat-id";

// Who controls the conversation right now? Both fields are null when idle.
var owner = await client.Chats.GetThreadOwnerAsync(chatId);
Console.WriteLine($"Owner app: {owner.AppId} until {owner.ExpiresAt}");

// Escalate to a human in Meta's inbox
var chat = await client.Chats.PassThreadControlAsync(chatId, PageInboxAppId, metadata: "escalated to support");
if (chat.Metadata is not null && chat.Metadata.TryGetValue("thread_control", out var threadControl))
    Console.WriteLine($"Thread control: {threadControl}");

// Later: take it back (needs Conversation Routing on the Page) ...
await client.Chats.TakeThreadControlAsync(chatId);
// ... or, without Conversation Routing, ask the owner to pass it back and
// wait for the chat.thread_control_changed webhook.
// await client.Chats.RequestThreadControlAsync(chatId);

// Done: let the conversation go idle
await client.Chats.ReleaseThreadControlAsync(chatId);

_ = InstagramInboxAppId;
