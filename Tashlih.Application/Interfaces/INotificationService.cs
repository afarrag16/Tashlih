using Tashlih.Application.DTOs.Notification;

namespace Tashlih.Application.Interfaces;

public interface INotificationService
{
    // ========== للمستخدم (API) ==========
    Task<NotificationsResponse> GetNotificationsAsync(long userId, string userType, int page = 1, int pageSize = 20);
    Task<UnreadCountResponse> GetUnreadCountAsync(long userId, string userType);
    Task<NotificationBaseResponse> MarkAsReadAsync(long userId, string userType, MarkAsReadRequest request);
    Task<NotificationBaseResponse> DeleteNotificationAsync(long userId, string userType, long notificationId);
    Task<NotificationBaseResponse> DeleteAllNotificationsAsync(long userId, string userType);

    // ========== للنظام (Internal) ==========
    Task<NotificationDto?> CreateNotificationAsync(CreateNotificationDto dto);
    Task SendOrderNotificationAsync(long orderId, string notificationType);
    Task SendChatNotificationAsync(long chatThreadId, long senderId, string senderName, string senderType, string messagePreview);
    Task SendReviewNotificationAsync(long reviewId, long supplierId, string customerName, int rating);
}