using HycoatApi.DTOs;
using HycoatApi.DTOs.Quality;
using HycoatApi.Helpers;
using HycoatApi.Services.Quality;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HycoatApi.Controllers;

[ApiController]
[Route("api/panel-tests")]
[Authorize]
public class PanelTestsController : ControllerBase
{
    private readonly IPanelTestService _service;

    public PanelTestsController(IPanelTestService service)
    {
        _service = service;
    }

    [HttpGet]
    [Authorize(Roles = "QA,Production,Admin,Leader")]
    public async Task<ActionResult<ApiResponse<PagedResponse<PanelTestDto>>>> GetAll(
        [FromQuery] string? search,
        [FromQuery] DateTime? date,
        [FromQuery] int? productionWorkOrderId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string sortBy = "Date",
        [FromQuery] bool sortDesc = true)
    {
        var result = await _service.GetAllAsync(search, date, productionWorkOrderId,
            page, pageSize, sortBy, sortDesc);
        return Ok(ApiResponse<PagedResponse<PanelTestDto>>.Ok(result));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<PanelTestDetailDto>>> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);
        return Ok(ApiResponse<PanelTestDetailDto>.Ok(result));
    }

    [HttpGet("by-pwo/{pwoId}")]
    public async Task<ActionResult<ApiResponse<List<PanelTestDto>>>> GetByPWO(int pwoId)
    {
        var result = await _service.GetByPWOAsync(pwoId);
        return Ok(ApiResponse<List<PanelTestDto>>.Ok(result));
    }

    [HttpPost]
    [Authorize(Roles = "QA,Admin,Leader")]
    public async Task<ActionResult<ApiResponse<PanelTestDto>>> Create(CreatePanelTestDto dto)
    {
        var userId = User.GetUserId()!;
        var result = await _service.CreateAsync(dto, userId);
        return CreatedAtAction(nameof(GetById), new { id = result.Id },
            ApiResponse<PanelTestDto>.Ok(result, "Panel test created successfully."));
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "QA,Admin,Leader")]
    public async Task<ActionResult<ApiResponse<PanelTestDto>>> Update(int id, CreatePanelTestDto dto)
    {
        var userId = User.GetUserId()!;
        var result = await _service.UpdateAsync(id, dto, userId);
        return Ok(ApiResponse<PanelTestDto>.Ok(result, "Panel test updated successfully."));
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
