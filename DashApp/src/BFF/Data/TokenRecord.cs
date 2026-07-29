namespace BFF.Data;

public record TokenRecord(
    string AccessToken,
    string? RefreshToken,
    DateTimeOffset? ExpiresAt
)
{
    public bool IsExpired => ExpiresAt.HasValue && DateTimeOffset.UtcNow >= ExpiresAt.Value;
}