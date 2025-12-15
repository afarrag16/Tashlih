using System;
using System.Collections.Generic;

namespace Tashlih.Core.Entities;

public partial class ChatThread
{
    public long Id { get; set; }

    public long CustomerId { get; set; }

    public long SupplierId { get; set; }

    public long? OrderId { get; set; }

    public long? PartId { get; set; }

    public string? LastMessage { get; set; }

    public DateTime? LastMessageAt { get; set; }

    public long? LastMessageBy { get; set; }

    public int CustomerUnreadCount { get; set; }

    public int SupplierUnreadCount { get; set; }

    public string Status { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<ChatMessage> ChatMessages { get; set; } = new List<ChatMessage>();

    public virtual User Customer { get; set; } = null!;

    public virtual Order? Order { get; set; }

    public virtual Part? Part { get; set; }

    public virtual SupplierProfile Supplier { get; set; } = null!;
}
