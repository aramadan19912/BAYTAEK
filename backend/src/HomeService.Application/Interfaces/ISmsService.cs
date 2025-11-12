namespace HomeService.Application.Interfaces;

public interface ISmsService
{
    /// <summary>
    /// Sends a simple SMS message
    /// </summary>
    Task<SmsResult> SendSmsAsync(string phoneNumber, string message, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends SMS to multiple recipients
    /// </summary>
    Task<SmsBatchResult> SendBatchSmsAsync(List<string> phoneNumbers, string message, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends OTP SMS
    /// </summary>
    Task<SmsResult> SendOtpSmsAsync(string phoneNumber, string otpCode, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends booking confirmation SMS
    /// </summary>
    Task<SmsResult> SendBookingConfirmationSmsAsync(string phoneNumber, string bookingId, string serviceName, DateTime scheduledDate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends booking reminder SMS
    /// </summary>
    Task<SmsResult> SendBookingReminderSmsAsync(string phoneNumber, string serviceName, DateTime scheduledDate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends provider on the way SMS
    /// </summary>
    Task<SmsResult> SendProviderOnTheWaySmsAsync(string phoneNumber, string providerName, int estimatedMinutes, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends service completion SMS
    /// </summary>
    Task<SmsResult> SendServiceCompletedSmsAsync(string phoneNumber, string bookingId, string serviceName, CancellationToken cancellationToken = default);
}

public class SmsResult
{
    public bool Success { get; set; }
    public string? MessageId { get; set; }
    public string? ErrorMessage { get; set; }
    public decimal? Cost { get; set; }
}

public class SmsBatchResult
{
    public int SuccessCount { get; set; }
    public int FailureCount { get; set; }
    public List<SmsResult> Results { get; set; } = new();
    public decimal? TotalCost { get; set; }
}
