using WebApi.Attributes;
using WebApi.Constants;
using WebApi.Models.Request;

namespace WebApi.Middleware;

/// <summary>
/// Populates the scoped standard request headers and correlation id for the
/// current request.
/// </summary>
public sealed class StandardRequestHeadersMiddleware(
    RequestDelegate next,
    ILogger<StandardRequestHeadersMiddleware> logger
)
{
    public async Task InvokeAsync(HttpContext context, StandardRequestHeaders requestHeaders)
    {
        if (context.GetEndpoint()?.Metadata.GetMetadata<RequiresStandardHeadersAttribute>() is null)
        {
            await next(context);
            return;
        }

        var correlationId = context
            .Request.Headers[ApiConstants.CorrelationIdHeaderName]
            .FirstOrDefault();

        if (string.IsNullOrEmpty(correlationId))
        {
            correlationId = Guid.NewGuid().ToString();
        }

        requestHeaders.CorrelationId = correlationId;
        context.Items[ApiConstants.CorrelationIdItem] = correlationId;

        requestHeaders.SessionId = context
            .Request.Headers[ApiConstants.SessionIdHeaderName]
            .FirstOrDefault();

        context.Response.Headers[ApiConstants.CorrelationIdHeaderName] =
            requestHeaders.CorrelationId;

        using var scope = logger.BeginScope(
            new Dictionary<string, object?>
            {
                ["CorrelationId"] = requestHeaders.CorrelationId,
                ["SessionId"] = requestHeaders.SessionId,
            }
        );

        await next(context);
    }
}
