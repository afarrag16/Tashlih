using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Tashlih.Application.DTOs.Reviews;
using Tashlih.Application.Interfaces;

namespace Tashlih.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReviewsController : ControllerBase
{
    private readonly IReviewsService _reviewsService;

    public ReviewsController(IReviewsService reviewsService)
    {
        _reviewsService = reviewsService;
    }

    /// <summary>
    /// إضافة تقييم جديد (للعميل)
    /// </summary>
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateReview([FromBody] CreateReviewRequest request)
    {
        if (!IsCustomer())
            return Forbid();

        var customerId = GetUserId();
        var result = await _reviewsService.CreateReviewAsync(customerId, request);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// تعديل تقييم (للعميل)
    /// </summary>
    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> UpdateReview(long id, [FromBody] UpdateReviewRequest request)
    {
        if (!IsCustomer())
            return Forbid();

        var customerId = GetUserId();
        var result = await _reviewsService.UpdateReviewAsync(customerId, id, request);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// حذف تقييم (للعميل)
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> DeleteReview(long id)
    {
        if (!IsCustomer())
            return Forbid();

        var customerId = GetUserId();
        var result = await _reviewsService.DeleteReviewAsync(customerId, id);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// جلب تقييماتي (للعميل)
    /// </summary>
    [HttpGet("my-reviews")]
    [Authorize]
    public async Task<IActionResult> GetMyReviews()
    {
        if (!IsCustomer())
            return Forbid();

        var customerId = GetUserId();
        var result = await _reviewsService.GetMyReviewsAsync(customerId);

        return Ok(result);
    }

    /// <summary>
    /// جلب تقييمات مورد (للكل)
    /// </summary>
    //[HttpGet("supplier/{supplierId}")]
    //[AllowAnonymous]
    //public async Task<IActionResult> GetSupplierReviews(long supplierId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    //{
    //    var result = await _reviewsService.GetSupplierReviewsAsync(supplierId, page, pageSize);
    //    return Ok(result);
    //}



    /// <summary>
    /// جلب تقييماتي (للمورد)
    /// </summary>
    [HttpGet("supplier/my-reviews")]
    [Authorize]
    public async Task<IActionResult> GetSupplierMyReviews()
    {
        if (!IsSupplier())
            return Forbid();

        var supplierId = GetSupplierId();
        var result = await _reviewsService.GetSupplierReviewsAsync(supplierId, 1, 100);
        return Ok(result);
    }

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

    private bool IsSupplier()
    {
        return User.FindFirst("user_type")?.Value == "supplier";
    }

    private long GetSupplierId()
    {
        var supplierIdClaim = User.FindFirst("supplier_id")?.Value;
        return long.TryParse(supplierIdClaim, out var id) ? id : 0;
    }

    #endregion
}