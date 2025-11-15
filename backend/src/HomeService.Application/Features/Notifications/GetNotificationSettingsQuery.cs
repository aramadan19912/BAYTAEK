using HomeService.Application.Common;
using HomeService.Domain.Entities;
using HomeService.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HomeService.Application.Features.Notifications;

public record GetNotificationSettingsQuery(Guid UserId) : IRequest<Result<NotificationSettingsDto>>;

public class GetNotificationSettingsQueryHandler
    : IRequestHandler<GetNotificationSettingsQuery, Result<NotificationSettingsDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetNotificationSettingsQueryHandler> _logger;

    public GetNotificationSettingsQueryHandler(
        IUnitOfWork unitOfWork,
        ILogger<GetNotificationSettingsQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<NotificationSettingsDto>> Handle(
        GetNotificationSettingsQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var settings = await _unitOfWork.Repository<NotificationSettings>()
                .GetQueryable()
                .FirstOrDefaultAsync(s => s.UserId == request.UserId, cancellationToken);

            if (settings == null)
            {
                // Return default settings
                return Result.Success(new NotificationSettingsDto
                {
                    EmailNotifications = true,
                    PushNotifications = true,
                    SmsNotifications = false,
                    BookingNotifications = true,
                    MessageNotifications = true,
                    PaymentNotifications = true,
                    PromotionNotifications = true,
                    SystemNotifications = true
                });
            }

            var dto = new NotificationSettingsDto
            {
                EmailNotifications = settings.EmailNotifications,
                PushNotifications = settings.PushNotifications,
                SmsNotifications = settings.SmsNotifications,
                BookingNotifications = settings.BookingNotifications,
                MessageNotifications = settings.MessageNotifications,
                PaymentNotifications = settings.PaymentNotifications,
                PromotionNotifications = settings.PromotionNotifications,
                SystemNotifications = settings.SystemNotifications
            };

            return Result.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting notification settings for user {UserId}", request.UserId);
            return Result.Failure<NotificationSettingsDto>(
                "An error occurred while retrieving notification settings",
                ex.Message);
        }
    }
}

public class NotificationSettingsDto
{
    public bool EmailNotifications { get; set; }
    public bool PushNotifications { get; set; }
    public bool SmsNotifications { get; set; }
    public bool BookingNotifications { get; set; }
    public bool MessageNotifications { get; set; }
    public bool PaymentNotifications { get; set; }
    public bool PromotionNotifications { get; set; }
    public bool SystemNotifications { get; set; }
}
