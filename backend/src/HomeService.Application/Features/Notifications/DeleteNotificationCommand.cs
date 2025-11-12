using HomeService.Application.Common;
using HomeService.Domain.Entities;
using HomeService.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HomeService.Application.Features.Notifications;

public record DeleteNotificationCommand(Guid NotificationId, Guid UserId) : IRequest<Result<bool>>;

public class DeleteNotificationCommandHandler
    : IRequestHandler<DeleteNotificationCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteNotificationCommandHandler> _logger;

    public DeleteNotificationCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<DeleteNotificationCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(
        DeleteNotificationCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            var notification = await _unitOfWork.Repository<Notification>()
                .GetByIdAsync(request.NotificationId, cancellationToken);

            if (notification == null)
                return Result.Failure<bool>("Notification not found");

            if (notification.UserId != request.UserId)
                return Result.Failure<bool>("Unauthorized to delete this notification");

            _unitOfWork.Repository<Notification>().Delete(notification);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Notification {NotificationId} deleted by user {UserId}",
                request.NotificationId, request.UserId);

            return Result.Success(true, "Notification deleted successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting notification {NotificationId}", request.NotificationId);
            return Result.Failure<bool>("An error occurred while deleting notification", ex.Message);
        }
    }
}
