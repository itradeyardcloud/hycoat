using HycoatApi.DTOs;
using HycoatApi.DTOs.Purchase;
using HycoatApi.Helpers;
using HycoatApi.Services.Purchase;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HycoatApi.Controllers;

[ApiController]
[Route("api/purchase-orders")]
[Authorize]
public class PurchaseOrdersController : ControllerBase
{
    private readonly IPurchaseOrderService _service;

    public PurchaseOrdersController(IPurchaseOrderService service)
    {
        _service = service;
    }

    [HttpGet]
    [Authorize(Roles = "Purchase,Finance,Admin,Leader")]
    public async Task<ActionResult<ApiResponse<PagedResponse<PurchaseOrderDto>>>> GetAll(
        [FromQuery] string? search,
        [FromQuery] string? status,
        [FromQuery] int? vendorId,
        [FromQuery] DateTime? dateFrom,
        [FromQuery] DateTime? dateTo,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string sortBy = "Date",
        [FromQuery] bool sortDesc = true)
    {
        var result = await _service.GetAllAsync(search, status, vendorId, dateFrom, dateTo,
            page, pageSize, sortBy, sortDesc);
        return Ok(ApiResponse<PagedResponse<PurchaseOrderDto>>.Ok(result));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<PurchaseOrderDetailDto>>> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);
        return Ok(ApiResponse<PurchaseOrderDetailDto>.Ok(result));
    }

    [HttpPost]
    [Authorize(Roles = "Purchase,Admin,Leader")]
    public async Task<ActionResult<ApiResponse<PurchaseOrderDto>>> Create(CreatePurchaseOrderDto dto)
    {
        var userId = User.GetUserId()!;
        var result = await _service.CreateAsync(dto, userId);
        return CreatedAtAction(nameof(GetById), new { id = result.Id },
            ApiResponse<PurchaseOrderDto>.Ok(result, "Purchase order created successfully."));
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Purchase,Admin,Leader")]
    public async Task<ActionResult<ApiResponse<PurchaseOrderDto>>> Update(int id, CreatePurchaseOrderDto dto)
    {
        var userId = User.GetUserId()!;
        var result = await _service.UpdateAsync(id, dto, userId);
        return Ok(ApiResponse<PurchaseOrderDto>.Ok(result, "Purchase order updated successfully."));
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
    public async Task<ActionResult<ApiResponse<PurchaseOrderDto>>> UpdateStatus(int id, UpdatePurchaseOrderStatusDto dto)
    {
        var userId = User.GetUserId()!;
        var result = await _service.UpdateStatusAsync(id, dto, userId);
        return Ok(ApiResponse<PurchaseOrderDto>.Ok(result, "PO status updated successfully."));
    }

    [HttpPost("{id}/generate-pdf")]
    [Authorize(Roles = "Purchase,Admin,Leader")]
    public async Task<IActionResult> GeneratePdf(int id)
    {
        var pdfBytes = await _service.GeneratePdfAsync(id);
        return File(pdfBytes, "application/pdf", $"PO-{id}.pdf");
    }

    [HttpGet("{id}/pdf")]
    public async Task<IActionResult> GetPdf(int id)
    {
        var pdfBytes = await _service.GeneratePdfAsync(id);
        return File(pdfBytes, "application/pdf", $"PO-{id}.pdf");
    }
}
