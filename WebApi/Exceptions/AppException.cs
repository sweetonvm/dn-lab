namespace WebApi.Exceptions;

/// <summary>
/// Base class for expected application exceptions that map to an HTTP status code.
/// </summary>
public abstract class AppException(
    string code,
    int statusCode,
    string message,
    Exception? inner = null)
    : Exception(message, inner)
{
    public string Code { get; } = code;
    public int StatusCode { get; } = statusCode;
    public string? ReferenceId { get; init; }
}
