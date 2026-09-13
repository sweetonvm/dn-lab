using Microsoft.AspNetCore.Diagnostics;
using WebApi.Constants;
using WebApi.Exceptions;
using WebApi.Models.Response;

namespace WebApi.Handlers;

/// <summary>
/// Maps unhandled exceptions to the standard error envelope.
/// </summary>
public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext context,
        Exception exception,
        CancellationToken cancellationToken
    )
    {
        if (context.RequestAborted.IsCancellationRequested)
        {
            // The client disconnected; nothing can be written back.
            logger.LogDebug(
                "Request {Method} {Path} was cancelled by the client.",
                context.Request.Method,
                context.Request.Path
            );
            return true;
        }

        var (statusCode, code, message, referenceId) = MapException(exception);

        if (statusCode >= StatusCodes.Status500InternalServerError)
        {
            logger.LogError(
                exception,
                "Unhandled exception on {Method} {Path}",
                context.Request.Method,
                context.Request.Path
            );
        }
        else
        {
            logger.LogInformation(
                "Handled client error {StatusCode} ({Code}) on {Method} {Path}: {Message}",
                statusCode,
                code,
                context.Request.Method,
                context.Request.Path,
                message
            );
        }

        var correlationId =
            context.Items[ApiConstants.CorrelationIdItem] as string
            ?? context.Request.Headers[ApiConstants.CorrelationIdHeaderName].FirstOrDefault();

        var response = new ErrorResponse
        {
            Code = code,
            Message = message,
            Timestamp = DateTimeOffset.UtcNow,
            TraceId = context.TraceIdentifier,
            CorrelationId = correlationId,
            ReferenceId = referenceId,
        };

        if (context.Response.HasStarted)
        {
            logger.LogWarning(
                "The response has already started, the global exception handler will not be executed."
            );
            return false;
        }

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(response, cancellationToken);

        return true;
    }

    private static (int StatusCode, string Code, string Message, string? ReferenceId) MapException(
        Exception exception
    )
    {
        return exception switch
        {
            ValidationAppException vex => (vex.StatusCode, vex.Code, vex.Message, vex.ReferenceId),

            AppException appEx => (appEx.StatusCode, appEx.Code, appEx.Message, appEx.ReferenceId),

            OperationCanceledException or TimeoutException => (
                StatusCodes.Status504GatewayTimeout,
                ErrorCodes.Timeout,
                ErrorMessages.Timeout,
                null
            ),

            _ => (
                StatusCodes.Status500InternalServerError,
                ErrorCodes.Unexpected,
                ErrorMessages.Unexpected,
                null
            ),
        };
    }
}
