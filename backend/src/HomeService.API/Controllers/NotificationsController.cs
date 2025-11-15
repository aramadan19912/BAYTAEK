using HomeService.Application.Features.Notifications;
using HomeService.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HomeService.API.Controllers;

[Authorize]
public class NotificationsController : BaseApiController
{
    /// <summary>
    /// Get user notifications with filters
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetNotifications(
        [FromQuery] bool? isRead,
        [FromQuery] NotificationType? type,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty)
            return Unauthorized();

        var query = new GetNotificationsQuery(userId, isRead, type, pageNumber, pageSize);
        var result = await Mediator.Send(query);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Get unread notifications count
    /// </summary>
    [HttpGet("unread-count")]
    public async Task<IActionResult> GetUnreadCount()
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty)
            return Unauthorized();

        var query = new GetUnreadCountQuery(userId);
        var result = await Mediator.Send(query);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Mark a specific notification as read
    /// </summary>
    [HttpPut("{id:guid}/read")]
    public async Task<IActionResult> MarkAsRead(Guid id)
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty)
            return Unauthorized();

        var command = new MarkNotificationAsReadCommand(id, userId);
        var result = await Mediator.Send(command);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Mark all notifications as read
    /// </summary>
    [HttpPut("read-all")]
    public async Task<IActionResult> MarkAllAsRead()
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty)
            return Unauthorized();

        var command = new MarkAllNotificationsAsReadCommand(userId);
        var result = await Mediator.Send(command);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Delete a notification
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteNotification(Guid id)
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty)
            return Unauthorized();

        var command = new DeleteNotificationCommand(id, userId);
        var result = await Mediator.Send(command);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Get notification settings for the current user
    /// </summary>
    [HttpGet("settings")]
    public async Task<IActionResult> GetNotificationSettings()
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty)
            return Unauthorized();

        var query = new GetNotificationSettingsQuery(userId);
        var result = await Mediator.Send(query);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Update notification settings
    /// </summary>
    [HttpPut("settings")]
    public async Task<IActionResult> UpdateNotificationSettings([FromBody] UpdateNotificationSettingsRequest request)
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty)
            return Unauthorized();

        var command = new UpdateNotificationSettingsCommand
        {
            UserId = userId,
            EmailNotifications = request.EmailNotifications,
            PushNotifications = request.PushNotifications,
            SmsNotifications = request.SmsNotifications,
            BookingNotifications = request.BookingNotifications,
            MessageNotifications = request.MessageNotifications,
            PaymentNotifications = request.PaymentNotifications,
            PromotionNotifications = request.PromotionNotifications,
            SystemNotifications = request.SystemNotifications
        };

        var result = await Mediator.Send(command);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Register device token for push notifications
    /// </summary>
    [HttpPost("device-token")]
    public async Task<IActionResult> RegisterDeviceToken([FromBody] RegisterDeviceTokenRequest request)
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty)
            return Unauthorized();

        var command = new RegisterDeviceTokenCommand
        {
            UserId = userId,
            Token = request.Token,
            DeviceType = request.DeviceType,
            DeviceId = request.DeviceId
        };

        var result = await Mediator.Send(command);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Unregister device token
    /// </summary>
    [HttpDelete("device-token")]
    public async Task<IActionResult> UnregisterDeviceToken([FromBody] UnregisterDeviceTokenRequest request)
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty)
            return Unauthorized();

        var command = new UnregisterDeviceTokenCommand
        {
            UserId = userId,
            Token = request.Token
        };

        var result = await Mediator.Send(command);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    #region Private Helper Methods

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
    }

    #endregion
}

// Request DTOs
public record UpdateNotificationSettingsRequest(
    bool EmailNotifications,
    bool PushNotifications,
    bool SmsNotifications,
    bool BookingNotifications,
    bool MessageNotifications,
    bool PaymentNotifications,
    bool PromotionNotifications,
    bool SystemNotifications
);

public record RegisterDeviceTokenRequest(
    string Token,
    string DeviceType,
    string? DeviceId = null
);

public record UnregisterDeviceTokenRequest(string Token);
