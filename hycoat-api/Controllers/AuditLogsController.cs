using HycoatApi.DTOs;
using HycoatApi.DTOs.Audit;
using HycoatApi.Services.Audit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HycoatApi.Controllers;

[ApiController]
[Route("api/audit-logs")]
[Authorize]
public class AuditLogsController : ControllerBase
{
    private readonly IAuditLogService _auditLogService;

    public AuditLogsController(IAuditLogService auditLogService)
    {
        _auditLogService = auditLogService;
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<PagedResponse<AuditLogDto>>>> GetAll(
        [FromQuery] string? entityName,
        [FromQuery] string? userId,
        [FromQuery] string? action,
        [FromQuery] DateTime? dateFrom,
        [FromQuery] DateTime? dateTo,
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _auditLogService.GetAllAsync(
            entityName,
            userId,
            action,
            dateFrom,
            dateTo,
            search,
            page,
            pageSize,
            cancellationToken);

        return Ok(ApiResponse<PagedResponse<AuditLogDto>>.Ok(result));
    }

    [HttpGet("entity/{entityName}/{entityId}")]
    [Authorize(Roles = "Admin,Leader")]
    public async Task<ActionResult<ApiResponse<PagedResponse<AuditLogDto>>>> GetByEntity(
        string entityName,
        string entityId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _auditLogService.GetByEntityAsync(
            entityName,
            entityId,
            page,
            pageSize,
            cancellationToken);

        return Ok(ApiResponse<PagedResponse<AuditLogDto>>.Ok(result));
    }
}
