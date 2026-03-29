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
            // Use Azure AD-style claims so ClaimsHelper works uniformly
            new Claim("http://schemas.microsoft.com/identity/claims/objectidentifier", "dev-bypass-user"),
            new Claim("preferred_username", "admin@hycoat.dev"),
            new Claim("name", "Dev Admin"),
            new Claim("roles", "Admin"),
            new Claim("department", "Development"),
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
