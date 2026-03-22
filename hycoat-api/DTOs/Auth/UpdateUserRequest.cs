namespace HycoatApi.DTOs.Auth;

public class UpdateUserRequest
{
    public string FullName { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
}
