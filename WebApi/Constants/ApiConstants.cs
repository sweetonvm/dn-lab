namespace WebApi.Constants;

/// <summary>
/// Shared names used by the cross-cutting header plumbing.
/// </summary>
public static class ApiConstants
{
    public static readonly object CorrelationIdItem = new();
    public const string CorrelationIdHeaderName = "x-correlation-id";
    public const string SessionIdHeaderName = "x-session-id";
}
