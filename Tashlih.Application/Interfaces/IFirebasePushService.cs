using Tashlih.Application.DTOs.Notification;

namespace Tashlih.Application.Interfaces;

public interface IFirebasePushService
{
    /// <summary>
    /// إرسال إشعار لمستخدم واحد
    /// </summary>
    Task<bool> SendToUserAsync(long userId, string userType, string title, string body, Dictionary<string, string>? data = null);

    /// <summary>
    /// إرسال إشعار لعدة مستخدمين
    /// </summary>
    Task<int> SendToUsersAsync(List<long> userIds, string userType, string title, string body, Dictionary<string, string>? data = null);

    /// <summary>
    /// إرسال إشعار لـ Token محدد
    /// </summary>
    Task<bool> SendToTokenAsync(string fcmToken, string title, string body, Dictionary<string, string>? data = null);

    /// <summary>
    /// إرسال إشعار لعدة Tokens
    /// </summary>
    Task<int> SendToTokensAsync(List<string> fcmTokens, string title, string body, Dictionary<string, string>? data = null);
}
