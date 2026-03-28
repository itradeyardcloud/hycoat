using HycoatApi.DTOs;
using HycoatApi.DTOs.Dispatch;
using HycoatApi.Helpers;
using HycoatApi.Services.Dispatch;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HycoatApi.Controllers;

[ApiController]
[Route("api/delivery-challans")]
[Authorize]
public class DeliveryChallansController : ControllerBase
{
    private readonly IDeliveryChallanService _service;

    public DeliveryChallansController(IDeliveryChallanService service)
    {
        _service = service;
    }

    [HttpGet]
    [Authorize(Roles = "SCM,Sales,Finance,Admin,Leader")]
    public async Task<ActionResult<ApiResponse<PagedResponse<DeliveryChallanDto>>>> GetAll(
        [FromQuery] string? search,
        [FromQuery] DateTime? date,
        [FromQuery] int? customerId,
        [FromQuery] int? workOrderId,
        [FromQuery] string? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string sortBy = "Date",
        [FromQuery] bool sortDesc = true)
    {
        var result = await _service.GetAllAsync(search, date, customerId, workOrderId, status,
            page, pageSize, sortBy, sortDesc);
        return Ok(ApiResponse<PagedResponse<DeliveryChallanDto>>.Ok(result));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<DeliveryChallanDetailDto>>> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);
        return Ok(ApiResponse<DeliveryChallanDetailDto>.Ok(result));
    }

    [HttpPost]
    [Authorize(Roles = "SCM,Admin,Leader")]
    public async Task<ActionResult<ApiResponse<DeliveryChallanDto>>> Create(CreateDeliveryChallanDto dto)
    {
        var userId = User.GetUserId()!;
        var result = await _service.CreateAsync(dto, userId);
        return CreatedAtAction(nameof(GetById), new { id = result.Id },
            ApiResponse<DeliveryChallanDto>.Ok(result, "Delivery Challan created successfully."));
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "SCM,Admin,Leader")]
    public async Task<ActionResult<ApiResponse<DeliveryChallanDto>>> Update(int id, CreateDeliveryChallanDto dto)
    {
        var userId = User.GetUserId()!;
        var result = await _service.UpdateAsync(id, dto, userId);
        return Ok(ApiResponse<DeliveryChallanDto>.Ok(result, "Delivery Challan updated successfully."));
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
    [Authorize(Roles = "SCM,Admin,Leader")]
    public async Task<ActionResult<ApiResponse<DeliveryChallanDto>>> UpdateStatus(int id, UpdateDCStatusDto dto)
    {
        var userId = User.GetUserId()!;
        var result = await _service.UpdateStatusAsync(id, dto.Status, userId);
        return Ok(ApiResponse<DeliveryChallanDto>.Ok(result, $"Status updated to {dto.Status}."));
    }

    [HttpPost("{id}/generate-pdf")]
    [Authorize(Roles = "SCM,Admin,Leader")]
    public async Task<IActionResult> GeneratePdf(int id)
    {
        var pdf = await _service.GeneratePdfAsync(id);
        return File(pdf, "application/pdf", $"DC-{id}.pdf");
    }

    [HttpGet("{id}/pdf")]
    public async Task<IActionResult> GetPdf(int id)
    {
        var pdf = await _service.GeneratePdfAsync(id);
        return File(pdf, "application/pdf", $"DC-{id}.pdf");
    }

    [HttpPost("{id}/loading-photos")]
    [Authorize(Roles = "SCM,Admin")]
    public async Task<ActionResult<ApiResponse<string>>> UploadLoadingPhotos(int id, [FromForm] List<IFormFile> files)
    {
        var userId = User.GetUserId()!;
        await _service.UploadLoadingPhotosAsync(id, files, userId);
        return Ok(ApiResponse<string>.Ok("Loading photos uploaded successfully."));
    }
}
