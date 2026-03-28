using HycoatApi.DTOs;
using HycoatApi.DTOs.Quality;
using HycoatApi.Helpers;
using HycoatApi.Services.Quality;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HycoatApi.Controllers;

[ApiController]
[Route("api/in-process-inspections")]
[Authorize]
public class InProcessInspectionsController : ControllerBase
{
    private readonly IInProcessInspectionService _service;

    public InProcessInspectionsController(IInProcessInspectionService service)
    {
        _service = service;
    }

    [HttpGet]
    [Authorize(Roles = "QA,Production,PPC,Admin,Leader")]
    public async Task<ActionResult<ApiResponse<PagedResponse<InProcessInspectionDto>>>> GetAll(
        [FromQuery] string? search,
        [FromQuery] DateTime? date,
        [FromQuery] int? productionWorkOrderId,
        [FromQuery] string? inspectorUserId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string sortBy = "Date",
        [FromQuery] bool sortDesc = true)
    {
        var result = await _service.GetAllAsync(search, date, productionWorkOrderId, inspectorUserId,
            page, pageSize, sortBy, sortDesc);
        return Ok(ApiResponse<PagedResponse<InProcessInspectionDto>>.Ok(result));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<InProcessInspectionDetailDto>>> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);
        return Ok(ApiResponse<InProcessInspectionDetailDto>.Ok(result));
    }

    [HttpGet("by-pwo/{pwoId}")]
    public async Task<ActionResult<ApiResponse<List<InProcessInspectionDto>>>> GetByPWO(int pwoId)
    {
        var result = await _service.GetByPWOAsync(pwoId);
        return Ok(ApiResponse<List<InProcessInspectionDto>>.Ok(result));
    }

    [HttpGet("dft-trend/{pwoId}")]
    public async Task<ActionResult<ApiResponse<List<DFTTrendDto>>>> GetDFTTrend(int pwoId)
    {
        var result = await _service.GetDFTTrendAsync(pwoId);
        return Ok(ApiResponse<List<DFTTrendDto>>.Ok(result));
    }

    [HttpPost]
    [Authorize(Roles = "QA,Admin,Leader")]
    public async Task<ActionResult<ApiResponse<InProcessInspectionDto>>> Create(CreateInProcessInspectionDto dto)
    {
        var userId = User.GetUserId()!;
        var result = await _service.CreateAsync(dto, userId);
        return CreatedAtAction(nameof(GetById), new { id = result.Id },
            ApiResponse<InProcessInspectionDto>.Ok(result, "In-process inspection created successfully."));
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "QA,Admin,Leader")]
    public async Task<ActionResult<ApiResponse<InProcessInspectionDto>>> Update(int id, CreateInProcessInspectionDto dto)
    {
        var userId = User.GetUserId()!;
        var result = await _service.UpdateAsync(id, dto, userId);
        return Ok(ApiResponse<InProcessInspectionDto>.Ok(result, "In-process inspection updated successfully."));
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
