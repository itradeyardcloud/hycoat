using HycoatApi.DTOs;
using HycoatApi.DTOs.Common;
using HycoatApi.DTOs.Masters;

namespace HycoatApi.Services.Masters;

public interface IVendorService
{
    Task<PagedResponse<VendorDto>> GetAllAsync(string? search, string? vendorType, int page, int pageSize, string sortBy, bool sortDesc);
    Task<VendorDetailDto> GetByIdAsync(int id);
    Task<VendorDto> CreateAsync(CreateVendorDto dto, string userId);
    Task<VendorDto> UpdateAsync(int id, UpdateVendorDto dto, string userId);
    Task DeleteAsync(int id, string userId);
    Task<List<LookupDto>> GetLookupAsync();
}
