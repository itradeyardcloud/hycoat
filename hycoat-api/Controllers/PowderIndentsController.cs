using HycoatApi.DTOs;
using HycoatApi.DTOs.Purchase;
using HycoatApi.Helpers;
using HycoatApi.Services.Purchase;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HycoatApi.Controllers;

[ApiController]
[Route("api/powder-indents")]
[Authorize]
public class PowderIndentsController : ControllerBase
{
    private readonly IPowderIndentService _service;

    public PowderIndentsController(IPowderIndentService service)
    {
        _service = service;
    }

    [HttpGet]
    [Authorize(Roles = "PPC,Purchase,Admin,Leader")]
    public async Task<ActionResult<ApiResponse<PagedResponse<PowderIndentDto>>>> GetAll(
        [FromQuery] string? search,
        [FromQuery] string? status,
        [FromQuery] DateTime? dateFrom,
        [FromQuery] DateTime? dateTo,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string sortBy = "Date",
        [FromQuery] bool sortDesc = true)
    {
        var result = await _service.GetAllAsync(search, status, dateFrom, dateTo,
            page, pageSize, sortBy, sortDesc);
        return Ok(ApiResponse<PagedResponse<PowderIndentDto>>.Ok(result));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<PowderIndentDetailDto>>> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);
        return Ok(ApiResponse<PowderIndentDetailDto>.Ok(result));
    }

    [HttpPost]
    [Authorize(Roles = "PPC,Admin,Leader")]
    public async Task<ActionResult<ApiResponse<PowderIndentDto>>> Create(CreatePowderIndentDto dto)
    {
        var userId = User.GetUserId()!;
        var result = await _service.CreateAsync(dto, userId);
        return CreatedAtAction(nameof(GetById), new { id = result.Id },
            ApiResponse<PowderIndentDto>.Ok(result, "Powder indent created successfully."));
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "PPC,Admin,Leader")]
    public async Task<ActionResult<ApiResponse<PowderIndentDto>>> Update(int id, CreatePowderIndentDto dto)
    {
        var userId = User.GetUserId()!;
        var result = await _service.UpdateAsync(id, dto, userId);
        return Ok(ApiResponse<PowderIndentDto>.Ok(result, "Powder indent updated successfully."));
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = User.GetUserId()!;
        await _service.DeleteAsync(id, userId);
        return NoContent();
    }

    [HttpPatch("{id}/status")]
    [Authorize(Roles = "Purchase,Admin,Leader")]
    public async Task<ActionResult<ApiResponse<PowderIndentDto>>> UpdateStatus(int id, UpdatePowderIndentStatusDto dto)
    {
        var userId = User.GetUserId()!;
        var result = await _service.UpdateStatusAsync(id, dto, userId);
        return Ok(ApiResponse<PowderIndentDto>.Ok(result, "Indent status updated successfully."));
    }
}
