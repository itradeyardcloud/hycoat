using HycoatApi.Data;
using HycoatApi.Models.Common;
using HycoatApi.Models.Masters;
using HycoatApi.Models.Production;
using HycoatApi.Models.Quality;
using HycoatApi.Models.Sales;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;

namespace HycoatApi.Services.Storage;

public record BlobBackfillSummary(
    int Scanned,
    int Migrated,
    int MissingLocalFiles,
    int AlreadyBlob,
    int UnsupportedPaths,
    bool DryRun);

public class BlobBackfillService
{
    private readonly AppDbContext _db;
    private readonly IWebHostEnvironment _env;
    private readonly IBlobStorageService _blobStorageService;
    private readonly ILogger<BlobBackfillService> _logger;
    private readonly FileExtensionContentTypeProvider _contentTypeProvider = new();

    public BlobBackfillService(
        AppDbContext db,
        IWebHostEnvironment env,
        IBlobStorageService blobStorageService,
        ILogger<BlobBackfillService> logger)
    {
        _db = db;
        _env = env;
        _blobStorageService = blobStorageService;
        _logger = logger;
    }

    public async Task<BlobBackfillSummary> RunAsync(bool dryRun, CancellationToken cancellationToken = default)
    {
        var scanned = 0;
        var migrated = 0;
        var missingLocalFiles = 0;
        var alreadyBlob = 0;
        var unsupportedPaths = 0;

        var uploadCache = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        var attachments = await _db.FileAttachments
            .Where(x => !x.IsDeleted && !string.IsNullOrWhiteSpace(x.StoredPath))
            .ToListAsync(cancellationToken);
        foreach (var entity in attachments)
        {
            scanned++;
            await BackfillPathAsync(
                entity.StoredPath,
                entity.ContentType,
                (newPath) => entity.StoredPath = newPath,
                dryRun,
                uploadCache,
                onMigrated: () => migrated++,
                onMissing: () => missingLocalFiles++,
                onBlob: () => alreadyBlob++,
                onUnsupported: () => unsupportedPaths++,
                cancellationToken);
        }

        var drawings = await _db.SectionProfiles
            .Where(x => !x.IsDeleted && !string.IsNullOrWhiteSpace(x.DrawingFileUrl))
            .ToListAsync(cancellationToken);
        foreach (var entity in drawings)
        {
            scanned++;
            await BackfillPathAsync(
                entity.DrawingFileUrl!,
                null,
                (newPath) => entity.DrawingFileUrl = newPath,
                dryRun,
                uploadCache,
                onMigrated: () => migrated++,
                onMissing: () => missingLocalFiles++,
                onBlob: () => alreadyBlob++,
                onUnsupported: () => unsupportedPaths++,
                cancellationToken);
        }

        var productionPhotos = await _db.ProductionPhotos
            .Where(x => !string.IsNullOrWhiteSpace(x.PhotoUrl))
            .ToListAsync(cancellationToken);
        foreach (var entity in productionPhotos)
        {
            scanned++;
            await BackfillPathAsync(
                entity.PhotoUrl,
                null,
                (newPath) => entity.PhotoUrl = newPath,
                dryRun,
                uploadCache,
                onMigrated: () => migrated++,
                onMissing: () => missingLocalFiles++,
                onBlob: () => alreadyBlob++,
                onUnsupported: () => unsupportedPaths++,
                cancellationToken);
        }

        var quotations = await _db.Quotations
            .Where(x => !x.IsDeleted && !string.IsNullOrWhiteSpace(x.FileUrl))
            .ToListAsync(cancellationToken);
        foreach (var entity in quotations)
        {
            scanned++;
            await BackfillPathAsync(
                entity.FileUrl!,
                "application/pdf",
                (newPath) => entity.FileUrl = newPath,
                dryRun,
                uploadCache,
                onMigrated: () => migrated++,
                onMissing: () => missingLocalFiles++,
                onBlob: () => alreadyBlob++,
                onUnsupported: () => unsupportedPaths++,
                cancellationToken);
        }

        var proformaInvoices = await _db.ProformaInvoices
            .Where(x => !x.IsDeleted && !string.IsNullOrWhiteSpace(x.FileUrl))
            .ToListAsync(cancellationToken);
        foreach (var entity in proformaInvoices)
        {
            scanned++;
            await BackfillPathAsync(
                entity.FileUrl!,
                "application/pdf",
                (newPath) => entity.FileUrl = newPath,
                dryRun,
                uploadCache,
                onMigrated: () => migrated++,
                onMissing: () => missingLocalFiles++,
                onBlob: () => alreadyBlob++,
                onUnsupported: () => unsupportedPaths++,
                cancellationToken);
        }

        var testCertificates = await _db.TestCertificates
            .Where(x => !x.IsDeleted && !string.IsNullOrWhiteSpace(x.FileUrl))
            .ToListAsync(cancellationToken);
        foreach (var entity in testCertificates)
        {
            scanned++;
            await BackfillPathAsync(
                entity.FileUrl!,
                "application/pdf",
                (newPath) => entity.FileUrl = newPath,
                dryRun,
                uploadCache,
                onMigrated: () => migrated++,
                onMissing: () => missingLocalFiles++,
                onBlob: () => alreadyBlob++,
                onUnsupported: () => unsupportedPaths++,
                cancellationToken);
        }

        if (!dryRun && migrated > 0)
        {
            await _db.SaveChangesAsync(cancellationToken);
        }

        return new BlobBackfillSummary(scanned, migrated, missingLocalFiles, alreadyBlob, unsupportedPaths, dryRun);
    }

