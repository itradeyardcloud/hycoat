using System.Security.Claims;

namespace HycoatApi.Helpers;

public static class ClaimsHelper
{
    // Azure AD uses 'http://schemas.microsoft.com/identity/claims/objectidentifier' for oid
    private const string OidClaim = "http://schemas.microsoft.com/identity/claims/objectidentifier";

    public static string? GetUserId(this ClaimsPrincipal user)
        => user.FindFirst(OidClaim)?.Value
        ?? user.FindFirst("oid")?.Value
        ?? user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

    public static string? GetEmail(this ClaimsPrincipal user)
        => user.FindFirst("preferred_username")?.Value
        ?? user.FindFirst(ClaimTypes.Email)?.Value
        ?? user.FindFirst("email")?.Value;

    public static string? GetRole(this ClaimsPrincipal user)
    {
        // Azure AD App Roles come as multiple "roles" claims
        var roles = user.FindAll("roles").Select(c => c.Value).ToList();
        if (roles.Count == 0)
            roles = user.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();

        // Return highest privilege role
        if (roles.Contains("Admin")) return "Admin";
        if (roles.Contains("Leader")) return "Leader";
        return roles.FirstOrDefault() ?? "User";
    }

    public static string? GetDepartment(this ClaimsPrincipal user)
        => user.FindFirst("department")?.Value;

    public static string? GetFullName(this ClaimsPrincipal user)
        => user.FindFirst("name")?.Value
        ?? user.FindFirst(ClaimTypes.Name)?.Value;

    public static bool IsAdmin(this ClaimsPrincipal user)
        => user.GetRole() == "Admin";

    public static bool IsLeader(this ClaimsPrincipal user)
        => user.GetRole() == "Leader" || user.IsAdmin();
}
