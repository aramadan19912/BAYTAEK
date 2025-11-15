using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace HomeService.API.Hubs;

[Authorize]
public class BookingHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!string.IsNullOrEmpty(userId))
        {
            // Add user to their booking updates group
            await Groups.AddToGroupAsync(Context.ConnectionId, $"bookings_{userId}");
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!string.IsNullOrEmpty(userId))
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"bookings_{userId}");
        }

        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Join a specific booking's update group
    /// </summary>
    public async Task JoinBooking(string bookingId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"booking_{bookingId}");
    }

    /// <summary>
    /// Leave a specific booking's update group
    /// </summary>
    public async Task LeaveBooking(string bookingId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"booking_{bookingId}");
    }

    /// <summary>
    /// Request current booking status
    /// </summary>
    public async Task RequestBookingStatus(string bookingId)
    {
        // This would typically call a service to get the current status
        // For now, just acknowledge the request
        await Clients.Caller.SendAsync("BookingStatusRequested", bookingId);
    }

    /// <summary>
    /// Update provider location (for tracking)
    /// </summary>
    public async Task UpdateProviderLocation(string bookingId, double latitude, double longitude)
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        // Broadcast location to customers tracking this booking
        await Clients.Group($"booking_{bookingId}").SendAsync("ProviderLocationUpdated", new
        {
            BookingId = bookingId,
            ProviderId = userId,
            Latitude = latitude,
            Longitude = longitude,
            Timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Provider marks service as started
    /// </summary>
    public async Task ServiceStarted(string bookingId)
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        await Clients.Group($"booking_{bookingId}").SendAsync("ServiceStarted", new
        {
            BookingId = bookingId,
            ProviderId = userId,
            StartedAt = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Provider marks service as completed
    /// </summary>
    public async Task ServiceCompleted(string bookingId)
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        await Clients.Group($"booking_{bookingId}").SendAsync("ServiceCompleted", new
        {
            BookingId = bookingId,
            ProviderId = userId,
            CompletedAt = DateTime.UtcNow
        });
    }
}
