using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tashlih.Application.Interfaces;
using Tashlih.Infrastructure.Services;

namespace Tashlih.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SuppliersController : ControllerBase
{
    private readonly ISuppliersService _suppliersService;
    private readonly SupplierDashboardService _dashboardService;

    public SuppliersController(ISuppliersService suppliersService, SupplierDashboardService dashboardService)
    {
        _suppliersService = suppliersService;
        _dashboardService = dashboardService;
    }

    /// <summary>
    /// جلب تفاصيل مورد
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetSupplierDetails(long id)
    {
        var result = await _suppliersService.GetSupplierDetailsAsync(id);

        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }

    /// <summary>
    /// جلب قائمة الموردين
    /// </summary>
    [HttpGet("list")]
    public async Task<IActionResult> GetSuppliersList([FromQuery] string? city)
    {
        var result = await _suppliersService.GetSuppliersListAsync(city, 1, 20);
        return Ok(result);
    }

    /// <summary>
    /// جلب الموردين القريبين من موقع العميل
    /// </summary>
    [HttpGet("nearby")]
    public async Task<IActionResult> GetNearbySuppliers([FromQuery] decimal latitude, [FromQuery] decimal longitude, [FromQuery] double radiusKm = 10)
    {
        if (latitude == 0 || longitude == 0)
            return BadRequest(new { Success = false, Message = "Location required", MessageAr = "الرجاء إدخال الموقع" });

        if (radiusKm <= 0 || radiusKm > 100)
            return BadRequest(new { Success = false, Message = "Radius must be between 1 and 100 km", MessageAr = "المسافة يجب أن تكون بين 1 و 100 كيلومتر" });

        var result = await _suppliersService.GetNearbySuppliersAsync(latitude, longitude, radiusKm);
        return Ok(result);
    }

    /// <summary>
    /// جلب إحصائيات المورد
    /// </summary>
    [HttpGet("my-statistics")]
    [Authorize]
    public async Task<IActionResult> GetMyStatistics(
        [FromQuery] string? period = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null)
    {
        if (!IsSupplier())
            return Forbid();

        var supplierId = GetSupplierId();
        var result = await _suppliersService.GetSupplierStatisticsAsync(supplierId, period, fromDate, toDate);

        return Ok(result);
    }

    /// <summary>
    /// إحصائيات داشبورد المورد
    /// </summary>
    [HttpGet("dashboard")]
    [Authorize]
    public async Task<IActionResult> GetDashboard([FromQuery] string period = "week")
    {
        if (!IsSupplier())
            return Forbid();

        var supplierId = GetSupplierId();
        if (supplierId == 0)
            return Unauthorized(new { success = false, message = "Unauthorized", messageAr = "غير مصرح" });

        var result = await _dashboardService.GetDashboardAsync(supplierId, period);
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