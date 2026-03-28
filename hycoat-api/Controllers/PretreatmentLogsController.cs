using HycoatApi.DTOs;
using HycoatApi.DTOs.Production;
using HycoatApi.Helpers;
using HycoatApi.Services.Production;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HycoatApi.Controllers;

[ApiController]
[Route("api/pretreatment-logs")]
[Authorize]
public class PretreatmentLogsController : ControllerBase
{
    private readonly IPretreatmentLogService _service;

    public PretreatmentLogsController(IPretreatmentLogService service)
    {
        _service = service;
    }

    [HttpGet]
    [Authorize(Roles = "Production,QA,PPC,Admin,Leader")]
    public async Task<ActionResult<ApiResponse<PagedResponse<PretreatmentLogDto>>>> GetAll(
        [FromQuery] string? search,
        [FromQuery] DateTime? date,
        [FromQuery] string? shift,
        [FromQuery] int? productionWorkOrderId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string sortBy = "Date",
        [FromQuery] bool sortDesc = true)
    {
        var result = await _service.GetAllAsync(search, date, shift, productionWorkOrderId,
            page, pageSize, sortBy, sortDesc);
        return Ok(ApiResponse<PagedResponse<PretreatmentLogDto>>.Ok(result));
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "Production,QA,PPC,Admin,Leader")]
    public async Task<ActionResult<ApiResponse<PretreatmentLogDetailDto>>> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);
        return Ok(ApiResponse<PretreatmentLogDetailDto>.Ok(result));
    }

    [HttpGet("by-pwo/{pwoId}")]
    public async Task<ActionResult<ApiResponse<List<PretreatmentLogDto>>>> GetByPWO(int pwoId)
    {
        var result = await _service.GetByPWOAsync(pwoId);
        return Ok(ApiResponse<List<PretreatmentLogDto>>.Ok(result));
    }

    [HttpPost]
    [Authorize(Roles = "Production,QA,Admin,Leader")]
    public async Task<ActionResult<ApiResponse<PretreatmentLogDto>>> Create(CreatePretreatmentLogDto dto)
    {
        var userId = User.GetUserId()!;
        var result = await _service.CreateAsync(dto, userId);
        return CreatedAtAction(nameof(GetById), new { id = result.Id },
            ApiResponse<PretreatmentLogDto>.Ok(result, "Pretreatment log created successfully."));
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Production,QA,Admin,Leader")]
    public async Task<ActionResult<ApiResponse<PretreatmentLogDto>>> Update(int id, CreatePretreatmentLogDto dto)
    {
        var userId = User.GetUserId()!;
        var result = await _service.UpdateAsync(id, dto, userId);
        return Ok(ApiResponse<PretreatmentLogDto>.Ok(result, "Pretreatment log updated successfully."));
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = User.GetUserId()!;
        await _service.DeleteAsync(id, userId);
        return NoContent();
    }

    [HttpPost("{id}/tank-readings")]
    [Authorize(Roles = "QA,Admin,Leader")]
    public async Task<IActionResult> AddTankReadings(int id, List<TankReadingDto> readings)
    {
        var userId = User.GetUserId()!;
        await _service.AddTankReadingsAsync(id, readings, userId);
        return Ok(ApiResponse<object>.Ok(null!, "Tank readings updated successfully."));
    }
}
