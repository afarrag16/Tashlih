using System;
using System.Collections.Generic;

namespace Tashlih.Core.Entities;

public partial class ChatAttachment
{
    public long Id { get; set; }

    public long MessageId { get; set; }

    public string FileType { get; set; } = null!;

    public string FileUrl { get; set; } = null!;

    public string? FileName { get; set; }

    public int? FileSize { get; set; }

    public string? MimeType { get; set; }

    public string? ThumbnailUrl { get; set; }

    public int? Width { get; set; }

    public int? Height { get; set; }

    public int? Duration { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ChatMessage Message { get; set; } = null!;
}
