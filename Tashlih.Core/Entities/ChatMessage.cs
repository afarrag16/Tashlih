using System;
using System.Collections.Generic;

namespace Tashlih.Core.Entities;

public partial class ChatMessage
{
    public long Id { get; set; }

    public long ThreadId { get; set; }

    public long SenderId { get; set; }

    public string SenderType { get; set; } = null!;

    public string MessageType { get; set; } = null!;

    public string? Content { get; set; }

    public string? Metadata { get; set; }

    public bool IsRead { get; set; }

    public DateTime? ReadAt { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<ChatAttachment> ChatAttachments { get; set; } = new List<ChatAttachment>();

    public virtual User Sender { get; set; } = null!;

    public virtual ChatThread Thread { get; set; } = null!;
}
