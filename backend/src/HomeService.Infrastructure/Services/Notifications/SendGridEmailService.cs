using HomeService.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace HomeService.Infrastructure.Services.Notifications;

public class SendGridEmailService : IEmailService
{
    private readonly ILogger<SendGridEmailService> _logger;
    private readonly IConfiguration _configuration;
    private readonly string _apiKey;
    private readonly string _fromEmail;
    private readonly string _fromName;
    private readonly SendGridClient _client;

    public SendGridEmailService(
        ILogger<SendGridEmailService> logger,
        IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
        _apiKey = configuration["SendGrid:ApiKey"] ?? string.Empty;
        _fromEmail = configuration["SendGrid:FromEmail"] ?? "noreply@homeservice.com";
        _fromName = configuration["SendGrid:FromName"] ?? "Home Service Platform";

        if (!string.IsNullOrEmpty(_apiKey))
        {
            _client = new SendGridClient(_apiKey);
        }
        else
        {
            _logger.LogWarning("SendGrid API key not configured. Emails will be simulated.");
            _client = null!;
        }
    }

    public async Task<bool> SendEmailAsync(
        string toEmail,
        string toName,
        string subject,
        string body,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (_client == null)
            {
                _logger.LogWarning("SendGrid not configured. Simulating email to {Email}: {Subject}", toEmail, subject);
                return true;
            }

            var from = new EmailAddress(_fromEmail, _fromName);
            var to = new EmailAddress(toEmail, toName);
            var msg = MailHelper.CreateSingleEmail(from, to, subject, body, body);

            var response = await _client.SendEmailAsync(msg, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Email sent successfully to {Email}", toEmail);
                return true;
            }
            else
            {
                var errorBody = await response.Body.ReadAsStringAsync();
                _logger.LogError("Failed to send email. Status: {StatusCode}, Error: {Error}",
                    response.StatusCode, errorBody);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending email to {Email}", toEmail);
            return false;
        }
    }

