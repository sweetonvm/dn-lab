using WebApi.Constants;

namespace WebApi.Exceptions;

/// <summary>
/// Represents a downstream service failure that maps to a 502 response.
/// </summary>
public sealed class DownstreamServiceException : AppException
{
    public DownstreamServiceException(
        string message,
        string? referenceId = null,
        Exception? inner = null
    )
        : base(ErrorCodes.Downstream, StatusCodes.Status502BadGateway, message, inner)
    {
        ReferenceId = referenceId;
    }
}
