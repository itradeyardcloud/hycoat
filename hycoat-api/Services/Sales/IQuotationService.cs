using HycoatApi.DTOs;
using HycoatApi.DTOs.Common;
using HycoatApi.DTOs.Sales;

namespace HycoatApi.Services.Sales;

public interface IQuotationService
{
    Task<PagedResponse<QuotationDto>> GetAllAsync(string? search, string? status, int? customerId,
        int? inquiryId, DateTime? fromDate, DateTime? toDate,
        int page, int pageSize, string sortBy, bool sortDesc);
    Task<QuotationDetailDto> GetByIdAsync(int id);
    Task<QuotationDto> CreateAsync(CreateQuotationDto dto, string userId);
    Task<QuotationDto> UpdateAsync(int id, CreateQuotationDto dto, string userId);
    Task UpdateStatusAsync(int id, UpdateQuotationStatusDto dto, string userId);
    Task DeleteAsync(int id, string userId);
    Task<byte[]> GeneratePdfAsync(int id);
    Task<List<LookupDto>> GetLookupAsync();
}
