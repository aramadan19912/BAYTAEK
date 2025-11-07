using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
        [FromQuery] string? type,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        var userId = Guid.Parse(User.FindFirst("sub")?.Value ?? User.FindFirst("userId")?.Value ?? Guid.Empty.ToString());

        var query = new Application.Queries.Notification.GetUserNotificationsQuery
        {
            UserId = userId,
            IsRead = isRead,
            Type = type,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await Mediator.Send(query);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Mark a notification as read
    /// </summary>
    [HttpPut("{id}/read")]
    public async Task<IActionResult> MarkAsRead(Guid id)
    {
        var userId = Guid.Parse(User.FindFirst("sub")?.Value ?? User.FindFirst("userId")?.Value ?? Guid.Empty.ToString());

        var command = new Application.Commands.Notification.MarkNotificationAsReadCommand
        {
            NotificationId = id,
            UserId = userId
        };

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
        var userId = Guid.Parse(User.FindFirst("sub")?.Value ?? User.FindFirst("userId")?.Value ?? Guid.Empty.ToString());

        var command = new Application.Commands.Notification.MarkAllNotificationsAsReadCommand
        {
            UserId = userId
        };

        var result = await Mediator.Send(command);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Get notification settings
    /// </summary>
    [HttpGet("settings")]
    public async Task<IActionResult> GetNotificationSettings()
    {
        // TODO: Implement when NotificationSettings entity is created
        return Ok(new
        {
            emailNotifications = true,
            pushNotifications = true,
            smsNotifications = false,
            message = "Notification settings pending implementation"
        });
    }

    /// <summary>
    /// Update notification settings
    /// </summary>
    [HttpPut("settings")]
    public async Task<IActionResult> UpdateNotificationSettings([FromBody] NotificationSettingsRequest request)
    {
        // TODO: Implement when NotificationSettings entity is created
        return Ok(new { message = "Notification settings update pending implementation" });
    }
}

// Request DTOs
public record NotificationSettingsRequest(
    bool EmailNotifications,
    bool PushNotifications,
    bool SmsNotifications);
