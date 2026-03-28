using HycoatApi.DTOs.Masters;

namespace HycoatApi.Services.Masters;

public interface IProcessTypeService
{
    Task<List<ProcessTypeDto>> GetAllAsync();
    Task<ProcessTypeDto> GetByIdAsync(int id);
    Task<ProcessTypeDto> CreateAsync(CreateProcessTypeDto dto, string userId);
    Task<ProcessTypeDto> UpdateAsync(int id, UpdateProcessTypeDto dto, string userId);
    Task DeleteAsync(int id, string userId);
}
