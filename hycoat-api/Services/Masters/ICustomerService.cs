using HycoatApi.DTOs;
using HycoatApi.DTOs.Common;
using HycoatApi.DTOs.Masters;

namespace HycoatApi.Services.Masters;

public interface ICustomerService
{
    Task<PagedResponse<CustomerDto>> GetAllAsync(string? search, int page, int pageSize, string sortBy, bool sortDesc);
    Task<CustomerDetailDto> GetByIdAsync(int id);
    Task<CustomerDto> CreateAsync(CreateCustomerDto dto, string userId);
    Task<CustomerDto> UpdateAsync(int id, UpdateCustomerDto dto, string userId);
    Task DeleteAsync(int id, string userId);
    Task<List<LookupDto>> GetLookupAsync();
}
