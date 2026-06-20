using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace HycoatApi.Middleware;

/// <summary>
/// Development-only middleware that injects a fake authenticated user
/// so controllers using User.GetUserId() etc. continue to work.
/// Controlled by the "BypassAuth" config flag in appsettings.Development.json.
/// </summary>
public class DevAuthBypassMiddleware
{
    private readonly RequestDelegate _next;

    public DevAuthBypassMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var identity = new ClaimsIdentity(new[]
        {
            // Use the seeded admin user id so FK-constrained CreatedBy/PreparedBy relations remain valid in dev.
            new Claim("http://schemas.microsoft.com/identity/claims/objectidentifier", "user-admin"),
            new Claim("preferred_username", "admin@hycoat.com"),
            new Claim("name", "System Administrator"),
            new Claim("roles", "Admin"),
            new Claim("department", "Admin"),
        }, "DevBypass");

        context.User = new ClaimsPrincipal(identity);
        await _next(context);
    }
}

/// <summary>
/// Development-only authorization handler that approves all requirements,
/// effectively disabling [Authorize] and role checks.
/// </summary>
public class DevBypassAuthorizationHandler : IAuthorizationHandler
{
    public Task HandleAsync(AuthorizationHandlerContext context)
    {
        foreach (var requirement in context.PendingRequirements.ToList())
        {
            context.Succeed(requirement);
        }
        return Task.CompletedTask;
    }
}
