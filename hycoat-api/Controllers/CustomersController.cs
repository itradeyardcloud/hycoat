using HycoatApi.DTOs;
using HycoatApi.DTOs.Common;
using HycoatApi.DTOs.Masters;
using HycoatApi.Helpers;
using HycoatApi.Services.Masters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HycoatApi.Controllers;

[ApiController]
[Route("api/customers")]
[Authorize]
public class CustomersController : ControllerBase
{
    private readonly ICustomerService _service;

    public CustomersController(ICustomerService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResponse<CustomerDto>>>> GetAll(
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string sortBy = "Name",
        [FromQuery] bool sortDesc = false)
    {
        var result = await _service.GetAllAsync(search, page, pageSize, sortBy, sortDesc);
        return Ok(ApiResponse<PagedResponse<CustomerDto>>.Ok(result));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<CustomerDetailDto>>> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);
        return Ok(ApiResponse<CustomerDetailDto>.Ok(result));
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Leader")]
    public async Task<ActionResult<ApiResponse<CustomerDto>>> Create(CreateCustomerDto dto)
    {
        var userId = User.GetUserId()!;
        var result = await _service.CreateAsync(dto, userId);
        return CreatedAtAction(nameof(GetById), new { id = result.Id },
            ApiResponse<CustomerDto>.Ok(result, "Customer created successfully."));
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Leader")]
    public async Task<ActionResult<ApiResponse<CustomerDto>>> Update(int id, UpdateCustomerDto dto)
    {
        var userId = User.GetUserId()!;
        var result = await _service.UpdateAsync(id, dto, userId);
        return Ok(ApiResponse<CustomerDto>.Ok(result, "Customer updated successfully."));
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = User.GetUserId()!;
        await _service.DeleteAsync(id, userId);
        return NoContent();
    }

    [HttpGet("lookup")]
    public async Task<ActionResult<ApiResponse<List<LookupDto>>>> GetLookup()
    {
        var result = await _service.GetLookupAsync();
        return Ok(ApiResponse<List<LookupDto>>.Ok(result));
    }
}
