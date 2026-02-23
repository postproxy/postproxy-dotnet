namespace PostProxy.Exceptions;

public class PostProxyException : Exception
{
    public int StatusCode { get; }
    public IReadOnlyDictionary<string, object>? Response { get; }

    public PostProxyException(string message, int statusCode, IReadOnlyDictionary<string, object>? response = null)
        : base(message)
    {
        StatusCode = statusCode;
        Response = response;
    }

    internal static PostProxyException FromStatusCode(int statusCode, string message, IReadOnlyDictionary<string, object>? response) =>
        statusCode switch
        {
            400 => new BadRequestException(message, response),
            401 => new AuthenticationException(message, response),
            404 => new NotFoundException(message, response),
            422 => new ValidationException(message, response),
            _ => new PostProxyException(message, statusCode, response),
        };
}

public class AuthenticationException(string message, IReadOnlyDictionary<string, object>? response = null)
    : PostProxyException(message, 401, response);

public class BadRequestException(string message, IReadOnlyDictionary<string, object>? response = null)
    : PostProxyException(message, 400, response);

public class NotFoundException(string message, IReadOnlyDictionary<string, object>? response = null)
    : PostProxyException(message, 404, response);

public class ValidationException(string message, IReadOnlyDictionary<string, object>? response = null)
    : PostProxyException(message, 422, response);
