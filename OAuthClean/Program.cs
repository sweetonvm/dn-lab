using System.Security.Claims;
using OAuthClean.Auth;
using OAuthClean.Auth.Youtube;
using OAuthClean.Clients;
using OAuthClean.Data;
using OAuthClean.Endpoints;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddTokenDatabase()
    .AddCookieAuth()
    .AddYoutubeOAuth(builder.Configuration)
    .AddYoutubeClient();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/login", () =>
{
    var principal = new ClaimsPrincipal(
        new ClaimsIdentity(
            [new Claim("user_id", Guid.NewGuid().ToString("N"))],
            "cookie"
        )
    );

    return Results.SignIn(principal, authenticationScheme: "cookie");
});

app.MapYoutubeEndpoints();

app.Run();