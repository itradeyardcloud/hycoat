using HycoatApi.DTOs;
using HycoatApi.DTOs.Quality;

namespace HycoatApi.Services.Quality;

public interface ITestCertificateService
{
    Task<PagedResponse<TestCertificateDto>> GetAllAsync(
        string? search, DateTime? date, int? customerId,
        int page, int pageSize, string sortBy, bool sortDesc);
    Task<TestCertificateDetailDto> GetByIdAsync(int id);
    Task<TestCertificateDetailDto?> GetByWorkOrderAsync(int workOrderId);
    Task<TestCertificateDto> CreateAsync(CreateTestCertificateDto dto, string userId);
    Task<TestCertificateDto> UpdateAsync(int id, CreateTestCertificateDto dto, string userId);
    Task DeleteAsync(int id, string userId);
    Task<byte[]> GeneratePdfAsync(int id);
    Task<(byte[] FileBytes, string FileName)?> DownloadPdfAsync(int id);
}
