using HycoatApi.DTOs;
using HycoatApi.DTOs.Common;
using HycoatApi.DTOs.Masters;

namespace HycoatApi.Services.Masters;

public interface ISectionProfileService
{
    Task<PagedResponse<SectionProfileDto>> GetAllAsync(string? search, int page, int pageSize, string sortBy, bool sortDesc);
    Task<SectionProfileDetailDto> GetByIdAsync(int id);
    Task<SectionProfileDto> CreateAsync(CreateSectionProfileDto dto, string userId);
    Task<SectionProfileDto> UpdateAsync(int id, UpdateSectionProfileDto dto, string userId);
    Task DeleteAsync(int id, string userId);
    Task<List<SectionProfileLookupDto>> GetLookupAsync();
    Task<string> UploadDrawingAsync(int id, IFormFile file, string userId);
}
