using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Tashlih.Application.Interfaces;

namespace Tashlih.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FavoritesController : ControllerBase
{
    private readonly IFavoritesService _favoritesService;

    public FavoritesController(IFavoritesService favoritesService)
    {
        _favoritesService = favoritesService;
    }

    #region القطع المفضلة

    /// <summary>
    /// إضافة قطعة للمفضلة
    /// </summary>
    [HttpPost("parts/{partId}")]
    public async Task<IActionResult> AddPartToFavorites(long partId)
    {
        if (!IsCustomer())
            return Forbid();

        var customerId = GetUserId();
        var result = await _favoritesService.AddPartToFavoritesAsync(customerId, partId);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// إزالة قطعة من المفضلة
    /// </summary>
    [HttpDelete("parts/{partId}")]
    public async Task<IActionResult> RemovePartFromFavorites(long partId)
    {
        if (!IsCustomer())
            return Forbid();

        var customerId = GetUserId();
        var result = await _favoritesService.RemovePartFromFavoritesAsync(customerId, partId);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// عرض القطع المفضلة
    /// </summary>
    [HttpGet("parts")]
    public async Task<IActionResult> GetFavoriteParts()
    {
        if (!IsCustomer())
            return Forbid();

        var customerId = GetUserId();
        var result = await _favoritesService.GetFavoritePartsAsync(customerId);

        return Ok(result);
    }

    /// <summary>
    /// التحقق إذا القطعة مفضلة
    /// </summary>
    [HttpGet("parts/{partId}/check")]
    public async Task<IActionResult> IsPartFavorite(long partId)
    {
        if (!IsCustomer())
            return Forbid();

        var customerId = GetUserId();
        var result = await _favoritesService.IsPartFavoriteAsync(customerId, partId);

        return Ok(result);
    }

    #endregion

    #region الموردين المفضلين

    /// <summary>
    /// إضافة مورد للمفضلة
    /// </summary>
    [HttpPost("suppliers/{supplierId}")]
    public async Task<IActionResult> AddSupplierToFavorites(long supplierId)
    {
        if (!IsCustomer())
            return Forbid();

        var customerId = GetUserId();
        var result = await _favoritesService.AddSupplierToFavoritesAsync(customerId, supplierId);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// إزالة مورد من المفضلة
    /// </summary>
    [HttpDelete("suppliers/{supplierId}")]
    public async Task<IActionResult> RemoveSupplierFromFavorites(long supplierId)
    {
        if (!IsCustomer())
            return Forbid();

        var customerId = GetUserId();
        var result = await _favoritesService.RemoveSupplierFromFavoritesAsync(customerId, supplierId);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// عرض الموردين المفضلين
    /// </summary>
    [HttpGet("suppliers")]
    public async Task<IActionResult> GetFavoriteSuppliers()
    {
        if (!IsCustomer())
            return Forbid();

        var customerId = GetUserId();
        var result = await _favoritesService.GetFavoriteSuppliersAsync(customerId);

        return Ok(result);
    }

    /// <summary>
    /// التحقق إذا المورد مفضل
    /// </summary>
    [HttpGet("suppliers/{supplierId}/check")]
    public async Task<IActionResult> IsSupplierFavorite(long supplierId)
    {
        if (!IsCustomer())
            return Forbid();

        var customerId = GetUserId();
        var result = await _favoritesService.IsSupplierFavoriteAsync(customerId, supplierId);

        return Ok(result);
    }

    #endregion

    #region Helper Methods

    private long GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return long.TryParse(userIdClaim, out var id) ? id : 0;
    }

    private bool IsCustomer()
    {
        return User.FindFirst("user_type")?.Value == "customer";
    }

    #endregion
}