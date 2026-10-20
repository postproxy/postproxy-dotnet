using System.Net;
using System.Net.Http.Headers;
using System.Text;

namespace PostProxy.Tests;

public record RecordedRequest(HttpMethod Method, string Url, string? Body, IReadOnlyDictionary<string, string> Headers);

public class MockHttpHandler : HttpMessageHandler
{
    private readonly Queue<Func<HttpResponseMessage>> _responses = new();
    private readonly List<RecordedRequest> _requests = [];

    public IReadOnlyList<RecordedRequest> Requests => _requests;

    public void EnqueueResponse(HttpStatusCode statusCode, string body, IReadOnlyDictionary<string, string>? headers = null)
    {
        _responses.Enqueue(() =>
        {
            var response = new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(body, Encoding.UTF8, "application/json"),
            };
            if (headers is not null)
            {
                foreach (var (name, value) in headers)
                    response.Headers.TryAddWithoutValidation(name, value);
            }
            return response;
        });
    }

    public void EnqueueResponse(string body) => EnqueueResponse(HttpStatusCode.OK, body);

    public void EnqueueBinaryResponse(byte[] data, string contentType)
    {
        _responses.Enqueue(() =>
        {
            var content = new ByteArrayContent(data);
            content.Headers.ContentType = new MediaTypeHeaderValue(contentType);
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = content };
        });
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        string? body = null;
        if (request.Content is not null)
            body = await request.Content.ReadAsStringAsync(cancellationToken);

        var headers = request.Headers.ToDictionary(h => h.Key, h => string.Join(",", h.Value));
        _requests.Add(new RecordedRequest(request.Method, request.RequestUri!.PathAndQuery, body, headers));

        if (_responses.Count == 0)
            throw new InvalidOperationException("No mock responses enqueued");

        return _responses.Dequeue()();
    }
}
