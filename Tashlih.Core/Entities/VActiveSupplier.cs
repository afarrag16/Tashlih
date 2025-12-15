using System;
using System.Collections.Generic;

namespace Tashlih.Core.Entities;

public partial class VActiveSupplier
{
    public long Id { get; set; }

    public string BusinessNameAr { get; set; } = null!;

    public string? BusinessNameEn { get; set; }

    public string? City { get; set; }

    public decimal RatingAverage { get; set; }

    public int RatingCount { get; set; }

    public int TotalOrders { get; set; }

    public bool IsVerified { get; set; }

    public string Phone { get; set; } = null!;

    public string FullName { get; set; } = null!;

    public int? ShopsCount { get; set; }

    public int? PartsCount { get; set; }
}
