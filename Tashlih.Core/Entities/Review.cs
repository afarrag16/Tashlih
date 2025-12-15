using System;
using System.Collections.Generic;

namespace Tashlih.Core.Entities;

public partial class Review
{
    public long Id { get; set; }

    public long OrderId { get; set; }

    public long CustomerId { get; set; }

    public long SupplierId { get; set; }

    public byte OverallRating { get; set; }

    public byte? QualityRating { get; set; }

    public byte? CommunicationRating { get; set; }

    public byte? SpeedRating { get; set; }

    public byte? PriceRating { get; set; }

    public string? Comment { get; set; }

    public string? SupplierReply { get; set; }

    public DateTime? SupplierReplyAt { get; set; }

    public bool IsVerified { get; set; }

    public bool IsVisible { get; set; }

    public bool IsReported { get; set; }

    public string? ReportReason { get; set; }

    public long? ModeratedBy { get; set; }

    public DateTime? ModeratedAt { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual User Customer { get; set; } = null!;

    public virtual User? ModeratedByNavigation { get; set; }

    public virtual Order Order { get; set; } = null!;

    public virtual SupplierProfile Supplier { get; set; } = null!;
}
