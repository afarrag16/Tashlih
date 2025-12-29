using System;
using System.Collections.Generic;
namespace Tashlih.Core.Entities;

public partial class SubscriptionPlan
{
    public long Id { get; set; }

    public string NameAr { get; set; } = null!;

    public string? NameEn { get; set; }

    public string? DescriptionAr { get; set; }

    public string? DescriptionEn { get; set; }

    public decimal Price { get; set; }
    public string? LogoUrl { get; set; }

    public string Currency { get; set; } = null!;

    public int DurationDays { get; set; }

    public int? MaxParts { get; set; }

    public int MaxImagesPerPart { get; set; }

    public int MaxShops { get; set; }

    public string? Features { get; set; }

    public int SortOrder { get; set; }

    public bool IsPopular { get; set; }

    public string? BadgeText { get; set; }

    public bool IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
}
