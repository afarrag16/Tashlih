using System;
using System.Collections.Generic;

namespace Tashlih.Core.Entities;

public partial class UserSession
{
    public long Id { get; set; }

    public long UserId { get; set; }

    public string? Token { get; set; }

    public string? RefreshToken { get; set; }

    public string? DeviceType { get; set; }

    public string? DeviceName { get; set; }

    public string? DeviceInfo { get; set; }

    public string? IpAddress { get; set; }

    public string? UserAgent { get; set; }

    public string? FcmToken { get; set; }

    public DateTime ExpiresAt { get; set; }

    public DateTime? LastActivityAt { get; set; }

    public bool IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual User User { get; set; } = null!;
}
