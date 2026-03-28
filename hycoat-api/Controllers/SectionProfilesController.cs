using HycoatApi.DTOs;
using HycoatApi.DTOs.Common;
using HycoatApi.DTOs.Masters;
using HycoatApi.Helpers;
using HycoatApi.Services.Masters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HycoatApi.Controllers;

[ApiController]
[Route("api/section-profiles")]
[Authorize]
public class SectionProfilesController : ControllerBase
{
    private readonly ISectionProfileService _service;

    public SectionProfilesController(ISectionProfileService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResponse<SectionProfileDto>>>> GetAll(
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string sortBy = "SectionNumber",
        [FromQuery] bool sortDesc = false)
    {
        var result = await _service.GetAllAsync(search, page, pageSize, sortBy, sortDesc);
        return Ok(ApiResponse<PagedResponse<SectionProfileDto>>.Ok(result));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<SectionProfileDetailDto>>> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);
        return Ok(ApiResponse<SectionProfileDetailDto>.Ok(result));
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Leader")]
    public async Task<ActionResult<ApiResponse<SectionProfileDto>>> Create(CreateSectionProfileDto dto)
    {
        var userId = User.GetUserId()!;
        var result = await _service.CreateAsync(dto, userId);
        return CreatedAtAction(nameof(GetById), new { id = result.Id },
            ApiResponse<SectionProfileDto>.Ok(result, "Section profile created successfully."));
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Leader")]
    public async Task<ActionResult<ApiResponse<SectionProfileDto>>> Update(int id, UpdateSectionProfileDto dto)
    {
        var userId = User.GetUserId()!;
        var result = await _service.UpdateAsync(id, dto, userId);
        return Ok(ApiResponse<SectionProfileDto>.Ok(result, "Section profile updated successfully."));
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
    public async Task<ActionResult<ApiResponse<List<SectionProfileLookupDto>>>> GetLookup()
    {
        var result = await _service.GetLookupAsync();
        return Ok(ApiResponse<List<SectionProfileLookupDto>>.Ok(result));
    }

    [HttpPost("{id}/upload-drawing")]
    [Authorize(Roles = "Admin,Leader")]
    public async Task<ActionResult<ApiResponse<string>>> UploadDrawing(int id, IFormFile file)
    {
        var userId = User.GetUserId()!;
        var url = await _service.UploadDrawingAsync(id, file, userId);
        return Ok(ApiResponse<string>.Ok(url, "Drawing uploaded successfully."));
    }
}
