using Microsoft.AspNetCore.Http;

namespace Tashlih.Application.DTOs.Order;

#region Requests

/// <summary>
/// طلب إنشاء طلب جديد
/// </summary>
public class CreateOrderRequest
{
    public long PartId { get; set; }
    public int Quantity { get; set; } = 1;
    public string? CustomerNotes { get; set; }
}

/// <summary>
/// طلب تغيير حالة الطلب (للمورد)
/// </summary>
public class UpdateOrderStatusRequest
{
    public string Status { get; set; } = null!;  // processing, completed
    public string? SupplierNotes { get; set; }
}

/// <summary>
/// طلب إلغاء الطلب (للعميل)
/// </summary>
public class CancelOrderRequest
{
    public string? CancelReason { get; set; }
}

/// <summary>
/// طلب رفض الطلب (للمورد)
/// </summary>
public class RejectOrderRequest
{
    public string? RejectReason { get; set; }
}

#endregion

#region Responses

/// <summary>
/// استجابة عامة للطلبات
/// </summary>
public class OrderResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public string? MessageAr { get; set; }
}

/// <summary>
/// استجابة إنشاء طلب
/// </summary>
public class CreateOrderResponse : OrderResponse
{
    public OrderDto? Order { get; set; }
}

/// <summary>
/// استجابة قائمة الطلبات
/// </summary>
public class OrdersListResponse : OrderResponse
{
    public List<OrderListDto>? Orders { get; set; }
    public PaginationInfo? Pagination { get; set; }
}

/// <summary>
/// استجابة تفاصيل الطلب
/// </summary>
public class OrderDetailsResponse : OrderResponse
{
    public OrderDto? Order { get; set; }
}

/// <summary>
/// معلومات الـ Pagination
/// </summary>
public class PaginationInfo
{
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public int TotalCount { get; set; }
    public bool HasPrevious => CurrentPage > 1;
    public bool HasNext => CurrentPage < TotalPages;
}

#endregion

#region DTOs

/// <summary>
/// بيانات الطلب للقائمة
/// </summary>
public class OrderListDto
{
    public long Id { get; set; }
    public string OrderNumber { get; set; } = null!;

    // بيانات الطرف الآخر
    public long OtherPartyId { get; set; }
    public string? OtherPartyName { get; set; }
    public string? OtherPartyImage { get; set; }

    // بيانات القطعة
    public string? PartName { get; set; }
    public string? PartImage { get; set; }
    public int Quantity { get; set; }

    // السعر
    public decimal TotalAmount { get; set; }
    public string Currency { get; set; } = "SAR";

    // الحالة
    public string Status { get; set; } = null!;
    public string StatusAr { get; set; } = null!;

    // التاريخ
    public DateTime? CreatedAt { get; set; }
}

/// <summary>
/// بيانات الطلب التفصيلية
/// </summary>
public class OrderDto
{
    public long Id { get; set; }
    public string OrderNumber { get; set; } = null!;

    // بيانات العميل
    public long CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public string? CustomerPhone { get; set; }
    public string? CustomerAvatar { get; set; }

    // بيانات المورد
    public long SupplierId { get; set; }
    public string? SupplierName { get; set; }
    public string? SupplierPhone { get; set; }
    public string? SupplierLogo { get; set; }

    // بيانات القطعة
    public OrderItemDto? Item { get; set; }

    // السعر
    public decimal Subtotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string Currency { get; set; } = "SAR";

    // الحالة
    public string Status { get; set; } = null!;
    public string StatusAr { get; set; } = null!;

    // الملاحظات
    public string? CustomerNotes { get; set; }
    public string? SupplierNotes { get; set; }
    public string? CancelReason { get; set; }

    // التواريخ
    public DateTime? CreatedAt { get; set; }
    public DateTime? ConfirmedAt { get; set; }
    public DateTime? ProcessingAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? ReceivedAt { get; set; }
    public DateTime? CancelledAt { get; set; }

    // الأزرار المتاحة
    public List<string> AvailableActions { get; set; } = new();
}

/// <summary>
/// بيانات عنصر الطلب (القطعة)
/// </summary>
public class OrderItemDto
{
    public long Id { get; set; }
    public long? PartId { get; set; }
    public string PartName { get; set; } = null!;
    public string? PartNumber { get; set; }
    public string Condition { get; set; } = null!;
    public string? ImageUrl { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
    public int? WarrantyDays { get; set; }
    public string? Notes { get; set; }
}

#endregion

#region Enums

/// <summary>
/// حالات الطلب
/// </summary>
public static class OrderStatus
{
    public const string Pending = "pending";           // في الانتظار
    public const string Processing = "processing";     // جاري التجهيز
    public const string Completed = "completed";       // تم التوصيل
    public const string Received = "received";         // مكتمل (العميل أكد)
    public const string Rejected = "rejected";         // مرفوض
    public const string Cancelled = "cancelled";       // ملغي

    public static string ToArabic(string status)
    {
        return status switch
        {
            Pending => "في الانتظار",
            Processing => "جاري التجهيز",
            Completed => "تم التوصيل",
            Received => "مكتمل",
            Rejected => "مرفوض",
            Cancelled => "ملغي",
            _ => status
        };
    }

    public static bool IsValidStatus(string status)
    {
        return status is Pending or Processing or Completed or Received or Rejected or Cancelled;
    }

    public static bool CanSupplierChangeTo(string currentStatus, string newStatus)
    {
        return (currentStatus, newStatus) switch
        {
            (Pending, Processing) => true,      // تأكيد الطلب
            (Processing, Completed) => true,    // تم التوصيل
            _ => false
        };
    }
}

#endregion
