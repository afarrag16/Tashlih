using Microsoft.AspNetCore.SignalR;
using Tashlih.Application.DTOs.Chat;
using Tashlih.Application.Interfaces;
using Tashlih.Api.Hubs;

namespace Tashlih.Api.Services;

/// <summary>
/// Implementation of IChatHubService using SignalR
/// </summary>
public class ChatHubService : IChatHubService
{
    private readonly IHubContext<Hubs.ChatHub> _hubContext;

    public ChatHubService(IHubContext<Hubs.ChatHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task SendNewThreadAsync(long userId, object threadData)
    {
        await _hubContext.Clients.Group($"user_{userId}").SendAsync("NewThread", threadData);
    }

    public async Task SendNewMessageAsync(long userId, long threadId, ChatMessageDto message)
    {
        await _hubContext.Clients.Group($"user_{userId}").SendAsync("NewMessage", new
        {
            ThreadId = threadId,
            Message = message
        });
    }

    public async Task SendMessageReceivedAsync(long threadId, long messageId, long senderId, string senderType)
    {
        await _hubContext.Clients.Group($"thread_{threadId}").SendAsync("MessageReceived", new
        {
            ThreadId = threadId,
            MessageId = messageId,
            SenderId = senderId,
            SenderType = senderType
        });
    }

    public async Task SendMessagesReadAsync(long userId, long threadId, long readBy, string readByType, List<long> messageIds)
    {
        await _hubContext.Clients.Group($"user_{userId}").SendAsync("MessagesRead", new
        {
            ThreadId = threadId,
            ReadBy = readBy,
            ReadByType = readByType,
            ReadAt = DateTime.UtcNow,
            MessageIds = messageIds
        });
    }

    public async Task SendMessagesReadToThreadAsync(long threadId, long readBy, string readByType, List<long> messageIds)
    {
        await _hubContext.Clients.Group($"thread_{threadId}").SendAsync("MessagesRead", new
        {
            ThreadId = threadId,
            ReadBy = readBy,
            ReadByType = readByType,
            ReadAt = DateTime.UtcNow,
            MessageIds = messageIds
        });
    }

    public async Task SendThreadClosedAsync(long userId, long threadId, long closedBy, string closedByType)
    {
        await _hubContext.Clients.Group($"user_{userId}").SendAsync("ThreadClosed", new
        {
            ThreadId = threadId,
            ClosedBy = closedBy,
            ClosedByType = closedByType,
            ClosedAt = DateTime.UtcNow
        });
    }
}
