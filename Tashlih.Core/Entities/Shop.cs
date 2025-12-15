using System;
using System.Collections.Generic;

namespace Tashlih.Core.Entities;
public partial class Shop
{
    public long Id { get; set; }

    public long SupplierId { get; set; }

    public string NameAr { get; set; } = null!;

    public string? NameEn { get; set; }

    public string? Description { get; set; }

    public string City { get; set; } = null!;

    public string? District { get; set; }

    public string? Street { get; set; }

    public string? Address { get; set; }

    public decimal? Latitude { get; set; }

    public decimal? Longitude { get; set; }

    public string? LocationUrl { get; set; }

    public string? Phone { get; set; }

    public string? Whatsapp { get; set; }

    public string? WorkingHours { get; set; }

    public bool IsMain { get; set; }

    public string Status { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual ICollection<Part> Parts { get; set; } = new List<Part>();

    public virtual SupplierProfile Supplier { get; set; } = null!;
}
