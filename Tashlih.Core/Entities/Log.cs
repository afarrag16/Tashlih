using System;

namespace Tashlih.Core.Entities;

public partial class Log
{
    public long Id { get; set; }

    // المستخدم اللي عمل العملية
    public long? UserId { get; set; }
    public string? UserType { get; set; }  // 👈 جديد: admin, supplier, customer
    public string? UserName { get; set; }  // 👈 جديد: اسم المستخدم للعرض

    // العملية
    public string Action { get; set; } = null!;
    public string? ActionAr { get; set; }  // 👈 جديد: اسم العملية بالعربي

    // الكيان المتأثر
    public string? EntityType { get; set; }
    public string? EntityTypeAr { get; set; }  // 👈 جديد: نوع الكيان بالعربي
    public long? EntityId { get; set; }

    // التفاصيل
    public string? Description { get; set; }
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }

    // معلومات الجهاز
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }

    public string? Context { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}