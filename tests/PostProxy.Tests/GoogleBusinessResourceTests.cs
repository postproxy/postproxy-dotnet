namespace PostProxy.Tests;

public class GoogleBusinessResourceTests
{
    private const string Base = "/api/profiles/prof_gbp/google_business";

    [Fact]
    public async Task GetLocationAsync_WithReadMask()
    {
        var (client, handler) = TestHelpers.CreateMockClient();
        handler.EnqueueResponse("""{"title": "Cafe"}""");

        var result = await client.GoogleBusiness.GetLocationAsync("prof_gbp", "loc_1", readMask: "title,phoneNumbers");

        Assert.Equal("Cafe", result.GetProperty("title").GetString());
        Assert.Equal($"{Base}/location?location_id=loc_1&read_mask=title%2CphoneNumbers", handler.Requests[0].Url);
    }

    [Fact]
    public async Task UpdateLocationAsync_KeepsGoogleCamelCase()
    {
        var (client, handler) = TestHelpers.CreateMockClient();
        handler.EnqueueResponse("{}");

        await client.GoogleBusiness.UpdateLocationAsync("prof_gbp", "loc_1", ["websiteUri"],
            new Dictionary<string, object?> { ["websiteUri"] = "https://acme.test" });

        var req = handler.Requests[0];
        Assert.Equal(HttpMethod.Patch, req.Method);
        Assert.Equal($"{Base}/update_location", req.Url);
        Assert.Equal("""{"location_id":"loc_1","websiteUri":"https://acme.test","fields":["websiteUri"]}""", req.Body);
    }

    [Fact]
    public async Task ListAvailableLocationCategoriesAsync_OmitsUnsetParams()
    {
        var (client, handler) = TestHelpers.CreateMockClient();
        handler.EnqueueResponse("""{"categories": []}""");

        await client.GoogleBusiness.ListAvailableLocationCategoriesAsync("prof_gbp", "loc_1", "US", pageSize: 50);

        Assert.Equal($"{Base}/available_location_categories?location_id=loc_1&region_code=US&page_size=50", handler.Requests[0].Url);
    }

    [Fact]
    public async Task UpdateHoursAsync_PassesNestedObjectsThrough()
    {
        var (client, handler) = TestHelpers.CreateMockClient();
        handler.EnqueueResponse("{}");

        await client.GoogleBusiness.UpdateHoursAsync("prof_gbp", "loc_1",
            fields: ["regularHours"],
            regularHours: new
            {
                periods = new[] { new { openDay = "MONDAY", openTime = new { hours = 9 }, closeDay = "MONDAY", closeTime = new { hours = 17 } } },
            });

        Assert.Equal(
            """{"location_id":"loc_1","fields":["regularHours"],"regularHours":{"periods":[{"openDay":"MONDAY","openTime":{"hours":9},"closeDay":"MONDAY","closeTime":{"hours":17}}]}}""",
            handler.Requests[0].Body);
    }

    [Fact]
    public async Task UpdatesAttributesServicesAndMenus()
    {
        var (client, handler) = TestHelpers.CreateMockClient();
        for (var i = 0; i < 3; i++) handler.EnqueueResponse("{}");

        await client.GoogleBusiness.UpdateAttributesAsync("prof_gbp", "loc_1",
            [new { name = "attributes/has_wifi", values = new[] { true } }], attributeMask: ["attributes/has_wifi"]);
        await client.GoogleBusiness.UpdateServiceListAsync("prof_gbp", "loc_1", [new { freeFormServiceItem = new { } }]);
        await client.GoogleBusiness.UpdateFoodMenusAsync("prof_gbp", "loc_1", [new { labels = Array.Empty<object>() }], fields: ["menus"]);

        Assert.Equal($"{Base}/update_attributes", handler.Requests[0].Url);
        Assert.Equal("""{"location_id":"loc_1","attributes":[{"name":"attributes/has_wifi","values":[true]}],"attribute_mask":["attributes/has_wifi"]}""", handler.Requests[0].Body);
        Assert.Equal("""{"location_id":"loc_1","serviceItems":[{"freeFormServiceItem":{}}]}""", handler.Requests[1].Body);
        Assert.Equal("""{"location_id":"loc_1","menus":[{"labels":[]}],"fields":["menus"]}""", handler.Requests[2].Body);
    }

