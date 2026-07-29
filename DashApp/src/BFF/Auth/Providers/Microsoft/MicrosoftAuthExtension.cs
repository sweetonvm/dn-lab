using BFF.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace BFF.Auth.Providers.Microsoft;

public static class MicrosoftAuthExtension
{
    public static IServiceCollection AddCookieAuth(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddTransient<IClaimsTransformation, MicrosoftClaimsTransformation>();

        services
            .AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
            })
            .AddCookie(
                CookieAuthenticationDefaults.AuthenticationScheme,
                options =>
                {
                    options.LoginPath = BffRoutes.Login;
                    options.Events.OnRedirectToLogin = ctx =>
                    {
                        if (ctx.Request.Path.StartsWithSegments(BffRoutes.ApiPrefix))
                        {
                            ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
                            return Task.CompletedTask;
                        }
                            
                        ctx.Response.Redirect(ctx.RedirectUri);
                        return Task.CompletedTask;
                    };
                }
            )
            .AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
            {
                options.Authority =
                    "https://login.microsoftonline.com/common/v2.0";
                options.TokenValidationParameters.ValidateIssuer = false;
                options.ClientId = configuration["Microsoft:ClientId"]
                                   ?? throw new InvalidOperationException("Microsoft:ClientId missing");
                options.ClientSecret = configuration["Microsoft:ClientSecret"]
                                       ?? throw new InvalidOperationException("Microsoft:ClientSecret missing");
                options.ResponseType = OpenIdConnectResponseType.Code;
                options.UsePkce = true;
                options.SaveTokens = false;
                options.MapInboundClaims = false;

                options.CallbackPath = "/signin-oidc";
                options.SignedOutCallbackPath = "/signout-callback-oidc";

                options.Scope.Clear();
                options.Scope.Add("openid");
                options.Scope.Add("profile");
                options.Scope.Add("email");
                options.Scope.Add("offline_access");

                options.Events = new OpenIdConnectEvents
                {
                    OnRedirectToIdentityProvider = ctx =>
                    {
                        if (ctx.Request.Path.StartsWithSegments(BffRoutes.ApiPrefix))
                        {
                            ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
                            ctx.HandleResponse();
                            return Task.CompletedTask;
                        }

                        return Task.CompletedTask;
                    },
                    
                    OnTokenValidated = async ctx =>
                    {
                        var principal = ctx.Principal;

                        var userId = principal?.GetApplicationUserId();
                        if (string.IsNullOrWhiteSpace(userId))
                        {
                            ctx.Fail("Missing user id.");
                            return;
                        }

                        var accessToken = ctx.TokenEndpointResponse?.AccessToken;
                        if (string.IsNullOrWhiteSpace(accessToken))
                        {
                            ctx.Fail("Missing access token.");
                            return;
                        }

                        var db = ctx.HttpContext.RequestServices
                            .GetRequiredService<TokenDatabase>();

                        db.StoreToken(
                            userId,
                            "microsoft",
                            new TokenRecord(
                                accessToken,
                                ctx.TokenEndpointResponse?.RefreshToken,
                                int.TryParse(ctx.TokenEndpointResponse?.ExpiresIn, out var seconds)
                                    ? DateTimeOffset.UtcNow.AddSeconds(seconds)
                                    : null
                            )
                        );

                        await Task.CompletedTask;
                    }
                };
            });

        return services;
    }
}