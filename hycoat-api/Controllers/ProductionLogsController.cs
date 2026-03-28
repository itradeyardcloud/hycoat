using HycoatApi.DTOs;
using HycoatApi.DTOs.Production;
using HycoatApi.Helpers;
using HycoatApi.Services.Production;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HycoatApi.Controllers;

[ApiController]
[Route("api/production-logs")]
[Authorize]
public class ProductionLogsController : ControllerBase
{
    private readonly IProductionLogService _service;

    public ProductionLogsController(IProductionLogService service)
    {
        _service = service;
    }

    [HttpGet]
    [Authorize(Roles = "Production,QA,PPC,Admin,Leader")]
    public async Task<ActionResult<ApiResponse<PagedResponse<ProductionLogDto>>>> GetAll(
        [FromQuery] string? search,
        [FromQuery] DateTime? date,
        [FromQuery] string? shift,
        [FromQuery] int? productionWorkOrderId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string sortBy = "Date",
        [FromQuery] bool sortDesc = true)
    {
        var result = await _service.GetAllAsync(search, date, shift, productionWorkOrderId,
            page, pageSize, sortBy, sortDesc);
        return Ok(ApiResponse<PagedResponse<ProductionLogDto>>.Ok(result));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<ProductionLogDetailDto>>> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);
        return Ok(ApiResponse<ProductionLogDetailDto>.Ok(result));
    }

    [HttpGet("by-pwo/{pwoId}")]
    public async Task<ActionResult<ApiResponse<List<ProductionLogDto>>>> GetByPWO(int pwoId)
    {
        var result = await _service.GetByPWOAsync(pwoId);
        return Ok(ApiResponse<List<ProductionLogDto>>.Ok(result));
    }

    [HttpPost]
    [Authorize(Roles = "Production,Admin,Leader")]
    public async Task<ActionResult<ApiResponse<ProductionLogDto>>> Create(CreateProductionLogDto dto)
    {
        var userId = User.GetUserId()!;
        var result = await _service.CreateAsync(dto, userId);
        return CreatedAtAction(nameof(GetById), new { id = result.Id },
            ApiResponse<ProductionLogDto>.Ok(result, "Production log created successfully."));
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Production,Admin,Leader")]
    public async Task<ActionResult<ApiResponse<ProductionLogDto>>> Update(int id, CreateProductionLogDto dto)
    {
        var userId = User.GetUserId()!;
        var result = await _service.UpdateAsync(id, dto, userId);
        return Ok(ApiResponse<ProductionLogDto>.Ok(result, "Production log updated successfully."));
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
    [Authorize(Roles = "Production,Admin,Leader")]
    public async Task<ActionResult<ApiResponse<ProductionPhotoDto>>> UploadPhoto(
        int id, IFormFile file, [FromForm] string? description)
    {
        var userId = User.GetUserId()!;
        var result = await _service.UploadPhotoAsync(id, file, description, userId);
        return Ok(ApiResponse<ProductionPhotoDto>.Ok(result, "Photo uploaded successfully."));
    }

    [HttpDelete("{id}/photos/{photoId}")]
    [Authorize(Roles = "Production,Admin")]
    public async Task<IActionResult> DeletePhoto(int id, int photoId)
    {
        var userId = User.GetUserId()!;
        await _service.DeletePhotoAsync(id, photoId, userId);
        return NoContent();
    }
}
