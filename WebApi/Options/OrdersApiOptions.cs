using System.ComponentModel.DataAnnotations;

namespace WebApi.Options;

/// <summary>
/// Configuration for the downstream orders API.
/// </summary>
public sealed class OrdersApiOptions
{
    public const string SectionName = "OrdersApi";

    [Required] 
    public required Uri BaseUrl { get; set; }

    [Range(1, 300)]
    public int TimeoutSeconds { get; set; } = 10;
}
