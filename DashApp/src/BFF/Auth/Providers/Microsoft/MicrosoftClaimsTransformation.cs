using System.Security.Claims;
using BFF.Data;
using Microsoft.AspNetCore.Authentication;

namespace BFF.Auth.Providers.Microsoft;

public class MicrosoftClaimsTransformation(TokenDatabase db) : IClaimsTransformation
{
    public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        var userId = principal.GetApplicationUserId();
        if (userId is null || !db.IsConnected(userId, "microsoft")) return Task.FromResult(principal);

        var record = db.GetToken(userId, "microsoft");
        if (record is null) return Task.FromResult(principal);

        var clone = principal.Clone();
        var identity = clone.Identities.First();

        if (!identity.HasClaim(c => c.Type == "microsoft-connected"))
            identity.AddClaim(new Claim("microsoft-connected", "y"));

        return Task.FromResult(clone);
    }
}