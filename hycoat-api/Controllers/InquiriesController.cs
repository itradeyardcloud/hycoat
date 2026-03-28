using HycoatApi.DTOs;
using HycoatApi.DTOs.Common;
using HycoatApi.DTOs.Sales;
using HycoatApi.Helpers;
using HycoatApi.Services.Sales;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HycoatApi.Controllers;

[ApiController]
[Route("api/inquiries")]
[Authorize]
public class InquiriesController : ControllerBase
{
    private readonly IInquiryService _service;

    public InquiriesController(IInquiryService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResponse<InquiryDto>>>> GetAll(
        [FromQuery] string? search,
        [FromQuery] string? status,
        [FromQuery] int? customerId,
        [FromQuery] string? assignedToUserId,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string sortBy = "Date",
        [FromQuery] bool sortDesc = true)
    {
        var result = await _service.GetAllAsync(search, status, customerId, assignedToUserId,
            fromDate, toDate, page, pageSize, sortBy, sortDesc);
        return Ok(ApiResponse<PagedResponse<InquiryDto>>.Ok(result));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<InquiryDetailDto>>> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);
        return Ok(ApiResponse<InquiryDetailDto>.Ok(result));
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Leader")]
    public async Task<ActionResult<ApiResponse<InquiryDto>>> Create(CreateInquiryDto dto)
    {
        var userId = User.GetUserId()!;
        var result = await _service.CreateAsync(dto, userId);
        return CreatedAtAction(nameof(GetById), new { id = result.Id },
            ApiResponse<InquiryDto>.Ok(result, "Inquiry created successfully."));
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Leader")]
    public async Task<ActionResult<ApiResponse<InquiryDto>>> Update(int id, UpdateInquiryDto dto)
    {
        var userId = User.GetUserId()!;
        var result = await _service.UpdateAsync(id, dto, userId);
        return Ok(ApiResponse<InquiryDto>.Ok(result, "Inquiry updated successfully."));
    }

    [HttpPatch("{id}/status")]
    [Authorize(Roles = "Admin,Leader")]
    public async Task<IActionResult> UpdateStatus(int id, UpdateInquiryStatusDto dto)
    {
        var userId = User.GetUserId()!;
        await _service.UpdateStatusAsync(id, dto, userId);
        return Ok(ApiResponse<object>.Ok(null!, "Inquiry status updated."));
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = User.GetUserId()!;
        await _service.DeleteAsync(id, userId);
        return NoContent();
    }

    [HttpGet("stats")]
    public async Task<ActionResult<ApiResponse<InquiryStatsDto>>> GetStats()
    {
        var result = await _service.GetStatsAsync();
        return Ok(ApiResponse<InquiryStatsDto>.Ok(result));
    }

    [HttpGet("lookup")]
    public async Task<ActionResult<ApiResponse<List<LookupDto>>>> GetLookup()
    {
        var result = await _service.GetLookupAsync();
        return Ok(ApiResponse<List<LookupDto>>.Ok(result));
    }
}
