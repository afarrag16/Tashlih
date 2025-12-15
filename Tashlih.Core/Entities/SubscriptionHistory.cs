using System;
using System.Collections.Generic;

namespace Tashlih.Core.Entities;

public partial class SubscriptionHistory
{
    public long Id { get; set; }

    public long SubscriptionId { get; set; }

    public long SupplierId { get; set; }

    public string Action { get; set; } = null!;

    public string? OldStatus { get; set; }

    public string? NewStatus { get; set; }

    public long? OldPlanId { get; set; }

    public long? NewPlanId { get; set; }

    public decimal? Amount { get; set; }

    public string? Notes { get; set; }

    public long? PerformedBy { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual User? PerformedByNavigation { get; set; }

    public virtual Subscription Subscription { get; set; } = null!;

    public virtual SupplierProfile Supplier { get; set; } = null!;
}
