using System.Security.Claims;

namespace HycoatApi.Helpers;

public static class ClaimsHelper
{
    public static string? GetUserId(this ClaimsPrincipal user)
        => user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

    public static string? GetEmail(this ClaimsPrincipal user)
        => user.FindFirst(ClaimTypes.Email)?.Value;

    public static string? GetRole(this ClaimsPrincipal user)
        => user.FindFirst(ClaimTypes.Role)?.Value;

    public static string? GetDepartment(this ClaimsPrincipal user)
        => user.FindFirst("department")?.Value;

    public static string? GetFullName(this ClaimsPrincipal user)
        => user.FindFirst(ClaimTypes.Name)?.Value;

    public static bool IsAdmin(this ClaimsPrincipal user)
        => user.GetRole() == "Admin";

    public static bool IsLeader(this ClaimsPrincipal user)
        => user.GetRole() == "Leader" || user.IsAdmin();
}