    private async Task BackfillPathAsync(
        string existingPath,
        string? declaredContentType,
        Action<string> applyNewPath,
        bool dryRun,
        Dictionary<string, string> uploadCache,
        Action onMigrated,
        Action onMissing,
        Action onBlob,
        Action onUnsupported,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(existingPath))
        {
            onUnsupported();
            return;
        }

        if (IsAbsoluteUrl(existingPath))
        {
            onBlob();
            return;
        }

        if (!TryGetUploadRelativePath(existingPath, out var relativeUploadPath))
        {
            onUnsupported();
            return;
        }

        if (uploadCache.TryGetValue(relativeUploadPath, out var cachedUrl))
        {
            if (!dryRun)
            {
                applyNewPath(cachedUrl);
            }
            onMigrated();
            return;
        }

        var localFilePath = BuildLocalFilePath(relativeUploadPath);
        if (!File.Exists(localFilePath))
        {
            _logger.LogWarning("Backfill skipped (missing local file): {Path}", localFilePath);
            onMissing();
            return;
        }

        if (dryRun)
        {
            onMigrated();
            return;
        }

        var bytes = await File.ReadAllBytesAsync(localFilePath, cancellationToken);
        var contentType = ResolveContentType(localFilePath, declaredContentType);
        var upload = await _blobStorageService.UploadBytesAsync(bytes, contentType, relativeUploadPath, cancellationToken);

        uploadCache[relativeUploadPath] = upload.BlobUrl;
        applyNewPath(upload.BlobUrl);
        onMigrated();
    }

    private static bool IsAbsoluteUrl(string value)
    {
        return Uri.TryCreate(value, UriKind.Absolute, out _);
    }

    private static bool TryGetUploadRelativePath(string value, out string relativeUploadPath)
    {
        var normalized = value.Trim().Replace('\\', '/').TrimStart('/');
        if (!normalized.StartsWith("uploads/", StringComparison.OrdinalIgnoreCase))
        {
            relativeUploadPath = string.Empty;
            return false;
        }

        relativeUploadPath = normalized;
        return true;
    }

    private string BuildLocalFilePath(string relativeUploadPath)
    {
        var webRoot = _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot");
        return Path.Combine(webRoot, relativeUploadPath.Replace('/', Path.DirectorySeparatorChar));
    }

    private string ResolveContentType(string localFilePath, string? declaredContentType)
    {
        if (!string.IsNullOrWhiteSpace(declaredContentType))
        {
            return declaredContentType;
        }

        if (_contentTypeProvider.TryGetContentType(localFilePath, out var inferred))
        {
            return inferred;
        }

        return "application/octet-stream";
    }
}