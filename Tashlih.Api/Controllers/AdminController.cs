using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Tashlih.Application.DTOs.Admin;
using Tashlih.Application.DTOs.Subscriptions;
using Tashlih.Application.Interfaces;

namespace Tashlih.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AdminController : ControllerBase
{
    private readonly IAdminAuthService _adminAuthService;
    private readonly ISubscriptionService _subscriptionService;

    public AdminController(IAdminAuthService adminAuthService, ISubscriptionService subscriptionService)
    {
        _adminAuthService = adminAuthService;
        _subscriptionService = subscriptionService;
    }

    #region المصادقة
   
    /// <summary>
    /// تسجيل دخول الأدمن
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] AdminLoginRequest request)
    {
        var result = await _adminAuthService.LoginAsync(request);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// بيانات الأدمن
    /// </summary>
    [HttpGet("profile")]
    [Authorize]
    public async Task<IActionResult> GetProfile()
    {
        if (!IsAdmin())
            return Forbid();

        var adminId = GetAdminId();
        var result = await _adminAuthService.GetProfileAsync(adminId);

        return Ok(result);
    }

    #endregion

    #region إدارة الباقات

    /// <summary>
    /// عرض كل الباقات
    /// </summary>
    [HttpGet("plans")]
    [Authorize]
    public async Task<IActionResult> GetAllPlans()
    {
        if (!IsAdmin())
            return Forbid();

        var result = await _subscriptionService.GetAllPlansAsync();
        return Ok(result);
    }

    /// <summary>
    /// إنشاء باقة جديدة
    /// </summary>
    [HttpPost("plans")]
    [Authorize]
    public async Task<IActionResult> CreatePlan([FromForm] AdminCreatePlanRequest request, IFormFile? logo)
    {
        if (!IsAdmin())
            return Forbid();

        var result = await _subscriptionService.CreatePlanAsync(request, logo);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// تعديل باقة
    /// </summary>
    [HttpPut("plans/{id}")]
    [Authorize]
    public async Task<IActionResult> UpdatePlan(long id, [FromForm] AdminUpdatePlanRequest request, IFormFile? logo)
    {
        if (!IsAdmin())
            return Forbid();

        var result = await _subscriptionService.UpdatePlanAsync(id, request, logo);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// حذف باقة
    /// </summary>
    [HttpDelete("plans/{id}")]
    [Authorize]
    public async Task<IActionResult> DeletePlan(long id)
    {
        if (!IsAdmin())
            return Forbid();

        var result = await _subscriptionService.DeletePlanAsync(id);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    #endregion

    #region إدارة الاشتراكات

    /// <summary>
    /// عرض كل الاشتراكات
    /// </summary>
    [HttpGet("subscriptions")]
    [Authorize]
    public async Task<IActionResult> GetAllSubscriptions()
    {
        if (!IsAdmin())
            return Forbid();

        var result = await _subscriptionService.GetAllSubscriptionsAsync();
        return Ok(result);
    }

    #endregion

    #region Helper Methods

    private long GetAdminId()
    {
        var adminIdClaim = User.FindFirst("admin_id")?.Value;
        return long.TryParse(adminIdClaim, out var id) ? id : 0;
    }

    private bool IsAdmin()
    {
        return User.FindFirst("user_type")?.Value == "admin";
    }

    #endregion
}