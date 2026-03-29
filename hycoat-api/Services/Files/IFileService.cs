using HycoatApi.DTOs.Files;

namespace HycoatApi.Services.Files;

public interface IFileService
{
    Task<FileAttachmentDto> UploadAsync(
        IFormFile file,
        string entityType,
        int entityId,
        string userId,
        string? category = null,
        CancellationToken cancellationToken = default);

    Task<(Stream stream, string contentType, string fileName)> DownloadAsync(
        int fileId,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        int fileId,
        string userId,
        bool isAdmin,
        CancellationToken cancellationToken = default);

    Task<List<FileAttachmentDto>> GetByEntityAsync(
        string entityType,
        int entityId,
        CancellationToken cancellationToken = default);
}
