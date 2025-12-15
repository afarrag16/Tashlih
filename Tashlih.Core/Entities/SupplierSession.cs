using System;

namespace Tashlih.Core.Entities;

public class SupplierSession
{
    public long Id { get; set; }
    public long SupplierId { get; set; }
    public string? Token { get; set; } = null!;
    public string? DeviceType { get; set; }
    public string? DeviceName { get; set; }
    public string? FcmToken { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime? LastActivityAt { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public virtual SupplierProfile Supplier { get; set; } = null!;
}
