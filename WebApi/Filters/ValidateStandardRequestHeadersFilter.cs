using FluentValidation;
using Microsoft.AspNetCore.Mvc.Filters;
using WebApi.Attributes;
using WebApi.Exceptions;
using WebApi.Models.Request;

namespace WebApi.Filters;

/// <summary>
/// Validates the standard request headers for endpoints marked with
/// <see cref="RequiresStandardHeadersAttribute"/>.
/// </summary>
public sealed class ValidateStandardRequestHeadersFilter(
    StandardRequestHeaders requestHeaders,
    IValidator<StandardRequestHeaders> validator
) : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        var attribute = context
            .ActionDescriptor.EndpointMetadata.OfType<RequiresStandardHeadersAttribute>()
            .FirstOrDefault();

        if (attribute is null)
            return; // skip validation

        var result = validator.Validate(requestHeaders);

        if (!result.IsValid)
        {
            throw ValidationAppException.From(result);
        }
    }

    public void OnActionExecuted(ActionExecutedContext context) { }
}
