namespace WebApi.Models.Request;

/// <summary>
/// Carries the cross-cutting request headers for the current request.
/// </summary>
public sealed class StandardRequestHeaders
{
    public string CorrelationId { get; set; } = string.Empty;

    public string? SessionId { get; set; }
}
