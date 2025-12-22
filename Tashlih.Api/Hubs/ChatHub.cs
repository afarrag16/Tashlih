using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Tashlih.Api.Hubs;

[Authorize]
public class ChatHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!string.IsNullOrEmpty(userId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!string.IsNullOrEmpty(userId))
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user_{userId}");
        }

        await base.OnDisconnectedAsync(exception);
    }

    public async Task JoinThread(long threadId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"thread_{threadId}");
    }

    public async Task LeaveThread(long threadId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"thread_{threadId}");
    }

    public async Task Typing(long threadId)
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var userType = Context.User?.FindFirst("user_type")?.Value;

        await Clients.OthersInGroup($"thread_{threadId}").SendAsync("UserTyping", new
        {
            ThreadId = threadId,
            UserId = userId,
            UserType = userType
        });
    }

    public async Task StopTyping(long threadId)
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        await Clients.OthersInGroup($"thread_{threadId}").SendAsync("UserStoppedTyping", new
        {
            ThreadId = threadId,
            UserId = userId
        });
    }
}