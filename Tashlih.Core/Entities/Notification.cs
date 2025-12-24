using System;
using System.Collections.Generic;

namespace Tashlih.Core.Entities;

public partial class Notification
{
    public long Id { get; set; }

    public long UserId { get; set; }

    public string Type { get; set; } = null!;

    public string Title { get; set; } = null!;

    public string Body { get; set; } = null!;

    public string? ImageUrl { get; set; }

    public string? Data { get; set; }
    public string UserType { get; set; } = "customer";

    public string Priority { get; set; } = null!;

    public bool IsRead { get; set; }

    public DateTime? ReadAt { get; set; }

    public bool IsPushSent { get; set; }

    public DateTime? PushSentAt { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual User User { get; set; } = null!;
}
