namespace HomeService.Application.Interfaces;

public interface IPushNotificationService
{
    /// <summary>
    /// Sends a push notification to a single device
    /// </summary>
    Task<bool> SendNotificationAsync(string deviceToken, PushNotificationMessage message, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a push notification to multiple devices
    /// </summary>
    Task<PushNotificationBatchResult> SendBatchNotificationAsync(List<string> deviceTokens, PushNotificationMessage message, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a push notification to a topic (all subscribed devices)
    /// </summary>
    Task<bool> SendTopicNotificationAsync(string topic, PushNotificationMessage message, CancellationToken cancellationToken = default);

    /// <summary>
    /// Subscribe a device to a topic
    /// </summary>
    Task<bool> SubscribeToTopicAsync(string deviceToken, string topic, CancellationToken cancellationToken = default);

    /// <summary>
    /// Unsubscribe a device from a topic
    /// </summary>
    Task<bool> UnsubscribeFromTopicAsync(string deviceToken, string topic, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends new booking notification to provider
    /// </summary>
    Task<bool> SendNewBookingNotificationAsync(string deviceToken, BookingNotificationData data, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends booking status update notification to customer
    /// </summary>
    Task<bool> SendBookingStatusUpdateAsync(string deviceToken, string bookingId, string status, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends payment confirmation notification
    /// </summary>
    Task<bool> SendPaymentConfirmationAsync(string deviceToken, string bookingId, decimal amount, string currency, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends message notification
    /// </summary>
    Task<bool> SendMessageNotificationAsync(string deviceToken, string senderName, string messagePreview, string chatId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends provider location update (for customer tracking)
    /// </summary>
    Task<bool> SendLocationUpdateAsync(string deviceToken, string bookingId, double latitude, double longitude, CancellationToken cancellationToken = default);
}

public class PushNotificationMessage
{
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public Dictionary<string, string> Data { get; set; } = new();
    public PushNotificationPriority Priority { get; set; } = PushNotificationPriority.Normal;
    public string? Sound { get; set; } = "default";
    public int? Badge { get; set; }
    public string? ClickAction { get; set; }
}

public enum PushNotificationPriority
{
    Normal,
    High
}

public class PushNotificationBatchResult
{
    public int SuccessCount { get; set; }
    public int FailureCount { get; set; }
    public List<string> FailedTokens { get; set; } = new();
    public List<string> ErrorMessages { get; set; } = new();
}

public class BookingNotificationData
{
    public string BookingId { get; set; } = string.Empty;
    public string ServiceName { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public DateTime ScheduledDate { get; set; }
    public string Location { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "SAR";
    public int ExpiryMinutes { get; set; } = 10;
}
