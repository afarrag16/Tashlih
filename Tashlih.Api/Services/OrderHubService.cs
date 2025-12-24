using Microsoft.AspNetCore.SignalR;
using Tashlih.Application.Interfaces;
using Tashlih.Api.Hubs;

namespace Tashlih.Api.Services;

/// <summary>
/// Implementation of IOrderHubService using SignalR
/// </summary>
public class OrderHubService : IOrderHubService
{
    private readonly IHubContext<ChatHub> _hubContext;

    public OrderHubService(IHubContext<ChatHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task SendNewOrderAsync(long supplierId, object orderData)
    {
        await _hubContext.Clients.Group($"user_{supplierId}").SendAsync("NewOrder", orderData);
    }

    public async Task SendOrderStatusUpdatedAsync(long customerId, object orderData)
    {
        await _hubContext.Clients.Group($"user_{customerId}").SendAsync("OrderStatusUpdated", orderData);
    }

    public async Task SendOrderCompletedAsync(long supplierId, object orderData)
    {
        await _hubContext.Clients.Group($"user_{supplierId}").SendAsync("OrderCompleted", orderData);
    }

    public async Task SendOrderCancelledAsync(long supplierId, object orderData)
    {
        await _hubContext.Clients.Group($"user_{supplierId}").SendAsync("OrderCancelled", orderData);
    }

    public async Task SendOrderRejectedAsync(long customerId, object orderData)
    {
        await _hubContext.Clients.Group($"user_{customerId}").SendAsync("OrderRejected", orderData);
    }
}
