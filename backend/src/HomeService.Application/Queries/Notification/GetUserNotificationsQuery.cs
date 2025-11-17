using HomeService.Application.Common;
using MediatR;

namespace HomeService.Application.Queries.Notification;

public class GetUserNotificationsQuery : IRequest<Result<UserNotificationsDto>>
{
    public Guid UserId { get; set; }
    public bool? IsRead { get; set; } // Filter by read status
    public string? Type { get; set; } // Filter by notification type
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class UserNotificationsDto
{
    public List<NotificationDto> Notifications { get; set; } = new();
    public int TotalCount { get; set; }
    public int UnreadCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

public class NotificationDto
{
    public Guid Id { get; set; }
    public string Type { get; set; } = string.Empty; // "booking", "payment", "review", "system"
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ReadAt { get; set; }

    // Optional metadata for navigation
    public Dictionary<string, string>? Data { get; set; }
}
