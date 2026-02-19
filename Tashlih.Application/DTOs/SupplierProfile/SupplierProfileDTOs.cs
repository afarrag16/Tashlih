using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Tashlih.Application.DTOs.SupplierProfile
{
    // ==================== Request DTOs ====================

    /// <summary>
    /// طلب تحديث ملف المورد
    /// </summary>
    public class UpdateSupplierProfileRequest
    {
        // البيانات الشخصية
        [StringLength(100)]
        public string? FullName { get; set; }

        [EmailAddress]
        public string? Email { get; set; }

        // بيانات النشاط
        [StringLength(150)]
        public string? BusinessNameAr { get; set; }

        [StringLength(150)]
        public string? BusinessNameEn { get; set; }

        [StringLength(50)]
        public string? BusinessType { get; set; }

        [StringLength(100)]
        public string? ManagerName { get; set; }

        public string? Description { get; set; }

        [StringLength(50)]
        public string? City { get; set; }

        [StringLength(100)]
        public string? District { get; set; }

        [StringLength(2)]
        public string? PreferredLanguage { get; set; }
        
    }

    /// <summary>
    /// طلب إعادة رفع المستندات بعد الرفض
    /// </summary>
    public class ResubmitVerificationRequest
    {
        [Required(ErrorMessage = "يجب رفع مستند واحد على الأقل")]
        public IFormFile Document1 { get; set; } = null!;

        [Required(ErrorMessage = "نوع المستند الأول مطلوب")]
        public string DocumentType1 { get; set; } = null!;

        public IFormFile? Document2 { get; set; }

        public string? DocumentType2 { get; set; }
    }

    /// <summary>
    /// طلب تحديث بيانات التوثيق
    /// </summary>
    public class UpdateVerificationDataRequest
    {
        [StringLength(20)]
        public string? IdNumber { get; set; }

        [StringLength(50)]
        public string? CrNumber { get; set; }

        public DateOnly? CrExpiryDate { get; set; }

        [StringLength(50)]
        public string? LicenseNumber { get; set; }

        public DateOnly? LicenseExpiryDate { get; set; }

        [StringLength(50)]
        public string? TaxNumber { get; set; }
    }

    /// <summary>
    /// طلب توثيق المورد (للأدمن)
    /// </summary>
    public class VerifySupplierRequest
    {
        [Required]
        public long SupplierId { get; set; }

        [Required]
        public bool IsApproved { get; set; }

        public string? RejectionReason { get; set; }

        public string? AdminNotes { get; set; }
        /// <summary>
        /// المستندات المطلوب إعادة رفعها عند الرفض
        /// ["id_front", "id_back", "commercial_register", "license", "tax_certificate"]
        /// </summary>
        public List<string>? RequiredDocuments { get; set; }
    }

    // ==================== Response DTOs ====================

    /// <summary>
    /// استجابة ملف المورد
    /// </summary>
    public class SupplierProfileResponse
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public string? MessageAr { get; set; }
        public SupplierProfileDto? Profile { get; set; }
    }

    /// <summary>
    /// استجابة التوثيق
    /// </summary>
    public class VerificationResponse
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public string? MessageAr { get; set; }
        public bool IsVerified { get; set; }
        public string? VerificationStatus { get; set; }
        public string? DocumentUrl { get; set; }
        public List<VerificationDocumentDto>? Documents { get; set; }
        public string? RejectionReason { get; set; }
        public List<string>? RequiredDocuments { get; set; }
        public string? AdminNotes { get; set; }
        public DateTime? SubmittedAt { get; set; }
        public DateTime? ReviewedAt { get; set; }
    }

    /// <summary>
    /// استجابة الإحصائيات
    /// </summary>
    public class SupplierStatsResponse
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public string? MessageAr { get; set; }
        public SupplierStatsDto? Stats { get; set; }
    }

    // ==================== Data DTOs ====================

    /// <summary>
    /// بيانات ملف المورد
    /// </summary>
    public class SupplierProfileDto
    {
        public long Id { get; set; }
        public string? FullName { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? LogoUrl { get; set; }
        public string? BusinessNameAr { get; set; }
        public string? BusinessNameEn { get; set; }
        public string? BusinessType { get; set; }
        public string? ManagerName { get; set; }
        public string? Description { get; set; }
        public string? City { get; set; }
        public string? District { get; set; }
        public string? CommercialRegister { get; set; }
        public string? LicenseNumber { get; set; }
        public string? TaxNumber { get; set; }
        public decimal RatingAverage { get; set; }
        public int RatingCount { get; set; }
        public int TotalOrders { get; set; }
        public int CompletedOrders { get; set; }
        public bool IsVerified { get; set; }
        public string? VerificationStatus { get; set; }
        public DateTime? VerifiedAt { get; set; }
        public string? Status { get; set; }
        public string? PreferredLanguage { get; set; }
        public DateTime? CreatedAt { get; set; }
        
        public int PartsCount { get; set; }
        public bool IsPhoneVerified { get; set; }
    }

    /// <summary>
    /// بيانات مستند التوثيق
    /// </summary>
    public class VerificationDocumentDto
    {
        public string? DocumentType { get; set; }
        public string? DocumentTypeAr { get; set; }
        public string? DocumentUrl { get; set; }
        public bool IsUploaded { get; set; }
        public bool IsRequired { get; set; }
        public string? Status { get; set; }
        public string? Number { get; set; }
        public DateOnly? ExpiryDate { get; set; }
        public DateTime? UploadedAt { get; set; }
    }
   
    /// <summary>
    /// إحصائيات المورد
    /// </summary>
    public class SupplierStatsDto
    {
        public int TotalShops { get; set; }
        public int TotalParts { get; set; }
        public int AvailableParts { get; set; }
        public int SoldParts { get; set; }
        public int TotalOrders { get; set; }
        public int PendingOrders { get; set; }
        public int CompletedOrders { get; set; }
        public int CancelledOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public int ThisMonthOrders { get; set; }
        public decimal ThisMonthRevenue { get; set; }
        public decimal RatingAverage { get; set; }
        public int RatingCount { get; set; }
        public int TotalViews { get; set; }
        public bool IsVerified { get; set; }
        public string? VerificationStatus { get; set; }
        public string? RejectionReason { get; set; }
    }
    /// <summary>
    /// طلب حذف الحساب
    /// </summary>
    public class DeleteSupplierAccountRequest
    {
        [Required(ErrorMessage = "كلمة المرور مطلوبة")]
        public string Password { get; set; } = null!;
    }

    /// <summary>
    /// استجابة حذف الحساب
    /// </summary>
    public class DeleteSupplierAccountResponse
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public string? MessageAr { get; set; }
    }
}