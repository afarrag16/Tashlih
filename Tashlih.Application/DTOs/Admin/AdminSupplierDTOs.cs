namespace Tashlih.Application.DTOs.Admin;

using Tashlih.Application.DTOs.Parts;

#region Response DTOs

public class AdminSuppliersResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public string? MessageAr { get; set; }
    public List<AdminSupplierDto>? Suppliers { get; set; }
    public int TotalCount { get; set; }
    public PaginationInfo? Pagination { get; set; }
}

public class AdminSupplierDetailResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public string? MessageAr { get; set; }
    public AdminSupplierDetailDto? Supplier { get; set; }
}

public class AdminSupplierActionResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public string? MessageAr { get; set; }
}

#endregion

#region Data DTOs

public class AdminSupplierDto
{
    public long Id { get; set; }
    public string? FullName { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? BusinessNameAr { get; set; }
    public string? BusinessNameEn { get; set; }
    public string? City { get; set; }
    public string? LogoUrl { get; set; }
    public bool IsVerified { get; set; }
    public string? VerificationStatus { get; set; }
    public string? Status { get; set; }
    public decimal RatingAverage { get; set; }
    public int TotalOrders { get; set; }
    public int PartsCount { get; set; }
    public string? CurrentPlan { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
}

public class AdminSupplierDetailDto : AdminSupplierDto
{
    public string? ManagerName { get; set; }
    public string? Description { get; set; }
    public string? BusinessType { get; set; }
    public string? District { get; set; }

    // المستندات
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

    // التوثيق
    public string? RejectionReason { get; set; }
    public string? AdminNotes { get; set; }
    public DateTime? VerificationSubmittedAt { get; set; }
    public DateTime? VerifiedAt { get; set; }

    // الإحصائيات
    public int RatingCount { get; set; }
    public int CompletedOrders { get; set; }

    // الاشتراك
    public SupplierSubscriptionDto? Subscription { get; set; }
}

public class SupplierSubscriptionDto
{
    public long Id { get; set; }
    public string? PlanName { get; set; }
    public string? Status { get; set; }
    public DateOnly? StartsAt { get; set; }
    public DateOnly? EndsAt { get; set; }
    public int DaysRemaining { get; set; }
}



#endregion

#region Request DTOs

public class AdminSuppliersRequest
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? Search { get; set; }
    public string? Status { get; set; }
    public string? VerificationStatus { get; set; }
    public string? City { get; set; }
}

public class AdminSupplierActionRequest
{
    public string? Reason { get; set; }
    public string? AdminNotes { get; set; }
}

#endregion