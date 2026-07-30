using System.Net.Http.Headers;
using System.Text.Json;
using BFF.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace BFF.Auth.Providers.Github;

public static class GithubAuthExtensions
{
    public static IServiceCollection AddGithubOAuth(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddTransient<IClaimsTransformation, GithubClaimsTransformation>();

        services.AddAuthentication()
            .AddOAuth("github", options =>
            {
                options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.ClientId = configuration["Github:ClientId"]
                                   ?? throw new InvalidOperationException("Github:ClientId missing");
                options.ClientSecret = configuration["Github:ClientSecret"]
                                       ?? throw new InvalidOperationException("Github:ClientSecret missing");

                options.AuthorizationEndpoint = "https://github.com/login/oauth/authorize";
                options.TokenEndpoint = "https://github.com/login/oauth/access_token";
                options.UserInformationEndpoint = "https://api.github.com/user";
                options.CallbackPath = BffRoutes.GithubCallback;
                options.SaveTokens = false;
                options.UsePkce = true;

                options.Scope.Add("read:user");

                options.Events.OnCreatingTicket = async ctx =>
                {
                    var authResult =
                        await ctx.HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

                    if (!authResult.Succeeded)
                    {
                        ctx.Fail("No session found during GitHub OAuth callback.");
                        return;
                    }

                    var userId = authResult.Principal!.GetApplicationUserId();
                    if (userId is null)
                    {
                        ctx.Fail("user_id claim missing from session.");
                        return;
                    }

                    var accessToken = ctx.AccessToken;
                    if (string.IsNullOrEmpty(accessToken))
                    {
                        ctx.Fail("No access token in GitHub OAuth response.");
                        return;
                    }

                    var db = ctx.HttpContext.RequestServices
                        .GetRequiredService<TokenDatabase>();

                    db.StoreToken(
                        userId,
                        "github",
                        new TokenRecord(
                            accessToken,
                            ctx.RefreshToken,
                            ctx.ExpiresIn.HasValue
                                ? DateTimeOffset.UtcNow.Add(ctx.ExpiresIn.Value)
                                : null
                        )
                    );

                    using var request = new HttpRequestMessage(
                        HttpMethod.Get,
                        ctx.Options.UserInformationEndpoint);

                    request.Headers.Authorization =
                        new AuthenticationHeaderValue("Bearer", ctx.AccessToken);

                    request.Headers.UserAgent.ParseAdd("BFF");

                    using var response = await ctx.Backchannel.SendAsync(
                        request,
                        ctx.HttpContext.RequestAborted);

                    response.EnsureSuccessStatusCode();

                    using var payload = JsonDocument.Parse(
                        await response.Content.ReadAsStringAsync());

                    var accountName = payload.RootElement.GetProperty("login").GetString();

                    db.StoreConnectedAccount(
                        userId,
                        "github",
                        accountName
                    );

                    ctx.Principal = authResult.Principal.Clone();
                };
            });

        services
            .AddAuthorizationBuilder()
            .AddPolicy("github-connected", pb =>
                pb.AddAuthenticationSchemes(CookieAuthenticationDefaults.AuthenticationScheme)
                    .RequireAuthenticatedUser()
                    .RequireClaim("github-connected", "y")
            );

        return services;
    }
}