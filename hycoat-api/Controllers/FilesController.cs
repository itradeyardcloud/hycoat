using HycoatApi.DTOs;
using HycoatApi.DTOs.Files;
using HycoatApi.Helpers;
using HycoatApi.Services.Files;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HycoatApi.Controllers;

[ApiController]
[Route("api/files")]
[Authorize]
public class FilesController : ControllerBase
{
    private readonly IFileService _fileService;

    public FilesController(IFileService fileService)
    {
        _fileService = fileService;
    }

    [HttpPost("upload")]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<ActionResult<ApiResponse<FileAttachmentDto>>> Upload(
        [FromForm] UploadFileDto dto,
        CancellationToken cancellationToken = default)
    {
        var userId = User.GetUserId()!;
        var result = await _fileService.UploadAsync(
            dto.File,
            dto.EntityType,
            dto.EntityId,
            userId,
            dto.Category,
            cancellationToken);

        return Ok(ApiResponse<FileAttachmentDto>.Ok(result, "File uploaded successfully."));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Download(int id, CancellationToken cancellationToken = default)
    {
        var (stream, contentType, fileName) = await _fileService.DownloadAsync(id, cancellationToken);
        return File(stream, contentType, fileName);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken = default)
    {
        var userId = User.GetUserId()!;
        await _fileService.DeleteAsync(id, userId, User.IsAdmin(), cancellationToken);
        return NoContent();
    }

    [HttpGet("entity/{entityType}/{entityId:int}")]
    public async Task<ActionResult<ApiResponse<List<FileAttachmentDto>>>> GetByEntity(
        string entityType,
        int entityId,
        CancellationToken cancellationToken = default)
    {
        var result = await _fileService.GetByEntityAsync(entityType, entityId, cancellationToken);
        return Ok(ApiResponse<List<FileAttachmentDto>>.Ok(result));
    }
}
