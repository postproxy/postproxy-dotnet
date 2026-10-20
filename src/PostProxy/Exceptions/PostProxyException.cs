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

    internal static PostProxyException FromStatusCode(int statusCode, string message, IReadOnlyDictionary<string, object>? response, int? retryAfter = null) =>
        statusCode switch
        {
            400 => new BadRequestException(message, response),
            401 => new AuthenticationException(message, response),
            403 => new ForbiddenException(message, response),
            404 => new NotFoundException(message, response),
            409 => new ConflictException(message, response),
            422 => new ValidationException(message, response),
            429 => new RateLimitException(message, response, retryAfter),
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

/// <summary>
/// 409. Thrown for a duplicate submission (<c>Response["duplicate_post_id"]</c>),
/// a backfill that is already running (<c>Response["profile_sync_id"]</c>), or a
/// request whose <c>Idempotency-Key</c> is still in flight.
/// </summary>
public class ConflictException(string message, IReadOnlyDictionary<string, object>? response = null)
    : PostProxyException(message, 409, response);

/// <summary>
/// 403. The account is canceled or past due, or the platform refused the
/// action for lack of permission.
/// </summary>
public class ForbiddenException(string message, IReadOnlyDictionary<string, object>? response = null)
    : PostProxyException(message, 403, response);

/// <summary>
/// 429. The platform is rate-limiting; retry after <see cref="RetryAfter"/> seconds.
/// </summary>
public class RateLimitException(string message, IReadOnlyDictionary<string, object>? response = null, int? retryAfter = null)
    : PostProxyException(message, 429, response)
{
    /// <summary>Seconds from the <c>Retry-After</c> header, or null when it was absent.</summary>
    public int? RetryAfter { get; } = retryAfter;
}
