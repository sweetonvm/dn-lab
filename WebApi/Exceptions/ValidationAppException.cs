using FluentValidation.Results;
using WebApi.Constants;

namespace WebApi.Exceptions;

/// <summary>
/// Represents a validation failure that maps to a 400 response.
/// </summary>
public sealed class ValidationAppException(IDictionary<string, string[]> errors)
    : AppException(
        ErrorCodes.Validation,
        StatusCodes.Status400BadRequest,
        string.Join("; ", errors.SelectMany(e => e.Value))
    )
{
    public static ValidationAppException From(ValidationResult result)
    {
        var errors = result.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

        return new ValidationAppException(errors);
    }
}
