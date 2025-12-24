using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Tashlih.Application.DTOs.Order;
using Tashlih.Application.Interfaces;

namespace Tashlih.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SupplierOrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    public SupplierOrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    /// <summary>
    /// جلب طلبات العملاء
    /// </summary>
    /// <param name="status">فلترة بالحالة (اختياري): pending, processing, completed, received, cancelled, rejected, incomplete, complete</param>
    /// <param name="page">رقم الصفحة</param>
    /// <param name="pageSize">عدد العناصر في الصفحة</param>
    [HttpGet]
    public async Task<IActionResult> GetOrders([FromQuery] string? status = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        if (!IsSupplier())
            return Forbid();

        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 20;
        if (pageSize > 50) pageSize = 50;

        var supplierId = GetSupplierId();
        var result = await _orderService.GetSupplierOrdersAsync(supplierId, status, page, pageSize);

        return Ok(result);
    }

    /// <summary>
    /// جلب تفاصيل طلب
    /// </summary>
    /// <param name="id">رقم الطلب</param>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetOrderDetails(long id)
    {
        if (!IsSupplier())
            return Forbid();

        var supplierId = GetSupplierId();
        var result = await _orderService.GetSupplierOrderDetailsAsync(supplierId, id);

        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }

    /// <summary>
    /// تغيير حالة الطلب
    /// </summary>
    /// <param name="id">رقم الطلب</param>
    /// <param name="request">الحالة الجديدة: processing (تأكيد) أو completed (تم التوصيل)</param>
    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateOrderStatus(long id, [FromBody] UpdateOrderStatusRequest request)
    {
        if (!IsSupplier())
            return Forbid();

        var supplierId = GetSupplierId();
        var result = await _orderService.UpdateOrderStatusAsync(supplierId, id, request);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// رفض الطلب
    /// </summary>
    /// <param name="id">رقم الطلب</param>
    /// <param name="request">سبب الرفض (اختياري)</param>
    [HttpPut("{id}/reject")]
    public async Task<IActionResult> RejectOrder(long id, [FromBody] RejectOrderRequest? request)
    {
        if (!IsSupplier())
            return Forbid();

        var supplierId = GetSupplierId();
        var result = await _orderService.RejectOrderAsync(supplierId, id, request ?? new RejectOrderRequest());

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

    private long GetSupplierId()
    {
        var supplierIdClaim = User.FindFirst("supplier_id")?.Value;
        return long.TryParse(supplierIdClaim, out var id) ? id : 0;
    }

    private string GetUserType()
    {
        return User.FindFirst("user_type")?.Value ?? "customer";
    }

    private bool IsSupplier()
    {
        return GetUserType() == "supplier";
    }

    #endregion
}
