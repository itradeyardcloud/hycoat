using HycoatApi.Data;
using HycoatApi.DTOs;
using HycoatApi.DTOs.Notifications;
using HycoatApi.Hubs;
using HycoatApi.Models.Common;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace HycoatApi.Services.Notifications;

public class NotificationService : INotificationService
{
    private readonly AppDbContext _db;
    private readonly IHubContext<NotificationHub> _hubContext;

    public NotificationService(AppDbContext db, IHubContext<NotificationHub> hubContext)
    {
        _db = db;
        _hubContext = hubContext;
    }

    public async Task<PagedResponse<NotificationDto>> GetForUserAsync(
        string userId,
        bool? isRead,
        string? category,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = _db.Notifications
            .AsNoTracking()
            .Where(n => n.UserId == userId && !n.IsDeleted)
            .AsQueryable();

        if (isRead.HasValue)
        {
            query = query.Where(n => n.IsRead == isRead.Value);
        }

        if (!string.IsNullOrWhiteSpace(category))
        {
            var filter = category.Trim();
            query = query.Where(n => n.Category == filter);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(n => n.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(n => MapDto(n))
            .ToListAsync(cancellationToken);

        return new PagedResponse<NotificationDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public Task<int> GetUnreadCountAsync(string userId, CancellationToken cancellationToken = default)
    {
        return _db.Notifications
            .AsNoTracking()
            .CountAsync(n => n.UserId == userId && !n.IsDeleted && !n.IsRead, cancellationToken);
    }

    public async Task MarkAsReadAsync(int id, string userId, CancellationToken cancellationToken = default)
    {
        var notification = await _db.Notifications
            .FirstOrDefaultAsync(n => n.Id == id && !n.IsDeleted, cancellationToken)
            ?? throw new KeyNotFoundException($"Notification with ID {id} not found.");

        if (notification.UserId != userId)
        {
            throw new UnauthorizedAccessException("You are not allowed to modify this notification.");
        }

        if (notification.IsRead)
        {
            return;
        }

        notification.IsRead = true;
        notification.ReadAt = DateTime.UtcNow;
        notification.UpdatedBy = userId;
        notification.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task MarkAllAsReadAsync(string userId, CancellationToken cancellationToken = default)
    {
        var unread = await _db.Notifications
            .Where(n => n.UserId == userId && !n.IsDeleted && !n.IsRead)
            .ToListAsync(cancellationToken);

        if (unread.Count == 0)
        {
            return;
        }

        var now = DateTime.UtcNow;
        foreach (var item in unread)
        {
            item.IsRead = true;
            item.ReadAt = now;
            item.UpdatedBy = userId;
            item.UpdatedAt = now;
        }

        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(int id, string userId, CancellationToken cancellationToken = default)
    {
        var notification = await _db.Notifications
            .FirstOrDefaultAsync(n => n.Id == id && !n.IsDeleted, cancellationToken)
            ?? throw new KeyNotFoundException($"Notification with ID {id} not found.");

        if (notification.UserId != userId)
        {
            throw new UnauthorizedAccessException("You are not allowed to delete this notification.");
        }

        notification.IsDeleted = true;
        notification.UpdatedBy = userId;
        notification.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task<NotificationDto> NotifyAsync(
        string userId,
        string title,
        string message,
        string type,
        string category,
        int? referenceId = null,
        string? referenceType = null,
        CancellationToken cancellationToken = default)
    {
        await EnforceUnreadLimitAsync(userId, cancellationToken);

        var notification = new Notification
        {
            UserId = userId,
            Title = title,
            Message = message,
            Type = type,
            Category = category,
            ReferenceId = referenceId,
            ReferenceType = referenceType,
            IsRead = false,
            CreatedBy = userId,
        };

        _db.Notifications.Add(notification);
        await _db.SaveChangesAsync(cancellationToken);

        var dto = MapDto(notification);
        await _hubContext.Clients.Group(GetUserGroup(userId))
            .SendAsync("ReceiveNotification", dto, cancellationToken);

        return dto;
    }

    public async Task<int> NotifyRoleAsync(
        string role,
        string title,
        string message,
        string type,
        string category,
        int? referenceId = null,
        string? referenceType = null,
        CancellationToken cancellationToken = default)
    {
        var userIds = await (
            from user in _db.Users
            join userRole in _db.UserRoles on user.Id equals userRole.UserId
            join appRole in _db.Roles on userRole.RoleId equals appRole.Id
            where user.IsActive && appRole.Name == role
            select user.Id)
            .Distinct()
            .ToListAsync(cancellationToken);

        foreach (var userId in userIds)
        {
            await NotifyAsync(userId, title, message, type, category, referenceId, referenceType, cancellationToken);
        }

        return userIds.Count;
    }

    public async Task<int> NotifyDepartmentAsync(
        string department,
        string title,
        string message,
        string type,
        string category,
        int? referenceId = null,
        string? referenceType = null,
        CancellationToken cancellationToken = default)
    {
        var userIds = await _db.Users
            .Where(u => u.IsActive && u.Department == department)
            .Select(u => u.Id)
            .ToListAsync(cancellationToken);

        foreach (var userId in userIds)
        {
            await NotifyAsync(userId, title, message, type, category, referenceId, referenceType, cancellationToken);
        }

        return userIds.Count;
    }

    private async Task EnforceUnreadLimitAsync(string userId, CancellationToken cancellationToken)
    {
        var unreadCount = await _db.Notifications
            .CountAsync(n => n.UserId == userId && !n.IsDeleted && !n.IsRead, cancellationToken);

        if (unreadCount < 500)
        {
            return;
        }

        var overflowCount = unreadCount - 499;
        var now = DateTime.UtcNow;

        var oldestUnread = await _db.Notifications
            .Where(n => n.UserId == userId && !n.IsDeleted && !n.IsRead)
            .OrderBy(n => n.CreatedAt)
            .Take(overflowCount)
            .ToListAsync(cancellationToken);

        foreach (var notification in oldestUnread)
        {
            notification.IsRead = true;
            notification.ReadAt = now;
            notification.UpdatedAt = now;
            notification.UpdatedBy = userId;
        }
    }

    private static NotificationDto MapDto(Notification n)
    {
        return new NotificationDto
        {
            Id = n.Id,
            Title = n.Title,
            Message = n.Message,
            Type = n.Type,
            Category = n.Category,
            ReferenceId = n.ReferenceId,
            ReferenceType = n.ReferenceType,
            IsRead = n.IsRead,
            CreatedAt = n.CreatedAt
        };
    }

    private static string GetUserGroup(string userId) => $"user-{userId}";
}
