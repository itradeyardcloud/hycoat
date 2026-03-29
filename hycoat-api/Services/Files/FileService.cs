using HycoatApi.Data;
using HycoatApi.DTOs.Files;
using HycoatApi.Models.Common;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;

namespace HycoatApi.Services.Files;

public class FileService : IFileService
{
    private static readonly string[] AllowedContentTypes =
    [
        "image/jpeg",
        "image/png",
        "image/webp",
        "application/pdf",
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        "application/acad",
        "image/vnd.dwg",
        "application/x-dwg",
        "application/dxf",
        "image/vnd.dxf"
    ];

    private readonly AppDbContext _db;
    private readonly IWebHostEnvironment _env;
    private readonly IConfiguration _configuration;
    private readonly FileExtensionContentTypeProvider _contentTypeProvider = new();

    public FileService(AppDbContext db, IWebHostEnvironment env, IConfiguration configuration)
    {
        _db = db;
        _env = env;
        _configuration = configuration;
    }

    public async Task<FileAttachmentDto> UploadAsync(
        IFormFile file,
        string entityType,
        int entityId,
        string userId,
        string? category = null,
        CancellationToken cancellationToken = default)
    {
        if (file == null || file.Length <= 0)
        {
            throw new ArgumentException("File is required.");
        }

        if (string.IsNullOrWhiteSpace(entityType))
        {
            throw new ArgumentException("Entity type is required.");
        }

        if (entityId <= 0)
        {
            throw new ArgumentException("Entity ID must be greater than zero.");
        }

        var maxSize = _configuration.GetValue<long?>("FileUpload:MaxFileSizeBytes") ?? 10 * 1024 * 1024;
        if (file.Length > maxSize)
        {
            throw new InvalidOperationException($"File size exceeds allowed limit ({maxSize / (1024 * 1024)} MB).");
        }

        var normalizedContentType = (file.ContentType ?? string.Empty).Trim().ToLowerInvariant();
        if (!AllowedContentTypes.Contains(normalizedContentType))
        {
            throw new InvalidOperationException("This file type is not allowed.");
        }

        var safeEntityType = SanitizePathSegment(entityType);
        var fileExtension = Path.GetExtension(file.FileName);
        var storedFileName = $"{Guid.NewGuid():N}{fileExtension}";

        var uploadRoot = _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot");
        var relativeFolder = Path.Combine("uploads", safeEntityType, entityId.ToString());
        var absoluteFolder = Path.Combine(uploadRoot, relativeFolder);
        Directory.CreateDirectory(absoluteFolder);

        var absolutePath = Path.Combine(absoluteFolder, storedFileName);

        await using (var stream = new FileStream(absolutePath, FileMode.CreateNew))
        {
            await file.CopyToAsync(stream, cancellationToken);
        }

        var relativePath = Path.Combine(relativeFolder, storedFileName)
            .Replace("\\", "/");

        var attachment = new FileAttachment
        {
            FileName = Path.GetFileName(file.FileName),
            StoredFileName = storedFileName,
            StoredPath = relativePath,
            ContentType = normalizedContentType,
            FileSizeBytes = file.Length,
            EntityType = entityType,
            EntityId = entityId,
            Category = category,
            UploadedByUserId = userId,
            UploadedBy = userId,
            UploadedAt = DateTime.UtcNow,
            CreatedBy = userId,
        };

        _db.FileAttachments.Add(attachment);
        await _db.SaveChangesAsync(cancellationToken);

        attachment = await _db.FileAttachments
            .AsNoTracking()
            .Include(x => x.UploadedByUser)
            .FirstAsync(x => x.Id == attachment.Id, cancellationToken);

        return MapDto(attachment);
    }

    public async Task<(Stream stream, string contentType, string fileName)> DownloadAsync(
        int fileId,
        CancellationToken cancellationToken = default)
    {
        var attachment = await _db.FileAttachments
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == fileId && !x.IsDeleted, cancellationToken)
            ?? throw new KeyNotFoundException($"File with ID {fileId} not found.");

        var absolutePath = BuildAbsolutePath(attachment.StoredPath);
        if (!System.IO.File.Exists(absolutePath))
        {
            throw new FileNotFoundException("Stored file does not exist on disk.", absolutePath);
        }

        var stream = new FileStream(absolutePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        var contentType = attachment.ContentType
            ?? (_contentTypeProvider.TryGetContentType(attachment.FileName, out var inferred) ? inferred : "application/octet-stream");

        return (stream, contentType, attachment.FileName);
    }

    public async Task DeleteAsync(
        int fileId,
        string userId,
        bool isAdmin,
        CancellationToken cancellationToken = default)
    {
        var attachment = await _db.FileAttachments
            .FirstOrDefaultAsync(x => x.Id == fileId && !x.IsDeleted, cancellationToken)
            ?? throw new KeyNotFoundException($"File with ID {fileId} not found.");

        var ownerId = attachment.UploadedByUserId ?? attachment.UploadedBy;
        if (!isAdmin && !string.Equals(ownerId, userId, StringComparison.OrdinalIgnoreCase))
        {
            throw new UnauthorizedAccessException("Only file owner or Admin can delete this file.");
        }

        attachment.IsDeleted = true;
        attachment.UpdatedBy = userId;
        attachment.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(cancellationToken);

        var absolutePath = BuildAbsolutePath(attachment.StoredPath);
        if (System.IO.File.Exists(absolutePath))
        {
            System.IO.File.Delete(absolutePath);
        }
    }

    public async Task<List<FileAttachmentDto>> GetByEntityAsync(
        string entityType,
        int entityId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(entityType))
        {
            throw new ArgumentException("Entity type is required.");
        }

        if (entityId <= 0)
        {
            throw new ArgumentException("Entity ID must be greater than zero.");
        }

        var files = await _db.FileAttachments
            .AsNoTracking()
            .Include(x => x.UploadedByUser)
            .Where(x => !x.IsDeleted && x.EntityType == entityType && x.EntityId == entityId)
            .OrderByDescending(x => x.UploadedAt)
            .ToListAsync(cancellationToken);

        return files.Select(MapDto).ToList();
    }

    private static string SanitizePathSegment(string value)
    {
        var cleaned = new string(value.Where(c => char.IsLetterOrDigit(c) || c is '-' or '_').ToArray());
        return string.IsNullOrWhiteSpace(cleaned) ? "entity" : cleaned;
    }

    private string BuildAbsolutePath(string storedPath)
    {
        var uploadRoot = _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot");
        var safePath = storedPath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
        return Path.Combine(uploadRoot, safePath);
    }

    private static FileAttachmentDto MapDto(FileAttachment file)
    {
        return new FileAttachmentDto
        {
            Id = file.Id,
            FileName = file.FileName,
            ContentType = file.ContentType ?? "application/octet-stream",
            FileSizeBytes = file.FileSizeBytes,
            EntityType = file.EntityType,
            EntityId = file.EntityId,
            Category = file.Category,
            UploadedByName = file.UploadedByUser?.FullName ?? file.UploadedBy,
            UploadedAt = file.UploadedAt,
            DownloadUrl = $"/api/files/{file.Id}"
        };
    }
}
