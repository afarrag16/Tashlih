using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Tashlih.Application.DTOs.Admin;
using Tashlih.Application.DTOs.Subscriptions;
using Tashlih.Application.DTOs.SupplierProfile;
using Tashlih.Application.Interfaces;
using Tashlih.Infrastructure.Services;


namespace Tashlih.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AdminController : ControllerBase
{
    private readonly IAdminAuthService _adminAuthService;
    private readonly ISubscriptionService _subscriptionService;
    private readonly AdminSupplierService _supplierService;
    private readonly AdminCustomerService _customerService;
    private readonly AdminDashboardService _dashboardService;
    private readonly AdminLogsService _logsService;


    public AdminController(IAdminAuthService adminAuthService,
        ISubscriptionService subscriptionService,
        AdminSupplierService supplierService,
        AdminCustomerService customerService,
        AdminDashboardService dashboardService,
        AdminLogsService logsService)
    {
        _adminAuthService = adminAuthService;
        _subscriptionService = subscriptionService;
        _supplierService = supplierService;
        _customerService = customerService;
        _dashboardService = dashboardService;
        _logsService = logsService;
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

        var adminId = GetAdminId();
        var result = await _subscriptionService.UpdatePlanAsync(id, request, logo, adminId);
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

        var adminId = GetAdminId();
        var result = await _subscriptionService.DeletePlanAsync(id, adminId);
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



    #region إدارة الموردين


    /// <summary>
    /// توثيق مورد (موافقة/رفض)
    /// </summary>
    [HttpPut("suppliers/{id}/verify")]
    [Authorize]
    public async Task<IActionResult> VerifySupplier(long id, [FromBody] VerifySupplierRequest request)
    {
        if (!IsAdmin())
            return Forbid();

        var adminId = GetAdminId();

        // تعيين الـ supplierId من الـ URL
        request.SupplierId = id;

        var result = await _supplierService.VerifySupplierAsync(adminId, request);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// عرض كل الموردين
    /// </summary>
    [HttpGet("suppliers")]
    [Authorize]
    public async Task<IActionResult> GetAllSuppliers([FromQuery] AdminSuppliersRequest request)
    {
        if (!IsAdmin())
            return Forbid();

        var result = await _supplierService.GetAllSuppliersAsync(request);
        return Ok(result);
    }

    /// <summary>
    /// تفاصيل مورد
    /// </summary>
    [HttpGet("suppliers/{id}")]
    [Authorize]
    public async Task<IActionResult> GetSupplierById(long id)
    {
        if (!IsAdmin())
            return Forbid();

        var result = await _supplierService.GetSupplierByIdAsync(id);

        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }

    /// <summary>
    /// تفعيل مورد
    /// </summary>
    [HttpPut("suppliers/{id}/activate")]
    [Authorize]
    public async Task<IActionResult> ActivateSupplier(long id, [FromBody] AdminSupplierActionRequest request)
    {
        if (!IsAdmin())
            return Forbid();

        var adminId = GetAdminId();
        var result = await _supplierService.ActivateSupplierAsync(id, request, adminId);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// إيقاف مورد
    /// </summary>
    [HttpPut("suppliers/{id}/deactivate")]
    [Authorize]
    public async Task<IActionResult> DeactivateSupplier(long id, [FromBody] AdminSupplierActionRequest request)
    {
        if (!IsAdmin())
            return Forbid();

        var adminId = GetAdminId();
        var result = await _supplierService.DeactivateSupplierAsync(id, request, adminId);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// حذف مورد
    /// </summary>
    [HttpDelete("suppliers/{id}")]
    [Authorize]
    public async Task<IActionResult> DeleteSupplier(long id)
    {
        if (!IsAdmin())
            return Forbid();

        var adminId = GetAdminId();
        var result = await _supplierService.DeleteSupplierAsync(id, adminId);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    #endregion

    #region إدارة العملاء

    /// <summary>
    /// عرض كل العملاء
    /// </summary>
    [HttpGet("customers")]
    [Authorize]
    public async Task<IActionResult> GetAllCustomers([FromQuery] AdminCustomersRequest request)
    {
        if (!IsAdmin())
            return Forbid();
        var result = await _customerService.GetAllCustomersAsync(request);
        return Ok(result);
    }

    /// <summary>
    /// تفاصيل عميل
    /// </summary>
    [HttpGet("customers/{id}")]
    [Authorize]
    public async Task<IActionResult> GetCustomerById(long id)
    {
        if (!IsAdmin())
            return Forbid();
        var result = await _customerService.GetCustomerByIdAsync(id);
        if (!result.Success)
            return NotFound(result);
        return Ok(result);
    }

    /// <summary>
    /// تفعيل عميل
    /// </summary>
    [HttpPut("customers/{id}/activate")]
    [Authorize]
    public async Task<IActionResult> ActivateCustomer(long id, [FromBody] AdminCustomerActionRequest request)
    {
        if (!IsAdmin())
            return Forbid();

        var adminId = GetAdminId();
        var result = await _customerService.ActivateCustomerAsync(id, request, adminId);
        if (!result.Success)
            return BadRequest(result);
        return Ok(result);
    }

    /// <summary>
    /// إيقاف عميل
    /// </summary>
    [HttpPut("customers/{id}/deactivate")]
    [Authorize]
    public async Task<IActionResult> DeactivateCustomer(long id, [FromBody] AdminCustomerActionRequest request)
    {
        if (!IsAdmin())
            return Forbid();

        var adminId = GetAdminId();
        var result = await _customerService.DeactivateCustomerAsync(id, request, adminId);
        if (!result.Success)
            return BadRequest(result);
        return Ok(result);
    }

    /// <summary>
    /// حذف عميل
    /// </summary>
    [HttpDelete("customers/{id}")]
    [Authorize]
    public async Task<IActionResult> DeleteCustomer(long id)
    {
        if (!IsAdmin())
            return Forbid();

        var adminId = GetAdminId();
        var result = await _customerService.DeleteCustomerAsync(id, adminId);
        if (!result.Success)
            return BadRequest(result);
        return Ok(result);
    }

  

    #endregion

    #region إحصائيات الداشبورد

    /// <summary>
    /// إحصائيات الداشبورد
    /// </summary>
    [HttpGet("dashboard/statistics")]
    [Authorize]
    public async Task<IActionResult> GetDashboardStatistics()
    {
        if (!IsAdmin())
            return Forbid();

        var result = await _dashboardService.GetStatisticsAsync();
        return Ok(result);
    }

    #endregion

    #region احصائيات النشاط
    /// <summary>
    /// عرض سجل العمليات
    /// </summary>
    [HttpGet("logs")]
    [Authorize]
    public async Task<IActionResult> GetLogs([FromQuery] LogsRequest request)
    {
        if (!IsAdmin())
            return Forbid();

        var result = await _logsService.GetLogsAsync(request);
        return Ok(result);
    }

    /// <summary>
    /// تفاصيل عملية
    /// </summary>
    [HttpGet("logs/{id}")]
    [Authorize]
    public async Task<IActionResult> GetLogById(long id)
    {
        if (!IsAdmin())
            return Forbid();

        var result = await _logsService.GetLogByIdAsync(id);
        if (!result.Success)
            return NotFound(result);
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