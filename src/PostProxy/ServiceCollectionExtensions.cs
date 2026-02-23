using System.Net.Http.Headers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace PostProxy;

public class PostProxyOptions
{
    public string ApiKey { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = "https://api.postproxy.dev";
    public string? ProfileGroupId { get; set; }
}

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPostProxy(this IServiceCollection services, Action<PostProxyOptions> configure)
    {
        services.Configure(configure);

        services.AddHttpClient("PostProxy", (sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<PostProxyOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/'));
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", options.ApiKey);
        });

        services.AddSingleton<PostProxyClient>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<PostProxyOptions>>().Value;
            var factory = sp.GetRequiredService<IHttpClientFactory>();
            var httpClient = factory.CreateClient("PostProxy");
            var client = new PostProxyHttpClient(httpClient);
            return new PostProxyClient(client, options.ProfileGroupId);
        });

        return services;
    }
}
