using HycoatApi.DTOs;
using HycoatApi.DTOs.Notifications;

namespace HycoatApi.Services.Notifications;

public interface INotificationService
{
    Task<PagedResponse<NotificationDto>> GetForUserAsync(
        string userId,
        bool? isRead,
        string? category,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<int> GetUnreadCountAsync(string userId, CancellationToken cancellationToken = default);

    Task MarkAsReadAsync(int id, string userId, CancellationToken cancellationToken = default);

    Task MarkAllAsReadAsync(string userId, CancellationToken cancellationToken = default);

    Task DeleteAsync(int id, string userId, CancellationToken cancellationToken = default);

    Task<NotificationDto> NotifyAsync(
        string userId,
        string title,
        string message,
        string type,
        string category,
        int? referenceId = null,
        string? referenceType = null,
        CancellationToken cancellationToken = default);

    Task<int> NotifyRoleAsync(
        string role,
        string title,
        string message,
        string type,
        string category,
        int? referenceId = null,
        string? referenceType = null,
        CancellationToken cancellationToken = default);

    Task<int> NotifyDepartmentAsync(
        string department,
        string title,
        string message,
        string type,
        string category,
        int? referenceId = null,
        string? referenceType = null,
        CancellationToken cancellationToken = default);
}
