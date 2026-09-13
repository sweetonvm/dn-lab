using FluentValidation;
using WebApi.Models.Request;

namespace WebApi.Validators;

/// <summary>
/// Validates the orders query parameters.
/// </summary>
public sealed class OrdersQueryValidator : AbstractValidator<OrdersQuery>
{
    public OrdersQueryValidator()
    {
        RuleFor(x => x.OrderType).NotEmpty();
    }
}
