using HycoatApi.DTOs;
using HycoatApi.DTOs.Purchase;
using HycoatApi.Helpers;
using HycoatApi.Services.Purchase;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HycoatApi.Controllers;

[ApiController]
[Route("api/grns")]
[Authorize]
public class GRNsController : ControllerBase
{
    private readonly IGRNService _service;

    public GRNsController(IGRNService service)
    {
        _service = service;
    }

    [HttpGet]
    [Authorize(Roles = "Purchase,SCM,Admin,Leader")]
    public async Task<ActionResult<ApiResponse<PagedResponse<GRNDto>>>> GetAll(
        [FromQuery] string? search,
        [FromQuery] int? purchaseOrderId,
        [FromQuery] DateTime? dateFrom,
        [FromQuery] DateTime? dateTo,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string sortBy = "Date",
        [FromQuery] bool sortDesc = true)
    {
        var result = await _service.GetAllAsync(search, purchaseOrderId, dateFrom, dateTo,
            page, pageSize, sortBy, sortDesc);
        return Ok(ApiResponse<PagedResponse<GRNDto>>.Ok(result));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<GRNDetailDto>>> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);
        return Ok(ApiResponse<GRNDetailDto>.Ok(result));
    }

    [HttpPost]
    [Authorize(Roles = "Purchase,SCM,Admin,Leader")]
    public async Task<ActionResult<ApiResponse<GRNDto>>> Create(CreateGRNDto dto)
    {
        var userId = User.GetUserId()!;
        var result = await _service.CreateAsync(dto, userId);
        return CreatedAtAction(nameof(GetById), new { id = result.Id },
            ApiResponse<GRNDto>.Ok(result, "GRN created successfully."));
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Purchase,Admin,Leader")]
    public async Task<ActionResult<ApiResponse<GRNDto>>> Update(int id, CreateGRNDto dto)
    {
        var userId = User.GetUserId()!;
        var result = await _service.UpdateAsync(id, dto, userId);
        return Ok(ApiResponse<GRNDto>.Ok(result, "GRN updated successfully."));
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = User.GetUserId()!;
        await _service.DeleteAsync(id, userId);
        return NoContent();
    }
}
