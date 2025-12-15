using System;
using System.Collections.Generic;

namespace Tashlih.Core.Entities;

public partial class VehicleModel
{
    public int Id { get; set; }

    public int MakeId { get; set; }

    public string NameAr { get; set; } = null!;

    public string NameEn { get; set; } = null!;

    public short? YearFrom { get; set; }

    public short? YearTo { get; set; }

    public bool IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual VehicleMake Make { get; set; } = null!;

    public virtual ICollection<Part> Parts { get; set; } = new List<Part>();
}