    [Fact]
    public async Task ReadsAttributesServicesAndMenus()
    {
        var (client, handler) = TestHelpers.CreateMockClient();
        for (var i = 0; i < 4; i++) handler.EnqueueResponse("{}");

        await client.GoogleBusiness.GetAttributesAsync("prof_gbp", "loc_1");
        await client.GoogleBusiness.ListAvailableAttributesAsync("prof_gbp", "loc_1", showAll: true);
        await client.GoogleBusiness.GetServiceListAsync("prof_gbp", "loc_1");
        await client.GoogleBusiness.GetFoodMenusAsync("prof_gbp", "loc_1", fields: ["menus", "name"]);

        Assert.Equal($"{Base}/attributes?location_id=loc_1", handler.Requests[0].Url);
        Assert.Equal($"{Base}/available_attributes?location_id=loc_1&show_all=true", handler.Requests[1].Url);
        Assert.Equal($"{Base}/service_list?location_id=loc_1", handler.Requests[2].Url);
        Assert.Equal($"{Base}/food_menus?location_id=loc_1&fields=menus%2Cname", handler.Requests[3].Url);
    }

    [Fact]
    public async Task ManagesPlaceActionLinks()
    {
        var (client, handler) = TestHelpers.CreateMockClient();
        for (var i = 0; i < 5; i++) handler.EnqueueResponse("{}");

        await client.GoogleBusiness.ListPlaceActionLinksAsync("prof_gbp", "loc_1");
        await client.GoogleBusiness.ListAvailablePlaceActionTypesAsync("prof_gbp", "loc_1", languageCode: "en");
        await client.GoogleBusiness.CreatePlaceActionLinkAsync("prof_gbp", "loc_1", "https://book.test", "APPOINTMENT", isPreferred: true);
        await client.GoogleBusiness.UpdatePlaceActionLinkAsync("prof_gbp", "loc_1", "locations/1/placeActionLinks/2", ["uri"], uri: "https://book2.test");
        await client.GoogleBusiness.DeletePlaceActionLinkAsync("prof_gbp", "loc_1", "locations/1/placeActionLinks/2");

        var reqs = handler.Requests;
        Assert.Equal(HttpMethod.Get, reqs[0].Method);
        Assert.Contains("language_code=en", reqs[1].Url);
        Assert.Equal(HttpMethod.Post, reqs[2].Method);
        Assert.Equal("""{"location_id":"loc_1","uri":"https://book.test","place_action_type":"APPOINTMENT","is_preferred":true}""", reqs[2].Body);
        Assert.Equal(HttpMethod.Patch, reqs[3].Method);
        Assert.Equal("""{"location_id":"loc_1","name":"locations/1/placeActionLinks/2","fields":["uri"],"uri":"https://book2.test"}""", reqs[3].Body);
        Assert.Equal(HttpMethod.Delete, reqs[4].Method);
        Assert.Equal($"{Base}/delete_place_action_link?location_id=loc_1&name=locations%2F1%2FplaceActionLinks%2F2", reqs[4].Url);
    }

    [Fact]
    public async Task ManagesMedia()
    {
        var (client, handler) = TestHelpers.CreateMockClient();
        for (var i = 0; i < 3; i++) handler.EnqueueResponse("{}");

        await client.GoogleBusiness.ListMediaAsync("prof_gbp", "loc_1", pageSize: 10);
        await client.GoogleBusiness.CreateMediaAsync("prof_gbp", "loc_1", "https://img.test/a.jpg", category: "COVER", mediaFormat: "PHOTO");
        await client.GoogleBusiness.DeleteMediaAsync("prof_gbp", "loc_1", "accounts/1/locations/1/media/9");

        var reqs = handler.Requests;
        Assert.Equal($"{Base}/media?location_id=loc_1&page_size=10", reqs[0].Url);
        Assert.Equal("""{"location_id":"loc_1","media_url":"https://img.test/a.jpg","category":"COVER","media_format":"PHOTO"}""", reqs[1].Body);
        Assert.Equal(HttpMethod.Delete, reqs[2].Method);
        Assert.Contains("media_name=accounts%2F1%2Flocations%2F1%2Fmedia%2F9", reqs[2].Url);
    }
}
