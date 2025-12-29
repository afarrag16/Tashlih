using System;

namespace Tashlih.Core.Entities;

public class FavoritePart
{
    public long Id { get; set; }
    public long CustomerId { get; set; }
    public long PartId { get; set; }
    public DateTime? CreatedAt { get; set; }

    // Navigation Properties
    public virtual User Customer { get; set; } = null!;
    public virtual Part Part { get; set; } = null!;
}
