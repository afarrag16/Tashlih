using System;
using System.Collections.Generic;

namespace Tashlih.Core.Entities;

public partial class VOrdersDetailed
{
    public long Id { get; set; }

    public string OrderNumber { get; set; } = null!;

    public long CustomerId { get; set; }

    public long ShopId { get; set; }

    public long SupplierId { get; set; }

    public decimal Subtotal { get; set; }

    public decimal DiscountAmount { get; set; }

    public decimal TotalAmount { get; set; }

    public string Currency { get; set; } = null!;

    public string Status { get; set; } = null!;

    public DateTime? ConfirmedAt { get; set; }

    public DateTime? ProcessingAt { get; set; }

    public DateTime? ReadyAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    public DateTime? CancelledAt { get; set; }

    public string? CancelledBy { get; set; }

    public string? CancelReason { get; set; }

    public string? CustomerNotes { get; set; }

    public string? SupplierNotes { get; set; }

    public string? InternalNotes { get; set; }

    public bool IsReviewed { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public string CustomerName { get; set; } = null!;

    public string CustomerPhone { get; set; } = null!;

    public string SupplierName { get; set; } = null!;

    public string ShopName { get; set; } = null!;

    public string ShopCity { get; set; } = null!;

    public int? ItemsCount { get; set; }
}
