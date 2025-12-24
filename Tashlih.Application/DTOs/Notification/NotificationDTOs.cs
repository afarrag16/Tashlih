namespace Tashlih.Application.DTOs.Notification;

// ========== Response DTOs ==========

public class NotificationDto
{
    public long Id { get; set; }
    public string Type { get; set; } = null!;
    public string Title { get; set; } = null!;
    public string Body { get; set; } = null!;
    public string? ImageUrl { get; set; }
    public Dictionary<string, object>? Data { get; set; }
    public string Priority { get; set; } = null!;
    public bool IsRead { get; set; }
    public DateTime? ReadAt { get; set; }
    public DateTime? CreatedAt { get; set; }

    // للتنقل السريع
    public string? ActionType { get; set; }  // order, chat, review, system
    public long? ActionId { get; set; }       // OrderId, ChatId, etc.
}

public class NotificationsResponse
{
    public List<NotificationDto> Notifications { get; set; } = new();
    public int UnreadCount { get; set; }
    public PaginationDto? Pagination { get; set; }
    public bool Success { get; set; }
    public string? Message { get; set; }
    public string? MessageAr { get; set; }
}

public class NotificationResponse
{
    public NotificationDto? Notification { get; set; }
    public bool Success { get; set; }
    public string? Message { get; set; }
    public string? MessageAr { get; set; }
}

public class UnreadCountResponse
{
    public int UnreadCount { get; set; }
    public bool Success { get; set; }
}

public class NotificationBaseResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public string? MessageAr { get; set; }
}

public class PaginationDto
{
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public int TotalCount { get; set; }
    public bool HasPrevious { get; set; }
    public bool HasNext { get; set; }
}

// ========== Request DTOs ==========

public class MarkAsReadRequest
{
    public List<long>? NotificationIds { get; set; }  // null = mark all as read
}

// ========== Internal DTOs (للاستخدام الداخلي) ==========

public class CreateNotificationDto
{
    public long UserId { get; set; }
    public string UserType { get; set; } = "customer";
    public string Type { get; set; } = null!;        // order, chat, review, system
    public string Title { get; set; } = null!;
    public string TitleAr { get; set; } = null!;
    public string Body { get; set; } = null!;
    public string BodyAr { get; set; } = null!;
    public string? ImageUrl { get; set; }
    public Dictionary<string, object>? Data { get; set; }
    public string Priority { get; set; } = "normal"; // low, normal, high
    public bool SendPush { get; set; } = true;
}

// ========== Notification Types ==========

public static class NotificationTypes
{
    // Orders
    public const string NewOrder = "new_order";
    public const string OrderConfirmed = "order_confirmed";
    public const string OrderProcessing = "order_processing";
    public const string OrderCompleted = "order_completed";
    public const string OrderReceived = "order_received";
    public const string OrderCancelled = "order_cancelled";
    public const string OrderRejected = "order_rejected";

    // Chat
    public const string NewMessage = "new_message";

    // Reviews
    public const string NewReview = "new_review";

    // System
    public const string System = "system";
    public const string Promotion = "promotion";
}