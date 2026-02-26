using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Tashlih.Application.Interfaces;
using Tashlih.Infrastructure.Models;

namespace Tashlih.Infrastructure.Services;

public class FirebasePushService : IFirebasePushService
{
    private readonly TashlihContext _context;
    private readonly ILogger<FirebasePushService> _logger;
    private readonly bool _isInitialized;

    public FirebasePushService(
        TashlihContext context,
        IConfiguration configuration,
        ILogger<FirebasePushService> logger)
    {
        _context = context;
        _logger = logger;

        // Initialize Firebase if not already initialized
        if (FirebaseApp.DefaultInstance == null)
        {
            try
            {
                var serviceAccountPath = configuration["Firebase:ServiceAccountPath"];
                if (!string.IsNullOrEmpty(serviceAccountPath) && File.Exists(serviceAccountPath))
                {
                    FirebaseApp.Create(new AppOptions()
                    {
                        Credential = GoogleCredential.FromFile(serviceAccountPath)
                    });
                    _isInitialized = true;
                    _logger.LogInformation("Firebase initialized successfully");
                }
                else
                {
                    _logger.LogWarning("Firebase service account file not found: {Path}", serviceAccountPath);
                    _isInitialized = false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize Firebase");
                _isInitialized = false;
            }
        }
        else
        {
            _isInitialized = true;
        }
    }

    public async Task<bool> SendToUserAsync(long userId, string userType, string title, string body, Dictionary<string, string>? data = null)
    {
      
       
        if (!_isInitialized)
        {
            _logger.LogWarning("Firebase not initialized, skipping push notification");
            return false;
        }

        try
        {
            string? fcmToken = null;
            var now = DateTime.UtcNow;

            if (userType == "supplier")
            {
                var session = await _context.SupplierSessions
                    .Where(s => s.SupplierId == userId && s.IsActive && s.FcmToken != null && s.ExpiresAt > now)
                    .OrderByDescending(s => s.UpdatedAt)
                    .FirstOrDefaultAsync();

                fcmToken = session?.FcmToken;
                
            }
            else
            {
                var session = await _context.UserSessions
                    .Where(s => s.UserId == userId && s.IsActive && s.FcmToken != null && s.ExpiresAt > now)
                    .OrderByDescending(s => s.UpdatedAt)
                    .FirstOrDefaultAsync();

                fcmToken = session?.FcmToken;
                
            }

           

            if (string.IsNullOrEmpty(fcmToken))
            {
                _logger.LogInformation("No FCM token found for user {UserId} ({UserType})", userId, userType);
                return false;
            }

            return await SendToTokenAsync(fcmToken, title, body, data);
        }
        catch (Exception ex)
        {
           
            _logger.LogError(ex, "Error sending push notification to user {UserId}", userId);
            return false;
        }
    }

    public async Task<int> SendToUsersAsync(List<long> userIds, string userType, string title, string body, Dictionary<string, string>? data = null)
    {
        if (!_isInitialized || userIds == null || !userIds.Any())
            return 0;

        int successCount = 0;
        foreach (var userId in userIds)
        {
            if (await SendToUserAsync(userId, userType, title, body, data))
                successCount++;
        }

        return successCount;
    }

    public async Task<bool> SendToTokenAsync(string fcmToken, string title, string body, Dictionary<string, string>? data = null)
    {
        if (!_isInitialized)
        {
            _logger.LogWarning("Firebase not initialized, skipping push notification");
            return false;
        }

        if (string.IsNullOrEmpty(fcmToken))
            return false;

        try
        {
            var message = new Message()
            {
                Token = fcmToken,
                Notification = new Notification()
                {
                    Title = title,
                    Body = body
                },
                Android = new AndroidConfig()
                {
                    Priority = Priority.High,
                    Notification = new AndroidNotification()
                    {
                        Sound = "default",
                        ClickAction = "FLUTTER_NOTIFICATION_CLICK"
                    }
                },
                Apns = new ApnsConfig()
                {
                    Aps = new Aps()
                    {
                        Sound = "default",
                        Badge = 1
                    }
                }
            };

            // إضافة Data إن وجدت
            if (data != null && data.Any())
            {
                message.Data = data;
            }

            var response = await FirebaseMessaging.DefaultInstance.SendAsync(message);
            _logger.LogInformation("Push notification sent successfully: {Response}", response);
            return true;
        }
        catch (FirebaseMessagingException ex)
        {
            _logger.LogError(ex, "Firebase messaging error: {Code} - {Message}", ex.MessagingErrorCode, ex.Message);

            // لو الـ Token غير صالح، ممكن نحذفه من الداتابيز
            if (ex.MessagingErrorCode == MessagingErrorCode.Unregistered)
            {
                _logger.LogWarning("FCM token is unregistered, should be removed: {Token}", fcmToken);
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending push notification");
            return false;
        }
    }

    public async Task<int> SendToTokensAsync(List<string> fcmTokens, string title, string body, Dictionary<string, string>? data = null)
    {
        if (!_isInitialized || fcmTokens == null || !fcmTokens.Any())
            return 0;

        // Firebase يدعم إرسال لـ 500 token في المرة الواحدة
        var validTokens = fcmTokens.Where(t => !string.IsNullOrEmpty(t)).ToList();
        if (!validTokens.Any())
            return 0;

        try
        {
            var message = new MulticastMessage()
            {
                Tokens = validTokens,
                Notification = new Notification()
                {
                    Title = title,
                    Body = body
                },
                Android = new AndroidConfig()
                {
                    Priority = Priority.High,
                    Notification = new AndroidNotification()
                    {
                        ChannelId = "high_importance_channel",
                        Sound = "default",
                        ClickAction = "FLUTTER_NOTIFICATION_CLICK"
                    }
                },
                Apns = new ApnsConfig()
                {
                    Aps = new Aps()
                    {
                        Sound = "default",
                        Badge = 1,
                        ContentAvailable = true
                    }
                }
            };

            if (data != null && data.Any())
            {
                message.Data = data;
            }

            var response = await FirebaseMessaging.DefaultInstance.SendEachForMulticastAsync(message);
            _logger.LogInformation("Multicast push sent: {Success}/{Total} successful", response.SuccessCount, validTokens.Count);

            return response.SuccessCount;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending multicast push notification");
            return 0;
        }
    }
}