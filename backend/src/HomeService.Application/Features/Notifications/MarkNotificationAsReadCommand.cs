using HomeService.Application.Common;
using HomeService.Domain.Entities;
using HomeService.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HomeService.Application.Features.Notifications;

public record MarkNotificationAsReadCommand(Guid NotificationId, Guid UserId) : IRequest<Result<bool>>;

public class MarkNotificationAsReadCommandHandler
    : IRequestHandler<MarkNotificationAsReadCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<MarkNotificationAsReadCommandHandler> _logger;

    public MarkNotificationAsReadCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<MarkNotificationAsReadCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(
        MarkNotificationAsReadCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            var notification = await _unitOfWork.Repository<Notification>()
                .GetByIdAsync(request.NotificationId, cancellationToken);

            if (notification == null)
                return Result.Failure<bool>("Notification not found");

            if (notification.UserId != request.UserId)
                return Result.Failure<bool>("Unauthorized to mark this notification as read");

            if (notification.IsRead)
                return Result.Success(true, "Notification already marked as read");

            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;

            _unitOfWork.Repository<Notification>().Update(notification);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Notification {NotificationId} marked as read by user {UserId}",
                request.NotificationId, request.UserId);

            return Result.Success(true, "Notification marked as read");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking notification {NotificationId} as read", request.NotificationId);
            return Result.Failure<bool>("An error occurred while marking notification as read", ex.Message);
        }
    }
}
