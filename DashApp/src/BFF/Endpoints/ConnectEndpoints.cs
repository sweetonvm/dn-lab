using BFF.Auth;
using BFF.Auth.Providers.Github;
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

        app.MapPost(BffRoutes.Unlink, async (
                string provider,
                HttpContext ctx,
                TokenDatabase db,
                GithubTokenRevocationService githubRevocation,
                ILogger<Program> logger) =>
            {
                var userId = ctx.User.GetApplicationUserId();

                if (userId is null) return Results.Unauthorized();

                if (string.Equals(provider, "github", StringComparison.OrdinalIgnoreCase))
                {
                    var tokenRecord = db.GetToken(userId, provider);

                    if (tokenRecord is not null)
                        try
                        {
                            var revoked = await githubRevocation.RevokeAsync(tokenRecord.AccessToken);

                            if (!revoked)
                                logger.LogWarning(
                                    "Failed to revoke Github token for user {UserId}.",
                                    userId);
                        }
                        catch (Exception ex)
                        {
                            logger.LogError(
                                ex,
                                "Error revoking Github token for user {UserId}.",
                                userId);
                        }
                }

                db.Unlink(userId, provider);

                return Results.NoContent();
            })
            .RequireAuthorization();

        return app;
    }
}