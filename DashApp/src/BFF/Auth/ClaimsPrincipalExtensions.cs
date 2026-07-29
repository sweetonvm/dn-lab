using System.Security.Claims;
using Microsoft.Identity.Web;

namespace BFF.Auth;

public static class ClaimsPrincipalExtensions
{
    public static string? GetApplicationUserId(this ClaimsPrincipal principal)
    {
        return principal.FindFirstValue(ClaimConstants.Oid)
               ?? principal.FindFirstValue(ClaimConstants.NameIdentifierId);
    }

    public static string? GetUserName(this ClaimsPrincipal principal)
    {
        return principal.FindFirstValue(ClaimConstants.Name)
               ?? principal.FindFirstValue(ClaimConstants.PreferredUserName);
    }
}