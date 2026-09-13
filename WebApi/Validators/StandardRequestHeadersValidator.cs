using FluentValidation;
using WebApi.Models.Request;

namespace WebApi.Validators;

/// <summary>
/// Validates the standard request headers.
/// </summary>
public sealed class StandardRequestHeadersValidator : AbstractValidator<StandardRequestHeaders>
{
    public StandardRequestHeadersValidator()
    {
        RuleFor(x => x.SessionId)
            .Must(x =>
                !string.IsNullOrWhiteSpace(x) && Guid.TryParse(x, out var guid) && guid.Version == 4
            )
            .WithMessage("SessionId must be a valid UUIDv4");
    }
}
