using HycoatApi.DTOs;
using HycoatApi.DTOs.Common;
using HycoatApi.DTOs.Sales;
using HycoatApi.Helpers;
using HycoatApi.Services.Sales;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HycoatApi.Controllers;

[ApiController]
[Route("api/proforma-invoices")]
[Authorize]
public class ProformaInvoicesController : ControllerBase
{
    private readonly IProformaInvoiceService _service;

    public ProformaInvoicesController(IProformaInvoiceService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResponse<PIDto>>>> GetAll(
        [FromQuery] string? search,
        [FromQuery] string? status,
        [FromQuery] int? customerId,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string sortBy = "Date",
        [FromQuery] bool sortDesc = true)
    {
        var result = await _service.GetAllAsync(search, status, customerId,
            fromDate, toDate, page, pageSize, sortBy, sortDesc);
        return Ok(ApiResponse<PagedResponse<PIDto>>.Ok(result));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<PIDetailDto>>> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);
        return Ok(ApiResponse<PIDetailDto>.Ok(result));
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Leader")]
    public async Task<ActionResult<ApiResponse<PIDto>>> Create(CreateProformaInvoiceDto dto)
    {
        var userId = User.GetUserId()!;
        var result = await _service.CreateAsync(dto, userId);
        return CreatedAtAction(nameof(GetById), new { id = result.Id },
            ApiResponse<PIDto>.Ok(result, "Proforma Invoice created successfully."));
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Leader")]
    public async Task<ActionResult<ApiResponse<PIDto>>> Update(int id, CreateProformaInvoiceDto dto)
    {
        var userId = User.GetUserId()!;
        var result = await _service.UpdateAsync(id, dto, userId);
        return Ok(ApiResponse<PIDto>.Ok(result, "Proforma Invoice updated successfully."));
    }

    [HttpPatch("{id}/status")]
    [Authorize(Roles = "Admin,Leader")]
    public async Task<IActionResult> UpdateStatus(int id, UpdatePIStatusDto dto)
    {
        var userId = User.GetUserId()!;
        await _service.UpdateStatusAsync(id, dto, userId);
        return Ok(ApiResponse<object>.Ok(null!, "Proforma Invoice status updated."));
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = User.GetUserId()!;
        await _service.DeleteAsync(id, userId);
        return NoContent();
    }

    [HttpGet("{id}/pdf")]
    public async Task<IActionResult> DownloadPdf(int id)
    {
        var pdf = await _service.GeneratePdfAsync(id);
        return File(pdf, "application/pdf", $"PI-{id}.pdf");
    }

    [HttpPost("calculate-area")]
    public async Task<ActionResult<ApiResponse<CalculateAreaResponseDto>>> CalculateArea(CalculateAreaRequestDto dto)
    {
        var result = await _service.CalculateAreaAsync(dto);
        return Ok(ApiResponse<CalculateAreaResponseDto>.Ok(result));
    }

    [HttpGet("lookup")]
    public async Task<ActionResult<ApiResponse<List<LookupDto>>>> GetLookup()
    {
        var result = await _service.GetLookupAsync();
        return Ok(ApiResponse<List<LookupDto>>.Ok(result));
    }
}
