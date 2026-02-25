using Microsoft.EntityFrameworkCore;
using Tashlih.Core.Entities;
using Tashlih.Infrastructure.Models;
using Tashlih.Infrastructure.Services;

namespace Tashlih.Api.BackgroundServices;

public class SubscriptionExpirationJob : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<SubscriptionExpirationJob> _logger;

    public SubscriptionExpirationJob(IServiceProvider serviceProvider, ILogger<SubscriptionExpirationJob> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Running subscription check at: {Time} (UTC)", DateTime.UtcNow);

            try
            {
                using var scope = _serviceProvider.CreateScope();
                var notificationService = scope.ServiceProvider
                    .GetRequiredService<SubscriptionNotificationService>();

                await notificationService.SendExpirationNotificationsAsync();
                _logger.LogInformation("Subscription expiration notifications sent successfully");

                // ✅ تحويل الاشتراكات المنتهية للباقة المجانية
                await ConvertExpiredSubscriptionsToFreeAsync();

                // ✅ تنظيف الـ Sessions المنتهية
                await CleanupExpiredSessionsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in subscription expiration job");
            }

            // ✅ انتظر ساعة قبل التشغيل التالي
            await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
        }
    }

    // ✅ تحويل الاشتراكات المنتهية للباقة المجانية
    private async Task ConvertExpiredSubscriptionsToFreeAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<TashlihContext>();

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        // جيب الاشتراكات المنتهية (مدفوعة فقط)
        var expiredSubscriptions = await context.Subscriptions
            .Include(s => s.Plan)
            .Where(s => s.Status == "active" && s.EndsAt < today && s.Plan.Price > 0)
            .ToListAsync();

        if (!expiredSubscriptions.Any())
        {
            _logger.LogInformation("No expired paid subscriptions found");
            return;
        }

        // جيب الباقة المجانية
        var freePlan = await context.SubscriptionPlans
            .Where(p => p.Price == 0 && p.IsActive)
            .OrderBy(p => p.Id)
            .FirstOrDefaultAsync();

        if (freePlan == null)
        {
            _logger.LogWarning("No free plan found, expired subscriptions will be marked as expired only");

            // لو مفيش باقة مجانية، بس نعمل الاشتراكات expired
            foreach (var sub in expiredSubscriptions)
            {
                sub.Status = "expired";
                sub.UpdatedAt = DateTime.UtcNow;
            }

            await context.SaveChangesAsync();
            return;
        }

        foreach (var expiredSub in expiredSubscriptions)
        {
            // 1. عدل الاشتراك القديم لـ expired
            expiredSub.Status = "expired";
            expiredSub.UpdatedAt = DateTime.UtcNow;

            // 2. أنشئ اشتراك جديد في الباقة المجانية
            var newSubscription = new Subscription
            {
                SupplierId = expiredSub.SupplierId,
                PlanId = freePlan.Id,
                Status = "active",
                StartsAt = today,
                EndsAt = today.AddDays(freePlan.DurationDays),
                AmountPaid = 0,
                PaymentMethod = "free",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            context.Subscriptions.Add(newSubscription);

            _logger.LogInformation(
                "Supplier {SupplierId} moved from plan {OldPlan} to free plan",
                expiredSub.SupplierId,
                expiredSub.Plan?.NameAr ?? "Unknown"
            );
        }

        await context.SaveChangesAsync();
        _logger.LogInformation("Converted {Count} expired subscriptions to free plan", expiredSubscriptions.Count);
    }

    // ✅ تنظيف الـ Sessions المنتهية
    private async Task CleanupExpiredSessionsAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<TashlihContext>();

        var now = DateTime.UtcNow;
        var expiredSessions = await context.UserSessions
            .Where(s => s.IsActive && s.ExpiresAt <= now)
            .ToListAsync();

        foreach (var session in expiredSessions)
        {
            session.FcmToken = null;
            session.IsActive = false;
            session.UpdatedAt = now;
        }

        if (expiredSessions.Count > 0)
        {
            await context.SaveChangesAsync();
            _logger.LogInformation("Cleaned up {Count} expired sessions", expiredSessions.Count);
        }
    }
}