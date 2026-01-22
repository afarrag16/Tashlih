using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Tashlih.Application.DTOs.Notification;
using Tashlih.Application.Interfaces;
using Tashlih.Core.Entities;
using Tashlih.Infrastructure.Models;

namespace Tashlih.Infrastructure.Services;

public class NotificationService : INotificationService
{
    private readonly TashlihContext _context;
    private readonly IFirebasePushService _firebasePushService;     

    public NotificationService(TashlihContext context, IFirebasePushService firebasePushService)
    {
        _context = context;
        _firebasePushService = firebasePushService;
    }

    #region API Methods

    public async Task<NotificationsResponse> GetNotificationsAsync(long userId, string userType, int page = 1, int pageSize = 20)
    {
        // تحديد الـ UserId الصحيح
        var actualUserId = await GetActualUserIdAsync(userId, userType);
        if (actualUserId == 0)
        {
            return new NotificationsResponse
            {
                Success = false,
                Message = "User not found",
                MessageAr = "المستخدم غير موجود"
            };
        }

        var query = _context.Notifications
            .Where(n => n.UserId == actualUserId)
            .OrderByDescending(n => n.CreatedAt);

        var totalCount = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        var notifications = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var notificationDtos = notifications.Select(n => MapToDto(n)).ToList();

        var unreadCount = await _context.Notifications
            .CountAsync(n => n.UserId == actualUserId && !n.IsRead);

        return new NotificationsResponse
        {
            Success = true,
            Notifications = notificationDtos,
            UnreadCount = unreadCount,
            Pagination = new PaginationDto
            {
                CurrentPage = page,
                PageSize = pageSize,
                TotalPages = totalPages,
                TotalCount = totalCount,
                HasPrevious = page > 1,
                HasNext = page < totalPages
            }
        };
    }

    public async Task<UnreadCountResponse> GetUnreadCountAsync(long userId, string userType)
    {
        var actualUserId = await GetActualUserIdAsync(userId, userType);

        var count = await _context.Notifications
            .CountAsync(n => n.UserId == actualUserId && !n.IsRead);

        return new UnreadCountResponse
        {
            UnreadCount = count,
            Success = true
        };
    }

    public async Task<NotificationBaseResponse> MarkAsReadAsync(long userId, string userType, MarkAsReadRequest request)
    {
        var actualUserId = await GetActualUserIdAsync(userId, userType);

        IQueryable<Notification> query = _context.Notifications
            .Where(n => n.UserId == actualUserId && !n.IsRead);

        // لو فيه IDs محددة
        if (request.NotificationIds != null && request.NotificationIds.Any())
        {
            query = query.Where(n => request.NotificationIds.Contains(n.Id));
        }

        var notifications = await query.ToListAsync();

        foreach (var notification in notifications)
        {
            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        return new NotificationBaseResponse
        {
            Success = true,
            Message = $"{notifications.Count} notifications marked as read",
            MessageAr = $"تم قراءة {notifications.Count} إشعارات"
        };
    }

    public async Task<NotificationBaseResponse> DeleteNotificationAsync(long userId, string userType, long notificationId)
    {
        var actualUserId = await GetActualUserIdAsync(userId, userType);

        var notification = await _context.Notifications
            .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == actualUserId);

        if (notification == null)
        {
            return new NotificationBaseResponse
            {
                Success = false,
                Message = "Notification not found",
                MessageAr = "الإشعار غير موجود"
            };
        }

        _context.Notifications.Remove(notification);
        await _context.SaveChangesAsync();

        return new NotificationBaseResponse
        {
            Success = true,
            Message = "Notification deleted",
            MessageAr = "تم حذف الإشعار"
        };
    }

    public async Task<NotificationBaseResponse> DeleteAllNotificationsAsync(long userId, string userType)
    {
        var actualUserId = await GetActualUserIdAsync(userId, userType);

        var notifications = await _context.Notifications
            .Where(n => n.UserId == actualUserId)
            .ToListAsync();

        _context.Notifications.RemoveRange(notifications);
        await _context.SaveChangesAsync();

        return new NotificationBaseResponse
        {
            Success = true,
            Message = $"{notifications.Count} notifications deleted",
            MessageAr = $"تم حذف {notifications.Count} إشعارات"
        };
    }

    #endregion

    #region Internal Methods

