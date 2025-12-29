using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Tashlih.Application.DTOs.Subscriptions;
using Tashlih.Application.Interfaces;

namespace Tashlih.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SubscriptionsController : ControllerBase
{
    private readonly ISubscriptionService _subscriptionService;

    public SubscriptionsController(ISubscriptionService subscriptionService)
    {
        _subscriptionService = subscriptionService;
    }

    /// <summary>
    /// عرض الباقات المتاحة (للمورد فقط)
    /// </summary>
    [HttpGet("plans")]
    [Authorize]
    public async Task<IActionResult> GetPlans()
    {
        if (!IsSupplier())
            return Forbid();

        var result = await _subscriptionService.GetPlansAsync();
        return Ok(result);
    }

    /// <summary>
    /// جلب اشتراكي الحالي
    /// </summary>
    [HttpGet("my-subscription")]
    public async Task<IActionResult> GetMySubscription()
    {
        if (!IsSupplier())
            return Forbid();

        var supplierId = GetSupplierId();
        var result = await _subscriptionService.GetMySubscriptionAsync(supplierId);
        return Ok(result);
    }

    /// <summary>
    /// الاشتراك في باقة
    /// </summary>
    [HttpPost("subscribe")]
    public async Task<IActionResult> Subscribe([FromBody] SubscribeRequest request)
    {
        if (!IsSupplier())
            return Forbid();

        var supplierId = GetSupplierId();
        var result = await _subscriptionService.SubscribeAsync(supplierId, request);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    #region Helper Methods

    private long GetSupplierId()
    {
        var supplierIdClaim = User.FindFirst("supplier_id")?.Value;
        return long.TryParse(supplierIdClaim, out var id) ? id : 0;
    }

    private bool IsSupplier()
    {
        return User.FindFirst("user_type")?.Value == "supplier";
    }

    #endregion
}
