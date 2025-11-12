using HomeService.Domain.Enums;

namespace HomeService.Application.Interfaces;

/// <summary>
/// Orchestration service for sending notifications through multiple channels
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// Sends notification to user through all enabled channels
    /// </summary>
    Task<NotificationResult> SendNotificationAsync(
        Guid userId,
        string titleEn,
        string titleAr,
        string messageEn,
        string messageAr,
        NotificationCategory category,
        Guid? relatedEntityId = null,
        string? relatedEntityType = null,
        string? actionUrl = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends notification through specific channels
    /// </summary>
    Task<NotificationResult> SendNotificationAsync(
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
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends new booking notification to provider
    /// </summary>
    Task<NotificationResult> SendNewBookingNotificationAsync(
        Guid providerId,
        Guid bookingId,
        string customerName,
        string serviceName,
        DateTime scheduledDate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends booking status update to customer
    /// </summary>
    Task<NotificationResult> SendBookingStatusUpdateAsync(
        Guid customerId,
        Guid bookingId,
        string status,
        string serviceName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends payment confirmation notification
    /// </summary>
    Task<NotificationResult> SendPaymentConfirmationAsync(
        Guid userId,
        Guid bookingId,
        decimal amount,
        string currency,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends new message notification
    /// </summary>
    Task<NotificationResult> SendNewMessageNotificationAsync(
        Guid recipientId,
        Guid conversationId,
        string senderName,
        string messagePreview,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends booking reminder notification
    /// </summary>
    Task<NotificationResult> SendBookingReminderAsync(
        Guid userId,
        Guid bookingId,
        string serviceName,
        DateTime scheduledDate,
        int hoursBeforeService,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends provider approval/rejection notification
    /// </summary>
    Task<NotificationResult> SendProviderApprovalNotificationAsync(
        Guid providerId,
        bool isApproved,
        string? reason = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends review request notification
    /// </summary>
    Task<NotificationResult> SendReviewRequestAsync(
        Guid customerId,
        Guid bookingId,
        string serviceName,
        CancellationToken cancellationToken = default);
}

public enum NotificationChannel
{
    InApp = 1,
    Push = 2,
    Email = 3,
    SMS = 4
}

public enum NotificationCategory
{
    Booking = 1,
    Payment = 2,
    Message = 3,
    System = 4,
    Promotion = 5,
    Reminder = 6,
    Account = 7
}

public class NotificationResult
{
    public bool Success { get; set; }
    public Dictionary<NotificationChannel, bool> ChannelResults { get; set; } = new();
    public List<string> ErrorMessages { get; set; } = new();
    public Guid? NotificationId { get; set; }
}
