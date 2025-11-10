using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace HomeService.API.Hubs;

[Authorize]
public class NotificationHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!string.IsNullOrEmpty(userId))
        {
            // Add user to their notification group
            await Groups.AddToGroupAsync(Context.ConnectionId, $"notifications_{userId}");
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!string.IsNullOrEmpty(userId))
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"notifications_{userId}");
        }

        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Mark notification as read
    /// </summary>
    public async Task MarkAsRead(string notificationId)
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        // Notify the user that notification was marked as read
        await Clients.Caller.SendAsync("NotificationRead", notificationId, DateTime.UtcNow);
    }

    /// <summary>
    /// Mark all notifications as read
    /// </summary>
    public async Task MarkAllAsRead()
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        // Notify the user that all notifications were marked as read
        await Clients.Caller.SendAsync("AllNotificationsRead", DateTime.UtcNow);
    }

    /// <summary>
    /// Get unread notification count
    /// </summary>
    public async Task GetUnreadCount()
    {
        // This would typically call a service to get the count
        // For now, just acknowledge the request
        await Clients.Caller.SendAsync("UnreadCountRequested");
    }
}
