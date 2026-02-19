using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Tashlih.Application.DTOs.Notification;
using Tashlih.Application.DTOs.Order;
using Tashlih.Application.Interfaces;
using Tashlih.Core.Entities;
using Tashlih.Infrastructure.Models;

namespace Tashlih.Infrastructure.Services;

public class OrderService : IOrderService
{
    private readonly TashlihContext _context;
    private readonly IOrderHubService _orderHubService;
    private readonly INotificationService _notificationService;
    private readonly string _baseUrl;
    private readonly ILogService _logService;
    public OrderService(TashlihContext context, IOrderHubService orderHubService, INotificationService notificationService, IConfiguration configuration, ILogService logService)
    {
        _context = context;
        _orderHubService = orderHubService;
        _notificationService = notificationService;
        _baseUrl = configuration["AppSettings:BaseUrl"] ?? "";
        _logService = logService;
    }

    #region Customer Methods

    /// <summary>
    /// إنشاء طلب جديد (للعميل)
    /// </summary>
    public async Task<CreateOrderResponse> CreateOrderAsync(long customerId, CreateOrderRequest request)
    {
        // التحقق من وجود القطعة
        var part = await _context.Parts
            .Include(p => p.Supplier)
            .Include(p => p.PartImages.Where(i => i.IsPrimary))
            .FirstOrDefaultAsync(p => p.Id == request.PartId && p.Status == "available" && p.DeletedAt == null);

        if (part == null)
        {
            return new CreateOrderResponse
            {
                Success = false,
                Message = "Part not found or not available",
                MessageAr = "القطعة غير موجودة أو غير متاحة"
            };
        }

        // التحقق من الكمية
        if (request.Quantity < 1)
        {
            return new CreateOrderResponse
            {
                Success = false,
                Message = "Quantity must be at least 1",
                MessageAr = "الكمية يجب أن تكون 1 على الأقل"
            };
        }

        // التحقق من توفر الكمية
        if (part.Quantity < request.Quantity)
        {
            return new CreateOrderResponse
            {
                Success = false,
                Message = $"Requested quantity not available. Available: {part.Quantity}",
                MessageAr = $"الكمية المطلوبة غير متوفرة. المتاح: {part.Quantity}"
            };
        }

        // حساب السعر
        var unitPrice = part.Price;
        var subtotal = unitPrice * request.Quantity;
        var totalAmount = subtotal; // بدون خصم حالياً

        // إنشاء رقم الطلب
        var orderNumber = GenerateOrderNumber();

        // إنشاء الطلب
        var order = new Order
        {
            OrderNumber = orderNumber,
            CustomerId = customerId,
            SupplierId = part.SupplierId,
            Subtotal = subtotal,
            DiscountAmount = 0,
            TotalAmount = totalAmount,
            Currency = "SAR",
            Status = OrderStatus.Pending,
            CustomerNotes = request.CustomerNotes,
            IsReviewed = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        // إنشاء عنصر الطلب
        var orderItem = new OrderItem
        {
            OrderId = order.Id,
            PartId = part.Id,
            PartNameSnapshot = part.NameAr,
            PartNumberSnapshot = part.PartNumber,
            ConditionSnapshot = part.Condition ?? "used",
            ImageUrlSnapshot = part.PartImages.FirstOrDefault()?.ImageUrl,
            Quantity = request.Quantity,
            UnitPrice = unitPrice,
            TotalPrice = subtotal,
            WarrantyDaysSnapshot = part.WarrantyDays,
            CreatedAt = DateTime.UtcNow
        };

        _context.OrderItems.Add(orderItem);
        await _context.SaveChangesAsync();
        // إرسال إشعار للمورد
        await _notificationService.SendOrderNotificationAsync(order.Id, NotificationTypes.NewOrder);

        // جلب بيانات العميل والمورد
        var customer = await _context.Users.FindAsync(customerId);
        var supplier = part.Supplier;

        // إرسال إشعار للمورد
        await _orderHubService.SendNewOrderAsync(part.SupplierId, new
        {
            OrderId = order.Id,
            OrderNumber = order.OrderNumber,
            CustomerId = customerId,
            CustomerName = customer?.FullName,
            PartName = part.NameAr,
            Quantity = request.Quantity,
            TotalAmount = totalAmount,
            CreatedAt = order.CreatedAt
        });

        // إرجاع الاستجابة
        return new CreateOrderResponse
        {
            Success = true,
            Message = "Order created successfully",
            MessageAr = "تم إنشاء الطلب بنجاح",
            Order = MapToOrderDto(order, orderItem, customer, supplier, "customer")
        };
    }

    /// <summary>
    /// جلب طلبات العميل
    /// </summary>
    public async Task<OrdersListResponse> GetCustomerOrdersAsync(long customerId, string? status = null, int page = 1, int pageSize = 20)
    {
        var query = _context.Orders
            .AsNoTracking()
            .Include(o => o.Supplier)
            .Include(o => o.OrderItems)
            .Where(o => o.CustomerId == customerId);

        // فلترة بالحالة
        if (!string.IsNullOrEmpty(status))
        {
            if (status == "incomplete")
            {
                query = query.Where(o => o.Status != OrderStatus.Received &&
                                         o.Status != OrderStatus.Cancelled &&
                                         o.Status != OrderStatus.Rejected);
            }
            else if (status == "complete")
            {
                query = query.Where(o => o.Status == OrderStatus.Received);
            }
            else
            {
                query = query.Where(o => o.Status == status);
            }
        }

        // حساب الإجمالي
        var totalCount = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        // جلب الطلبات
        var orders = await query
            .OrderByDescending(o => o.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var orderDtos = orders.Select(o => new OrderListDto
        {
            Id = o.Id,
            OrderNumber = o.OrderNumber,
            OtherPartyId = o.SupplierId,
            OtherPartyName = o.Supplier?.BusinessNameAr,
            OtherPartyImage = GetFullUrl(o.Supplier?.LogoUrl),  // ✅
            PartName = o.OrderItems.FirstOrDefault()?.PartNameSnapshot,
            PartImage = GetFullUrl(o.OrderItems.FirstOrDefault()?.ImageUrlSnapshot),  // ✅
            Quantity = o.OrderItems.FirstOrDefault()?.Quantity ?? 0,
            TotalAmount = o.TotalAmount,
            Currency = o.Currency,
            Status = o.Status,
            StatusAr = OrderStatus.ToArabic(o.Status),
            CreatedAt = o.CreatedAt
        }).ToList();

        return new OrdersListResponse
        {
            Success = true,
            Orders = orderDtos,
            Pagination = new PaginationInfo
            {
                CurrentPage = page,
                PageSize = pageSize,
                TotalPages = totalPages,
                TotalCount = totalCount
            }
        };
    }

    /// <summary>
    /// جلب تفاصيل طلب (للعميل)
    /// </summary>
    public async Task<OrderDetailsResponse> GetCustomerOrderDetailsAsync(long customerId, long orderId)
    {
        var order = await _context.Orders
            .AsNoTracking()
            .Include(o => o.Customer)
            .Include(o => o.Supplier)
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.Id == orderId && o.CustomerId == customerId);

        if (order == null)
        {
            return new OrderDetailsResponse
            {
                Success = false,
                Message = "Order not found",
                MessageAr = "الطلب غير موجود"
            };
        }

        var orderItem = order.OrderItems.FirstOrDefault();

        return new OrderDetailsResponse
        {
            Success = true,
            Order = MapToOrderDto(order, orderItem, order.Customer, order.Supplier, "customer")
        };
    }

    /// <summary>
    /// تأكيد استلام الطلب (للعميل)
    /// </summary>
    public async Task<OrderResponse> CompleteOrderAsync(long customerId, long orderId)
    {
        var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == orderId && o.CustomerId == customerId);

        if (order == null)
        {
            return new OrderResponse
            {
                Success = false,
                Message = "Order not found",
                MessageAr = "الطلب غير موجود"
            };
        }

        // التحقق من الحالة
        if (order.Status != OrderStatus.Completed)
        {
            return new OrderResponse
            {
                Success = false,
                Message = "Order cannot be completed. Current status: " + order.Status,
                MessageAr = "لا يمكن تأكيد استلام الطلب. الحالة الحالية: " + OrderStatus.ToArabic(order.Status)
            };
        }

        // تحديث الحالة
        order.Status = OrderStatus.Received;
        order.ReadyAt = DateTime.UtcNow;  // نستخدم ReadyAt للـ ReceivedAt
        order.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        // إرسال إشعار للمورد ✅
        await _notificationService.SendOrderNotificationAsync(order.Id, NotificationTypes.OrderReceived);

        // إرسال إشعار للمورد
        await _orderHubService.SendOrderCompletedAsync(order.SupplierId, new
        {
            OrderId = order.Id,
            OrderNumber = order.OrderNumber,
            Status = order.Status,
            StatusAr = OrderStatus.ToArabic(order.Status),
            ReceivedAt = order.ReadyAt
        });

        return new OrderResponse
        {
            Success = true,
            Message = "Order completed successfully",
            MessageAr = "تم تأكيد استلام الطلب بنجاح"
        };
    }

