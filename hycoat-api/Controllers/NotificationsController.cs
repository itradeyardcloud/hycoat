using HycoatApi.DTOs;
using HycoatApi.DTOs.Notifications;
using HycoatApi.Helpers;
using HycoatApi.Services.Notifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HycoatApi.Controllers;

[ApiController]
[Route("api/notifications")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationsController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResponse<NotificationDto>>>> GetAll(
        [FromQuery] bool? isRead,
        [FromQuery] string? category,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var userId = User.GetUserId()!;
        var result = await _notificationService.GetForUserAsync(
            userId,
            isRead,
            category,
            page,
            pageSize,
            cancellationToken);

        return Ok(ApiResponse<PagedResponse<NotificationDto>>.Ok(result));
    }

    [HttpGet("unread-count")]
    public async Task<ActionResult<ApiResponse<int>>> GetUnreadCount(CancellationToken cancellationToken = default)
    {
        var userId = User.GetUserId()!;
        var count = await _notificationService.GetUnreadCountAsync(userId, cancellationToken);
        return Ok(ApiResponse<int>.Ok(count));
    }

    [HttpPatch("{id:int}/read")]
    public async Task<ActionResult<ApiResponse<object>>> MarkAsRead(int id, CancellationToken cancellationToken = default)
    {
        var userId = User.GetUserId()!;
        await _notificationService.MarkAsReadAsync(id, userId, cancellationToken);
        return Ok(ApiResponse<object>.Ok(null!, "Notification marked as read."));
    }

    [HttpPost("read-all")]
    public async Task<ActionResult<ApiResponse<object>>> MarkAllAsRead(CancellationToken cancellationToken = default)
    {
        var userId = User.GetUserId()!;
        await _notificationService.MarkAllAsReadAsync(userId, cancellationToken);
        return Ok(ApiResponse<object>.Ok(null!, "All notifications marked as read."));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken = default)
    {
        var userId = User.GetUserId()!;
        await _notificationService.DeleteAsync(id, userId, cancellationToken);
        return NoContent();
    }
}
