using System;
using System.Collections.Generic;

namespace Tashlih.Core.Entities;

public partial class Log
{
    public long Id { get; set; }

    public long? UserId { get; set; }

    public string Action { get; set; } = null!;

    public string? EntityType { get; set; }

    public long? EntityId { get; set; }

    public string? Description { get; set; }

    public string? OldValues { get; set; }

    public string? NewValues { get; set; }

    public string? IpAddress { get; set; }

    public string? UserAgent { get; set; }

    public string? Context { get; set; }

    public DateTime? CreatedAt { get; set; }
}
