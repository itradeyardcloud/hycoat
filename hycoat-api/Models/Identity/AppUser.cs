using Microsoft.AspNetCore.Identity;

namespace HycoatApi.Models.Identity;

public class AppUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiryTime { get; set; }
}
