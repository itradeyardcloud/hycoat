using HycoatApi.DTOs;
using HycoatApi.DTOs.Common;
using HycoatApi.DTOs.Masters;
using HycoatApi.Helpers;
using HycoatApi.Services.Masters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HycoatApi.Controllers;

[ApiController]
[Route("api/vendors")]
[Authorize]
public class VendorsController : ControllerBase
{
    private readonly IVendorService _service;

    public VendorsController(IVendorService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResponse<VendorDto>>>> GetAll(
        [FromQuery] string? search,
        [FromQuery] string? vendorType,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string sortBy = "Name",
        [FromQuery] bool sortDesc = false)
    {
        var result = await _service.GetAllAsync(search, vendorType, page, pageSize, sortBy, sortDesc);
        return Ok(ApiResponse<PagedResponse<VendorDto>>.Ok(result));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<VendorDetailDto>>> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);
        return Ok(ApiResponse<VendorDetailDto>.Ok(result));
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Leader")]
    public async Task<ActionResult<ApiResponse<VendorDto>>> Create(CreateVendorDto dto)
    {
        var userId = User.GetUserId()!;
        var result = await _service.CreateAsync(dto, userId);
        return CreatedAtAction(nameof(GetById), new { id = result.Id },
            ApiResponse<VendorDto>.Ok(result, "Vendor created successfully."));
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Leader")]
    public async Task<ActionResult<ApiResponse<VendorDto>>> Update(int id, UpdateVendorDto dto)
    {
        var userId = User.GetUserId()!;
        var result = await _service.UpdateAsync(id, dto, userId);
        return Ok(ApiResponse<VendorDto>.Ok(result, "Vendor updated successfully."));
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
