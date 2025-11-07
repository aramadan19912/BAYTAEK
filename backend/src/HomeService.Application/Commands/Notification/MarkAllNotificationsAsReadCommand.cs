using HomeService.Application.Common.Models;
using MediatR;

namespace HomeService.Application.Commands.Notification;

public class MarkAllNotificationsAsReadCommand : IRequest<Result<int>>
{
    public Guid UserId { get; set; }
}
