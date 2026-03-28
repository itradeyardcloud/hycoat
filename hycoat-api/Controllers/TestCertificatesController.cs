using HycoatApi.DTOs;
using HycoatApi.DTOs.Quality;
using HycoatApi.Helpers;
using HycoatApi.Services.Quality;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HycoatApi.Controllers;

[ApiController]
[Route("api/test-certificates")]
[Authorize]
public class TestCertificatesController : ControllerBase
{
    private readonly ITestCertificateService _service;

    public TestCertificatesController(ITestCertificateService service)
    {
        _service = service;
    }

    [HttpGet]
    [Authorize(Roles = "QA,SCM,Sales,Admin,Leader")]
    public async Task<ActionResult<ApiResponse<PagedResponse<TestCertificateDto>>>> GetAll(
        [FromQuery] string? search,
        [FromQuery] DateTime? date,
        [FromQuery] int? customerId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string sortBy = "Date",
        [FromQuery] bool sortDesc = true)
    {
        var result = await _service.GetAllAsync(search, date, customerId,
            page, pageSize, sortBy, sortDesc);
        return Ok(ApiResponse<PagedResponse<TestCertificateDto>>.Ok(result));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<TestCertificateDetailDto>>> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);
        return Ok(ApiResponse<TestCertificateDetailDto>.Ok(result));
    }

    [HttpGet("by-work-order/{woId}")]
    public async Task<ActionResult<ApiResponse<TestCertificateDetailDto>>> GetByWorkOrder(int woId)
    {
        var result = await _service.GetByWorkOrderAsync(woId);
        if (result == null)
            return NotFound(ApiResponse<TestCertificateDetailDto>.Fail("No test certificate found for this work order."));
        return Ok(ApiResponse<TestCertificateDetailDto>.Ok(result));
    }

    [HttpPost]
    [Authorize(Roles = "QA,Admin,Leader")]
    public async Task<ActionResult<ApiResponse<TestCertificateDto>>> Create(CreateTestCertificateDto dto)
    {
        var userId = User.GetUserId()!;
        var result = await _service.CreateAsync(dto, userId);
        return CreatedAtAction(nameof(GetById), new { id = result.Id },
            ApiResponse<TestCertificateDto>.Ok(result, "Test certificate created successfully."));
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "QA,Admin,Leader")]
    public async Task<ActionResult<ApiResponse<TestCertificateDto>>> Update(int id, CreateTestCertificateDto dto)
    {
        var userId = User.GetUserId()!;
        var result = await _service.UpdateAsync(id, dto, userId);
        return Ok(ApiResponse<TestCertificateDto>.Ok(result, "Test certificate updated successfully."));
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = User.GetUserId()!;
        await _service.DeleteAsync(id, userId);
        return NoContent();
    }

    [HttpPost("{id}/generate-pdf")]
    [Authorize(Roles = "QA,Admin,Leader")]
    public async Task<IActionResult> GeneratePdf(int id)
    {
        var pdfBytes = await _service.GeneratePdfAsync(id);
        return File(pdfBytes, "application/pdf", $"TC-{id}.pdf");
    }

    [HttpGet("{id}/pdf")]
    public async Task<IActionResult> DownloadPdf(int id)
    {
        var result = await _service.DownloadPdfAsync(id);
        if (result == null)
            return NotFound(ApiResponse<string>.Fail("PDF not generated yet."));
        return File(result.Value.FileBytes, "application/pdf", result.Value.FileName);
    }
}
