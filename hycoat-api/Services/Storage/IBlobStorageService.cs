namespace HycoatApi.Services.Storage;

public record BlobUploadResult(string BlobName, string BlobUrl, string ContentType, long SizeBytes);

public record BlobDownloadResult(Stream Stream, string ContentType);

public interface IBlobStorageService
{
    Task<BlobUploadResult> UploadAsync(IFormFile file, string blobName, CancellationToken cancellationToken = default);

    Task<BlobUploadResult> UploadBytesAsync(
        byte[] content,
        string contentType,
        string blobName,
        CancellationToken cancellationToken = default);

    Task<BlobDownloadResult?> DownloadAsync(string blobUrlOrName, CancellationToken cancellationToken = default);

    Task DeleteIfExistsAsync(string blobUrlOrName, CancellationToken cancellationToken = default);
}
