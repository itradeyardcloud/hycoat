using HycoatApi.DTOs;
using HycoatApi.DTOs.Common;
using HycoatApi.DTOs.Sales;

namespace HycoatApi.Services.Sales;

public interface IProformaInvoiceService
{
    Task<PagedResponse<PIDto>> GetAllAsync(string? search, string? status, int? customerId,
        DateTime? fromDate, DateTime? toDate,
        int page, int pageSize, string sortBy, bool sortDesc);
    Task<PIDetailDto> GetByIdAsync(int id);
    Task<PIDto> CreateAsync(CreateProformaInvoiceDto dto, string userId);
    Task<PIDto> UpdateAsync(int id, CreateProformaInvoiceDto dto, string userId);
    Task UpdateStatusAsync(int id, UpdatePIStatusDto dto, string userId);
    Task DeleteAsync(int id, string userId);
    Task<byte[]> GeneratePdfAsync(int id);
    Task<CalculateAreaResponseDto> CalculateAreaAsync(CalculateAreaRequestDto dto);
    Task<List<LookupDto>> GetLookupAsync();
}
