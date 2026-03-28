using HycoatApi.DTOs;
using HycoatApi.DTOs.Common;
using HycoatApi.DTOs.Sales;

namespace HycoatApi.Services.Sales;

public interface IInquiryService
{
    Task<PagedResponse<InquiryDto>> GetAllAsync(string? search, string? status, int? customerId,
        string? assignedToUserId, DateTime? fromDate, DateTime? toDate,
        int page, int pageSize, string sortBy, bool sortDesc);
    Task<InquiryDetailDto> GetByIdAsync(int id);
    Task<InquiryDto> CreateAsync(CreateInquiryDto dto, string userId);
    Task<InquiryDto> UpdateAsync(int id, UpdateInquiryDto dto, string userId);
    Task UpdateStatusAsync(int id, UpdateInquiryStatusDto dto, string userId);
    Task DeleteAsync(int id, string userId);
    Task<InquiryStatsDto> GetStatsAsync();
    Task<List<LookupDto>> GetLookupAsync();
}
