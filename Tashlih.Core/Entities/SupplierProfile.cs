using System;
using System.Collections.Generic;

namespace Tashlih.Core.Entities;

public partial class SupplierProfile
{
    public long Id { get; set; }

    // ========== بيانات المصادقة ==========
    public string? FullName { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? PasswordHash { get; set; }
    public string PreferredLanguage { get; set; } = "ar";
    public string? OtpCode { get; set; }
    public DateTime? OtpExpiresAt { get; set; }

    public DateTime? LastLoginAt { get; set; }

    // ========== بيانات النشاط ==========
    public string BusinessNameAr { get; set; } = null!;
    public string? BusinessNameEn { get; set; }
    public string? ManagerName { get; set; }
    public string? Description { get; set; }
    public string? BusinessType { get; set; }
    public string? City { get; set; }
    public string? District { get; set; }

    // ========== المستندات ==========
    public string? IdFrontUrl { get; set; }
    public string? IdBackUrl { get; set; }
    public string? IdNumber { get; set; }
    public string? CommercialRegisterImageUrl { get; set; }
    public string? CommercialRegister { get; set; }
    public DateOnly? CommercialRegisterExpiryDate { get; set; }
    public string? LicenseImageUrl { get; set; }
    public string? LicenseNumber { get; set; }
    public DateOnly? LicenseExpiryDate { get; set; }
    public string? TaxCertificateUrl { get; set; }
    public string? TaxNumber { get; set; }

    // ========== التوثيق ==========
    public bool IsVerified { get; set; }
    public string VerificationStatus { get; set; } = "pending";
    public string? RejectionReason { get; set; }
    public string? AdminNotes { get; set; }
    public DateTime? VerificationSubmittedAt { get; set; }
    public DateTime? VerificationReviewedAt { get; set; }
    public DateTime? VerifiedAt { get; set; }
    public long? VerifiedBy { get; set; }

    // ========== الإحصائيات ==========
    public decimal RatingAverage { get; set; }
    public int RatingCount { get; set; }
    public int TotalOrders { get; set; }
    public int CompletedOrders { get; set; }

    // ========== الحالة ==========
    public string Status { get; set; } = "active";
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }

    // ========== Legacy ==========
    public long? UserId { get; set; }

    // ========== Navigation Properties ==========
    public virtual User? User { get; set; }
    public virtual User? VerifiedByNavigation { get; set; }
    public virtual ICollection<SupplierSession> SupplierSessions { get; set; } = new List<SupplierSession>();
    public virtual ICollection<Shop> Shops { get; set; } = new List<Shop>();
    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
    public virtual ICollection<ChatThread> ChatThreads { get; set; } = new List<ChatThread>();
    public virtual ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
    public virtual ICollection<SubscriptionHistory> SubscriptionHistories { get; set; } = new List<SubscriptionHistory>();
}