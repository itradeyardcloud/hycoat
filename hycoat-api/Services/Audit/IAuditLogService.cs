using HycoatApi.DTOs;
using HycoatApi.DTOs.Audit;

namespace HycoatApi.Services.Audit;

public interface IAuditLogService
{
    Task<PagedResponse<AuditLogDto>> GetAllAsync(
        string? entityName,
        string? userId,
        string? action,
        DateTime? dateFrom,
        DateTime? dateTo,
        string? search,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<PagedResponse<AuditLogDto>> GetByEntityAsync(
        string entityName,
        string entityId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
}
