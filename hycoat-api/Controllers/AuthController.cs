using HycoatApi.DTOs;
using HycoatApi.DTOs.Auth;
using HycoatApi.Helpers;
using HycoatApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HycoatApi.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;

    public AuthController(AuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<LoginResponse>>> Login(LoginRequest request)
    {
        var result = await _authService.LoginAsync(request);
        return Ok(ApiResponse<LoginResponse>.Ok(result, "Login successful."));
    }

    [HttpPost("refresh-token")]
    public async Task<ActionResult<ApiResponse<LoginResponse>>> RefreshToken(RefreshTokenRequest request)
    {
        var result = await _authService.RefreshTokenAsync(request);
        return Ok(ApiResponse<LoginResponse>.Ok(result, "Token refreshed."));
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<ActionResult<ApiResponse<string>>> Logout()
    {
        var userId = User.GetUserId()!;
        await _authService.LogoutAsync(userId);
        return Ok(ApiResponse<string>.Ok(null!, "Logged out successfully."));
    }

    [Authorize]
    [HttpPost("change-password")]
    public async Task<ActionResult<ApiResponse<string>>> ChangePassword(ChangePasswordRequest request)
    {
        var userId = User.GetUserId()!;
        await _authService.ChangePasswordAsync(userId, request);
        return Ok(ApiResponse<string>.Ok(null!, "Password changed successfully."));
    }
}
