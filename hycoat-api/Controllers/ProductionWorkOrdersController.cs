using HycoatApi.DTOs;
using HycoatApi.DTOs.Common;
using HycoatApi.DTOs.Planning;
using HycoatApi.Helpers;
using HycoatApi.Services.Planning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HycoatApi.Controllers;

[ApiController]
[Route("api/production-work-orders")]
[Authorize]
public class ProductionWorkOrdersController : ControllerBase
{
    private readonly IProductionWorkOrderService _service;

    public ProductionWorkOrdersController(IProductionWorkOrderService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResponse<ProductionWorkOrderDto>>>> GetAll(
        [FromQuery] string? search,
        [FromQuery] string? status,
        [FromQuery] int? workOrderId,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string sortBy = "Date",
        [FromQuery] bool sortDesc = true)
    {
        var result = await _service.GetAllAsync(search, status, workOrderId,
            fromDate, toDate, page, pageSize, sortBy, sortDesc);
        return Ok(ApiResponse<PagedResponse<ProductionWorkOrderDto>>.Ok(result));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<ProductionWorkOrderDetailDto>>> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);
        return Ok(ApiResponse<ProductionWorkOrderDetailDto>.Ok(result));
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Leader")]
    public async Task<ActionResult<ApiResponse<ProductionWorkOrderDto>>> Create(CreateProductionWorkOrderDto dto)
    {
        var userId = User.GetUserId()!;
        var result = await _service.CreateAsync(dto, userId);
        return CreatedAtAction(nameof(GetById), new { id = result.Id },
            ApiResponse<ProductionWorkOrderDto>.Ok(result, "Production Work Order created successfully."));
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Leader")]
    public async Task<ActionResult<ApiResponse<ProductionWorkOrderDto>>> Update(int id, UpdateProductionWorkOrderDto dto)
    {
        var userId = User.GetUserId()!;
        var result = await _service.UpdateAsync(id, dto, userId);
        return Ok(ApiResponse<ProductionWorkOrderDto>.Ok(result, "Production Work Order updated successfully."));
    }

    [HttpPatch("{id}/status")]
    [Authorize(Roles = "Admin,Leader")]
    public async Task<IActionResult> UpdateStatus(int id, UpdatePWOStatusDto dto)
    {
        var userId = User.GetUserId()!;
        await _service.UpdateStatusAsync(id, dto, userId);
        return Ok(ApiResponse<object>.Ok(null!, "Production Work Order status updated."));
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = User.GetUserId()!;
        await _service.DeleteAsync(id, userId);
        return NoContent();
    }

    [HttpPost("calculate-time")]
    [Authorize(Roles = "Admin,Leader")]
    public async Task<ActionResult<ApiResponse<ProductionTimeCalcResultDto>>> CalculateTime(
        ProductionTimeCalcRequestDto dto)
    {
        var result = await _service.CalculateTimeAsync(dto);
        return Ok(ApiResponse<ProductionTimeCalcResultDto>.Ok(result));
    }

    [HttpGet("lookup")]
    public async Task<ActionResult<ApiResponse<List<LookupDto>>>> GetLookup(
        [FromQuery] string? status)
    {
        var result = await _service.GetLookupAsync(status);
        return Ok(ApiResponse<List<LookupDto>>.Ok(result));
    }

    [HttpGet("{id}/pdf")]
    [Authorize(Roles = "Admin,Leader")]
    public IActionResult GetPdf(int id)
    {
        // PDF generation stubbed — to be implemented
        return StatusCode(501, ApiResponse<object>.Fail("PDF generation not yet implemented."));
    }
}
