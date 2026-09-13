namespace WebApi.Models.Response;

/// <summary>
/// The standard error envelope returned by the API.
/// </summary>
public sealed class ErrorResponse
{
    public required string Code { get; init; }
    public required string Message { get; init; }
    public required DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;
    public required string TraceId { get; init; }
    public string? CorrelationId { get; init; }
    public string? ReferenceId { get; init; }
}
