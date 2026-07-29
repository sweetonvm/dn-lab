using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;

namespace BFF.Endpoints;

public static class LoginEndpoints
{
    public static IEndpointRouteBuilder MapLoginEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet(BffRoutes.Login, (string? returnUrl) =>
        {
            var props = new AuthenticationProperties
            {
                RedirectUri = string.IsNullOrWhiteSpace(returnUrl)
                    ? BffRoutes.Dashboard
                    : returnUrl
            };

            return Results.Challenge(
                props,
                [OpenIdConnectDefaults.AuthenticationScheme]);
        });

        app.MapPost(BffRoutes.Logout, async ctx =>
        {
            await ctx.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            await ctx.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme, new AuthenticationProperties
            {
                RedirectUri = "/"
            });
        });

        return app;
    }
}