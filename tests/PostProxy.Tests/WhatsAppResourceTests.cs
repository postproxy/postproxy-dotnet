using PostProxy.Parameters;

namespace PostProxy.Tests;

public class WhatsAppTemplatesResourceTests
{
    private const string MockTemplate = """
    {
        "id": "tpl_1",
        "external_id": "998877",
        "profile_id": "prof_wa",
        "name": "order_update",
        "language": "en_US",
        "category": "UTILITY",
        "status": "APPROVED",
        "parameter_format": "POSITIONAL",
        "components": [{"type": "BODY", "text": "Order {{1}} shipped"}],
        "variable_count": 1,
        "created_at": "2026-09-28T00:00:00Z",
        "updated_at": "2026-09-28T00:00:00Z"
    }
    """;

    [Fact]
    public async Task ListAsync_WithFilters()
    {
        var (client, handler) = TestHelpers.CreateMockClient();
        handler.EnqueueResponse($$"""{"data": [{{MockTemplate}}]}""");

        var result = await client.WhatsAppTemplates.ListAsync("prof_wa", status: "APPROVED", refresh: true);

        Assert.Equal("order_update", result.Data[0].Name);
        Assert.Equal("BODY", result.Data[0].Components[0]["type"].GetString());
        Assert.Equal("/api/profiles/prof_wa/templates?status=APPROVED&refresh=true", handler.Requests[0].Url);
    }

    [Fact]
    public async Task GetAsync_EncodesNameAndSendsLanguage()
    {
        var (client, handler) = TestHelpers.CreateMockClient();
        handler.EnqueueResponse(MockTemplate);

        await client.WhatsAppTemplates.GetAsync("prof_wa", "order update", language: "en_US");

        Assert.Equal("/api/profiles/prof_wa/templates/order%20update?language=en_US", handler.Requests[0].Url);
    }

    [Fact]
    public async Task CreateUpdateDelete()
    {
        var (client, handler) = TestHelpers.CreateMockClient();
        handler.EnqueueResponse(MockTemplate);
        handler.EnqueueResponse(MockTemplate);
        handler.EnqueueResponse("""{"success": true}""");

        await client.WhatsAppTemplates.CreateAsync("prof_wa", new CreateWhatsAppTemplateParams
        {
            Name = "order_update",
            Category = "UTILITY",
            Language = "en_US",
            Components = [new { type = "BODY", text = "Order {{1}} shipped" }],
        });
        await client.WhatsAppTemplates.UpdateAsync("prof_wa", "tpl_1", new UpdateWhatsAppTemplateParams { MessageSendTtlSeconds = 3600 });
        await client.WhatsAppTemplates.DeleteAsync("prof_wa", "order_update");

        var reqs = handler.Requests;
        Assert.Equal(HttpMethod.Post, reqs[0].Method);
        Assert.Equal("""{"name":"order_update","category":"UTILITY","language":"en_US","components":[{"type":"BODY","text":"Order {{1}} shipped"}]}""", reqs[0].Body);
        Assert.Equal(HttpMethod.Patch, reqs[1].Method);
        Assert.Equal("/api/profiles/prof_wa/templates/tpl_1", reqs[1].Url);
        Assert.Equal("""{"message_send_ttl_seconds":3600}""", reqs[1].Body);
        Assert.Equal(HttpMethod.Delete, reqs[2].Method);
        Assert.Equal("/api/profiles/prof_wa/templates/order_update", reqs[2].Url);
    }

    [Fact]
    public async Task LibraryAsync_BuildsQuery()
    {
        var (client, handler) = TestHelpers.CreateMockClient();
        handler.EnqueueResponse("""{"data": []}""");

        await client.WhatsAppTemplates.LibraryAsync("prof_wa", name: "delivery");

        Assert.Equal("/api/profiles/prof_wa/templates/library?name=delivery", handler.Requests[0].Url);
    }
}

public class WhatsAppResourceTests
{
    private const string Base = "/api/profiles/prof_wa/whatsapp";

    [Fact]
    public async Task Reads_SendPhoneNumberIdAsQuery()
    {
        var (client, handler) = TestHelpers.CreateMockClient();
        handler.EnqueueResponse("""{"verified_name": "Acme"}""");
        handler.EnqueueResponse("""{"data": []}""");

        var info = await client.WhatsApp.GetNumberInfoAsync("prof_wa", "pn_1");
        await client.WhatsApp.ListBlockedUsersAsync("prof_wa", "pn_1", limit: 10);

        Assert.Equal("Acme", info.GetProperty("verified_name").GetString());
        Assert.Equal($"{Base}/number_info?phone_number_id=pn_1", handler.Requests[0].Url);
        Assert.Equal($"{Base}/blocked_users?phone_number_id=pn_1&limit=10", handler.Requests[1].Url);
    }

