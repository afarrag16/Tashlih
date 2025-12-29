namespace Tashlih.Application.DTOs.Subscriptions;

#region Response DTOs

public class SubscriptionResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public string? MessageAr { get; set; }
}

public class SubscriptionPlansResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public string? MessageAr { get; set; }
    public List<SubscriptionPlanDto>? Plans { get; set; }
}

public class MySubscriptionResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public string? MessageAr { get; set; }
    public SubscriptionDto? Subscription { get; set; }
    public SubscriptionPlanDto? Plan { get; set; }
    public SubscriptionUsageDto? Usage { get; set; }
}

public class SubscribeResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public string? MessageAr { get; set; }
    public SubscriptionDto? Subscription { get; set; }
    public string? PaymentUrl { get; set; }
}

#endregion

#region Data DTOs

public class SubscriptionPlanDto
{
    public long Id { get; set; }
    public string? NameAr { get; set; }
    public string? NameEn { get; set; }
    public string? DescriptionAr { get; set; }
    public string? DescriptionEn { get; set; }
    public string? LogoUrl { get; set; }
    public decimal Price { get; set; }
    public string? Currency { get; set; }
    public int DurationDays { get; set; }
    public int? MaxParts { get; set; }
    public int MaxImagesPerPart { get; set; }
    public int MaxShops { get; set; }
    public List<string>? Features { get; set; }
    public bool IsPopular { get; set; }
    public string? BadgeText { get; set; }
}

public class SubscriptionDto
{
    public long Id { get; set; }
    public long SupplierId { get; set; }
    public long PlanId { get; set; }
    public string? PlanName { get; set; }
    public string? Status { get; set; }
    public string? StatusAr { get; set; }
    public DateOnly? StartsAt { get; set; }
    public DateOnly? EndsAt { get; set; }
    public int? DaysRemaining { get; set; }
    public decimal? AmountPaid { get; set; }
    public string? PaymentMethod { get; set; }
    public bool AutoRenew { get; set; }
    public DateTime? CreatedAt { get; set; }
}

public class SubscriptionUsageDto
{
    public int CurrentParts { get; set; }
    public int? MaxParts { get; set; }
    public int PartsRemaining { get; set; }
    public int CurrentImages { get; set; }
    public int MaxImagesPerPart { get; set; }
    public bool CanAddPart { get; set; }
    public string? UpgradeMessage { get; set; }
    public string? UpgradeMessageAr { get; set; }
}

#endregion

#region Request DTOs

public class SubscribeRequest
{
    public long PlanId { get; set; }
    public string? PaymentMethod { get; set; }
    public bool AutoRenew { get; set; }
}

#endregion

#region Admin DTOs

public class AdminCreatePlanRequest
{
    public string NameAr { get; set; } = null!;
    public string? NameEn { get; set; }
    public string? DescriptionAr { get; set; }
    public string? DescriptionEn { get; set; }
    public decimal Price { get; set; }
    public string Currency { get; set; } = "SAR";
    public int DurationDays { get; set; } = 30;
    public int? MaxParts { get; set; }
    public int MaxImagesPerPart { get; set; } = 5;
    public int MaxShops { get; set; } = 1;
    public List<string>? Features { get; set; }
    public int SortOrder { get; set; }
    public bool IsPopular { get; set; }
    public string? BadgeText { get; set; }
    public bool IsActive { get; set; } = true;
}

public class AdminUpdatePlanRequest
{
    public string? NameAr { get; set; }
    public string? NameEn { get; set; }
    public string? DescriptionAr { get; set; }
    public string? DescriptionEn { get; set; }
    public decimal? Price { get; set; }
    public string? Currency { get; set; }
    public int? DurationDays { get; set; }
    public int? MaxParts { get; set; }
    public int? MaxImagesPerPart { get; set; }
    public int? MaxShops { get; set; }
    public List<string>? Features { get; set; }
    public int? SortOrder { get; set; }
    public bool? IsPopular { get; set; }
    public string? BadgeText { get; set; }
    public bool? IsActive { get; set; }
}

public class AdminSubscriptionsResponse
{
    public bool Success { get; set; }
    public List<AdminSubscriptionDto>? Subscriptions { get; set; }
    public int TotalCount { get; set; }
}

public class AdminSubscriptionDto
{
    public long Id { get; set; }
    public long SupplierId { get; set; }
    public string? SupplierName { get; set; }
    public string? SupplierPhone { get; set; }
    public string? PlanName { get; set; }
    public decimal Price { get; set; }
    public string? Status { get; set; }
    public string? StatusAr { get; set; }
    public DateOnly? StartsAt { get; set; }
    public DateOnly? EndsAt { get; set; }
    public decimal? AmountPaid { get; set; }
    public string? PaymentMethod { get; set; }
    public DateTime? CreatedAt { get; set; }
}

public class AdminPlansResponse
{
    public bool Success { get; set; }
    public List<SubscriptionPlanDto>? Plans { get; set; }
    public int TotalCount { get; set; }
}

#endregion