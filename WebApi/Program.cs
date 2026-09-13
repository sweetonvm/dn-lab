using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using WebApi.Constants;
using WebApi.Filters;
using WebApi.Handlers;
using WebApi.Middleware;
using WebApi.Models.Request;
using WebApi.Models.Response;
using WebApi.Options;
using WebApi.Services;
using WebApi.Validators;

var builder = WebApplication.CreateBuilder(args);

builder
    .Services.AddOptions<OrdersApiOptions>()
    .Bind(builder.Configuration.GetSection(OrdersApiOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddHttpClient<OrderService>((sp, client) =>
{
    var options = sp.GetRequiredService<IOptions<OrdersApiOptions>>().Value;
    client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
});
builder.Services.AddScoped<StandardRequestHeaders>();
builder.Services.AddValidatorsFromAssemblyContaining<StandardRequestHeadersValidator>();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddControllers(options =>
{
    options.Filters.Add<ValidateStandardRequestHeadersFilter>();
});

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var messages = context.ModelState.Values
            .SelectMany(entry => entry.Errors)
            .Select(e => e.ErrorMessage)
            .ToArray();

        var httpContext = context.HttpContext;

        return new BadRequestObjectResult(
            new ErrorResponse
            {
                Code = ErrorCodes.Validation,
                Message = messages.Length > 0
                    ? string.Join("; ", messages)
                    : "One or more validation errors occurred.",
                Timestamp = DateTimeOffset.UtcNow,
                TraceId = httpContext.TraceIdentifier,
                CorrelationId =
                    httpContext.Items[ApiConstants.CorrelationIdItem] as string
                    ?? httpContext.Request
                        .Headers[ApiConstants.CorrelationIdHeaderName]
                        .FirstOrDefault(),
            }
        );
    };
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.OperationFilter<StandardRequestHeadersFilter>();
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseExceptionHandler();
app.UseHttpsRedirection();
app.UseRouting();
app.UseMiddleware<StandardRequestHeadersMiddleware>();
app.UseAuthorization();
app.MapControllers();

app.Run();
