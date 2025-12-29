using System;

namespace Tashlih.Core.Entities;

public class FavoriteSupplier
{
    public long Id { get; set; }
    public long CustomerId { get; set; }
    public long SupplierId { get; set; }
    public DateTime? CreatedAt { get; set; }

    // Navigation Properties
    public virtual User Customer { get; set; } = null!;
    public virtual SupplierProfile Supplier { get; set; } = null!;
}