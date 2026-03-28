namespace HycoatApi.DTOs.Common;

public class FileAttachmentDto
{
    public int Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string StoredPath { get; set; } = string.Empty;
    public string? ContentType { get; set; }
    public long FileSizeBytes { get; set; }
    public DateTime UploadedAt { get; set; }
}
