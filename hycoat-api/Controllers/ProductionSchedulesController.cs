using HycoatApi.DTOs;
using HycoatApi.DTOs.Planning;
using HycoatApi.Helpers;
using HycoatApi.Services.Planning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HycoatApi.Controllers;

[ApiController]
[Route("api/production-schedule")]
[Authorize]
public class ProductionSchedulesController : ControllerBase
{
    private readonly IProductionScheduleService _service;

    public ProductionSchedulesController(IProductionScheduleService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<ScheduleEntryDto>>>> GetSchedule(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        [FromQuery] int? productionUnitId,
        [FromQuery] string? shift)
    {
        var result = await _service.GetScheduleAsync(startDate, endDate, productionUnitId, shift);
        return Ok(ApiResponse<List<ScheduleEntryDto>>.Ok(result));
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Leader")]
    public async Task<ActionResult<ApiResponse<ScheduleEntryDto>>> Create(CreateScheduleEntryDto dto)
    {
        var userId = User.GetUserId()!;
        var result = await _service.CreateAsync(dto, userId);
        return CreatedAtAction(nameof(GetSchedule), null,
            ApiResponse<ScheduleEntryDto>.Ok(result, "Schedule entry created successfully."));
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Leader")]
    public async Task<ActionResult<ApiResponse<ScheduleEntryDto>>> Update(int id, UpdateScheduleEntryDto dto)
    {
        var userId = User.GetUserId()!;
        var result = await _service.UpdateAsync(id, dto, userId);
        return Ok(ApiResponse<ScheduleEntryDto>.Ok(result, "Schedule entry updated successfully."));
    }

    [HttpPatch("{id}/status")]
    [Authorize(Roles = "Admin,Leader")]
    public async Task<IActionResult> UpdateStatus(int id, UpdateScheduleStatusDto dto)
    {
        var userId = User.GetUserId()!;
        await _service.UpdateStatusAsync(id, dto, userId);
        return Ok(ApiResponse<object>.Ok(null!, "Schedule status updated."));
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = User.GetUserId()!;
        await _service.DeleteAsync(id, userId);
        return NoContent();
    }

    [HttpPost("reorder")]
    [Authorize(Roles = "Admin,Leader")]
    public async Task<IActionResult> Reorder(ReorderScheduleDto dto)
    {
        var userId = User.GetUserId()!;
        await _service.ReorderAsync(dto, userId);
        return Ok(ApiResponse<object>.Ok(null!, "Schedule reordered."));
    }
}
