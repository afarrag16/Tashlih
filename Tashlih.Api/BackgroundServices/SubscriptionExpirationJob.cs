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
            // حساب الوقت للساعة 9 صباحاً
            var now = DateTime.Now;
            var nextRun = now.Date.AddHours(9);

            if (now > nextRun)
            {
                nextRun = nextRun.AddDays(1);
            }

            var delay = nextRun - now;
            _logger.LogInformation($"Next subscription check at: {nextRun}");

            await Task.Delay(delay, stoppingToken);

            try
            {
                using var scope = _serviceProvider.CreateScope();
                var notificationService = scope.ServiceProvider.GetRequiredService<SubscriptionNotificationService>();

                await notificationService.SendExpirationNotificationsAsync();
                _logger.LogInformation("Subscription expiration notifications sent successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending subscription expiration notifications");
            }
        }
    }
}