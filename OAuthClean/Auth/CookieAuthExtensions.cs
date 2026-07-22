using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace OAuthClean.Auth;

// Each provider registers its own path→scheme mapping via Configure<ChallengeRouteOptions>
public class ChallengeRouteOptions
{
    public List<(string Prefix, string Scheme)> Routes { get; } = [];
}

public static class CookieAuthExtensions
{
    public static IServiceCollection AddCookieAuth(this IServiceCollection services)
    {
        services.AddOptions<ChallengeRouteOptions>();

        services
            .AddAuthentication(defaultScheme: "cookie")
            .AddCookie("cookie", options =>
            {
                options.LoginPath = "/login";
                var del = options.Events.OnRedirectToAccessDenied;

                options.Events.OnSigningIn = ctx =>
                {
                    var returnUrl = ctx.Request.Query[ctx.Options.ReturnUrlParameter].FirstOrDefault();

                    ctx.Properties.RedirectUri =
                        !string.IsNullOrEmpty(returnUrl) && Uri.IsWellFormedUriString(returnUrl, UriKind.Relative)
                            ? returnUrl
                            : "/";

                    return Task.CompletedTask;

                };

                options.Events.OnRedirectToAccessDenied = ctx =>
                {
                    var opts = ctx.HttpContext.RequestServices
                        .GetRequiredService<IOptions<ChallengeRouteOptions>>()
                        .Value;

                    foreach (var (prefix, scheme) in opts.Routes)
                    {
                        if (ctx.Request.Path.StartsWithSegments(prefix))
                            return ctx.HttpContext.ChallengeAsync(scheme);
                    }

                    return del(ctx);
                };
            });

        return services;
    }
}