using Microsoft.EntityFrameworkCore;
using Tashlih.Core.Entities;
using Tashlih.Infrastructure.Models;

namespace Tashlih.Infrastructure.Services;

public class SubscriptionNotificationService
{
    private readonly TashlihContext _context;

    public SubscriptionNotificationService(TashlihContext context)
    {
        _context = context;
    }

    /// <summary>
    /// إرسال إشعارات انتهاء الباقات
    /// </summary>
    public async Task SendExpirationNotificationsAsync()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        // الاشتراكات اللي هتنتهي خلال 7 أيام
        var in7Days = today.AddDays(7);
        var expiring7Days = await _context.Subscriptions
            .Include(s => s.Plan)
            .Where(s => s.Status == "active" && s.EndsAt == in7Days)
            .ToListAsync();

        foreach (var sub in expiring7Days)
        {
            await CreateNotification(sub.SupplierId,
                "تنبيه انتهاء الباقة",
                $"باقتك ({sub.Plan.NameAr}) ستنتهي خلال 7 أيام. جدد الآن للحفاظ على ظهور قطعك.",
                "subscription_expiring");
        }

        // الاشتراكات اللي هتنتهي خلال 3 أيام
        var in3Days = today.AddDays(3);
        var expiring3Days = await _context.Subscriptions
            .Include(s => s.Plan)
            .Where(s => s.Status == "active" && s.EndsAt == in3Days)
            .ToListAsync();

        foreach (var sub in expiring3Days)
        {
            await CreateNotification(sub.SupplierId,
                "تنبيه عاجل",
                $"باقتك ({sub.Plan.NameAr}) ستنتهي خلال 3 أيام! جدد الآن.",
                "subscription_expiring_urgent");
        }

        // الاشتراكات اللي انتهت اليوم
        var expiredToday = await _context.Subscriptions
            .Include(s => s.Plan)
            .Where(s => s.Status == "active" && s.EndsAt == today)
            .ToListAsync();

        foreach (var sub in expiredToday)
        {
            // تحديث الحالة لـ expired
            sub.Status = "expired";

            await CreateNotification(sub.SupplierId,
                "انتهت باقتك",
                $"انتهت باقتك ({sub.Plan.NameAr}). قطعك مخفية الآن عن العملاء. جدد اشتراكك لإظهارها.",
                "subscription_expired");
        }

        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// إنشاء إشعار
    /// </summary>
    private async Task CreateNotification(long supplierId, string title, string body, string type)
    {
        // تحقق من عدم وجود إشعار مكرر اليوم
        var today = DateTime.UtcNow.Date;
        var exists = await _context.Notifications
            .AnyAsync(n => n.UserId == supplierId
                && n.UserType == "supplier"
                && n.Type == type
                && n.CreatedAt >= today);

        if (exists) return;

        var notification = new Notification
        {
            UserId = supplierId,
            UserType = "supplier",
            Type = type,
            Title = title,
            Body = body,
            Priority = type.Contains("urgent") || type.Contains("expired") ? "high" : "normal",
            IsRead = false,
            IsPushSent = false,
            CreatedAt = DateTime.UtcNow
        };

        _context.Notifications.Add(notification);
    }
}