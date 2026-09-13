namespace WebApi.Models.Request;

/// <summary>
/// Query parameters accepted by the orders endpoint.
/// </summary>
public sealed record OrdersQuery
{
    public string? OrderType { get; init; }

    public bool DownstreamErrorScenario { get; init; }
}
