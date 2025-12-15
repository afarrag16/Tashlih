using System;
using System.Collections.Generic;

namespace Tashlih.Core.Entities;

public partial class OrderItem
{
    public long Id { get; set; }

    public long OrderId { get; set; }

    public long? PartId { get; set; }

    public string PartNameSnapshot { get; set; } = null!;

    public string? PartNumberSnapshot { get; set; }

    public string ConditionSnapshot { get; set; } = null!;

    public string? ImageUrlSnapshot { get; set; }

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal TotalPrice { get; set; }

    public int? WarrantyDaysSnapshot { get; set; }

    public string? Notes { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Order Order { get; set; } = null!;

    public virtual Part? Part { get; set; }
}
