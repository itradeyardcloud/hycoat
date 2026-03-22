using Microsoft.AspNetCore.Identity;

namespace HycoatApi.Models.Identity;

public class AppRole : IdentityRole
{
    public string? Description { get; set; }
}
