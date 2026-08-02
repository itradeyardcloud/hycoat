using HycoatApi.DTOs;
using HycoatApi.DTOs.Masters;
using HycoatApi.Helpers;
using HycoatApi.Services.Masters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HycoatApi.Controllers;

[ApiController]
[Route("api/profile-diagrams")]
[Authorize]
public class ProfileDiagramsController : ControllerBase
{
    private readonly IProfileDiagramService _service;

    public ProfileDiagramsController(IProfileDiagramService service)
    {
        _service = service;
    }

    /// <summary>
    /// Search/list endpoint.
    /// Use ?search=AS23 for text search (management list).
    /// Use ?codes=AS23PS01,AS23PS02&amp;mode=exact for gallery lookup by exact codes.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResponse<ProfileDiagramDto>>>> GetAll(
        [FromQuery] string? search,
        [FromQuery] string? codes,
        [FromQuery] string mode = "auto",
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] string sortBy = "Code",
        [FromQuery] bool sortDesc = false,
        CancellationToken ct = default)
    {
        // When codes param is provided, return exact-code lookup (for gallery)
        if (!string.IsNullOrWhiteSpace(codes))
        {
            var codeList = codes
                .Split(new[] { ',', ' ', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(c => c.Trim())
                .Where(c => c.Length > 0)
                .ToList();

            var byCode = await _service.GetByCodesAsync(codeList, ct);
            return Ok(ApiResponse<PagedResponse<ProfileDiagramDto>>.Ok(new PagedResponse<ProfileDiagramDto>
            {
                Items = byCode,
                TotalCount = byCode.Count,
                Page = 1,
                PageSize = byCode.Count > 0 ? byCode.Count : 1
            }));
        }

        var result = await _service.GetAllAsync(search, page, pageSize, sortBy, sortDesc, ct);
        return Ok(ApiResponse<PagedResponse<ProfileDiagramDto>>.Ok(result));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<ProfileDiagramDetailDto>>> GetById(
        int id, CancellationToken ct = default)
    {
        var result = await _service.GetByIdAsync(id, ct);
        return Ok(ApiResponse<ProfileDiagramDetailDto>.Ok(result));
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Leader")]
    public async Task<ActionResult<ApiResponse<ProfileDiagramDto>>> Create(
        CreateProfileDiagramDto dto, CancellationToken ct = default)
    {
        var userId = User.GetUserId()!;
        var result = await _service.CreateAsync(dto, userId, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id },
            ApiResponse<ProfileDiagramDto>.Ok(result, "Profile diagram created successfully."));
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin,Leader")]
    public async Task<ActionResult<ApiResponse<ProfileDiagramDto>>> Update(
        int id, UpdateProfileDiagramDto dto, CancellationToken ct = default)
    {
        var userId = User.GetUserId()!;
        var result = await _service.UpdateAsync(id, dto, userId, ct);
        return Ok(ApiResponse<ProfileDiagramDto>.Ok(result, "Profile diagram updated successfully."));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct = default)
    {
        var userId = User.GetUserId()!;
        await _service.DeleteAsync(id, userId, ct);
        return NoContent();
    }

    [HttpPost("{id:int}/upload-image")]
    [Authorize(Roles = "Admin,Leader")]
    public async Task<ActionResult<ApiResponse<string>>> UploadImage(
        int id, IFormFile file, CancellationToken ct = default)
    {
        var userId = User.GetUserId()!;
        var url = await _service.UploadImageAsync(id, file, userId, ct);
        return Ok(ApiResponse<string>.Ok(url, "Image uploaded successfully."));
    }

    /// <summary>
    /// Download a combined PDF for one or more codes.
    /// GET /api/profile-diagrams/download-pdf?codes=AS23PS01,AS23PS02
    /// </summary>
    [HttpGet("download-pdf")]
    public async Task<IActionResult> DownloadPdf(
        [FromQuery] string codes, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(codes))
            return BadRequest(ApiResponse<object>.Fail("codes parameter is required."));

        var codeList = codes
            .Split(new[] { ',', ' ', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(c => c.Trim())
            .Where(c => c.Length > 0)
            .ToList();

        var pdfBytes = await _service.DownloadCombinedPdfAsync(codeList, ct);

        var fileName = codeList.Count == 1
            ? $"profile-{codeList[0]}.pdf"
            : $"profiles-{DateTime.UtcNow:yyyyMMdd}.pdf";

        return File(pdfBytes, "application/pdf", fileName);
    }
}
