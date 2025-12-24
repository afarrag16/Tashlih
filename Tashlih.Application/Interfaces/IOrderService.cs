using Tashlih.Application.DTOs.Order;

namespace Tashlih.Application.Interfaces;

public interface IOrderService
{
    #region Customer Methods

    /// <summary>
    /// إنشاء طلب جديد (للعميل)
    /// </summary>
    Task<CreateOrderResponse> CreateOrderAsync(long customerId, CreateOrderRequest request);

    /// <summary>
    /// جلب طلبات العميل
    /// </summary>
    Task<OrdersListResponse> GetCustomerOrdersAsync(long customerId, string? status = null, int page = 1, int pageSize = 20);

    /// <summary>
    /// جلب تفاصيل طلب (للعميل)
    /// </summary>
    Task<OrderDetailsResponse> GetCustomerOrderDetailsAsync(long customerId, long orderId);

    /// <summary>
    /// تأكيد استلام الطلب (للعميل)
    /// </summary>
    Task<OrderResponse> CompleteOrderAsync(long customerId, long orderId);

    /// <summary>
    /// إلغاء الطلب (للعميل)
    /// </summary>
    Task<OrderResponse> CancelOrderAsync(long customerId, long orderId, CancelOrderRequest request);

    #endregion

    #region Supplier Methods

    /// <summary>
    /// جلب طلبات المورد
    /// </summary>
    Task<OrdersListResponse> GetSupplierOrdersAsync(long supplierId, string? status = null, int page = 1, int pageSize = 20);

    /// <summary>
    /// جلب تفاصيل طلب (للمورد)
    /// </summary>
    Task<OrderDetailsResponse> GetSupplierOrderDetailsAsync(long supplierId, long orderId);

    /// <summary>
    /// تغيير حالة الطلب (للمورد)
    /// </summary>
    Task<OrderResponse> UpdateOrderStatusAsync(long supplierId, long orderId, UpdateOrderStatusRequest request);

    /// <summary>
    /// رفض الطلب (للمورد)
    /// </summary>
    Task<OrderResponse> RejectOrderAsync(long supplierId, long orderId, RejectOrderRequest request);

    #endregion
}
