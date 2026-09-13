using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using WebApi.Attributes;
using WebApi.Exceptions;
using WebApi.Models.Request;
using WebApi.Services;

namespace WebApi.Controllers;

/// <summary>
/// Provides order-related operations for the BFF.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public sealed class OrdersController(
    OrderService service,
    IValidator<OrdersQuery> queryValidator,
    ILogger<OrdersController> logger
) : ControllerBase
{
    [HttpGet]
    [RequiresStandardHeaders]
    public async Task<IActionResult> GetAsync(
        [FromQuery] OrdersQuery query,
        CancellationToken cancellationToken
    )
    {
        var validationResult = await queryValidator.ValidateAsync(query, cancellationToken);

        if (!validationResult.IsValid)
        {
            throw ValidationAppException.From(validationResult);
        }

        logger.LogInformation("Processing order request with query: {Query}", query);

        var result = await service.PlaceOrderAsync(query, cancellationToken);
        return Ok(result);
    }
}