    public async Task<NotificationDto?> CreateNotificationAsync(CreateNotificationDto dto)
    {
        var notification = new Notification
        {
            UserId = dto.UserId,
            UserType = dto.UserType,
            Type = dto.Type,
            Title = dto.TitleAr,  // نستخدم العربي كـ default
            Body = dto.BodyAr,
            ImageUrl = dto.ImageUrl,
            Data = dto.Data != null ? JsonSerializer.Serialize(dto.Data) : null,
            Priority = dto.Priority,
            IsRead = false,
            IsPushSent = false,
            CreatedAt = DateTime.UtcNow
        };

        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();

        // TODO: إرسال Push Notification عبر Firebase
        if (dto.SendPush)
        {
            var pushData = new Dictionary<string, string>
    {
        { "type", dto.Type },
        { "notificationId", notification.Id.ToString() }
    };

            if (dto.Data != null)
            {
                foreach (var item in dto.Data)
                {
                    pushData[item.Key] = item.Value?.ToString() ?? "";
                }
            }

            var pushSent = await _firebasePushService.SendToUserAsync(
                dto.UserId,
                dto.UserType,
                dto.TitleAr,
                dto.BodyAr,
                pushData
            );

            notification.IsPushSent = pushSent;
            notification.PushSentAt = pushSent ? DateTime.UtcNow : null;
            await _context.SaveChangesAsync();
        }

        return MapToDto(notification);
    }

    public async Task SendOrderNotificationAsync(long orderId, string notificationType)
    {
        var order = await _context.Orders
            .Include(o => o.Customer)
            .Include(o => o.Supplier)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Part)
            .FirstOrDefaultAsync(o => o.Id == orderId);

        if (order == null) return;

        var partName = order.OrderItems.FirstOrDefault()?.PartNameSnapshot ?? "قطعة";

