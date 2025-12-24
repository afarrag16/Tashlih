namespace Tashlih.Application.Interfaces;

/// <summary>
/// Interface for Order SignalR notifications
/// </summary>
public interface IOrderHubService
{
    /// <summary>
    /// إشعار المورد بطلب جديد
    /// </summary>
    Task SendNewOrderAsync(long supplierId, object orderData);

    /// <summary>
    /// إشعار العميل بتحديث حالة الطلب
    /// </summary>
    Task SendOrderStatusUpdatedAsync(long customerId, object orderData);

    /// <summary>
    /// إشعار المورد بتأكيد استلام الطلب
    /// </summary>
    Task SendOrderCompletedAsync(long supplierId, object orderData);

    /// <summary>
    /// إشعار المورد بإلغاء الطلب
    /// </summary>
    Task SendOrderCancelledAsync(long supplierId, object orderData);

    /// <summary>
    /// إشعار العميل برفض الطلب
    /// </summary>
    Task SendOrderRejectedAsync(long customerId, object orderData);
}
