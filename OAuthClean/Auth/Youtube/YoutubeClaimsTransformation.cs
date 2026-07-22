using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using OAuthClean.Data;

namespace OAuthClean.Auth.Youtube;

public class YoutubeClaimsTransformation(TokenDatabase db) : IClaimsTransformation
{
    public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        var userId = principal.FindFirstValue("user_id");
        if (userId is null || !db.TryGetValue(userId, out var token))
            return Task.FromResult(principal);

        var clone = principal.Clone();
        clone.Identities
            .First(i => i.AuthenticationType == "cookie")
            .AddClaim(new Claim("yt-access-token", token));

        return Task.FromResult(clone);
    }
}