using HycoatApi.DTOs;
using HycoatApi.DTOs.Reports;
using HycoatApi.Helpers;
using HycoatApi.Services.Reports;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HycoatApi.Controllers;

[ApiController]
[Route("api/yield-reports")]
[Authorize]
public class YieldReportsController : ControllerBase
{
    private readonly IYieldReportService _service;

    public YieldReportsController(IYieldReportService service)
    {
        _service = service;
    }

    [HttpGet]
    [Authorize(Roles = "Production,Admin,Leader,User")]
    public async Task<ActionResult<ApiResponse<PagedResponse<YieldReportDto>>>> GetAll(
        [FromQuery] DateTime? dateFrom,
        [FromQuery] DateTime? dateTo,
        [FromQuery] int? productionUnitId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await _service.GetAllAsync(dateFrom, dateTo, productionUnitId, page, pageSize);
        return Ok(ApiResponse<PagedResponse<YieldReportDto>>.Ok(result));
    }

    [HttpGet("summary")]
    [Authorize(Roles = "Production,Admin,Leader,User")]
    public async Task<ActionResult<ApiResponse<YieldSummaryDto>>> GetSummary([FromQuery] DateTime? date)
    {
        var result = await _service.GetSummaryAsync(date);
        return Ok(ApiResponse<YieldSummaryDto>.Ok(result));
    }

    [HttpPost]
    [Authorize(Roles = "Production,Admin,Leader,User")]
    public async Task<ActionResult<ApiResponse<YieldReportDto>>> Create(CreateYieldReportDto dto)
    {
        var userId = User.GetUserId()!;
        var result = await _service.CreateAsync(dto, userId);
        return Ok(ApiResponse<YieldReportDto>.Ok(result, "Yield report created successfully."));
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,Leader")]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = User.GetUserId()!;
        await _service.DeleteAsync(id, userId);
        return NoContent();
    }
}