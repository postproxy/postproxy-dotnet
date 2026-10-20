using PostProxy;
using PostProxy.Parameters;

var client = PostProxyClient.Builder("your-api-key")
    .Build();
var wa = client.WhatsApp;

var profileId = "your-whatsapp-profile-id";
var phoneNumberId = "your-phone-number-id";

// Business profile
await wa.UpdateBusinessProfileAsync(profileId, phoneNumberId,
    about: "Fresh coffee, fast replies",
    websites: ["https://example.com"]);

// Create a group and share its invite link
var group = await wa.CreateGroupAsync(profileId, phoneNumberId, "VIP customers",
    description: "Early access to new roasts",
    joinApprovalMode: "approval_required");
var groupId = group.GetProperty("id").ToString();
var invite = await wa.CreateGroupInviteLinkAsync(profileId, phoneNumberId, groupId);
Console.WriteLine($"Invite link: {invite}");

// Approve everyone waiting to join
var requests = await wa.ListGroupJoinRequestsAsync(profileId, phoneNumberId, groupId);
Console.WriteLine($"Join requests: {requests}");
await wa.ApproveGroupJoinRequestsAsync(profileId, phoneNumberId, groupId, ["15551234567"]);

// Block a spammer
await wa.BlockUsersAsync(profileId, phoneNumberId, ["15557654321"]);

// Report a purchase back to Meta for a click-to-WhatsApp ad
await wa.SendConversionEventAsync(profileId, new WhatsAppConversionEvent
{
    EventName = "Purchase",
    EventId = "order-1234",
    Phone = "15551234567",
    Currency = "USD",
    Value = 24.5m,
});
