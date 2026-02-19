using Tashlih.Application.DTOs.Chat;

namespace Tashlih.Application.Interfaces;

/// <summary>
/// Interface for Chat SignalR notifications
/// </summary>
public interface IChatHubService
{
    /// <summary>
    /// إشعار بمحادثة جديدة
    /// </summary>
    Task SendNewThreadAsync(long userId, object threadData);

    /// <summary>
    /// إشعار برسالة جديدة
    /// </summary>
    Task SendNewMessageAsync(long userId, long threadId, ChatMessageDto message);

    /// <summary>
    /// إشعار باستلام رسالة
    /// </summary>
    Task SendMessageReceivedAsync(long threadId, long messageId, long senderId, string senderType);

    /// <summary>
    /// إشعار بقراءة الرسائل
    /// </summary>
    Task SendMessagesReadAsync(long userId, long threadId, long readBy, string readByType, List<long> messageIds);

    /// <summary>
    /// إشعار بقراءة الرسائل للمحادثة
    /// </summary>
    Task SendMessagesReadToThreadAsync(long threadId, long readBy, string readByType, List<long> messageIds);

    /// <summary>
    /// إشعار بإغلاق المحادثة
    /// </summary>
    Task SendThreadClosedAsync(long userId, long threadId, long closedBy, string closedByType);

    Task SendChatListUpdatedAsync(long userId, long threadId);
}