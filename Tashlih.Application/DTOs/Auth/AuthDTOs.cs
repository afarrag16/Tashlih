using Microsoft.AspNetCore.Http;
using System;
using System.ComponentModel.DataAnnotations;

namespace Tashlih.Application.DTOs.Auth
{
    // ==================== Auth Responses ====================

    public class AuthResponse
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public string? MessageAr { get; set; }
        public string? Token { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public UserDto? User { get; set; }
     
    }

  
    // ==================== User DTOs ====================

    public class UserDto
    {
        public long Id { get; set; }
        public string? FullName { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? AvatarUrl { get; set; }
        public string? UserType { get; set; }
        public string? Status { get; set; }
        public bool IsVerified { get; set; }
        public string? VerificationStatus { get; set; }
        public string? PreferredLanguage { get; set; }
        public bool NotificationsEnabled { get; set; }
        public DateTime? CreatedAt { get; set; }
    }

  
    }

    // ==================== Register Requests ====================

    public class CustomerRegisterRequest
    {
        [Required(ErrorMessage = "الاسم الكامل مطلوب")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "الاسم يجب أن يكون بين 2 و 100 حرف")]
        public string FullName { get; set; } = null!;

        [Required(ErrorMessage = "رقم الجوال مطلوب")]
        [RegularExpression(@"^05\d{8}$", ErrorMessage = "رقم الجوال يجب أن يبدأ بـ 05 ويتكون من 10 أرقام")]
        public string Phone { get; set; } = null!;

        [EmailAddress(ErrorMessage = "البريد الإلكتروني غير صحيح")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "كلمة المرور مطلوبة")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "كلمة المرور يجب أن تكون 6 أحرف على الأقل")]
        public string Password { get; set; } = null!;

        public string PreferredLanguage { get; set; } = "ar";
        public string? DeviceType { get; set; }
        public string? DeviceName { get; set; }
        public string? FcmToken { get; set; }
    }

    public class SupplierRegisterRequest
    {
        // البيانات الشخصية
        [Required(ErrorMessage = "الاسم الكامل مطلوب")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "الاسم يجب أن يكون بين 2 و 100 حرف")]
        public string FullName { get; set; } = null!;

        [Required(ErrorMessage = "رقم الجوال مطلوب")]
        [RegularExpression(@"^05\d{8}$", ErrorMessage = "رقم الجوال يجب أن يبدأ بـ 05 ويتكون من 10 أرقام")]
        public string Phone { get; set; } = null!;

        [EmailAddress(ErrorMessage = "البريد الإلكتروني غير صحيح")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "كلمة المرور مطلوبة")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "كلمة المرور يجب أن تكون 6 أحرف على الأقل")]
        public string Password { get; set; } = null!;

        // بيانات النشاط
        [Required(ErrorMessage = "اسم النشاط بالعربي مطلوب")]
        [StringLength(150, MinimumLength = 2, ErrorMessage = "اسم النشاط يجب أن يكون بين 2 و 150 حرف")]
        public string BusinessNameAr { get; set; } = null!;

        [StringLength(150)]
        public string? BusinessNameEn { get; set; }

        [Required(ErrorMessage = "نوع النشاط مطلوب")]
        [StringLength(50)]
        public string BusinessType { get; set; } = null!;

        [Required(ErrorMessage = "المدينة مطلوبة")]
        [StringLength(50)]
        public string City { get; set; } = null!;

        [StringLength(100)]
        public string? District { get; set; }

        // السجل التجاري
        [Required(ErrorMessage = "رقم السجل التجاري مطلوب")]
        [StringLength(50)]
        public string CommercialRegisterNumber { get; set; } = null!;

        [Required(ErrorMessage = "صورة السجل التجاري مطلوبة")]
        public IFormFile CommercialRegisterImage { get; set; } = null!;

        // الهوية
        [Required(ErrorMessage = "صورة الهوية مطلوبة")]
        public IFormFile IdentityImage { get; set; } = null!;

        // إعدادات
        public string PreferredLanguage { get; set; } = "ar";
        public string? DeviceType { get; set; }
        public string? DeviceName { get; set; }
        public string? FcmToken { get; set; }
    }

    // ==================== Login Requests ====================

    public class LoginRequest
    {
        [Required(ErrorMessage = "رقم الجوال مطلوب")]
        public string Phone { get; set; } = null!;

        [Required(ErrorMessage = "كلمة المرور مطلوبة")]
        public string Password { get; set; } = null!;

        public string? DeviceType { get; set; }
        public string? DeviceName { get; set; }
        public string? FcmToken { get; set; }
    }

    public class ChangePasswordRequest
    {
        [Required(ErrorMessage = "كلمة المرور الحالية مطلوبة")]
        public string CurrentPassword { get; set; } = null!;

        [Required(ErrorMessage = "كلمة المرور الجديدة مطلوبة")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "كلمة المرور يجب أن تكون 6 أحرف على الأقل")]
        public string NewPassword { get; set; } = null!;
    }

    public class ResetPasswordRequest
    {
        [Required]
        public string Phone { get; set; } = null!;

        [Required]
        public string OtpCode { get; set; } = null!;

        [Required]
        [StringLength(100, MinimumLength = 6)]
        public string NewPassword { get; set; } = null!;
    }

    // ==================== OTP Requests ====================

    public class SendOtpRequest
    {
        [Required]
        public string Phone { get; set; } = null!;

        public string Purpose { get; set; } = "login";
    }

    public class VerifyOtpRequest
    {
        [Required]
        public string Phone { get; set; } = null!;
    [Required]
    public string OtpCode { get; set; } = null!;

       
    }

    public class OtpLoginRequest
    {
        [Required]
        public string Phone { get; set; } = null!;

        [Required]
        public string OtpCode { get; set; } = null!;

        public string? DeviceType { get; set; }
        public string? DeviceName { get; set; }
        public string? FcmToken { get; set; }
    }

    public class LoginWithOtpRequest : OtpLoginRequest { }
