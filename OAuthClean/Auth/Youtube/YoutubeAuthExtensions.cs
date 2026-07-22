using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using OAuthClean.Data;

namespace OAuthClean.Auth.Youtube;

public static class YoutubeAuthExtensions
{
    public static IServiceCollection AddYoutubeOAuth(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Plugs into the generic challenge router in CookieAuthExtensions
        services.Configure<ChallengeRouteOptions>(o => o.Routes.Add(("/yt", "youtube")));
        services.AddTransient<IClaimsTransformation, YoutubeClaimsTransformation>();

        services.AddAuthentication()
            .AddOAuth("youtube", options =>
            {
                options.SignInScheme = "cookie";
                options.ClientId = configuration["Youtube:ClientId"]
                    ?? throw new InvalidOperationException("Youtube:ClientId missing");
                options.ClientSecret = configuration["Youtube:ClientSecret"]
                    ?? throw new InvalidOperationException("Youtube:ClientSecret missing");

                options.SaveTokens = false;
                options.AuthorizationEndpoint = "https://accounts.google.com/o/oauth2/v2/auth";
                options.TokenEndpoint = "https://oauth2.googleapis.com/token";
                options.CallbackPath = "/oauth/yt-cb";

                options.Scope.Clear();
                options.Scope.Add("https://www.googleapis.com/auth/youtube.readonly");

                options.Events.OnCreatingTicket = async ctx =>
                {
                    var authResult = await ctx.HttpContext.AuthenticateAsync("cookie");
                    if (!authResult.Succeeded)
                    {
                        ctx.Fail("No session cookie found during OAuth callback.");
                        return;
                    }

                    var userId = authResult.Principal!.FindFirstValue("user_id");
                    if (userId is null)
                    {
                        ctx.Fail("user_id claim missing from session.");
                        return;
                    }

                    if (string.IsNullOrEmpty(ctx.AccessToken))
                    {
                        ctx.Fail("No access token in OAuth response.");
                        return;
                    }

                    var db = ctx.HttpContext.RequestServices.GetRequiredService<TokenDatabase>();
                    db[userId] = ctx.AccessToken;

                    ctx.Principal = authResult.Principal.Clone();
                    ctx.Principal.Identities
                        .First(i => i.AuthenticationType == "cookie")
                        .AddClaim(new Claim("yt-token", "y"));
                };
            });

        services.AddAuthorizationBuilder()
            .AddPolicy("youtube-enabled", pb =>
                pb.AddAuthenticationSchemes("cookie")
                  .RequireAuthenticatedUser()
                  .RequireClaim("yt-token", "y"));

        return services;
    }
}