    /// <summary>
    /// إلغاء الطلب (للعميل)
    /// </summary>
    public async Task<OrderResponse> CancelOrderAsync(long customerId, long orderId, CancelOrderRequest request)
    {
        var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == orderId && o.CustomerId == customerId);
        if (order == null)
        {
            return new OrderResponse
            {
                Success = false,
                Message = "Order not found",
                MessageAr = "الطلب غير موجود"
            };
        }

        // التحقق من الحالة - العميل يقدر يلغي بس لو pending
        if (order.Status != OrderStatus.Pending)
        {
            return new OrderResponse
            {
                Success = false,
                Message = "Order cannot be cancelled. Current status: " + order.Status,
                MessageAr = "لا يمكن إلغاء الطلب. الحالة الحالية: " + OrderStatus.ToArabic(order.Status)
            };
        }

        var oldStatus = order.Status;

        // تحديث الحالة
        order.Status = OrderStatus.Cancelled;
        order.CancelledAt = DateTime.UtcNow;
        order.CancelledBy = "customer";
        order.CancelReason = request.CancelReason;
        order.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        // ✅ تسجيل العملية
        var customer = await _context.Users.FindAsync(customerId);
        await _logService.LogAsync(
            userId: customerId,
            userType: "customer",
            userName: customer?.FullName ?? "عميل",
            action: "cancel",
            actionAr: "إلغاء",
            entityType: "order",
            entityTypeAr: "طلب",
            entityId: order.Id,
            oldValues: new { Status = oldStatus },
            newValues: new { Status = order.Status, CancelReason = request.CancelReason },
            description: $"تم إلغاء الطلب رقم {order.OrderNumber} بواسطة العميل"
        );

