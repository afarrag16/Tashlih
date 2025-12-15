using System;
using System.Collections.Generic;

namespace Tashlih.Core.Entities;

public partial class Subscription
{
    public long Id { get; set; }

    public long SupplierId { get; set; }

    public long PlanId { get; set; }

    public string Status { get; set; } = null!;

    public DateOnly? StartsAt { get; set; }

    public DateOnly? EndsAt { get; set; }

    public decimal? AmountPaid { get; set; }

    public decimal? DiscountAmount { get; set; }

    public string? PaymentMethod { get; set; }

    public string? PaymentReference { get; set; }

    public string? PaymentNotes { get; set; }

    public long? ApprovedBy { get; set; }

    public DateTime? ApprovedAt { get; set; }

    public string? RejectionReason { get; set; }

    public DateTime? CancelledAt { get; set; }

    public string? CancellationReason { get; set; }

    public bool AutoRenew { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual User? ApprovedByNavigation { get; set; }

    public virtual SubscriptionPlan Plan { get; set; } = null!;

    public virtual ICollection<SubscriptionHistory> SubscriptionHistories { get; set; } = new List<SubscriptionHistory>();

    public virtual SupplierProfile Supplier { get; set; } = null!;
}
