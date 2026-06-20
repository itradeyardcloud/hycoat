using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Options;

namespace HycoatApi.Services.Storage;

public class BlobStorageService : IBlobStorageService
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly AzureBlobStorageOptions _options;
    private readonly SemaphoreSlim _containerInitLock = new(1, 1);
    private bool _containerReady;

    public BlobStorageService(BlobServiceClient blobServiceClient, IOptions<AzureBlobStorageOptions> options)
    {
        _blobServiceClient = blobServiceClient;
        _options = options.Value;
    }

    public async Task<BlobUploadResult> UploadAsync(IFormFile file, string blobName, CancellationToken cancellationToken = default)
    {
        await using var stream = file.OpenReadStream();
        return await UploadStreamAsync(stream, file.ContentType ?? "application/octet-stream", file.Length, blobName, cancellationToken);
    }

    public async Task<BlobUploadResult> UploadBytesAsync(
        byte[] content,
        string contentType,
        string blobName,
        CancellationToken cancellationToken = default)
    {
        await using var stream = new MemoryStream(content, writable: false);
        return await UploadStreamAsync(stream, contentType, content.LongLength, blobName, cancellationToken);
    }

    public async Task<BlobDownloadResult?> DownloadAsync(string blobUrlOrName, CancellationToken cancellationToken = default)
    {
        var blobName = TryResolveBlobName(blobUrlOrName);
        if (string.IsNullOrWhiteSpace(blobName))
            return null;

        var containerClient = await GetContainerClientAsync(cancellationToken);
        var blobClient = containerClient.GetBlobClient(blobName);

        try
        {
            var response = await blobClient.DownloadStreamingAsync(cancellationToken: cancellationToken);
            var contentType = response.Value.Details.ContentType;
            return new BlobDownloadResult(response.Value.Content, string.IsNullOrWhiteSpace(contentType) ? "application/octet-stream" : contentType);
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            return null;
        }
    }

    public async Task DeleteIfExistsAsync(string blobUrlOrName, CancellationToken cancellationToken = default)
    {
        var blobName = TryResolveBlobName(blobUrlOrName);
        if (string.IsNullOrWhiteSpace(blobName))
            return;

        var containerClient = await GetContainerClientAsync(cancellationToken);
        var blobClient = containerClient.GetBlobClient(blobName);
        await blobClient.DeleteIfExistsAsync(cancellationToken: cancellationToken);
    }

    private async Task<BlobUploadResult> UploadStreamAsync(
        Stream content,
        string contentType,
        long sizeBytes,
        string blobName,
        CancellationToken cancellationToken)
    {
        var normalizedBlobName = NormalizeBlobName(blobName);
        var containerClient = await GetContainerClientAsync(cancellationToken);
        var blobClient = containerClient.GetBlobClient(normalizedBlobName);

        await blobClient.UploadAsync(
            content,
            new BlobUploadOptions
            {
                HttpHeaders = new BlobHttpHeaders { ContentType = contentType },
            },
            cancellationToken);

        return new BlobUploadResult(normalizedBlobName, blobClient.Uri.ToString(), contentType, sizeBytes);
    }

    private async Task<BlobContainerClient> GetContainerClientAsync(CancellationToken cancellationToken)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(_options.ContainerName);

        if (_containerReady || !_options.CreateContainerIfNotExists)
            return containerClient;

        await _containerInitLock.WaitAsync(cancellationToken);
        try
        {
            if (!_containerReady)
            {
                await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob, cancellationToken: cancellationToken);
                _containerReady = true;
            }
        }
        finally
        {
            _containerInitLock.Release();
        }

        return containerClient;
    }

    private string? TryResolveBlobName(string blobUrlOrName)
    {
        if (string.IsNullOrWhiteSpace(blobUrlOrName))
            return null;

        if (!Uri.TryCreate(blobUrlOrName, UriKind.Absolute, out var uri))
        {
            return NormalizeBlobName(blobUrlOrName);
        }

        var segments = uri.AbsolutePath.Trim('/').Split('/', 2, StringSplitOptions.RemoveEmptyEntries);
        if (segments.Length == 2 && string.Equals(segments[0], _options.ContainerName, StringComparison.OrdinalIgnoreCase))
            return NormalizeBlobName(segments[1]);

        if (segments.Length >= 1)
            return NormalizeBlobName(string.Join('/', segments.Skip(1)));

        return null;
    }

    private static string NormalizeBlobName(string blobName)
    {
        return blobName.Replace('\\', '/').TrimStart('/');
    }
}
