using System.Security.Claims;
using BFF.Data;
using Microsoft.AspNetCore.Authentication;

namespace BFF.Auth.Providers.Github;

public class GithubClaimsTransformation(TokenDatabase db) : IClaimsTransformation
{
    public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        var userId = principal.GetApplicationUserId();
        if (userId is null || !db.IsConnected(userId, "github")) return Task.FromResult(principal);

        var record = db.GetToken(userId, "github");
        if (record is null) return Task.FromResult(principal);

        var clone = principal.Clone();
        var identity = clone.Identities.First();

        if (!identity.HasClaim(c => c.Type == "github-connected"))
            identity.AddClaim(new Claim("github-connected", "y"));

        return Task.FromResult(clone);
    }
}