using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Tashlih.Application.DTOs.Notification;
using Tashlih.Application.Interfaces;

namespace Tashlih.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationsController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    /// <summary>
    /// جلب الإشعارات
    /// </summary>
    /// <param name="page">رقم الصفحة</param>
    /// <param name="pageSize">عدد العناصر في الصفحة</param>
    [HttpGet]
    public async Task<IActionResult> GetNotifications([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 20;
        if (pageSize > 50) pageSize = 50;

        var userId = GetUserId();
        var userType = GetUserType();

        var result = await _notificationService.GetNotificationsAsync(userId, userType, page, pageSize);
        return Ok(result);
    }

    /// <summary>
    /// جلب عدد الإشعارات غير المقروءة
    /// </summary>
    [HttpGet("unread-count")]
    public async Task<IActionResult> GetUnreadCount()
    {
        var userId = GetUserId();
        var userType = GetUserType();

        var result = await _notificationService.GetUnreadCountAsync(userId, userType);
        return Ok(result);
    }

    /// <summary>
    /// تحديد الإشعارات كمقروءة
    /// </summary>
    /// <param name="request">قائمة الـ IDs (اختياري - لو فاضي يقرأ الكل)</param>
    [HttpPut("mark-as-read")]
    public async Task<IActionResult> MarkAsRead([FromBody] MarkAsReadRequest? request)
    {
        var userId = GetUserId();
        var userType = GetUserType();

        var result = await _notificationService.MarkAsReadAsync(userId, userType, request ?? new MarkAsReadRequest());
        return Ok(result);
    }

    /// <summary>
    /// حذف إشعار
    /// </summary>
    /// <param name="id">رقم الإشعار</param>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteNotification(long id)
    {
        var userId = GetUserId();
        var userType = GetUserType();

        var result = await _notificationService.DeleteNotificationAsync(userId, userType, id);

        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }

    /// <summary>
    /// حذف جميع الإشعارات
    /// </summary>
    [HttpDelete]
    public async Task<IActionResult> DeleteAllNotifications()
    {
        var userId = GetUserId();
        var userType = GetUserType();

        var result = await _notificationService.DeleteAllNotificationsAsync(userId, userType);
        return Ok(result);
    }

    #region Helper Methods

    private long GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return long.TryParse(userIdClaim, out var id) ? id : 0;
    }

    private string GetUserType()
    {
        return User.FindFirst("user_type")?.Value ?? "customer";
    }

    #endregion
}