namespace WebApi.Constants;

/// <summary>
/// Machine-readable error codes returned in error responses.
/// </summary>
public static class ErrorCodes
{
    public const string Validation = "VALIDATION_ERROR";
    public const string Downstream = "DOWNSTREAM_ERROR";
    public const string Timeout = "TIMEOUT";
    public const string Unexpected = "UNEXPECTED_ERROR";
}
