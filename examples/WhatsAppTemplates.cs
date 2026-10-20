using PostProxy;
using PostProxy.Models;
using PostProxy.Parameters;

var client = PostProxyClient.Builder("your-api-key")
    .Build();

var profileId = "your-whatsapp-profile-id";

// Create a template — Meta reviews it before it can be sent
var template = await client.WhatsAppTemplates.CreateAsync(profileId, new CreateWhatsAppTemplateParams
{
    Name = "order_shipped",
    Category = "UTILITY",
    Language = "en_US",
    Components = [new { type = "BODY", text = "Hi {{1}}, your order {{2}} has shipped." }],
});
Console.WriteLine($"{template.Name} {template.Status}"); // "order_shipped PENDING"

// Check approval status (or subscribe to message_template.status_updated)
var approved = await client.WhatsAppTemplates.ListAsync(profileId, status: "APPROVED", refresh: true);
Console.WriteLine($"Approved: {string.Join(", ", approved.Data.Select(t => t.Name))}");

// Send it to a customer. The phone number id is a placement of the profile.
var numbers = await client.Profiles.PlacementsAsync(profileId);
var chat = await client.Chats.CreateAsync(profileId, new CreateChatParams
{
    ParticipantExternalId = "15551234567",
    PlacementId = numbers.Data[0].Id,
});

await client.Messages.SendAsync(chat.Id, new SendMessageParams
{
    Template = new MessageTemplateInput
    {
        Name = "order_shipped",
        Language = "en_US",
        Variables = ["Jane", "#1234"],
    },
});

// Inside the 24h window, free-form messages work too
var fresh = await client.Chats.GetAsync(chat.Id);
if (fresh.WithinMessagingWindow == true)
{
    await client.Messages.SendAsync(chat.Id, new SendMessageParams
    {
        Location = new MessageLocation { Latitude = 52.52, Longitude = 13.405, Name = "Pickup point" },
    });
}
