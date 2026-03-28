using HycoatApi.DTOs;
using HycoatApi.DTOs.Common;
using HycoatApi.DTOs.MaterialInward;
using HycoatApi.Helpers;
using HycoatApi.Services.MaterialInward;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HycoatApi.Controllers;

[ApiController]
[Route("api/material-inwards")]
[Authorize]
public class MaterialInwardsController : ControllerBase
{
    private readonly IMaterialInwardService _service;

    public MaterialInwardsController(IMaterialInwardService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResponse<MaterialInwardDto>>>> GetAll(
        [FromQuery] string? search,
        [FromQuery] string? status,
        [FromQuery] int? customerId,
        [FromQuery] int? workOrderId,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string sortBy = "Date",
        [FromQuery] bool sortDesc = true)
    {
        var result = await _service.GetAllAsync(search, status, customerId, workOrderId,
            fromDate, toDate, page, pageSize, sortBy, sortDesc);
        return Ok(ApiResponse<PagedResponse<MaterialInwardDto>>.Ok(result));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<MaterialInwardDetailDto>>> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);
        return Ok(ApiResponse<MaterialInwardDetailDto>.Ok(result));
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Leader")]
    public async Task<ActionResult<ApiResponse<MaterialInwardDto>>> Create(CreateMaterialInwardDto dto)
    {
        var userId = User.GetUserId()!;
        var result = await _service.CreateAsync(dto, userId);
        return CreatedAtAction(nameof(GetById), new { id = result.Id },
            ApiResponse<MaterialInwardDto>.Ok(result, "Material Inward created successfully."));
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Leader")]
    public async Task<ActionResult<ApiResponse<MaterialInwardDto>>> Update(int id, UpdateMaterialInwardDto dto)
    {
        var userId = User.GetUserId()!;
        var result = await _service.UpdateAsync(id, dto, userId);
        return Ok(ApiResponse<MaterialInwardDto>.Ok(result, "Material Inward updated successfully."));
    }

    [HttpPatch("{id}/status")]
    [Authorize(Roles = "Admin,Leader")]
    public async Task<IActionResult> UpdateStatus(int id, UpdateMaterialInwardStatusDto dto)
    {
        var userId = User.GetUserId()!;
        await _service.UpdateStatusAsync(id, dto, userId);
        return Ok(ApiResponse<object>.Ok(null!, "Material Inward status updated."));
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = User.GetUserId()!;
        await _service.DeleteAsync(id, userId);
        return NoContent();
    }

    [HttpGet("lookup")]
    public async Task<ActionResult<ApiResponse<List<LookupDto>>>> GetLookup(
        [FromQuery] bool? hasInspection)
    {
        var result = await _service.GetLookupAsync(hasInspection);
        return Ok(ApiResponse<List<LookupDto>>.Ok(result));
    }

    [HttpGet("wo-lookup")]
    public async Task<ActionResult<ApiResponse<List<WorkOrderLookupDto>>>> GetWorkOrderLookup(
        [FromQuery] string? search)
    {
        var result = await _service.GetWorkOrderLookupAsync(search);
        return Ok(ApiResponse<List<WorkOrderLookupDto>>.Ok(result));
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

    [HttpDelete("{id}/photos/{photoId}")]
    [Authorize(Roles = "Admin,Leader")]
    public async Task<IActionResult> DeletePhoto(int id, int photoId)
    {
        var userId = User.GetUserId()!;
        await _service.DeletePhotoAsync(id, photoId, userId);
        return NoContent();
    }
}
