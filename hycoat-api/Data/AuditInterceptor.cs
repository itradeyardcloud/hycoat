using System.Text.Json;
using HycoatApi.Models.Common;
using HycoatApi.Services.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace HycoatApi.Data;

public class AuditInterceptor : SaveChangesInterceptor
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuditInterceptor(
        ICurrentUserService currentUserService,
        IHttpContextAccessor httpContextAccessor)
    {
        _currentUserService = currentUserService;
        _httpContextAccessor = httpContextAccessor;
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not AppDbContext context)
        {
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        var entries = context.ChangeTracker.Entries()
            .Where(e => e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            .Where(e => e.Entity is not AuditLog)
            .ToList();

        foreach (var entry in entries)
        {
            var log = new AuditLog
            {
                EntityName = entry.Metadata.ClrType.Name,
                EntityId = GetPrimaryKeyValue(entry),
                Action = MapAction(entry.State),
                UserId = _currentUserService.UserId,
                UserName = _currentUserService.UserName,
                Timestamp = DateTime.UtcNow,
                IpAddress = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString(),
            };

            if (entry.State == EntityState.Modified)
            {
                var changedProps = entry.Properties
                    .Where(p => p.IsModified && !p.Metadata.IsPrimaryKey())
                    .ToList();

                if (changedProps.Count == 0)
                {
                    continue;
                }

                log.OldValues = JsonSerializer.Serialize(
                    changedProps.ToDictionary(p => p.Metadata.Name, p => p.OriginalValue));
                log.NewValues = JsonSerializer.Serialize(
                    changedProps.ToDictionary(p => p.Metadata.Name, p => p.CurrentValue));
                log.ChangedColumns = string.Join(',', changedProps.Select(p => p.Metadata.Name));
            }
            else if (entry.State == EntityState.Added)
            {
                log.NewValues = JsonSerializer.Serialize(
                    entry.Properties.ToDictionary(p => p.Metadata.Name, p => p.CurrentValue));
            }
            else if (entry.State == EntityState.Deleted)
            {
                log.OldValues = JsonSerializer.Serialize(
                    entry.Properties.ToDictionary(p => p.Metadata.Name, p => p.OriginalValue));
            }

            context.AuditLogs.Add(log);
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private static string GetPrimaryKeyValue(EntityEntry entry)
    {
        var primaryKey = entry.Properties.FirstOrDefault(p => p.Metadata.IsPrimaryKey());
        if (primaryKey == null)
        {
            return string.Empty;
        }

        var value = entry.State == EntityState.Deleted
            ? primaryKey.OriginalValue
            : primaryKey.CurrentValue;

        if (primaryKey.IsTemporary)
        {
            return "(temporary)";
        }

        return value?.ToString() ?? string.Empty;
    }

    private static string MapAction(EntityState state)
    {
        return state switch
        {
            EntityState.Added => "Create",
            EntityState.Modified => "Update",
            EntityState.Deleted => "Delete",
            _ => "Update"
        };
    }
}
