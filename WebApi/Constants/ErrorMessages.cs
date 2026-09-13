namespace WebApi.Constants;

/// <summary>
/// Human-readable messages used in error responses.
/// </summary>
public static class ErrorMessages
{
    public const string Downstream = "Downstream service returned an error.";
    public const string Timeout = "The downstream request timed out.";
    public const string Unexpected = "An unexpected error occurred.";
}
