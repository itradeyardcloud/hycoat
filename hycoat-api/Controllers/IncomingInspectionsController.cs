using HycoatApi.DTOs;
using HycoatApi.DTOs.Common;
using HycoatApi.DTOs.MaterialInward;
using HycoatApi.Helpers;
using HycoatApi.Services.MaterialInward;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HycoatApi.Controllers;

[ApiController]
[Route("api/incoming-inspections")]
[Authorize]
public class IncomingInspectionsController : ControllerBase
{
    private readonly IIncomingInspectionService _service;

    public IncomingInspectionsController(IIncomingInspectionService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResponse<IncomingInspectionDto>>>> GetAll(
        [FromQuery] string? search,
        [FromQuery] string? overallStatus,
        [FromQuery] int? materialInwardId,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string sortBy = "Date",
        [FromQuery] bool sortDesc = true)
    {
        var result = await _service.GetAllAsync(search, overallStatus, materialInwardId,
            fromDate, toDate, page, pageSize, sortBy, sortDesc);
        return Ok(ApiResponse<PagedResponse<IncomingInspectionDto>>.Ok(result));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<IncomingInspectionDetailDto>>> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);
        return Ok(ApiResponse<IncomingInspectionDetailDto>.Ok(result));
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Leader")]
    public async Task<ActionResult<ApiResponse<IncomingInspectionDto>>> Create(CreateIncomingInspectionDto dto)
    {
        var userId = User.GetUserId()!;
        var result = await _service.CreateAsync(dto, userId);
        return CreatedAtAction(nameof(GetById), new { id = result.Id },
            ApiResponse<IncomingInspectionDto>.Ok(result, "Incoming Inspection created successfully."));
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Leader")]
    public async Task<ActionResult<ApiResponse<IncomingInspectionDto>>> Update(int id, CreateIncomingInspectionDto dto)
    {
        var userId = User.GetUserId()!;
        var result = await _service.UpdateAsync(id, dto, userId);
        return Ok(ApiResponse<IncomingInspectionDto>.Ok(result, "Incoming Inspection updated successfully."));
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = User.GetUserId()!;
        await _service.DeleteAsync(id, userId);
        return NoContent();
    }

    [HttpPost("{id}/photos")]
    [Authorize(Roles = "Admin,Leader")]
    public async Task<ActionResult<ApiResponse<List<FileAttachmentDto>>>> UploadPhotos(int id, List<IFormFile> files)
    {
        var userId = User.GetUserId()!;
        var result = await _service.UploadPhotosAsync(id, files, userId);
        return Ok(ApiResponse<List<FileAttachmentDto>>.Ok(result, "Photos uploaded successfully."));
    }

    [HttpGet("{id}/photos")]
    public async Task<ActionResult<ApiResponse<List<FileAttachmentDto>>>> GetPhotos(int id)
    {
        var result = await _service.GetPhotosAsync(id);
        return Ok(ApiResponse<List<FileAttachmentDto>>.Ok(result));
    }
}