    public async Task<bool> SendTemplateEmailAsync(
        string toEmail,
        string toName,
        string templateId,
        Dictionary<string, string> templateData,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (_client == null)
            {
                _logger.LogWarning("SendGrid not configured. Simulating template email to {Email}", toEmail);
                return true;
            }

            var from = new EmailAddress(_fromEmail, _fromName);
            var to = new EmailAddress(toEmail, toName);
            var msg = MailHelper.CreateSingleTemplateEmail(from, to, templateId, templateData);

            var response = await _client.SendEmailAsync(msg, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Template email sent successfully to {Email} using template {TemplateId}",
                    toEmail, templateId);
                return true;
            }
            else
            {
                var errorBody = await response.Body.ReadAsStringAsync();
                _logger.LogError("Failed to send template email. Status: {StatusCode}, Error: {Error}",
                    response.StatusCode, errorBody);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending template email to {Email}", toEmail);
            return false;
        }
    }

    public async Task<bool> SendWelcomeEmailAsync(
        string toEmail,
        string userName,
        string verificationLink,
        CancellationToken cancellationToken = default)
    {
        var subject = "Welcome to Home Service Platform!";
        var body = $@"
            <html>
            <body style='font-family: Arial, sans-serif;'>
                <h2>Welcome {userName}!</h2>
                <p>Thank you for joining Home Service Platform. We're excited to have you with us!</p>
                <p>To get started, please verify your email address by clicking the link below:</p>
                <p><a href='{verificationLink}' style='background-color: #4CAF50; color: white; padding: 12px 24px; text-decoration: none; border-radius: 4px; display: inline-block;'>Verify Email</a></p>
                <p>If you didn't create an account, you can safely ignore this email.</p>
                <p>Best regards,<br>Home Service Team</p>
            </body>
            </html>";

        return await SendEmailAsync(toEmail, userName, subject, body, cancellationToken);
    }

    public async Task<bool> SendPasswordResetEmailAsync(
        string toEmail,
        string userName,
        string resetLink,
        CancellationToken cancellationToken = default)
    {
        var subject = "Reset Your Password";
        var body = $@"
            <html>
            <body style='font-family: Arial, sans-serif;'>
                <h2>Password Reset Request</h2>
                <p>Hi {userName},</p>
                <p>We received a request to reset your password. Click the link below to create a new password:</p>
                <p><a href='{resetLink}' style='background-color: #2196F3; color: white; padding: 12px 24px; text-decoration: none; border-radius: 4px; display: inline-block;'>Reset Password</a></p>
                <p>This link will expire in 1 hour.</p>
                <p>If you didn't request a password reset, you can safely ignore this email.</p>
                <p>Best regards,<br>Home Service Team</p>
            </body>
            </html>";

        return await SendEmailAsync(toEmail, userName, subject, body, cancellationToken);
    }

    public async Task<bool> SendBookingConfirmationEmailAsync(
        string toEmail,
        string userName,
        BookingEmailDetails booking,
        CancellationToken cancellationToken = default)
    {
        var subject = $"Booking Confirmation - {booking.ServiceName}";
        var body = $@"
            <html>
            <body style='font-family: Arial, sans-serif;'>
                <h2>Booking Confirmed!</h2>
                <p>Hi {userName},</p>
                <p>Your booking has been confirmed. Here are the details:</p>
                <table style='border-collapse: collapse; width: 100%; max-width: 500px;'>
                    <tr><td style='padding: 8px; border-bottom: 1px solid #ddd;'><strong>Booking ID:</strong></td><td style='padding: 8px; border-bottom: 1px solid #ddd;'>{booking.BookingId}</td></tr>
                    <tr><td style='padding: 8px; border-bottom: 1px solid #ddd;'><strong>Service:</strong></td><td style='padding: 8px; border-bottom: 1px solid #ddd;'>{booking.ServiceName}</td></tr>
                    <tr><td style='padding: 8px; border-bottom: 1px solid #ddd;'><strong>Provider:</strong></td><td style='padding: 8px; border-bottom: 1px solid #ddd;'>{booking.ProviderName}</td></tr>
                    <tr><td style='padding: 8px; border-bottom: 1px solid #ddd;'><strong>Date & Time:</strong></td><td style='padding: 8px; border-bottom: 1px solid #ddd;'>{booking.ScheduledDate:dddd, MMMM dd, yyyy 'at' hh:mm tt}</td></tr>
                    <tr><td style='padding: 8px; border-bottom: 1px solid #ddd;'><strong>Location:</strong></td><td style='padding: 8px; border-bottom: 1px solid #ddd;'>{booking.Location}</td></tr>
                    <tr><td style='padding: 8px; border-bottom: 1px solid #ddd;'><strong>Total Amount:</strong></td><td style='padding: 8px; border-bottom: 1px solid #ddd;'>{booking.TotalAmount} {booking.Currency}</td></tr>
                </table>
                <p>We'll send you another notification when the provider is on the way.</p>
                <p>Best regards,<br>Home Service Team</p>
            </body>
            </html>";

        return await SendEmailAsync(toEmail, userName, subject, body, cancellationToken);
    }

    public async Task<bool> SendBookingStatusUpdateEmailAsync(
        string toEmail,
        string userName,
        string bookingId,
        string status,
        CancellationToken cancellationToken = default)
    {
        var subject = $"Booking Status Update - {bookingId}";
        var body = $@"
            <html>
            <body style='font-family: Arial, sans-serif;'>
                <h2>Booking Status Update</h2>
                <p>Hi {userName},</p>
                <p>Your booking <strong>{bookingId}</strong> status has been updated to:</p>
                <p style='font-size: 18px; color: #4CAF50;'><strong>{status}</strong></p>
                <p>You can view more details in your bookings page.</p>
                <p>Best regards,<br>Home Service Team</p>
            </body>
            </html>";

        return await SendEmailAsync(toEmail, userName, subject, body, cancellationToken);
    }

    public async Task<bool> SendPaymentReceiptEmailAsync(
        string toEmail,
        string userName,
        PaymentReceiptDetails receipt,
        CancellationToken cancellationToken = default)
    {
        var subject = $"Payment Receipt - {receipt.TransactionId}";
        var body = $@"
            <html>
            <body style='font-family: Arial, sans-serif;'>
                <h2>Payment Receipt</h2>
                <p>Hi {userName},</p>
                <p>Thank you for your payment. Here's your receipt:</p>
                <table style='border-collapse: collapse; width: 100%; max-width: 500px;'>
                    <tr><td style='padding: 8px; border-bottom: 1px solid #ddd;'><strong>Transaction ID:</strong></td><td style='padding: 8px; border-bottom: 1px solid #ddd;'>{receipt.TransactionId}</td></tr>
                    <tr><td style='padding: 8px; border-bottom: 1px solid #ddd;'><strong>Booking ID:</strong></td><td style='padding: 8px; border-bottom: 1px solid #ddd;'>{receipt.BookingId}</td></tr>
                    <tr><td style='padding: 8px; border-bottom: 1px solid #ddd;'><strong>Service:</strong></td><td style='padding: 8px; border-bottom: 1px solid #ddd;'>{receipt.ServiceName}</td></tr>
                    <tr><td style='padding: 8px; border-bottom: 1px solid #ddd;'><strong>Payment Date:</strong></td><td style='padding: 8px; border-bottom: 1px solid #ddd;'>{receipt.PaymentDate:dddd, MMMM dd, yyyy 'at' hh:mm tt}</td></tr>
                    <tr><td style='padding: 8px; border-bottom: 1px solid #ddd;'><strong>Amount:</strong></td><td style='padding: 8px; border-bottom: 1px solid #ddd;'>{receipt.Amount} {receipt.Currency}</td></tr>
                    <tr><td style='padding: 8px; border-bottom: 1px solid #ddd;'><strong>Payment Method:</strong></td><td style='padding: 8px; border-bottom: 1px solid #ddd;'>{receipt.PaymentMethod}</td></tr>
                </table>
                <p>This is your official receipt for tax purposes.</p>
                <p>Best regards,<br>Home Service Team</p>
            </body>
            </html>";

        return await SendEmailAsync(toEmail, userName, subject, body, cancellationToken);
    }

    public async Task<bool> SendProviderApprovalEmailAsync(
        string toEmail,
        string providerName,
        bool isApproved,
        string? reason = null,
        CancellationToken cancellationToken = default)
    {
        var subject = isApproved ? "Welcome to Home Service - Application Approved!" : "Home Service Application Update";
        var body = isApproved
            ? $@"
                <html>
                <body style='font-family: Arial, sans-serif;'>
                    <h2>Congratulations {providerName}!</h2>
                    <p>Your application to become a service provider has been approved!</p>
                    <p>You can now log in to your provider account and start accepting bookings.</p>
                    <p>Best regards,<br>Home Service Team</p>
                </body>
                </html>"
            : $@"
                <html>
                <body style='font-family: Arial, sans-serif;'>
                    <h2>Application Update</h2>
                    <p>Hi {providerName},</p>
                    <p>Thank you for your interest in becoming a service provider. Unfortunately, we are unable to approve your application at this time.</p>
                    {(!string.IsNullOrEmpty(reason) ? $"<p><strong>Reason:</strong> {reason}</p>" : "")}
                    <p>You may reapply after addressing the concerns mentioned above.</p>
                    <p>Best regards,<br>Home Service Team</p>
                </body>
                </html>";

        return await SendEmailAsync(toEmail, providerName, subject, body, cancellationToken);
    }

    public async Task<bool> SendNewJobNotificationEmailAsync(
        string toEmail,
        string providerName,
        JobNotificationDetails job,
        CancellationToken cancellationToken = default)
    {
        var subject = $"New Job Available - {job.ServiceName}";
        var body = $@"
            <html>
            <body style='font-family: Arial, sans-serif;'>
                <h2>New Job Available!</h2>
                <p>Hi {providerName},</p>
                <p>A new job matching your services is available:</p>
                <table style='border-collapse: collapse; width: 100%; max-width: 500px;'>
                    <tr><td style='padding: 8px; border-bottom: 1px solid #ddd;'><strong>Job ID:</strong></td><td style='padding: 8px; border-bottom: 1px solid #ddd;'>{job.JobId}</td></tr>
                    <tr><td style='padding: 8px; border-bottom: 1px solid #ddd;'><strong>Service:</strong></td><td style='padding: 8px; border-bottom: 1px solid #ddd;'>{job.ServiceName}</td></tr>
                    <tr><td style='padding: 8px; border-bottom: 1px solid #ddd;'><strong>Customer:</strong></td><td style='padding: 8px; border-bottom: 1px solid #ddd;'>{job.CustomerName}</td></tr>
                    <tr><td style='padding: 8px; border-bottom: 1px solid #ddd;'><strong>Scheduled:</strong></td><td style='padding: 8px; border-bottom: 1px solid #ddd;'>{job.ScheduledDate:dddd, MMMM dd, yyyy 'at' hh:mm tt}</td></tr>
                    <tr><td style='padding: 8px; border-bottom: 1px solid #ddd;'><strong>Location:</strong></td><td style='padding: 8px; border-bottom: 1px solid #ddd;'>{job.Location}</td></tr>
                    <tr><td style='padding: 8px; border-bottom: 1px solid #ddd;'><strong>Your Earnings:</strong></td><td style='padding: 8px; border-bottom: 1px solid #ddd;'>{job.Earnings} {job.Currency}</td></tr>
                </table>
                <p>Log in to your provider app to accept this job.</p>
                <p>Best regards,<br>Home Service Team</p>
            </body>
            </html>";

        return await SendEmailAsync(toEmail, providerName, subject, body, cancellationToken);
    }
}
