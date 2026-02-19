using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Tashlih.Application.DTOs.CustomerProfile;
using Tashlih.Application.Interfaces;

namespace Tashlih.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CustomerProfileController : ControllerBase
{
    private readonly ICustomerProfileService _customerProfileService;

    public CustomerProfileController(ICustomerProfileService customerProfileService)
    {
        _customerProfileService = customerProfileService;
    }

    /// <summary>
    /// جلب بيانات الملف الشخصي للعميل الحالي
    /// </summary>
    [HttpGet("me")]
    public async Task<IActionResult> GetProfile()
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized(new { message = "غير مصرح" });

        // التحقق من أن المستخدم عميل
        var userType = User.FindFirst("user_type")?.Value;
        if (userType != "customer")
            return Forbid();

        var profile = await _customerProfileService.GetProfileAsync(userId.Value);

        if (profile == null)
            return NotFound(new { message = "المستخدم غير موجود" });

        return Ok(profile);
    }

    /// <summary>
    /// تحديث بيانات الملف الشخصي
    /// </summary>
    [HttpPut("Update-Profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateCustomerProfileRequest request)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized(new { message = "غير مصرح" });

        var userType = User.FindFirst("user_type")?.Value;
        if (userType != "customer")
            return Forbid();

        var result = await _customerProfileService.UpdateProfileAsync(userId.Value, request);

        if (!result.Success)
            return BadRequest(new { message = result.Message });

        return Ok(new { message = result.Message });
    }

    /// <summary>
    /// رفع صورة الملف الشخصي
    /// </summary>
    [HttpPost("Upload-Profile-Image")]
    public async Task<IActionResult> UploadAvatar(IFormFile file)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized(new { message = "غير مصرح" });

        var userType = User.FindFirst("user_type")?.Value;
        if (userType != "customer")
            return Forbid();

        if (file == null || file.Length == 0)
            return BadRequest(new { message = "الرجاء اختيار صورة" });

        var result = await _customerProfileService.UploadAvatarAsync(userId.Value, file);

        if (!result.Success)
            return BadRequest(new { message = result.Message });

        return Ok(new { message = result.Message, avatarUrl = result.AvatarUrl });
    }

    /// <summary>
    /// حذف صورة الملف الشخصي
    /// </summary>
    [HttpDelete("Delete-Profile-Image")]
    public async Task<IActionResult> DeleteAvatar()
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized(new { message = "غير مصرح" });

        var userType = User.FindFirst("user_type")?.Value;
        if (userType != "customer")
            return Forbid();

        var result = await _customerProfileService.DeleteAvatarAsync(userId.Value);

        if (!result.Success)
            return BadRequest(new { message = result.Message });

        return Ok(new { message = result.Message });
    }

    /// <summary>
    /// تحديث الموقع من GPS
    /// </summary>
    [HttpPut("Update-Location")]
    public async Task<IActionResult> UpdateLocation([FromBody] UpdateLocationRequest request)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized(new { message = "غير مصرح" });

        var userType = User.FindFirst("user_type")?.Value;
        if (userType != "customer")
            return Forbid();

        var result = await _customerProfileService.UpdateLocationAsync(userId.Value, request);

        if (!result.Success)
            return BadRequest(new { message = result.Message });

        return Ok(new { message = result.Message });
    }

    /// <summary>
    /// جلب قائمة المدن
    /// </summary>
    [HttpGet("Cities")]
    [AllowAnonymous]
    public async Task<IActionResult> GetCities()
    {
        var cities = await _customerProfileService.GetCitiesAsync();
        return Ok(cities);
    }

    private long? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (long.TryParse(userIdClaim, out var userId))
            return userId;
        return null;
    }

    /// <summary>
    /// حذف الحساب
    /// </summary>
    [HttpDelete("delete-account")]
    public async Task<IActionResult> DeleteAccount([FromBody] DeleteAccountRequest request)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized(new { message = "غير مصرح" });

        var userType = User.FindFirst("user_type")?.Value;
        if (userType != "customer")
            return Forbid();

        var result = await _customerProfileService.DeleteAccountAsync(userId.Value, request);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }
}
