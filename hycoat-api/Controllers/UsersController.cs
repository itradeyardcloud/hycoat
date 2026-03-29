using HycoatApi.DTOs;
using HycoatApi.DTOs.Auth;
using HycoatApi.Helpers;
using HycoatApi.Models.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HycoatApi.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly UserManager<AppUser> _userManager;

    public UsersController(UserManager<AppUser> userManager)
    {
        _userManager = userManager;
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<PagedResponse<UserDto>>>> GetUsers(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var query = _userManager.Users.AsNoTracking();
        var totalCount = await query.CountAsync();

        var users = await query
            .OrderBy(u => u.FullName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var userDtos = new List<UserDto>();
        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            userDtos.Add(ToUserDto(user, roles.FirstOrDefault() ?? "User"));
        }

        var response = new PagedResponse<UserDto>
        {
            Items = userDtos,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };

        return Ok(ApiResponse<PagedResponse<UserDto>>.Ok(response));
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<UserDto>>> GetUser(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
            return NotFound(ApiResponse<UserDto>.Fail("User not found."));

        var roles = await _userManager.GetRolesAsync(user);
        var dto = ToUserDto(user, roles.FirstOrDefault() ?? "User");
        return Ok(ApiResponse<UserDto>.Ok(dto));
    }

    [HttpGet("me")]
    public async Task<ActionResult<ApiResponse<UserDto>>> GetProfile()
    {
        var userId = User.GetUserId()!;
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return NotFound(ApiResponse<UserDto>.Fail("User not found."));

        var dto = ToUserDto(user, User.GetRole() ?? "User");
        return Ok(ApiResponse<UserDto>.Ok(dto));
    }

    private static UserDto ToUserDto(AppUser user, string role)
    {
        return new UserDto
        {
            Id = user.Id,
            Email = user.Email ?? string.Empty,
            FullName = user.FullName,
            Department = user.Department,
            Role = role,
            PhoneNumber = user.PhoneNumber,
            IsActive = user.IsActive
        };
    }
}
