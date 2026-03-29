using System.Security.Claims;

namespace HycoatApi.Services.Auth;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private const string OidClaim = "http://schemas.microsoft.com/identity/claims/objectidentifier";

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string? UserId =>
        _httpContextAccessor.HttpContext?.User.FindFirstValue(OidClaim)
        ?? _httpContextAccessor.HttpContext?.User.FindFirstValue("oid")
        ?? _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);

    public string? UserName =>
        _httpContextAccessor.HttpContext?.User.FindFirstValue("name")
        ?? _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.Name);
}
