using HycoatApi.DTOs;
using HycoatApi.DTOs.Common;
using HycoatApi.DTOs.Masters;

namespace HycoatApi.Services.Masters;

public interface IPowderColorService
{
    Task<PagedResponse<PowderColorDto>> GetAllAsync(string? search, int? vendorId, int page, int pageSize, string sortBy, bool sortDesc);
    Task<PowderColorDetailDto> GetByIdAsync(int id);
    Task<PowderColorDto> CreateAsync(CreatePowderColorDto dto, string userId);
    Task<PowderColorDto> UpdateAsync(int id, UpdatePowderColorDto dto, string userId);
    Task DeleteAsync(int id, string userId);
    Task<List<PowderColorLookupDto>> GetLookupAsync();
}
