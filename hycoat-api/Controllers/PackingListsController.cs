using HycoatApi.DTOs;
using HycoatApi.DTOs.Dispatch;
using HycoatApi.Helpers;
using HycoatApi.Services.Dispatch;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HycoatApi.Controllers;

[ApiController]
[Route("api/packing-lists")]
[Authorize]
public class PackingListsController : ControllerBase
{
    private readonly IPackingListService _service;

    public PackingListsController(IPackingListService service)
    {
        _service = service;
    }

    [HttpGet]
    [Authorize(Roles = "SCM,Admin,Leader")]
    public async Task<ActionResult<ApiResponse<PagedResponse<PackingListDto>>>> GetAll(
        [FromQuery] string? search,
        [FromQuery] int? workOrderId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string sortBy = "Date",
        [FromQuery] bool sortDesc = true)
    {
        var result = await _service.GetAllAsync(search, workOrderId, page, pageSize, sortBy, sortDesc);
        return Ok(ApiResponse<PagedResponse<PackingListDto>>.Ok(result));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<PackingListDetailDto>>> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);
        return Ok(ApiResponse<PackingListDetailDto>.Ok(result));
    }

    [HttpGet("by-work-order/{woId}")]
    public async Task<ActionResult<ApiResponse<PackingListDetailDto>>> GetByWorkOrder(int woId)
    {
        var result = await _service.GetByWorkOrderIdAsync(woId);
        if (result == null)
            return NotFound(ApiResponse<PackingListDetailDto>.Fail("No packing list found for this Work Order."));
        return Ok(ApiResponse<PackingListDetailDto>.Ok(result));
    }

    [HttpPost]
    [Authorize(Roles = "SCM,Admin,Leader")]
    public async Task<ActionResult<ApiResponse<PackingListDto>>> Create(CreatePackingListDto dto)
    {
        var userId = User.GetUserId()!;
        var result = await _service.CreateAsync(dto, userId);
        return CreatedAtAction(nameof(GetById), new { id = result.Id },
            ApiResponse<PackingListDto>.Ok(result, "Packing list created successfully."));
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "SCM,Admin,Leader")]
    public async Task<ActionResult<ApiResponse<PackingListDto>>> Update(int id, CreatePackingListDto dto)
    {
        var userId = User.GetUserId()!;
        var result = await _service.UpdateAsync(id, dto, userId);
        return Ok(ApiResponse<PackingListDto>.Ok(result, "Packing list updated successfully."));
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
