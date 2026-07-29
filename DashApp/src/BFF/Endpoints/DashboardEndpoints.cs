using BFF.Auth;
using BFF.Data;

namespace BFF.Endpoints;

public static class DashboardEndpoints
{
    private static readonly string[] SupportedProviders = ["github"];

    public static IEndpointRouteBuilder MapDashboardEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet(BffRoutes.ApiDashboard, (HttpContext ctx, TokenDatabase db) =>
            {
                var userId = ctx.User.GetApplicationUserId();

                var tiles = SupportedProviders.Select(provider =>
                {
                    var connectedAccount = userId is null ? null : db.GetConnectedAccount(userId, provider);

                    var connected = connectedAccount is not null;

                    return new
                    {
                        Provider = provider,
                        Connected = connected,
                        ConnectUrl = connected ? null : BffRoutes.ConnectUrl(provider),
                        ConnectedAccount = connectedAccount
                    };
                });

                return Results.Ok(new { tiles });
            })
            .RequireAuthorization();

        app.MapGet(BffRoutes.ApiSession, (HttpContext ctx) =>
            {
                var userId = ctx.User.GetApplicationUserId();
                var userName = ctx.User.GetUserName();

                return Results.Ok(new { userId, userName });
            })
            .RequireAuthorization();

        return app;
    }
}