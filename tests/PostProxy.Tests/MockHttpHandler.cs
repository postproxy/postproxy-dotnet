using System.Net;
using System.Text;

namespace PostProxy.Tests;

public record RecordedRequest(HttpMethod Method, string Url, string? Body);

public class MockHttpHandler : HttpMessageHandler
{
    private readonly Queue<(HttpStatusCode StatusCode, string Body)> _responses = new();
    private readonly List<RecordedRequest> _requests = [];

    public IReadOnlyList<RecordedRequest> Requests => _requests;

    public void EnqueueResponse(HttpStatusCode statusCode, string body)
    {
        _responses.Enqueue((statusCode, body));
    }

    public void EnqueueResponse(string body) => EnqueueResponse(HttpStatusCode.OK, body);

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        string? body = null;
        if (request.Content is not null)
            body = await request.Content.ReadAsStringAsync(cancellationToken);

        _requests.Add(new RecordedRequest(request.Method, request.RequestUri!.PathAndQuery, body));

        if (_responses.Count == 0)
            throw new InvalidOperationException("No mock responses enqueued");

        var (statusCode, responseBody) = _responses.Dequeue();
        return new HttpResponseMessage(statusCode)
        {
            Content = new StringContent(responseBody, Encoding.UTF8, "application/json"),
        };
    }
}
