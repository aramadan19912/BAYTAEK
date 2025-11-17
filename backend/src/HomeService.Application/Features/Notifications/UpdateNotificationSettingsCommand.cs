using HomeService.Application.Common;
using HomeService.Domain.Entities;
using HomeService.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace HomeService.Application.Features.Notifications;

public class UpdateNotificationSettingsCommand : IRequest<Result<bool>>
{
    public Guid UserId { get; set; }
    public bool EmailNotifications { get; set; }
    public bool PushNotifications { get; set; }
    public bool SmsNotifications { get; set; }
    public bool BookingNotifications { get; set; }
    public bool MessageNotifications { get; set; }
    public bool PaymentNotifications { get; set; }
    public bool PromotionNotifications { get; set; }
    public bool SystemNotifications { get; set; }
}

public class UpdateNotificationSettingsCommandHandler
    : IRequestHandler<UpdateNotificationSettingsCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateNotificationSettingsCommandHandler> _logger;

    public UpdateNotificationSettingsCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<UpdateNotificationSettingsCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(
        UpdateNotificationSettingsCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            var settings = await _unitOfWork.Repository<NotificationSettings>()
                .GetQueryable()
                .FirstOrDefaultAsync(s => s.UserId == request.UserId, cancellationToken);

            if (settings == null)
            {
                // Create new settings
                settings = new NotificationSettings
                {
                    UserId = request.UserId,
                    EmailNotifications = request.EmailNotifications,
                    PushNotifications = request.PushNotifications,
                    SmsNotifications = request.SmsNotifications,
                    BookingNotifications = request.BookingNotifications,
                    MessageNotifications = request.MessageNotifications,
                    PaymentNotifications = request.PaymentNotifications,
                    PromotionNotifications = request.PromotionNotifications,
                    SystemNotifications = request.SystemNotifications
                };

                await _unitOfWork.Repository<NotificationSettings>().AddAsync(settings, cancellationToken);
            }
            else
            {
                // Update existing settings
                settings.EmailNotifications = request.EmailNotifications;
                settings.PushNotifications = request.PushNotifications;
                settings.SmsNotifications = request.SmsNotifications;
                settings.BookingNotifications = request.BookingNotifications;
                settings.MessageNotifications = request.MessageNotifications;
                settings.PaymentNotifications = request.PaymentNotifications;
                settings.PromotionNotifications = request.PromotionNotifications;
                settings.SystemNotifications = request.SystemNotifications;

                _unitOfWork.Repository<NotificationSettings>().Update(settings);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Notification settings updated for user {UserId}", request.UserId);

            return Result.Success(true, "Notification settings updated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating notification settings for user {UserId}", request.UserId);
            return Result.Failure<bool>(
                "An error occurred while updating notification settings",
                ex.Message);
        }
    }
}
