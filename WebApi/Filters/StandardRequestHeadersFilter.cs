using System.Reflection;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using WebApi.Attributes;

namespace WebApi.Filters;

/// <summary>
/// Documents the standard request headers in the generated OpenAPI specification.
/// </summary>
public sealed class StandardRequestHeadersFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var hasAttribute =
            context.MethodInfo.GetCustomAttribute<RequiresStandardHeadersAttribute>(true)
            is not null;

        if (!hasAttribute)
            return;

        operation.Parameters?.Add(
            new OpenApiParameter
            {
                Name = "x-correlation-id",
                In = ParameterLocation.Header,
                Required = false,
                Schema = new OpenApiSchema { Type = JsonSchemaType.String },
                Description = "Correlation identifier",
            }
        );

        operation.Parameters?.Add(
            new OpenApiParameter
            {
                Name = "x-session-id",
                In = ParameterLocation.Header,
                Required = true,
                Schema = new OpenApiSchema { Type = JsonSchemaType.String, Format = "uuid" },
                Description = "Session identifier",
            }
        );
    }
}