        switch (notificationType)
        {
            case NotificationTypes.NewOrder:
                // إشعار للمورد
                await CreateNotificationAsync(new CreateNotificationDto
                {
                    UserId = order.SupplierId,
                    UserType = "supplier",
                    Type = NotificationTypes.NewOrder,
                    Title = "New Order",
                    TitleAr = "طلب جديد",
                    Body = $"You have a new order #{order.OrderNumber}",
                    BodyAr = $"لديك طلب جديد #{order.OrderNumber} - {partName}",
                    Data = new Dictionary<string, object>
                    {
                        { "orderId", order.Id },
                        { "orderNumber", order.OrderNumber ?? "" }
                    },
                    Priority = "high"
                });
                break;

            case NotificationTypes.OrderConfirmed:
            case NotificationTypes.OrderProcessing:
                // إشعار للعميل
                await CreateNotificationAsync(new CreateNotificationDto
                {
                    UserId = order.CustomerId,
                    UserType = "customer",
                    Type = notificationType,
                    Title = "Order Confirmed",
                    TitleAr = "تم تأكيد الطلب",
                    Body = $"Your order #{order.OrderNumber} has been confirmed",
                    BodyAr = $"تم تأكيد طلبك #{order.OrderNumber}",
                    Data = new Dictionary<string, object>
                    {
                        { "orderId", order.Id },
                        { "orderNumber", order.OrderNumber ?? "" }
                    }
                });
                break;

            case NotificationTypes.OrderCompleted:
                // إشعار للعميل
                await CreateNotificationAsync(new CreateNotificationDto
                {
                    UserId = order.CustomerId,
                    UserType = "customer",
                    Type = NotificationTypes.OrderCompleted,
                    Title = "Order Ready",
                    TitleAr = "الطلب جاهز",
                    Body = $"Your order #{order.OrderNumber} is ready",
                    BodyAr = $"طلبك #{order.OrderNumber} جاهز للاستلام",
                    Data = new Dictionary<string, object>
                    {
                        { "orderId", order.Id },
                        { "orderNumber", order.OrderNumber ?? "" }
                    },
                    Priority = "high"
                });
                break;

            case NotificationTypes.OrderReceived:
                // إشعار للمورد
                await CreateNotificationAsync(new CreateNotificationDto
                {
                    UserId = order.SupplierId,
                    UserType = "supplier",
                    Type = NotificationTypes.OrderReceived,
                    Title = "Order Received",
                    TitleAr = "تم استلام الطلب",
                    Body = $"Order #{order.OrderNumber} has been received",
                    BodyAr = $"تم استلام الطلب #{order.OrderNumber}",
                    Data = new Dictionary<string, object>
                    {
                        { "orderId", order.Id },
                        { "orderNumber", order.OrderNumber ?? "" }
                    }
                });
                break;

            case NotificationTypes.OrderCancelled:
                // إشعار للمورد
                await CreateNotificationAsync(new CreateNotificationDto
                {
                    UserId = order.SupplierId,
                    UserType = "supplier",
                    Type = NotificationTypes.OrderCancelled,
                    Title = "Order Cancelled",
                    TitleAr = "تم إلغاء الطلب",
                    Body = $"Order #{order.OrderNumber} has been cancelled",
                    BodyAr = $"تم إلغاء الطلب #{order.OrderNumber}",
                    Data = new Dictionary<string, object>
                    {
                        { "orderId", order.Id },
                        { "orderNumber", order.OrderNumber ?? "" }
                    }
                });
                break;

            case NotificationTypes.OrderRejected:
                // إشعار للعميل
                await CreateNotificationAsync(new CreateNotificationDto
                {
                    UserId = order.CustomerId,
                    UserType = "customer",
                    Type = NotificationTypes.OrderRejected,
                    Title = "Order Rejected",
                    TitleAr = "تم رفض الطلب",
                    Body = $"Your order #{order.OrderNumber} has been rejected",
                    BodyAr = $"للأسف تم رفض طلبك #{order.OrderNumber}",
                    Data = new Dictionary<string, object>
                    {
                        { "orderId", order.Id },
                        { "orderNumber", order.OrderNumber ?? "" }
                    }
                });
                break;
        }
    }

    public async Task SendChatNotificationAsync(long chatThreadId, long senderId, string senderName, string messagePreview)
    {



        var chat = await _context.ChatThreads
            .FirstOrDefaultAsync(c => c.Id == chatThreadId);

        if (chat == null) return;

        // حدد المستلم (اللي مش الـ sender)
        long recipientId;
        string recipientType;
        if (chat.CustomerId == senderId)
        {
            // المرسل عميل، الإشعار للمورد
            recipientId = chat.SupplierId;
            recipientType = "supplier";
        }
        else
        {
            // المرسل مورد، الإشعار للعميل
            recipientId = chat.CustomerId;
            recipientType = "customer";
        }

        // اقتطاع الرسالة لو طويلة
        var preview = messagePreview.Length > 50
            ? messagePreview.Substring(0, 50) + "..."
            : messagePreview;

        await CreateNotificationAsync(new CreateNotificationDto
        {
            UserId = recipientId,
            UserType = recipientType,
            Type = NotificationTypes.NewMessage,
            Title = "New Message",
            TitleAr = "رسالة جديدة",
            Body = $"Message from {senderName}",
            BodyAr = $"رسالة من {senderName}: {preview}",
            Data = new Dictionary<string, object>
            {
                { "chatThreadId", chatThreadId },
                { "senderId", senderId }
            },
            Priority = "high"
        });
    }

    public async Task SendReviewNotificationAsync(long reviewId, long supplierId, string customerName, int rating)
    {
        var stars = new string('⭐', rating);

        await CreateNotificationAsync(new CreateNotificationDto
        {
            UserId = supplierId,
            UserType = "supplier",
            Type = NotificationTypes.NewReview,
            Title = "New Review",
            TitleAr = "تقييم جديد",
            Body = $"You received a {rating}-star review",
            BodyAr = $"حصلت على تقييم {stars} من {customerName}",
            Data = new Dictionary<string, object>
            {
                { "reviewId", reviewId },
                { "rating", rating }
            },
            Priority = "high"
        });
    }

    #endregion

    #region Helper Methods

    private async Task<long> GetActualUserIdAsync(long userId, string userType)
    {
        if (userType == "supplier")
        {
            // المورد - الإشعارات مرتبطة بـ SupplierId مباشرة
            return userId;
        }
        return userId; // العميل - الـ userId هو نفسه
    }

    private static NotificationDto MapToDto(Notification n)
    {
        Dictionary<string, object>? data = null;
        string? actionType = null;
        long? actionId = null;

        if (!string.IsNullOrEmpty(n.Data))
        {
            try
            {
                data = JsonSerializer.Deserialize<Dictionary<string, object>>(n.Data);

                // استخراج الـ ActionType و ActionId
                if (n.Type.Contains("order"))
                {
                    actionType = "order";
                    if (data?.ContainsKey("orderId") == true)
                    {
                        var orderIdValue = data["orderId"];
                        if (orderIdValue is JsonElement jsonElement)
                        {
                            actionId = jsonElement.GetInt64();
                        }
                        else
                        {
                            actionId = Convert.ToInt64(orderIdValue);
                        }
                    }
                }
                else if (n.Type == NotificationTypes.NewMessage)
                {
                    actionType = "chat";
                    if (data?.ContainsKey("chatThreadId") == true)
                    {
                        var chatIdValue = data["chatThreadId"];
                        if (chatIdValue is JsonElement jsonElement)
                        {
                            actionId = jsonElement.GetInt64();
                        }
                        else
                        {
                            actionId = Convert.ToInt64(chatIdValue);
                        }
                    }
                }
                else if (n.Type == NotificationTypes.NewReview)
                {
                    actionType = "review";
                    if (data?.ContainsKey("reviewId") == true)
                    {
                        var reviewIdValue = data["reviewId"];
                        if (reviewIdValue is JsonElement jsonElement)
                        {
                            actionId = jsonElement.GetInt64();
                        }
                        else
                        {
                            actionId = Convert.ToInt64(reviewIdValue);
                        }
                    }
                }
            }
            catch { }
        }

        return new NotificationDto
        {
            Id = n.Id,
            Type = n.Type,
            Title = n.Title,
            Body = n.Body,
            ImageUrl = n.ImageUrl,
            Data = data,
            Priority = n.Priority,
            IsRead = n.IsRead,
            ReadAt = n.ReadAt,
            CreatedAt = n.CreatedAt,
            ActionType = actionType,
            ActionId = actionId
        };
    }

    #endregion
}