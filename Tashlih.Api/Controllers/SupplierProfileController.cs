using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Tashlih.Application.DTOs.SupplierProfile;
using Tashlih.Application.Interfaces;

namespace Tashlih.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SupplierProfileController : ControllerBase
    {
        private readonly ISupplierProfileService _supplierProfileService;

        public SupplierProfileController(ISupplierProfileService supplierProfileService)
        {
            _supplierProfileService = supplierProfileService;
        }

        /// <summary>
        /// الحصول على ملف المورد الحالي
        /// </summary>
        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetMyProfile()
        {
            // التأكد إن المستخدم مورد
            if (!IsSupplier())
                return Forbid();

            var supplierId = GetSupplierId();
            if (supplierId == 0)
                return Unauthorized(new { success = false, message = "Unauthorized", messageAr = "غير مصرح" });

            var result = await _supplierProfileService.GetMyProfileAsync(supplierId);

            if (!result.Success)
                return NotFound(result);

            return Ok(result);
        }

        /// <summary>
        /// الحصول على ملف مورد بالـ ID (للعرض العام)
        /// </summary>
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetProfile(long id)
        {
            var result = await _supplierProfileService.GetProfileByIdAsync(id);

            if (!result.Success)
                return NotFound(result);

            return Ok(result);
        }

        /// <summary>
        /// تحديث ملف المورد
        /// </summary>
        [HttpPut("update")]
        [Authorize]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateSupplierProfileRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!IsSupplier())
                return Forbid();

            var supplierId = GetSupplierId();
            if (supplierId == 0)
                return Unauthorized(new { success = false, message = "Unauthorized", messageAr = "غير مصرح" });

            var result = await _supplierProfileService.UpdateProfileAsync(supplierId, request);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        /// <summary>
        /// إعادة رفع المستندات بعد الرفض
        /// </summary>
        [HttpPost("verification/resubmit")]
        [Authorize]
        public async Task<IActionResult> ResubmitVerification([FromForm] ResubmitVerificationRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!IsSupplier())
                return Forbid();

            var supplierId = GetSupplierId();
            if (supplierId == 0)
                return Unauthorized(new { success = false, message = "Unauthorized", messageAr = "غير مصرح" });

            var result = await _supplierProfileService.ResubmitVerificationAsync(supplierId, request);
            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        /// <summary>
        /// تحديث بيانات التوثيق (أرقام، تواريخ)
        /// </summary>
        [HttpPut("verification/data")]
        [Authorize]
        public async Task<IActionResult> UpdateVerificationData([FromBody] UpdateVerificationDataRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!IsSupplier())
                return Forbid();

            var supplierId = GetSupplierId();
            if (supplierId == 0)
                return Unauthorized(new { success = false, message = "Unauthorized", messageAr = "غير مصرح" });

            var result = await _supplierProfileService.UpdateVerificationDataAsync(supplierId, request);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        /// <summary>
        /// الحصول على حالة التوثيق والمستندات
        /// </summary>
        [HttpGet("verification/status")]
        [Authorize]
        public async Task<IActionResult> GetVerificationStatus()
        {
            if (!IsSupplier())
                return Forbid();

            var supplierId = GetSupplierId();
            if (supplierId == 0)
                return Unauthorized(new { success = false, message = "Unauthorized", messageAr = "غير مصرح" });

            var result = await _supplierProfileService.GetVerificationStatusAsync(supplierId);

            if (!result.Success)
                return NotFound(result);

            return Ok(result);
        }

        /// <summary>
        /// طلب التوثيق (إرسال للمراجعة)
        /// </summary>
        [HttpPost("verification/request")]
        [Authorize]
        public async Task<IActionResult> RequestVerification()
        {
            if (!IsSupplier())
                return Forbid();

            var supplierId = GetSupplierId();
            if (supplierId == 0)
                return Unauthorized(new { success = false, message = "Unauthorized", messageAr = "غير مصرح" });

            var result = await _supplierProfileService.RequestVerificationAsync(supplierId);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

       

        /// <summary>
        /// الحصول على إحصائيات المورد
        /// </summary>
        [HttpGet("stats")]
        [Authorize]
        public async Task<IActionResult> GetStats()
        {
            if (!IsSupplier())
                return Forbid();

            var supplierId = GetSupplierId();
            if (supplierId == 0)
                return Unauthorized(new { success = false, message = "Unauthorized", messageAr = "غير مصرح" });

            var result = await _supplierProfileService.GetSupplierStatsAsync(supplierId);

            if (!result.Success)
                return NotFound(result);

            return Ok(result);
        }

        /// <summary>
        /// حذف الحساب
        /// </summary>
        [HttpDelete("delete-account")]
        [Authorize]
        public async Task<IActionResult> DeleteAccount([FromBody] DeleteSupplierAccountRequest request)
        {
            if (!IsSupplier())
                return Forbid();

            var supplierId = GetSupplierId();
            if (supplierId == 0)
                return Unauthorized(new { success = false, message = "Unauthorized", messageAr = "غير مصرح" });

            var result = await _supplierProfileService.DeleteAccountAsync(supplierId, request);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        // ==================== Helper Methods ====================

        /// <summary>
        /// جلب الـ Supplier ID من الـ Token
        /// </summary>
        private long GetSupplierId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return long.TryParse(userIdClaim, out var id) ? id : 0;
        }

        /// <summary>
        /// جلب الـ User ID (للأدمن)
        /// </summary>
        private long GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return long.TryParse(userIdClaim, out var id) ? id : 0;
        }

        /// <summary>
        /// التحقق من أن المستخدم مورد
        /// </summary>
        private bool IsSupplier()
        {
            var userType = User.FindFirst("user_type")?.Value;
            return userType == "supplier";
        }

        /// <summary>
        /// جلب نوع المستخدم
        /// </summary>
        private string GetUserType()
        {
            return User.FindFirst("user_type")?.Value ?? "customer";
        }
    }
}