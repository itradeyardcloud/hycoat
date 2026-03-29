namespace HycoatApi.Services.Auth;

public interface ICurrentUserService
{
    string? UserId { get; }
    string? UserName { get; }
}
