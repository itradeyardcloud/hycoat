using HycoatApi.DTOs;
using HycoatApi.DTOs.Dispatch;
using HycoatApi.Helpers;
using HycoatApi.Services.Dispatch;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HycoatApi.Controllers;

[ApiController]
[Route("api/invoices")]
[Authorize]
public class InvoicesController : ControllerBase
{
    private readonly IInvoiceService _service;
    private readonly IDocumentBundleEmailService _emailService;

    public InvoicesController(IInvoiceService service, IDocumentBundleEmailService emailService)
    {
        _service = service;
        _emailService = emailService;
    }

    [HttpGet]
    [Authorize(Roles = "Finance,Sales,SCM,Admin,Leader")]
    public async Task<ActionResult<ApiResponse<PagedResponse<InvoiceDto>>>> GetAll(
        [FromQuery] string? search,
        [FromQuery] DateTime? dateFrom,
        [FromQuery] DateTime? dateTo,
        [FromQuery] int? customerId,
        [FromQuery] string? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string sortBy = "Date",
        [FromQuery] bool sortDesc = true)
    {
        var result = await _service.GetAllAsync(search, dateFrom, dateTo, customerId, status,
            page, pageSize, sortBy, sortDesc);
        return Ok(ApiResponse<PagedResponse<InvoiceDto>>.Ok(result));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<InvoiceDetailDto>>> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);
        return Ok(ApiResponse<InvoiceDetailDto>.Ok(result));
    }

    [HttpGet("by-work-order/{woId}")]
    public async Task<ActionResult<ApiResponse<InvoiceDetailDto>>> GetByWorkOrder(int woId)
    {
        var result = await _service.GetByWorkOrderIdAsync(woId);
        if (result == null)
            return NotFound(ApiResponse<InvoiceDetailDto>.Fail("No invoice found for this Work Order."));
        return Ok(ApiResponse<InvoiceDetailDto>.Ok(result));
    }

    [HttpGet("auto-fill/{woId}")]
    [Authorize(Roles = "Finance,Admin,Leader")]
    public async Task<ActionResult<ApiResponse<InvoiceAutoFillDto>>> AutoFill(int woId)
    {
        var result = await _service.AutoFillAsync(woId);
        return Ok(ApiResponse<InvoiceAutoFillDto>.Ok(result));
    }

    [HttpPost]
    [Authorize(Roles = "Finance,Admin,Leader")]
    public async Task<ActionResult<ApiResponse<InvoiceDto>>> Create(CreateInvoiceDto dto)
    {
        var userId = User.GetUserId()!;
        var result = await _service.CreateAsync(dto, userId);
        return CreatedAtAction(nameof(GetById), new { id = result.Id },
            ApiResponse<InvoiceDto>.Ok(result, "Invoice created successfully."));
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Finance,Admin,Leader")]
    public async Task<ActionResult<ApiResponse<InvoiceDto>>> Update(int id, CreateInvoiceDto dto)
    {
        var userId = User.GetUserId()!;
        var result = await _service.UpdateAsync(id, dto, userId);
        return Ok(ApiResponse<InvoiceDto>.Ok(result, "Invoice updated successfully."));
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
    [Authorize(Roles = "Finance,Admin,Leader")]
    public async Task<ActionResult<ApiResponse<InvoiceDto>>> UpdateStatus(int id, UpdateInvoiceStatusDto dto)
    {
        var userId = User.GetUserId()!;
        var result = await _service.UpdateStatusAsync(id, dto.Status, userId);
        return Ok(ApiResponse<InvoiceDto>.Ok(result, $"Status updated to {dto.Status}."));
    }

    [HttpPost("{id}/generate-pdf")]
    [Authorize(Roles = "Finance,Admin,Leader")]
    public async Task<IActionResult> GeneratePdf(int id)
    {
        var pdf = await _service.GeneratePdfAsync(id);
        return File(pdf, "application/pdf", $"Invoice-{id}.pdf");
    }

    [HttpGet("{id}/pdf")]
    public async Task<IActionResult> GetPdf(int id)
    {
        var pdf = await _service.GeneratePdfAsync(id);
        return File(pdf, "application/pdf", $"Invoice-{id}.pdf");
    }

    [HttpPost("{id}/send-email")]
    [Authorize(Roles = "Finance,Sales,Admin,Leader")]
    public async Task<ActionResult<ApiResponse<string>>> SendEmail(int id, SendEmailDto dto)
    {
        await _emailService.SendAsync(id, dto.RecipientEmails, dto.Subject, dto.Body);
        return Ok(ApiResponse<string>.Ok("Document bundle email sent successfully."));
    }
}
