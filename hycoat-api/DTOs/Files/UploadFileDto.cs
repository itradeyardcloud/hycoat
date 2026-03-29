using Microsoft.AspNetCore.Mvc;

namespace HycoatApi.DTOs.Files;

public class UploadFileDto
{
    [FromForm(Name = "file")]
    public IFormFile File { get; set; } = null!;

    [FromForm(Name = "entityType")]
    public string EntityType { get; set; } = string.Empty;

    [FromForm(Name = "entityId")]
    public int EntityId { get; set; }

    [FromForm(Name = "category")]
    public string? Category { get; set; }
}
