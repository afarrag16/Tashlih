using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;
using Tashlih.Application.DTOs.Auth;
using Tashlih.Application.Interfaces;

namespace Tashlih.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        #region التسجيل

        /// <summary>
        /// تسجيل عميل جديد
        /// </summary>
        [AllowAnonymous]
        [HttpPost("register/customer")]
        public async Task<IActionResult> RegisterCustomer([FromBody] CustomerRegisterRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.RegisterCustomerAsync(request);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        /// <summary>
        /// تسجيل مورد جديد (مع رفع الملفات)
        /// </summary>
        [AllowAnonymous]
        [HttpPost("register/supplier")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> RegisterSupplier([FromForm] SupplierRegisterRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.RegisterSupplierAsync(request);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        #endregion

        #region تسجيل الدخول

        /// <summary>
        /// تسجيل دخول موحد (للعملاء والموردين)
        /// </summary>
        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.LoginAsync(request);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        #endregion

        #region OTP

        /// <summary>
        /// إرسال رمز التحقق
        /// </summary>
        [AllowAnonymous]
        [HttpPost("send-otp")]
        public async Task<IActionResult> SendOtp([FromBody] SendOtpRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.SendOtpAsync(request);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        /// <summary>
        /// التحقق من رمز OTP
        /// </summary>
        [AllowAnonymous]
        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.VerifyOtpAsync(request);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        /// <summary>
        /// تسجيل دخول بـ OTP
        /// </summary>
        [AllowAnonymous]
        [HttpPost("login-otp")]
        public async Task<IActionResult> LoginWithOtp([FromBody] LoginWithOtpRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.LoginWithOtpAsync(request);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        #endregion

        #region كلمة المرور

        /// <summary>
        /// تغيير كلمة المرور
        /// </summary>
        [Authorize]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = GetCurrentUserId();
            var userType = GetCurrentUserType();

            if (userId == 0)
                return Unauthorized(new { Success = false, Message = "Unauthorized", MessageAr = "غير مصرح" });

            var result = await _authService.ChangePasswordAsync(userId, userType, request);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        /// <summary>
        /// إعادة تعيين كلمة المرور
        /// </summary>
        [AllowAnonymous]
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.ResetPasswordAsync(request);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        #endregion

        #region تسجيل الخروج

        /// <summary>
        /// تسجيل خروج
        /// </summary>
        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var userId = GetCurrentUserId();
            var userType = GetCurrentUserType();
            var token = GetCurrentToken();

            if (userId == 0 || string.IsNullOrEmpty(token))
                return Unauthorized(new { Success = false, Message = "Unauthorized", MessageAr = "غير مصرح" });

            var result = await _authService.LogoutAsync(userId, userType, token);

            return Ok(new { Success = result, Message = result ? "Logged out successfully" : "Logout failed", MessageAr = result ? "تم تسجيل الخروج بنجاح" : "فشل تسجيل الخروج" });
        }

        /// <summary>
        /// تسجيل خروج من كل الأجهزة
        /// </summary>
        [Authorize]
        [HttpPost("logout-all")]
        public async Task<IActionResult> LogoutAll()
        {
            var userId = GetCurrentUserId();
            var userType = GetCurrentUserType();

            if (userId == 0)
                return Unauthorized(new { Success = false, Message = "Unauthorized", MessageAr = "غير مصرح" });

            var result = await _authService.LogoutAllAsync(userId, userType);

            return Ok(new { Success = result, Message = "Logged out from all devices", MessageAr = "تم تسجيل الخروج من جميع الأجهزة" });
        }

        #endregion

        #region المستخدم الحالي

        /// <summary>
        /// الحصول على بيانات المستخدم الحالي
        /// </summary>
        [Authorize]
        [HttpGet("me")]
        public IActionResult GetCurrentUser()
        {
            var userId = GetCurrentUserId();
            var userType = GetCurrentUserType();

            if (userId == 0)
                return Unauthorized(new { Success = false, Message = "Unauthorized", MessageAr = "غير مصرح" });

            return Ok(new
            {
                Success = true,
                User = new
                {
                    Id = userId,
                    Name = User.FindFirst(ClaimTypes.Name)?.Value,
                    Phone = User.FindFirst(ClaimTypes.MobilePhone)?.Value,
                    Email = User.FindFirst(ClaimTypes.Email)?.Value,
                    UserType = userType,
                    Status = User.FindFirst("status")?.Value,
                    IsVerified = User.FindFirst("is_verified")?.Value,
                    VerificationStatus = User.FindFirst("verification_status")?.Value,
                    BusinessName = User.FindFirst("business_name")?.Value
                }
            });
        }

        #endregion

        #region التحقق

        /// <summary>
        /// التحقق من وجود رقم الجوال
        /// </summary>
        [AllowAnonymous]
        [HttpGet("check-phone/{phone}")]
        public async Task<IActionResult> CheckPhone(string phone)
        {
            var exists = await _authService.IsPhoneExistsAsync(phone);
            return Ok(new { Exists = exists });
        }

        /// <summary>
        /// التحقق من وجود البريد الإلكتروني
        /// </summary>
        [AllowAnonymous]
        [HttpGet("check-email/{email}")]
        public async Task<IActionResult> CheckEmail(string email)
        {
            var exists = await _authService.IsEmailExistsAsync(email);
            return Ok(new { Exists = exists });
        }

        #endregion

        #region Helper Methods

        private long GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return long.TryParse(userIdClaim, out var id) ? id : 0;
        }

        private string GetCurrentUserType()
        {
            return User.FindFirst("user_type")?.Value ?? "customer";
        }

        private string? GetCurrentToken()
        {
            var authHeader = Request.Headers["Authorization"].ToString();
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
                return null;

            return authHeader.Substring("Bearer ".Length).Trim();
        }

        #endregion
    }
}