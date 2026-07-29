using BFF.Auth;
using BFF.Data;
using Microsoft.AspNetCore.Authentication;

namespace BFF.Endpoints;

public static class ConnectEndpoints
{
    private static readonly string[] SupportedProviders = ["github"];

    public static IEndpointRouteBuilder MapConnectEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet(BffRoutes.Connect, (string provider) =>
            {
                if (!SupportedProviders.Contains(provider))
                    return Results.NotFound($"Provider '{provider}' is not supported.");

                return Results.Challenge(
                    new AuthenticationProperties { RedirectUri = BffRoutes.Dashboard },
                    [provider]
                );
            })
            .RequireAuthorization();

        app.MapPost(BffRoutes.Unlink, (
                string provider,
                HttpContext ctx,
                TokenDatabase db) =>
            {
                var userId = ctx.User.GetApplicationUserId();

                if (userId is null) return Results.Unauthorized();

                db.Unlink(userId, provider);

                return Results.NoContent();
            })
            .RequireAuthorization();

        return app;
    }
}