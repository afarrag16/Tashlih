using System;
using System.Collections.Generic;

namespace Tashlih.Core.Entities;

public partial class PartImage
{
    public long Id { get; set; }

    public long PartId { get; set; }

    public string ImageUrl { get; set; } = null!;

    public string? ThumbnailUrl { get; set; }

    public bool IsPrimary { get; set; }

    public byte DisplayOrder { get; set; }

    public int? FileSize { get; set; }

    public string? MimeType { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Part Part { get; set; } = null!;
}
