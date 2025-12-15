using System;
using System.Collections.Generic;

namespace Tashlih.Core.Entities;

public partial class PartCategoryMapping
{
    public int Id { get; set; }

    public int VehicleTypeId { get; set; }

    public long CategoryId { get; set; }

    public bool IsActive { get; set; }

    public virtual PartCategory Category { get; set; } = null!;

    public virtual VehicleType VehicleType { get; set; } = null!;
}