        // إرسال إشعار للمورد ✅
        await _notificationService.SendOrderNotificationAsync(order.Id, NotificationTypes.OrderCancelled);

        // إرسال إشعار للمورد
        await _orderHubService.SendOrderCancelledAsync(order.SupplierId, new
        {
            OrderId = order.Id,
            OrderNumber = order.OrderNumber,
            CancelledBy = "customer",
            CancelReason = request.CancelReason,
            CancelledAt = order.CancelledAt
        });

        return new OrderResponse
        {
            Success = true,
            Message = "Order cancelled successfully",
            MessageAr = "تم إلغاء الطلب بنجاح"
        };
    }

    #endregion

    #region Supplier Methods

    /// <summary>
    /// جلب طلبات المورد
    /// </summary>
    public async Task<OrdersListResponse> GetSupplierOrdersAsync(long supplierId, string? status = null, int page = 1, int pageSize = 20)
    {
        var query = _context.Orders
            .AsNoTracking()
            .Include(o => o.Customer)
            .Include(o => o.OrderItems)
            .Where(o => o.SupplierId == supplierId);

        // فلترة بالحالة
        if (!string.IsNullOrEmpty(status))
        {
            if (status == "incomplete")
            {
                query = query.Where(o => o.Status != OrderStatus.Received &&
                                         o.Status != OrderStatus.Cancelled &&
                                         o.Status != OrderStatus.Rejected);
            }
            else if (status == "complete")
            {
                query = query.Where(o => o.Status == OrderStatus.Received);
            }
            else
            {
                query = query.Where(o => o.Status == status);
            }
        }

        // حساب الإجمالي
        var totalCount = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        // جلب الطلبات
        var orders = await query
            .OrderByDescending(o => o.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var orderDtos = orders.Select(o => new OrderListDto
        {
            Id = o.Id,
            OrderNumber = o.OrderNumber,
            OtherPartyId = o.CustomerId,
            OtherPartyName = o.Customer?.FullName,
            OtherPartyImage = GetFullUrl(o.Customer?.AvatarUrl),  // ✅
            PartName = o.OrderItems.FirstOrDefault()?.PartNameSnapshot,
            PartImage = GetFullUrl(o.OrderItems.FirstOrDefault()?.ImageUrlSnapshot),  // ✅
            Quantity = o.OrderItems.FirstOrDefault()?.Quantity ?? 0,
            TotalAmount = o.TotalAmount,
            Currency = o.Currency,
            Status = o.Status,
            StatusAr = OrderStatus.ToArabic(o.Status),
            CreatedAt = o.CreatedAt
        }).ToList();

        return new OrdersListResponse
        {
            Success = true,
            Orders = orderDtos,
            Pagination = new PaginationInfo
            {
                CurrentPage = page,
                PageSize = pageSize,
                TotalPages = totalPages,
                TotalCount = totalCount
            }
        };
    }

    /// <summary>
    /// جلب تفاصيل طلب (للمورد)
    /// </summary>
    public async Task<OrderDetailsResponse> GetSupplierOrderDetailsAsync(long supplierId, long orderId)
    {
        var order = await _context.Orders
            .AsNoTracking()
            .Include(o => o.Customer)
            .Include(o => o.Supplier)
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.Id == orderId && o.SupplierId == supplierId);

        if (order == null)
        {
            return new OrderDetailsResponse
            {
                Success = false,
                Message = "Order not found",
                MessageAr = "الطلب غير موجود"
            };
        }

        var orderItem = order.OrderItems.FirstOrDefault();

        return new OrderDetailsResponse
        {
            Success = true,
            Order = MapToOrderDto(order, orderItem, order.Customer, order.Supplier, "supplier")
        };
    }

    /// <summary>
    /// تغيير حالة الطلب (للمورد)
    /// </summary>
    public async Task<OrderResponse> UpdateOrderStatusAsync(long supplierId, long orderId, UpdateOrderStatusRequest request)
    {
        var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == orderId && o.SupplierId == supplierId);

        if (order == null)
        {
            return new OrderResponse
            {
                Success = false,
                Message = "Order not found",
                MessageAr = "الطلب غير موجود"
            };
        }

        // التحقق من صحة الحالة الجديدة
        if (!OrderStatus.CanSupplierChangeTo(order.Status, request.Status))
        {
            return new OrderResponse
            {
                Success = false,
                Message = $"Cannot change status from '{order.Status}' to '{request.Status}'",
                MessageAr = $"لا يمكن تغيير الحالة من '{OrderStatus.ToArabic(order.Status)}' إلى '{OrderStatus.ToArabic(request.Status)}'"
            };
        }

        // تحديث الحالة والتاريخ المناسب
        order.Status = request.Status;
        order.UpdatedAt = DateTime.UtcNow;

        if (!string.IsNullOrEmpty(request.SupplierNotes))
        {
            order.SupplierNotes = request.SupplierNotes;
        }

        switch (request.Status)
        {
            case OrderStatus.Processing:
                order.ProcessingAt = DateTime.UtcNow;
                order.ConfirmedAt = DateTime.UtcNow;
                break;
            case OrderStatus.Completed:
                order.CompletedAt = DateTime.UtcNow;
                break;
        }

        await _context.SaveChangesAsync();

        // إرسال إشعار للعميل ✅
        if (request.Status == OrderStatus.Processing)
        {
            await _notificationService.SendOrderNotificationAsync(order.Id, NotificationTypes.OrderProcessing);
        }
        else if (request.Status == OrderStatus.Completed)
        {
            await _notificationService.SendOrderNotificationAsync(order.Id, NotificationTypes.OrderCompleted);
        }

        // إرسال إشعار للعميل
        await _orderHubService.SendOrderStatusUpdatedAsync(order.CustomerId, new
        {
            OrderId = order.Id,
            OrderNumber = order.OrderNumber,
            Status = order.Status,
            StatusAr = OrderStatus.ToArabic(order.Status),
            UpdatedAt = order.UpdatedAt
        });

        return new OrderResponse
        {
            Success = true,
            Message = "Order status updated successfully",
            MessageAr = "تم تحديث حالة الطلب بنجاح"
        };
    }

    /// <summary>
    /// رفض الطلب (للمورد)
    /// </summary>
    public async Task<OrderResponse> RejectOrderAsync(long supplierId, long orderId, RejectOrderRequest request)
    {
        var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == orderId && o.SupplierId == supplierId);
        if (order == null)
        {
            return new OrderResponse
            {
                Success = false,
                Message = "Order not found",
                MessageAr = "الطلب غير موجود"
            };
        }

        // التحقق من الحالة - المورد يقدر يرفض بس لو pending
        if (order.Status != OrderStatus.Pending)
        {
            return new OrderResponse
            {
                Success = false,
                Message = "Order cannot be rejected. Current status: " + order.Status,
                MessageAr = "لا يمكن رفض الطلب. الحالة الحالية: " + OrderStatus.ToArabic(order.Status)
            };
        }

        var oldStatus = order.Status;

        // تحديث الحالة
        order.Status = OrderStatus.Rejected;
        order.CancelledAt = DateTime.UtcNow;
        order.CancelledBy = "supplier";
        order.CancelReason = request.RejectReason;
        order.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        // ✅ تسجيل العملية
        var supplier = await _context.SupplierProfiles.FindAsync(supplierId);
        await _logService.LogAsync(
            userId: supplierId,
            userType: "supplier",
            userName: supplier?.FullName ?? "مورد",
            action: "reject",
            actionAr: "رفض",
            entityType: "order",
            entityTypeAr: "طلب",
            entityId: order.Id,
            oldValues: new { Status = oldStatus },
            newValues: new { Status = order.Status, RejectReason = request.RejectReason },
            description: $"تم رفض الطلب رقم {order.OrderNumber} بواسطة المورد"
        );

        // إرسال إشعار للعميل ✅
        await _notificationService.SendOrderNotificationAsync(order.Id, NotificationTypes.OrderRejected);

        // إرسال إشعار للعميل
        await _orderHubService.SendOrderRejectedAsync(order.CustomerId, new
        {
            OrderId = order.Id,
            OrderNumber = order.OrderNumber,
            RejectReason = request.RejectReason,
            RejectedAt = order.CancelledAt
        });

        return new OrderResponse
        {
            Success = true,
            Message = "Order rejected successfully",
            MessageAr = "تم رفض الطلب بنجاح"
        };
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// توليد رقم الطلب
    /// </summary>
    private string GenerateOrderNumber()
    {
        var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        var random = new Random().Next(1000, 9999);
        return $"ORD-{timestamp}-{random}";
    }

    private string? GetFullUrl(string? path)
    {
        if (string.IsNullOrEmpty(path)) return null;
        if (path.StartsWith("http")) return path;
        return _baseUrl + path;
    }

    /// <summary>
    /// تحويل الطلب إلى DTO
    /// </summary>
    private OrderDto MapToOrderDto(Order order, OrderItem? item, User? customer, SupplierProfile? supplier, string viewerType)
    {
        var dto = new OrderDto
        {
            Id = order.Id,
            OrderNumber = order.OrderNumber,
            CustomerId = order.CustomerId,
            CustomerName = customer?.FullName,
            CustomerPhone = customer?.Phone,
            CustomerAvatar = GetFullUrl(customer?.AvatarUrl),  // ✅
            SupplierId = order.SupplierId,
            SupplierName = supplier?.BusinessNameAr,
            SupplierPhone = supplier?.Phone,
            SupplierLogo = GetFullUrl(supplier?.LogoUrl),  // ✅
            Subtotal = order.Subtotal,
            DiscountAmount = order.DiscountAmount,
            TotalAmount = order.TotalAmount,
            Currency = order.Currency,
            Status = order.Status,
            StatusAr = OrderStatus.ToArabic(order.Status),
            CustomerNotes = order.CustomerNotes,
            SupplierNotes = order.SupplierNotes,
            CancelReason = order.CancelReason,
            CreatedAt = order.CreatedAt,
            ConfirmedAt = order.ConfirmedAt,
            ProcessingAt = order.ProcessingAt,
            CompletedAt = order.CompletedAt,
            ReceivedAt = order.ReadyAt,
            CancelledAt = order.CancelledAt
        };

        // إضافة عنصر الطلب
        if (item != null)
        {
            dto.Item = new OrderItemDto
            {
                Id = item.Id,
                PartId = item.PartId,
                PartName = item.PartNameSnapshot,
                PartNumber = item.PartNumberSnapshot,
                Condition = item.ConditionSnapshot,
                ImageUrl = GetFullUrl(item.ImageUrlSnapshot),  // ✅
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                TotalPrice = item.TotalPrice,
                WarrantyDays = item.WarrantyDaysSnapshot,
                Notes = item.Notes
            };
        }

        // تحديد الأزرار المتاحة
        dto.AvailableActions = GetAvailableActions(order.Status, viewerType);

        return dto;
    }

    /// <summary>
    /// الحصول على الأزرار المتاحة حسب الحالة والمستخدم
    /// </summary>
    private List<string> GetAvailableActions(string status, string viewerType)
    {
        var actions = new List<string>();

        if (viewerType == "customer")
        {
            switch (status)
            {
                case OrderStatus.Pending:
                    actions.Add("cancel");
                    break;
                case OrderStatus.Completed:
                    actions.Add("complete");  // تأكيد الاستلام
                    break;
            }
        }
        else if (viewerType == "supplier")
        {
            switch (status)
            {
                case OrderStatus.Pending:
                    actions.Add("confirm");   // تأكيد الطلب (processing)
                    actions.Add("reject");
                    break;
                case OrderStatus.Processing:
                    actions.Add("deliver");   // تم التوصيل (completed)
                    break;
            }
        }

        return actions;
    }

    #endregion
}
