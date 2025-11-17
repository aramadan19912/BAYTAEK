using HomeService.Application.Common;
using MediatR;

namespace HomeService.Application.Commands.Notification;

public class MarkNotificationAsReadCommand : IRequest<Result<bool>>
{
    public Guid NotificationId { get; set; }
    public Guid UserId { get; set; }
}
