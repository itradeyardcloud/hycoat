using HycoatApi.DTOs;
using HycoatApi.DTOs.Common;
using HycoatApi.DTOs.Masters;
using HycoatApi.Helpers;
using HycoatApi.Services.Masters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HycoatApi.Controllers;

[ApiController]
[Route("api/powder-colors")]
[Authorize]
public class PowderColorsController : ControllerBase
{
    private readonly IPowderColorService _service;

    public PowderColorsController(IPowderColorService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResponse<PowderColorDto>>>> GetAll(
        [FromQuery] string? search,
        [FromQuery] int? vendorId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string sortBy = "PowderCode",
        [FromQuery] bool sortDesc = false)
    {
        var result = await _service.GetAllAsync(search, vendorId, page, pageSize, sortBy, sortDesc);
        return Ok(ApiResponse<PagedResponse<PowderColorDto>>.Ok(result));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<PowderColorDetailDto>>> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);
        return Ok(ApiResponse<PowderColorDetailDto>.Ok(result));
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Leader")]
    public async Task<ActionResult<ApiResponse<PowderColorDto>>> Create(CreatePowderColorDto dto)
    {
        var userId = User.GetUserId()!;
        var result = await _service.CreateAsync(dto, userId);
        return CreatedAtAction(nameof(GetById), new { id = result.Id },
            ApiResponse<PowderColorDto>.Ok(result, "Powder color created successfully."));
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Leader")]
    public async Task<ActionResult<ApiResponse<PowderColorDto>>> Update(int id, UpdatePowderColorDto dto)
    {
        var userId = User.GetUserId()!;
        var result = await _service.UpdateAsync(id, dto, userId);
        return Ok(ApiResponse<PowderColorDto>.Ok(result, "Powder color updated successfully."));
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
    public async Task<ActionResult<ApiResponse<List<PowderColorLookupDto>>>> GetLookup()
    {
        var result = await _service.GetLookupAsync();
        return Ok(ApiResponse<List<PowderColorLookupDto>>.Ok(result));
    }
}
