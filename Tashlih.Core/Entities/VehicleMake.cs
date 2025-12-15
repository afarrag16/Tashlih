using System;
using System.Collections.Generic;

namespace Tashlih.Core.Entities;

public partial class VehicleMake
{
    public int Id { get; set; }

    public int VehicleTypeId { get; set; }

    public string NameAr { get; set; } = null!;

    public string NameEn { get; set; } = null!;

    public string? LogoUrl { get; set; }

    public string? Country { get; set; }

    public int SortOrder { get; set; }

    public bool IsPopular { get; set; }

    public bool IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<Part> Parts { get; set; } = new List<Part>();

    public virtual ICollection<VehicleModel> VehicleModels { get; set; } = new List<VehicleModel>();

    public virtual VehicleType VehicleType { get; set; } = null!;
}
