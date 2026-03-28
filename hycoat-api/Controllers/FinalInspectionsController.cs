using HycoatApi.DTOs;
using HycoatApi.DTOs.Quality;
using HycoatApi.Helpers;
using HycoatApi.Services.Quality;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HycoatApi.Controllers;

[ApiController]
[Route("api/final-inspections")]
[Authorize]
public class FinalInspectionsController : ControllerBase
{
    private readonly IFinalInspectionService _service;

    public FinalInspectionsController(IFinalInspectionService service)
    {
        _service = service;
    }

    [HttpGet]
    [Authorize(Roles = "QA,SCM,Admin,Leader")]
    public async Task<ActionResult<ApiResponse<PagedResponse<FinalInspectionDto>>>> GetAll(
        [FromQuery] string? search,
        [FromQuery] DateTime? date,
        [FromQuery] int? productionWorkOrderId,
        [FromQuery] string? overallStatus,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string sortBy = "Date",
        [FromQuery] bool sortDesc = true)
    {
        var result = await _service.GetAllAsync(search, date, productionWorkOrderId, overallStatus,
            page, pageSize, sortBy, sortDesc);
        return Ok(ApiResponse<PagedResponse<FinalInspectionDto>>.Ok(result));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<FinalInspectionDetailDto>>> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);
        return Ok(ApiResponse<FinalInspectionDetailDto>.Ok(result));
    }

    [HttpGet("by-pwo/{pwoId}")]
    public async Task<ActionResult<ApiResponse<FinalInspectionDetailDto>>> GetByPWO(int pwoId)
    {
        var result = await _service.GetByPWOAsync(pwoId);
        if (result == null)
            return NotFound(ApiResponse<FinalInspectionDetailDto>.Fail("No final inspection found for this PWO."));
        return Ok(ApiResponse<FinalInspectionDetailDto>.Ok(result));
    }

    [HttpPost]
    [Authorize(Roles = "QA,Admin,Leader")]
    public async Task<ActionResult<ApiResponse<FinalInspectionDto>>> Create(CreateFinalInspectionDto dto)
    {
        var userId = User.GetUserId()!;
        var result = await _service.CreateAsync(dto, userId);
        return CreatedAtAction(nameof(GetById), new { id = result.Id },
            ApiResponse<FinalInspectionDto>.Ok(result, "Final inspection created successfully."));
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "QA,Admin,Leader")]
    public async Task<ActionResult<ApiResponse<FinalInspectionDto>>> Update(int id, CreateFinalInspectionDto dto)
    {
        var userId = User.GetUserId()!;
        var result = await _service.UpdateAsync(id, dto, userId);
        return Ok(ApiResponse<FinalInspectionDto>.Ok(result, "Final inspection updated successfully."));
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
