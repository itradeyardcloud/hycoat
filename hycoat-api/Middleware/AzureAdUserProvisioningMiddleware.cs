using HycoatApi.Helpers;
using HycoatApi.Models.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HycoatApi.Middleware;

/// <summary>
/// Auto-provisions a local AppUser record from Azure AD claims on each authenticated request.
/// Uses the Azure AD Object ID (oid) as the local user ID to keep FK references intact.
/// </summary>
public class AzureAdUserProvisioningMiddleware
{
    private readonly RequestDelegate _next;

    public AzureAdUserProvisioningMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, UserManager<AppUser> userManager)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var oid = context.User.GetUserId();
            if (!string.IsNullOrEmpty(oid))
            {
                var user = await userManager.Users.AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Id == oid);

                if (user == null)
                {
                    var fullName = context.User.GetFullName() ?? "Unknown";
                    var email = context.User.GetEmail() ?? "";
                    var department = context.User.GetDepartment() ?? "";

                    var newUser = new AppUser
                    {
                        Id = oid,
                        UserName = email,
                        NormalizedUserName = email.ToUpperInvariant(),
                        Email = email,
                        NormalizedEmail = email.ToUpperInvariant(),
                        FullName = fullName,
                        Department = department,
                        IsActive = true,
                        EmailConfirmed = true,
                        SecurityStamp = Guid.NewGuid().ToString()
                    };

                    await userManager.CreateAsync(newUser);

                    // Assign the highest-privilege role from token
                    var role = context.User.GetRole();
                    if (!string.IsNullOrEmpty(role))
                    {
                        try { await userManager.AddToRoleAsync(newUser, role); }
                        catch { /* Role may not exist in DB — skip */ }
                    }
                }
            }
        }

        await _next(context);
    }
}
