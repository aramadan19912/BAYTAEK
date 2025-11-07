namespace HomeService.Application.Interfaces;

public interface IEmailService
{
    /// <summary>
    /// Sends a simple email with subject and body
    /// </summary>
    Task<bool> SendEmailAsync(string toEmail, string toName, string subject, string body, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends an email using a template
    /// </summary>
    Task<bool> SendTemplateEmailAsync(string toEmail, string toName, string templateId, Dictionary<string, string> templateData, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends welcome email to new users
    /// </summary>
    Task<bool> SendWelcomeEmailAsync(string toEmail, string userName, string verificationLink, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends password reset email
    /// </summary>
    Task<bool> SendPasswordResetEmailAsync(string toEmail, string userName, string resetLink, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends booking confirmation email
    /// </summary>
    Task<bool> SendBookingConfirmationEmailAsync(string toEmail, string userName, BookingEmailDetails booking, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends booking status update email
    /// </summary>
    Task<bool> SendBookingStatusUpdateEmailAsync(string toEmail, string userName, string bookingId, string status, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends payment receipt email
    /// </summary>
    Task<bool> SendPaymentReceiptEmailAsync(string toEmail, string userName, PaymentReceiptDetails receipt, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends provider approval email
    /// </summary>
    Task<bool> SendProviderApprovalEmailAsync(string toEmail, string providerName, bool isApproved, string? reason = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends new job notification to provider
    /// </summary>
    Task<bool> SendNewJobNotificationEmailAsync(string toEmail, string providerName, JobNotificationDetails job, CancellationToken cancellationToken = default);
}

public class BookingEmailDetails
{
    public string BookingId { get; set; } = string.Empty;
    public string ServiceName { get; set; } = string.Empty;
    public string ProviderName { get; set; } = string.Empty;
    public DateTime ScheduledDate { get; set; }
    public string Location { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public string Currency { get; set; } = "SAR";
}

public class PaymentReceiptDetails
{
    public string TransactionId { get; set; } = string.Empty;
    public string BookingId { get; set; } = string.Empty;
    public DateTime PaymentDate { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "SAR";
    public string PaymentMethod { get; set; } = string.Empty;
    public string ServiceName { get; set; } = string.Empty;
}

public class JobNotificationDetails
{
    public string JobId { get; set; } = string.Empty;
    public string ServiceName { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public DateTime ScheduledDate { get; set; }
    public string Location { get; set; } = string.Empty;
    public decimal Earnings { get; set; }
    public string Currency { get; set; } = "SAR";
}
