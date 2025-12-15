using System;
using System.Collections.Generic;

namespace Tashlih.Core.Entities;

public partial class VehicleType
{
    public int Id { get; set; }

    public string NameAr { get; set; } = null!;

    public string NameEn { get; set; } = null!;

    public string? Icon { get; set; }

    public int SortOrder { get; set; }

    public bool IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<PartCategoryMapping> PartCategoryMappings { get; set; } = new List<PartCategoryMapping>();

    public virtual ICollection<Part> Parts { get; set; } = new List<Part>();

    public virtual ICollection<VehicleMake> VehicleMakes { get; set; } = new List<VehicleMake>();

    public virtual ICollection<VehicleSubcategory> VehicleSubcategories { get; set; } = new List<VehicleSubcategory>();
}
