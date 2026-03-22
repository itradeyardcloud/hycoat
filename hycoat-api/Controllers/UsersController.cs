using HycoatApi.DTOs;
using HycoatApi.DTOs.Auth;
using HycoatApi.Helpers;
using HycoatApi.Models.Identity;
using HycoatApi.Services;
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
    private readonly RoleManager<AppRole> _roleManager;

    private static readonly string[] Departments =
        ["Sales", "PPC", "SCM", "Production", "QA", "Purchase", "Finance"];

    public UsersController(UserManager<AppUser> userManager, RoleManager<AppRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
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
            userDtos.Add(AuthService.MapToUserDto(user, roles.FirstOrDefault() ?? "User"));
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
        var dto = AuthService.MapToUserDto(user, roles.FirstOrDefault() ?? "User");
        return Ok(ApiResponse<UserDto>.Ok(dto));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<UserDto>>> CreateUser(CreateUserRequest request)
    {
        if (!Departments.Contains(request.Department))
            return BadRequest(ApiResponse<UserDto>.Fail("Invalid department."));

        if (!await _roleManager.RoleExistsAsync(request.Role))
            return BadRequest(ApiResponse<UserDto>.Fail("Invalid role."));

        var user = new AppUser
        {
            UserName = request.Email,
            Email = request.Email,
            FullName = request.FullName,
            Department = request.Department,
            PhoneNumber = request.PhoneNumber,
            IsActive = true
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description).ToList();
            return BadRequest(ApiResponse<UserDto>.Fail(errors));
        }

        await _userManager.AddToRoleAsync(user, request.Role);

        var dto = AuthService.MapToUserDto(user, request.Role);
        return CreatedAtAction(nameof(GetUser), new { id = user.Id },
            ApiResponse<UserDto>.Ok(dto, "User created successfully."));
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<UserDto>>> UpdateUser(string id, UpdateUserRequest request)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
            return NotFound(ApiResponse<UserDto>.Fail("User not found."));

        if (!Departments.Contains(request.Department))
            return BadRequest(ApiResponse<UserDto>.Fail("Invalid department."));

        if (!await _roleManager.RoleExistsAsync(request.Role))
            return BadRequest(ApiResponse<UserDto>.Fail("Invalid role."));

        user.FullName = request.FullName;
        user.Department = request.Department;
        user.PhoneNumber = request.PhoneNumber;

        await _userManager.UpdateAsync(user);

        // Update role
        var currentRoles = await _userManager.GetRolesAsync(user);
        if (!currentRoles.Contains(request.Role))
        {
            await _userManager.RemoveFromRolesAsync(user, currentRoles);
            await _userManager.AddToRoleAsync(user, request.Role);
        }

        var dto = AuthService.MapToUserDto(user, request.Role);
        return Ok(ApiResponse<UserDto>.Ok(dto, "User updated successfully."));
    }

    [HttpPut("{id}/toggle-active")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<UserDto>>> ToggleActive(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
            return NotFound(ApiResponse<UserDto>.Fail("User not found."));

        user.IsActive = !user.IsActive;

        // Clear refresh token when deactivating
        if (!user.IsActive)
        {
            user.RefreshToken = null;
            user.RefreshTokenExpiryTime = null;
        }

        await _userManager.UpdateAsync(user);

        var roles = await _userManager.GetRolesAsync(user);
        var dto = AuthService.MapToUserDto(user, roles.FirstOrDefault() ?? "User");
        var status = user.IsActive ? "activated" : "deactivated";
        return Ok(ApiResponse<UserDto>.Ok(dto, $"User {status} successfully."));
    }

    [HttpGet("me")]
    public async Task<ActionResult<ApiResponse<UserDto>>> GetProfile()
    {
        var userId = User.GetUserId()!;
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return NotFound(ApiResponse<UserDto>.Fail("User not found."));

        var roles = await _userManager.GetRolesAsync(user);
        var dto = AuthService.MapToUserDto(user, roles.FirstOrDefault() ?? "User");
        return Ok(ApiResponse<UserDto>.Ok(dto));
    }

    [HttpPut("me")]
    public async Task<ActionResult<ApiResponse<UserDto>>> UpdateProfile(UpdateProfileRequest request)
    {
        var userId = User.GetUserId()!;
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return NotFound(ApiResponse<UserDto>.Fail("User not found."));

        user.FullName = request.FullName;
        user.PhoneNumber = request.PhoneNumber;
        await _userManager.UpdateAsync(user);

        var roles = await _userManager.GetRolesAsync(user);
        var dto = AuthService.MapToUserDto(user, roles.FirstOrDefault() ?? "User");
        return Ok(ApiResponse<UserDto>.Ok(dto, "Profile updated successfully."));
    }

    [HttpGet("roles")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<List<object>>>> GetRoles()
    {
        var roles = await _roleManager.Roles
            .Select(r => new { r.Id, r.Name, r.Description })
            .ToListAsync();

        return Ok(ApiResponse<List<object>>.Ok(roles.Cast<object>().ToList()));
    }

    [HttpGet("departments")]
    [Authorize(Roles = "Admin")]
    public ActionResult<ApiResponse<string[]>> GetDepartments()
    {
        return Ok(ApiResponse<string[]>.Ok(Departments));
    }
}
