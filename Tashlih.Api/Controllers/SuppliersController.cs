using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tashlih.Application.Interfaces;

namespace Tashlih.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SuppliersController : ControllerBase
{
    private readonly ISuppliersService _suppliersService;

    public SuppliersController(ISuppliersService suppliersService)
    {
        _suppliersService = suppliersService;
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
}