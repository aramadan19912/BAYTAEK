using HomeService.Application.Common;
using HomeService.Domain.Entities;
using HomeService.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace HomeService.Application.Features.Notifications;

public record MarkAllNotificationsAsReadCommand(Guid UserId) : IRequest<Result<int>>;

public class MarkAllNotificationsAsReadCommandHandler
    : IRequestHandler<MarkAllNotificationsAsReadCommand, Result<int>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<MarkAllNotificationsAsReadCommandHandler> _logger;

    public MarkAllNotificationsAsReadCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<MarkAllNotificationsAsReadCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<int>> Handle(
        MarkAllNotificationsAsReadCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            var unreadNotifications = await _unitOfWork.Repository<Notification>()
                .GetQueryable()
                .Where(n => n.UserId == request.UserId && !n.IsRead)
                .ToListAsync(cancellationToken);

            if (!unreadNotifications.Any())
                return Result.Success(0, "No unread notifications");

            foreach (var notification in unreadNotifications)
            {
                notification.IsRead = true;
                notification.ReadAt = DateTime.UtcNow;
                _unitOfWork.Repository<Notification>().Update(notification);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Marked {Count} notifications as read for user {UserId}",
                unreadNotifications.Count, request.UserId);

            return Result.Success(unreadNotifications.Count,
                $"Marked {unreadNotifications.Count} notifications as read");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking all notifications as read for user {UserId}", request.UserId);
            return Result.Failure<int>("An error occurred while marking notifications as read", ex.Message);
        }
    }
}
