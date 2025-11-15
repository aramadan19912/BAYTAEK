using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace HomeService.API.Hubs;

[Authorize]
public class ChatHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!string.IsNullOrEmpty(userId))
        {
            // Add user to their personal group
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!string.IsNullOrEmpty(userId))
        {
            // Remove user from their personal group
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user_{userId}");

            // Notify others that user is offline
            await Clients.AllExcept(Context.ConnectionId).SendAsync("UserOffline", userId);
        }

        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Join a conversation
    /// </summary>
    public async Task JoinConversation(string conversationId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"conversation_{conversationId}");
    }

    /// <summary>
    /// Leave a conversation
    /// </summary>
    public async Task LeaveConversation(string conversationId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"conversation_{conversationId}");
    }

    /// <summary>
    /// Send a message to a conversation
    /// </summary>
    public async Task SendMessage(string conversationId, string messageId, string content, DateTime timestamp)
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var userName = Context.User?.FindFirst(ClaimTypes.Name)?.Value;

        await Clients.Group($"conversation_{conversationId}").SendAsync("ReceiveMessage", new
        {
            MessageId = messageId,
            ConversationId = conversationId,
            SenderId = userId,
            SenderName = userName,
            Content = content,
            Timestamp = timestamp
        });
    }

    /// <summary>
    /// Send typing indicator
    /// </summary>
    public async Task SendTypingIndicator(string conversationId, bool isTyping)
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var userName = Context.User?.FindFirst(ClaimTypes.Name)?.Value;

        await Clients.OthersInGroup($"conversation_{conversationId}").SendAsync("UserTyping", new
        {
            UserId = userId,
            UserName = userName,
            ConversationId = conversationId,
            IsTyping = isTyping
        });
    }

    /// <summary>
    /// Mark messages as read
    /// </summary>
    public async Task MarkAsRead(string conversationId, string messageId)
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        await Clients.Group($"conversation_{conversationId}").SendAsync("MessageRead", new
        {
            ConversationId = conversationId,
            MessageId = messageId,
            ReadBy = userId,
            ReadAt = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Notify user is online
    /// </summary>
    public async Task NotifyOnline()
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!string.IsNullOrEmpty(userId))
        {
            await Clients.All.SendAsync("UserOnline", userId);
        }
    }
}
