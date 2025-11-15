using HomeService.Application.Interfaces;
using HomeService.Domain.Entities;
using HomeService.Domain.Enums;
using HomeService.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HomeService.Infrastructure.Services.Notifications;

public class NotificationService : INotificationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPushNotificationService _pushNotificationService;
    private readonly IEmailService _emailService;
    private readonly ISmsService _smsService;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
        IUnitOfWork unitOfWork,
        IPushNotificationService pushNotificationService,
        IEmailService emailService,
        ISmsService smsService,
        ILogger<NotificationService> logger)
    {
        _unitOfWork = unitOfWork;
        _pushNotificationService = pushNotificationService;
        _emailService = emailService;
        _smsService = smsService;
        _logger = logger;
    }

    public async Task<NotificationResult> SendNotificationAsync(
        Guid userId,
        string titleEn,
        string titleAr,
        string messageEn,
        string messageAr,
        NotificationCategory category,
        Guid? relatedEntityId = null,
        string? relatedEntityType = null,
        string? actionUrl = null,
        CancellationToken cancellationToken = default)
    {
        // Get user's notification settings to determine which channels to use
        var settings = await GetUserNotificationSettings(userId, cancellationToken);
        var channels = GetEnabledChannels(settings, category);

        return await SendNotificationAsync(
            userId,
            titleEn,
            titleAr,
            messageEn,
            messageAr,
            category,
            channels,
            relatedEntityId,
            relatedEntityType,
            actionUrl,
            cancellationToken);
    }

    public async Task<NotificationResult> SendNotificationAsync(
        Guid userId,
        string titleEn,
        string titleAr,
        string messageEn,
        string messageAr,
        NotificationCategory category,
        List<NotificationChannel> channels,
        Guid? relatedEntityId = null,
        string? relatedEntityType = null,
        string? actionUrl = null,
        CancellationToken cancellationToken = default)
    {
        var result = new NotificationResult { Success = true };

        try
        {
            // Get user details
            var user = await _unitOfWork.Repository<User>().GetByIdAsync(userId, cancellationToken);
            if (user == null)
            {
                result.Success = false;
                result.ErrorMessages.Add("User not found");
                return result;
            }

            // Create in-app notification
            if (channels.Contains(NotificationChannel.InApp))
            {
                var notification = new Notification
                {
                    UserId = userId,
                    TitleEn = titleEn,
                    TitleAr = titleAr,
                    MessageEn = messageEn,
                    MessageAr = messageAr,
                    Type = NotificationType.InApp,
                    IsRead = false,
                    RelatedEntityId = relatedEntityId,
                    RelatedEntityType = relatedEntityType,
                    ActionUrl = actionUrl
                };

                await _unitOfWork.Repository<Notification>().AddAsync(notification, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                result.NotificationId = notification.Id;
                result.ChannelResults[NotificationChannel.InApp] = true;
            }

            // Send push notification
            if (channels.Contains(NotificationChannel.Push))
            {
                var pushSuccess = await SendPushNotification(userId, titleEn, messageEn, relatedEntityId, relatedEntityType, cancellationToken);
                result.ChannelResults[NotificationChannel.Push] = pushSuccess;
                if (!pushSuccess) result.ErrorMessages.Add("Push notification failed");
            }

            // Send email
            if (channels.Contains(NotificationChannel.Email))
            {
                var emailSuccess = await _emailService.SendEmailAsync(
                    user.Email,
                    $"{user.FirstName} {user.LastName}",
                    titleEn,
                    messageEn,
                    cancellationToken);

                result.ChannelResults[NotificationChannel.Email] = emailSuccess;
                if (!emailSuccess) result.ErrorMessages.Add("Email notification failed");
            }

            // Send SMS
            if (channels.Contains(NotificationChannel.SMS))
            {
                var smsResult = await _smsService.SendSmsAsync(
                    user.PhoneNumber,
                    messageEn,
                    cancellationToken);

                result.ChannelResults[NotificationChannel.SMS] = smsResult.Success;
                if (!smsResult.Success) result.ErrorMessages.Add($"SMS notification failed: {smsResult.ErrorMessage}");
            }

            _logger.LogInformation("Notification sent to user {UserId} via {ChannelCount} channels",
                userId, result.ChannelResults.Count);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending notification to user {UserId}", userId);
            result.Success = false;
            result.ErrorMessages.Add($"Error sending notification: {ex.Message}");
            return result;
        }
    }

    public async Task<NotificationResult> SendNewBookingNotificationAsync(
        Guid providerId,
        Guid bookingId,
        string customerName,
        string serviceName,
        DateTime scheduledDate,
        CancellationToken cancellationToken = default)
    {
        var titleEn = "New Booking Request";
        var titleAr = "طلب حجز جديد";
        var messageEn = $"New booking from {customerName} for {serviceName} on {scheduledDate:MMM dd, yyyy 'at' hh:mm tt}";
        var messageAr = $"حجز جديد من {customerName} لخدمة {serviceName} في {scheduledDate:dd/MM/yyyy 'الساعة' hh:mm tt}";

        return await SendNotificationAsync(
            providerId,
            titleEn,
            titleAr,
            messageEn,
            messageAr,
            NotificationCategory.Booking,
            bookingId,
            nameof(Booking),
            $"/bookings/{bookingId}",
            cancellationToken);
    }

    public async Task<NotificationResult> SendBookingStatusUpdateAsync(
        Guid customerId,
        Guid bookingId,
        string status,
        string serviceName,
        CancellationToken cancellationToken = default)
    {
        var titleEn = "Booking Status Update";
        var titleAr = "تحديث حالة الحجز";
        var messageEn = $"Your {serviceName} booking status: {status}";
        var messageAr = $"حالة حجز {serviceName}: {status}";

        return await SendNotificationAsync(
            customerId,
            titleEn,
            titleAr,
            messageEn,
            messageAr,
            NotificationCategory.Booking,
            bookingId,
            nameof(Booking),
            $"/bookings/{bookingId}",
            cancellationToken);
    }

    public async Task<NotificationResult> SendPaymentConfirmationAsync(
        Guid userId,
        Guid bookingId,
        decimal amount,
        string currency,
        CancellationToken cancellationToken = default)
    {
        var titleEn = "Payment Confirmed";
        var titleAr = "تم تأكيد الدفع";
        var messageEn = $"Payment of {amount} {currency} has been processed successfully";
        var messageAr = $"تم معالجة دفعة بقيمة {amount} {currency} بنجاح";

        return await SendNotificationAsync(
            userId,
            titleEn,
            titleAr,
            messageEn,
            messageAr,
            NotificationCategory.Payment,
            bookingId,
            nameof(Booking),
            $"/bookings/{bookingId}",
            cancellationToken);
    }

    public async Task<NotificationResult> SendNewMessageNotificationAsync(
        Guid recipientId,
        Guid conversationId,
        string senderName,
        string messagePreview,
        CancellationToken cancellationToken = default)
    {
        var titleEn = $"New message from {senderName}";
        var titleAr = $"رسالة جديدة من {senderName}";
        var messageEn = messagePreview.Length > 100 ? messagePreview.Substring(0, 100) + "..." : messagePreview;
        var messageAr = messagePreview;

        return await SendNotificationAsync(
            recipientId,
            titleEn,
            titleAr,
            messageEn,
            messageAr,
            NotificationCategory.Message,
            conversationId,
            nameof(Conversation),
            $"/messages/{conversationId}",
            cancellationToken);
    }

    public async Task<NotificationResult> SendBookingReminderAsync(
        Guid userId,
        Guid bookingId,
        string serviceName,
        DateTime scheduledDate,
        int hoursBeforeService,
        CancellationToken cancellationToken = default)
    {
        var titleEn = "Booking Reminder";
        var titleAr = "تذكير بالحجز";
        var messageEn = $"Your {serviceName} service is scheduled in {hoursBeforeService} hours at {scheduledDate:hh:mm tt}";
        var messageAr = $"خدمة {serviceName} مجدولة خلال {hoursBeforeService} ساعة في {scheduledDate:hh:mm tt}";

        return await SendNotificationAsync(
            userId,
            titleEn,
            titleAr,
            messageEn,
            messageAr,
            NotificationCategory.Reminder,
            bookingId,
            nameof(Booking),
            $"/bookings/{bookingId}",
            cancellationToken);
    }

    public async Task<NotificationResult> SendProviderApprovalNotificationAsync(
        Guid providerId,
        bool isApproved,
        string? reason = null,
        CancellationToken cancellationToken = default)
    {
        var titleEn = isApproved ? "Application Approved" : "Application Status";
        var titleAr = isApproved ? "تمت الموافقة على الطلب" : "حالة الطلب";
        var messageEn = isApproved
            ? "Congratulations! Your provider application has been approved."
            : $"Your provider application status has been updated. {reason}";
        var messageAr = isApproved
            ? "تهانينا! تمت الموافقة على طلب مقدم الخدمة الخاص بك."
            : $"تم تحديث حالة طلب مقدم الخدمة. {reason}";

        return await SendNotificationAsync(
            providerId,
            titleEn,
            titleAr,
            messageEn,
            messageAr,
            NotificationCategory.Account,
            providerId,
            nameof(User),
            "/provider/profile",
            cancellationToken);
    }

    public async Task<NotificationResult> SendReviewRequestAsync(
        Guid customerId,
        Guid bookingId,
        string serviceName,
        CancellationToken cancellationToken = default)
    {
        var titleEn = "Rate Your Experience";
        var titleAr = "قيّم تجربتك";
        var messageEn = $"How was your {serviceName} experience? Share your feedback!";
        var messageAr = $"كيف كانت تجربتك مع {serviceName}؟ شاركنا رأيك!";

        return await SendNotificationAsync(
            customerId,
            titleEn,
            titleAr,
            messageEn,
            messageAr,
            NotificationCategory.System,
            bookingId,
            nameof(Booking),
            $"/bookings/{bookingId}/review",
            cancellationToken);
    }

    #region Private Helper Methods

    private async Task<NotificationSettings?> GetUserNotificationSettings(Guid userId, CancellationToken cancellationToken)
    {
        return await _unitOfWork.Repository<NotificationSettings>()
            .GetQueryable()
            .FirstOrDefaultAsync(s => s.UserId == userId, cancellationToken);
    }

    private List<NotificationChannel> GetEnabledChannels(NotificationSettings? settings, NotificationCategory category)
    {
        var channels = new List<NotificationChannel> { NotificationChannel.InApp }; // Always send in-app

        if (settings == null)
        {
            // Default: enable all channels
            channels.AddRange(new[] { NotificationChannel.Push, NotificationChannel.Email });
            return channels;
        }

        // Check category-specific settings
        switch (category)
        {
            case NotificationCategory.Booking:
                if (settings.BookingNotifications) channels.Add(NotificationChannel.Push);
                if (settings.EmailNotifications) channels.Add(NotificationChannel.Email);
                if (settings.SmsNotifications) channels.Add(NotificationChannel.SMS);
                break;

            case NotificationCategory.Payment:
                if (settings.PaymentNotifications) channels.Add(NotificationChannel.Push);
                if (settings.EmailNotifications) channels.Add(NotificationChannel.Email);
                break;

            case NotificationCategory.Message:
                if (settings.MessageNotifications) channels.Add(NotificationChannel.Push);
                break;

            case NotificationCategory.Promotion:
                if (settings.PromotionNotifications) channels.Add(NotificationChannel.Push);
                if (settings.EmailNotifications) channels.Add(NotificationChannel.Email);
                break;

            default:
                if (settings.SystemNotifications) channels.Add(NotificationChannel.Push);
                if (settings.EmailNotifications) channels.Add(NotificationChannel.Email);
                break;
        }

        return channels;
    }

    private async Task<bool> SendPushNotification(
        Guid userId,
        string title,
        string body,
        Guid? relatedEntityId,
        string? relatedEntityType,
        CancellationToken cancellationToken)
    {
        try
        {
            // Get user's device tokens
            var deviceTokens = await _unitOfWork.Repository<DeviceToken>()
                .GetQueryable()
                .Where(dt => dt.UserId == userId && dt.IsActive)
                .ToListAsync(cancellationToken);

            if (!deviceTokens.Any())
            {
                _logger.LogWarning("No active device tokens found for user {UserId}", userId);
                return false;
            }

            var message = new PushNotificationMessage
            {
                Title = title,
                Body = body,
                Priority = PushNotificationPriority.High,
                Data = new Dictionary<string, string>
                {
                    ["userId"] = userId.ToString(),
                    ["relatedEntityId"] = relatedEntityId?.ToString() ?? string.Empty,
                    ["relatedEntityType"] = relatedEntityType ?? string.Empty
                }
            };

            var batchResult = await _pushNotificationService.SendBatchNotificationAsync(
                deviceTokens.Select(dt => dt.Token).ToList(),
                message,
                cancellationToken);

            return batchResult.SuccessCount > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending push notification to user {UserId}", userId);
            return false;
        }
    }

    #endregion
}
