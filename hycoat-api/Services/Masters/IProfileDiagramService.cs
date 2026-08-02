using HycoatApi.DTOs;
using HycoatApi.DTOs.Masters;
using Microsoft.AspNetCore.Http;

namespace HycoatApi.Services.Masters;

public interface IProfileDiagramService
{
    Task<PagedResponse<ProfileDiagramDto>> GetAllAsync(
        string? search, int page, int pageSize, string sortBy, bool sortDesc,
        CancellationToken ct = default);

    Task<ProfileDiagramDetailDto> GetByIdAsync(int id, CancellationToken ct = default);

    Task<List<ProfileDiagramDto>> GetByCodesAsync(
        IEnumerable<string> codes, CancellationToken ct = default);

    Task<ProfileDiagramDto> CreateAsync(
        CreateProfileDiagramDto dto, string userId, CancellationToken ct = default);

    Task<ProfileDiagramDto> UpdateAsync(
        int id, UpdateProfileDiagramDto dto, string userId, CancellationToken ct = default);

    Task DeleteAsync(int id, string userId, CancellationToken ct = default);

    Task<string> UploadImageAsync(
        int id, IFormFile file, string userId, CancellationToken ct = default);

    Task<byte[]> DownloadCombinedPdfAsync(
        IEnumerable<string> codes, CancellationToken ct = default);
}
