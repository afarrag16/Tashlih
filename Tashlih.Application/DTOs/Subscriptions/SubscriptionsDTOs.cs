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
    public string? PaymentUrl { get; set; }  // لبوابة الدفع لاحقاً
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
    public string? PaymentMethod { get; set; }  // manual, moyasar, myfatoorah
    public bool AutoRenew { get; set; }
}

#endregion

#region Admin DTOs

public class AdminActivateSubscriptionRequest
{
    public long SupplierId { get; set; }
    public long PlanId { get; set; }
    public decimal? AmountPaid { get; set; }
    public decimal? DiscountAmount { get; set; }
    public string? PaymentMethod { get; set; }
    public string? PaymentReference { get; set; }
    public string? PaymentNotes { get; set; }
}

public class AdminExtendSubscriptionRequest
{
    public long SubscriptionId { get; set; }
    public int ExtraDays { get; set; }
    public string? Notes { get; set; }
}

public class AdminCancelSubscriptionRequest
{
    public long SubscriptionId { get; set; }
    public string? CancellationReason { get; set; }
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

#endregion
