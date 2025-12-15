using System;
using System.Collections.Generic;

namespace Tashlih.Core.Entities;

public partial class Attachment
{
    public long Id { get; set; }

    public string AttachableType { get; set; } = null!;

    public long AttachableId { get; set; }

    public long? UploadedBy { get; set; }

    public string FileName { get; set; } = null!;

    public string FilePath { get; set; } = null!;

    public string FileUrl { get; set; } = null!;

    public string? FileType { get; set; }

    public int? FileSize { get; set; }

    public string? MimeType { get; set; }

    public string? ThumbnailUrl { get; set; }

    public string? Purpose { get; set; }

    public string Visibility { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public virtual User? UploadedByNavigation { get; set; }
}
