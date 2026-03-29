using HycoatApi.DTOs;
using HycoatApi.DTOs.Purchase;

namespace HycoatApi.Services.Purchase;

public interface IPowderIndentService
{
    Task<PagedResponse<PowderIndentDto>> GetAllAsync(
        string? search, string? status, DateTime? dateFrom, DateTime? dateTo,
        int page, int pageSize, string sortBy, bool sortDesc);
    Task<PowderIndentDetailDto> GetByIdAsync(int id);
    Task<PowderIndentDto> CreateAsync(CreatePowderIndentDto dto, string userId);
    Task<PowderIndentDto> UpdateAsync(int id, CreatePowderIndentDto dto, string userId);
    Task DeleteAsync(int id, string userId);
    Task<PowderIndentDto> UpdateStatusAsync(int id, UpdatePowderIndentStatusDto dto, string userId);
}
