using System;
using System.Collections.Generic;

namespace Tashlih.Core.Entities;

public partial class PartCategory
{
    public long Id { get; set; }

    public long? ParentId { get; set; }

    public string NameAr { get; set; } = null!;

    public string? NameEn { get; set; }

    public string? DescriptionAr { get; set; }

    public string? Icon { get; set; }

    public string? ImageUrl { get; set; }

    public int SortOrder { get; set; }

    public byte Level { get; set; }

    public bool IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<PartCategory> InverseParent { get; set; } = new List<PartCategory>();

    public virtual PartCategory? Parent { get; set; }

    public virtual ICollection<PartCategoryMapping> PartCategoryMappings { get; set; } = new List<PartCategoryMapping>();

    public virtual ICollection<Part> Parts { get; set; } = new List<Part>();
}
