using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Tashlih.Application.DTOs.Order;
using Tashlih.Application.Interfaces;

namespace Tashlih.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CustomerOrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    public CustomerOrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    /// <summary>
    /// إنشاء طلب جديد
    /// </summary>
    /// <param name="request">بيانات الطلب</param>
    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
    {
        if (!IsCustomer())
            return Forbid();

        var customerId = GetUserId();
        var result = await _orderService.CreateOrderAsync(customerId, request);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// جلب طلباتي
    /// </summary>
    /// <param name="status">فلترة بالحالة (اختياري): pending, processing, completed, received, cancelled, rejected, incomplete, complete</param>
    /// <param name="page">رقم الصفحة</param>
    /// <param name="pageSize">عدد العناصر في الصفحة</param>
    [HttpGet("my-orders")]
    public async Task<IActionResult> GetMyOrders([FromQuery] string? status = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        if (!IsCustomer())
            return Forbid();

        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 20;
        if (pageSize > 50) pageSize = 50;

        var customerId = GetUserId();
        var result = await _orderService.GetCustomerOrdersAsync(customerId, status, page, pageSize);

        return Ok(result);
    }

    /// <summary>
    /// جلب تفاصيل طلب
    /// </summary>
    /// <param name="id">رقم الطلب</param>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetOrderDetails(long id)
    {
        if (!IsCustomer())
            return Forbid();

        var customerId = GetUserId();
        var result = await _orderService.GetCustomerOrderDetailsAsync(customerId, id);

        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }

    /// <summary>
    /// تأكيد استلام الطلب
    /// </summary>
    /// <param name="id">رقم الطلب</param>
    [HttpPut("{id}/complete")]
    public async Task<IActionResult> CompleteOrder(long id)
    {
        if (!IsCustomer())
            return Forbid();

        var customerId = GetUserId();
        var result = await _orderService.CompleteOrderAsync(customerId, id);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// إلغاء الطلب
    /// </summary>
    /// <param name="id">رقم الطلب</param>
    /// <param name="request">سبب الإلغاء (اختياري)</param>
    [HttpPut("{id}/cancel")]
    public async Task<IActionResult> CancelOrder(long id, [FromBody] CancelOrderRequest? request)
    {
        if (!IsCustomer())
            return Forbid();

        var customerId = GetUserId();
        var result = await _orderService.CancelOrderAsync(customerId, id, request ?? new CancelOrderRequest());

        if (!result.Success)
            return BadRequest(result);

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

    private bool IsCustomer()
    {
        return GetUserType() == "customer";
    }

    #endregion
}