    [Fact]
    public async Task Writes_SendJsonBodies()
    {
        var (client, handler) = TestHelpers.CreateMockClient();
        for (var i = 0; i < 8; i++) handler.EnqueueResponse("""{"success": true}""");

        await client.WhatsApp.RegisterPhoneNumberAsync("prof_wa", "pn_1", "123456");
        await client.WhatsApp.RequestVerificationCodeAsync("prof_wa", "pn_1", method: "SMS", language: "en");
        await client.WhatsApp.VerifyPhoneNumberAsync("prof_wa", "pn_1", "000111");
        await client.WhatsApp.UpdateBusinessProfileAsync("prof_wa", "pn_1", about: "Hi", websites: ["https://acme.test"]);
        await client.WhatsApp.UpdateBusinessProfilePhotoAsync("prof_wa", "pn_1", url: "https://img.test/a.png");
        await client.WhatsApp.RequestDisplayNameChangeAsync("prof_wa", "pn_1", "Acme Co");
        await client.WhatsApp.SetUsernameAsync("prof_wa", "pn_1", "acme", transferAction: "force_transfer");
        await client.WhatsApp.BlockUsersAsync("prof_wa", "pn_1", ["15551234567"]);

        var reqs = handler.Requests;
        Assert.Equal(
            new[] { HttpMethod.Post, HttpMethod.Post, HttpMethod.Post, HttpMethod.Patch, HttpMethod.Post, HttpMethod.Post, HttpMethod.Put, HttpMethod.Post },
            reqs.Select(r => r.Method));
        Assert.Equal("""{"phone_number_id":"pn_1","pin":"123456"}""", reqs[0].Body);
        Assert.Equal("""{"phone_number_id":"pn_1","method":"SMS","language":"en"}""", reqs[1].Body);
        Assert.Equal("""{"phone_number_id":"pn_1","about":"Hi","websites":["https://acme.test"]}""", reqs[3].Body);
        Assert.Equal("""{"phone_number_id":"pn_1","url":"https://img.test/a.png"}""", reqs[4].Body);
        Assert.Equal("""{"phone_number_id":"pn_1","display_name":"Acme Co"}""", reqs[5].Body);
        Assert.Equal("""{"phone_number_id":"pn_1","username":"acme","transfer_action":"force_transfer"}""", reqs[6].Body);
        Assert.Equal("""{"phone_number_id":"pn_1","users":["15551234567"]}""", reqs[7].Body);
    }

    [Fact]
    public async Task DeleteUsernameAsync_UsesQueryParams()
    {
        var (client, handler) = TestHelpers.CreateMockClient();
        handler.EnqueueResponse("""{"success": true}""");

        await client.WhatsApp.DeleteUsernameAsync("prof_wa", "pn_1");

        var req = handler.Requests[0];
        Assert.Equal(HttpMethod.Delete, req.Method);
        Assert.Null(req.Body);
        Assert.Equal($"{Base}/delete_username?phone_number_id=pn_1", req.Url);
    }

    [Fact]
    public async Task ManagesGroups()
    {
        var (client, handler) = TestHelpers.CreateMockClient();
        for (var i = 0; i < 7; i++) handler.EnqueueResponse("{}");

        await client.WhatsApp.CreateGroupAsync("prof_wa", "pn_1", "Customers", joinApprovalMode: "approval_required");
        await client.WhatsApp.GetGroupAsync("prof_wa", "pn_1", "g_1");
        await client.WhatsApp.UpdateGroupAsync("prof_wa", "pn_1", "g_1", subject: "VIPs");
        await client.WhatsApp.AddGroupParticipantsAsync("prof_wa", "pn_1", "g_1", ["15551234567"]);
        await client.WhatsApp.CreateGroupInviteLinkAsync("prof_wa", "pn_1", "g_1");
        await client.WhatsApp.ApproveGroupJoinRequestsAsync("prof_wa", "pn_1", "g_1", ["15557654321"]);
        await client.WhatsApp.DeleteGroupAsync("prof_wa", "pn_1", "g_1");

        var reqs = handler.Requests;
        Assert.Equal("""{"phone_number_id":"pn_1","subject":"Customers","join_approval_mode":"approval_required"}""", reqs[0].Body);
        Assert.Equal($"{Base}/group?phone_number_id=pn_1&group_id=g_1", reqs[1].Url);
        Assert.Equal("""{"phone_number_id":"pn_1","group_id":"g_1","subject":"VIPs"}""", reqs[2].Body);
        Assert.Equal($"{Base}/add_group_participants", reqs[3].Url);
        Assert.Equal("""{"phone_number_id":"pn_1","group_id":"g_1","phone_numbers":["15551234567"]}""", reqs[3].Body);
        Assert.Equal($"{Base}/create_group_invite_link", reqs[4].Url);
        Assert.Equal($"{Base}/approve_group_join_requests", reqs[5].Url);
        Assert.Equal(HttpMethod.Delete, reqs[6].Method);
        Assert.Equal($"{Base}/delete_group?phone_number_id=pn_1&group_id=g_1", reqs[6].Url);
    }

    [Fact]
    public async Task ConversionEvents_TakeNoPhoneNumberId()
    {
        var (client, handler) = TestHelpers.CreateMockClient();
        handler.EnqueueResponse("""{"id": "ds_1"}""");
        handler.EnqueueResponse("""{"events_received": 1}""");

        await client.WhatsApp.GetDatasetAsync("prof_wa");
        await client.WhatsApp.SendConversionEventAsync("prof_wa", new WhatsAppConversionEvent
        {
            EventName = "Purchase",
            EventId = "order-42",
            ChatId = "chat_1",
            Currency = "USD",
            Value = 19.99m,
        });

        Assert.Equal($"{Base}/dataset", handler.Requests[0].Url);
        Assert.Equal($"{Base}/send_conversion_event", handler.Requests[1].Url);
        Assert.Equal("""{"event_name":"Purchase","event_id":"order-42","chat_id":"chat_1","currency":"USD","value":19.99}""", handler.Requests[1].Body);
    }
}
