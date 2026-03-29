using System.Text.Json;
using HycoatApi.Data;
using HycoatApi.DTOs;
using HycoatApi.DTOs.Audit;
using Microsoft.EntityFrameworkCore;

namespace HycoatApi.Services.Audit;

public class AuditLogService : IAuditLogService
{
    private readonly AppDbContext _db;

    public AuditLogService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<PagedResponse<AuditLogDto>> GetAllAsync(
        string? entityName,
        string? userId,
        string? action,
        DateTime? dateFrom,
        DateTime? dateTo,
        string? search,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = _db.AuditLogs
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(entityName))
        {
            var entityFilter = entityName.Trim();
            query = query.Where(x => x.EntityName == entityFilter);
        }

        if (!string.IsNullOrWhiteSpace(userId))
        {
            var userFilter = userId.Trim();
            query = query.Where(x => x.UserId == userFilter);
        }

        if (!string.IsNullOrWhiteSpace(action))
        {
            var actionFilter = action.Trim();
            query = query.Where(x => x.Action == actionFilter);
        }

        if (dateFrom.HasValue)
        {
            query = query.Where(x => x.Timestamp >= dateFrom.Value);
        }

        if (dateTo.HasValue)
        {
            query = query.Where(x => x.Timestamp <= dateTo.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(x =>
                x.EntityName.ToLower().Contains(term) ||
                (x.UserName != null && x.UserName.ToLower().Contains(term)) ||
                x.EntityId.ToLower().Contains(term));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var logs = await query
            .OrderByDescending(x => x.Timestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResponse<AuditLogDto>
        {
            Items = logs.Select(MapDto).ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<PagedResponse<AuditLogDto>> GetByEntityAsync(
        string entityName,
        string entityId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = _db.AuditLogs
            .AsNoTracking()
            .Where(x => x.EntityName == entityName && x.EntityId == entityId);

        var totalCount = await query.CountAsync(cancellationToken);

        var logs = await query
            .OrderByDescending(x => x.Timestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResponse<AuditLogDto>
        {
            Items = logs.Select(MapDto).ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    private static AuditLogDto MapDto(Models.Common.AuditLog log)
    {
        return new AuditLogDto
        {
            Id = log.Id,
            EntityName = log.EntityName,
            EntityId = log.EntityId,
            Action = log.Action,
            UserName = log.UserName,
            Timestamp = log.Timestamp,
            OldValues = DeserializeToDictionary(log.OldValues),
            NewValues = DeserializeToDictionary(log.NewValues),
            ChangedColumns = log.ChangedColumns,
        };
    }

    private static Dictionary<string, object?>? DeserializeToDictionary(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<Dictionary<string, object?>>(json);
        }
        catch
        {
            return new Dictionary<string, object?> { ["raw"] = json };
        }
    }
}
