using HomeService.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Text;

namespace HomeService.Infrastructure.Services;

public class SendGridEmailService : IEmailService
{
    private readonly ISendGridClient _sendGridClient;
    private readonly ILogger<SendGridEmailService> _logger;
    private readonly string _fromEmail;
    private readonly string _fromName;

    public SendGridEmailService(
        IConfiguration configuration,
        ILogger<SendGridEmailService> logger)
    {
        var apiKey = configuration["Email:SendGrid:ApiKey"]
            ?? throw new ArgumentNullException("SendGrid API Key not configured");

        _fromEmail = configuration["Email:SendGrid:FromEmail"]
            ?? throw new ArgumentNullException("SendGrid FromEmail not configured");

        _fromName = configuration["Email:SendGrid:FromName"] ?? "Home Service";

        _sendGridClient = new SendGridClient(apiKey);
        _logger = logger;
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
            var from = new EmailAddress(_fromEmail, _fromName);
            var to = new EmailAddress(toEmail, toName);
            var msg = MailHelper.CreateSingleEmail(from, to, subject, body, body);

            var response = await _sendGridClient.SendEmailAsync(msg, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Email sent successfully to {Email}", toEmail);
                return true;
            }

            var responseBody = await response.Body.ReadAsStringAsync(cancellationToken);
            _logger.LogError("Failed to send email to {Email}. Status: {StatusCode}, Response: {Response}",
                toEmail, response.StatusCode, responseBody);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while sending email to {Email}", toEmail);
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
            var from = new EmailAddress(_fromEmail, _fromName);
            var to = new EmailAddress(toEmail, toName);

            var msg = new SendGridMessage
            {
                From = from,
                TemplateId = templateId
            };

            msg.AddTo(to);
            msg.SetTemplateData(templateData);

            var response = await _sendGridClient.SendEmailAsync(msg, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Template email sent successfully to {Email} using template {TemplateId}",
                    toEmail, templateId);
                return true;
            }

            var responseBody = await response.Body.ReadAsStringAsync(cancellationToken);
            _logger.LogError("Failed to send template email to {Email}. Status: {StatusCode}, Response: {Response}",
                toEmail, response.StatusCode, responseBody);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while sending template email to {Email}", toEmail);
            return false;
        }
    }

    public async Task<bool> SendWelcomeEmailAsync(
        string toEmail,
        string userName,
        string verificationLink,
        CancellationToken cancellationToken = default)
    {
        var subject = "Welcome to Home Service!";
        var body = BuildWelcomeEmailBody(userName, verificationLink);
        return await SendEmailAsync(toEmail, userName, subject, body, cancellationToken);
    }

    public async Task<bool> SendPasswordResetEmailAsync(
        string toEmail,
        string userName,
        string resetLink,
        CancellationToken cancellationToken = default)
    {
        var subject = "Password Reset Request";
        var body = BuildPasswordResetEmailBody(userName, resetLink);
        return await SendEmailAsync(toEmail, userName, subject, body, cancellationToken);
    }

    public async Task<bool> SendBookingConfirmationEmailAsync(
        string toEmail,
        string userName,
        BookingEmailDetails booking,
        CancellationToken cancellationToken = default)
    {
        var subject = $"Booking Confirmed - {booking.ServiceName}";
        var body = BuildBookingConfirmationEmailBody(userName, booking);
        return await SendEmailAsync(toEmail, userName, subject, body, cancellationToken);
    }

    public async Task<bool> SendBookingStatusUpdateEmailAsync(
        string toEmail,
        string userName,
        string bookingId,
        string status,
        CancellationToken cancellationToken = default)
    {
        var subject = $"Booking Update - {status}";
        var body = BuildBookingStatusUpdateEmailBody(userName, bookingId, status);
        return await SendEmailAsync(toEmail, userName, subject, body, cancellationToken);
    }

    public async Task<bool> SendPaymentReceiptEmailAsync(
        string toEmail,
        string userName,
        PaymentReceiptDetails receipt,
        CancellationToken cancellationToken = default)
    {
        var subject = $"Payment Receipt - {receipt.TransactionId}";
        var body = BuildPaymentReceiptEmailBody(userName, receipt);
        return await SendEmailAsync(toEmail, userName, subject, body, cancellationToken);
    }

    public async Task<bool> SendProviderApprovalEmailAsync(
        string toEmail,
        string providerName,
        bool isApproved,
        string? reason = null,
        CancellationToken cancellationToken = default)
    {
        var subject = isApproved
            ? "Your Provider Application has been Approved!"
            : "Provider Application Status Update";
        var body = BuildProviderApprovalEmailBody(providerName, isApproved, reason);
        return await SendEmailAsync(toEmail, providerName, subject, body, cancellationToken);
    }

    public async Task<bool> SendNewJobNotificationEmailAsync(
        string toEmail,
        string providerName,
        JobNotificationDetails job,
        CancellationToken cancellationToken = default)
    {
        var subject = $"New Job Request - {job.ServiceName}";
        var body = BuildNewJobNotificationEmailBody(providerName, job);
        return await SendEmailAsync(toEmail, providerName, subject, body, cancellationToken);
    }

    #region Email Body Builders

    private string BuildWelcomeEmailBody(string userName, string verificationLink)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"<!DOCTYPE html>");
        sb.AppendLine($"<html>");
        sb.AppendLine($"<body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>");
        sb.AppendLine($"  <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>");
        sb.AppendLine($"    <h2 style='color: #4CAF50;'>Welcome to Home Service, {userName}!</h2>");
        sb.AppendLine($"    <p>Thank you for joining our platform. We're excited to have you on board!</p>");
        sb.AppendLine($"    <p>To get started, please verify your email address by clicking the button below:</p>");
        sb.AppendLine($"    <div style='text-align: center; margin: 30px 0;'>");
        sb.AppendLine($"      <a href='{verificationLink}' style='background-color: #4CAF50; color: white; padding: 12px 24px; text-decoration: none; border-radius: 4px; display: inline-block;'>Verify Email</a>");
        sb.AppendLine($"    </div>");
        sb.AppendLine($"    <p>If the button doesn't work, copy and paste this link into your browser:</p>");
        sb.AppendLine($"    <p style='word-break: break-all; color: #666;'>{verificationLink}</p>");
        sb.AppendLine($"    <p>If you didn't create this account, please ignore this email.</p>");
        sb.AppendLine($"    <hr style='margin: 30px 0; border: none; border-top: 1px solid #ddd;'>");
        sb.AppendLine($"    <p style='font-size: 12px; color: #666;'>Best regards,<br>The Home Service Team</p>");
        sb.AppendLine($"  </div>");
        sb.AppendLine($"</body>");
        sb.AppendLine($"</html>");
        return sb.ToString();
    }

    private string BuildPasswordResetEmailBody(string userName, string resetLink)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"<!DOCTYPE html>");
        sb.AppendLine($"<html>");
        sb.AppendLine($"<body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>");
        sb.AppendLine($"  <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>");
        sb.AppendLine($"    <h2 style='color: #FF5722;'>Password Reset Request</h2>");
        sb.AppendLine($"    <p>Hi {userName},</p>");
        sb.AppendLine($"    <p>We received a request to reset your password. Click the button below to create a new password:</p>");
        sb.AppendLine($"    <div style='text-align: center; margin: 30px 0;'>");
        sb.AppendLine($"      <a href='{resetLink}' style='background-color: #FF5722; color: white; padding: 12px 24px; text-decoration: none; border-radius: 4px; display: inline-block;'>Reset Password</a>");
        sb.AppendLine($"    </div>");
        sb.AppendLine($"    <p>If the button doesn't work, copy and paste this link into your browser:</p>");
        sb.AppendLine($"    <p style='word-break: break-all; color: #666;'>{resetLink}</p>");
        sb.AppendLine($"    <p><strong>This link will expire in 1 hour.</strong></p>");
        sb.AppendLine($"    <p>If you didn't request this password reset, please ignore this email or contact support if you have concerns.</p>");
        sb.AppendLine($"    <hr style='margin: 30px 0; border: none; border-top: 1px solid #ddd;'>");
        sb.AppendLine($"    <p style='font-size: 12px; color: #666;'>Best regards,<br>The Home Service Team</p>");
        sb.AppendLine($"  </div>");
        sb.AppendLine($"</body>");
        sb.AppendLine($"</html>");
        return sb.ToString();
    }

    private string BuildBookingConfirmationEmailBody(string userName, BookingEmailDetails booking)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"<!DOCTYPE html>");
        sb.AppendLine($"<html>");
        sb.AppendLine($"<body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>");
        sb.AppendLine($"  <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>");
        sb.AppendLine($"    <h2 style='color: #4CAF50;'>Booking Confirmed!</h2>");
        sb.AppendLine($"    <p>Hi {userName},</p>");
        sb.AppendLine($"    <p>Your booking has been confirmed. Here are the details:</p>");
        sb.AppendLine($"    <div style='background-color: #f5f5f5; padding: 20px; border-radius: 4px; margin: 20px 0;'>");
        sb.AppendLine($"      <p><strong>Booking ID:</strong> {booking.BookingId}</p>");
        sb.AppendLine($"      <p><strong>Service:</strong> {booking.ServiceName}</p>");
        sb.AppendLine($"      <p><strong>Provider:</strong> {booking.ProviderName}</p>");
        sb.AppendLine($"      <p><strong>Date & Time:</strong> {booking.ScheduledDate:dddd, MMMM dd, yyyy 'at' hh:mm tt}</p>");
        sb.AppendLine($"      <p><strong>Location:</strong> {booking.Location}</p>");
        sb.AppendLine($"      <p><strong>Total Amount:</strong> {booking.TotalAmount:N2} {booking.Currency}</p>");
        sb.AppendLine($"    </div>");
        sb.AppendLine($"    <p>Your provider will contact you shortly to confirm the appointment.</p>");
        sb.AppendLine($"    <p>You can track your booking status in the app.</p>");
        sb.AppendLine($"    <hr style='margin: 30px 0; border: none; border-top: 1px solid #ddd;'>");
        sb.AppendLine($"    <p style='font-size: 12px; color: #666;'>Best regards,<br>The Home Service Team</p>");
        sb.AppendLine($"  </div>");
        sb.AppendLine($"</body>");
        sb.AppendLine($"</html>");
        return sb.ToString();
    }

    private string BuildBookingStatusUpdateEmailBody(string userName, string bookingId, string status)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"<!DOCTYPE html>");
        sb.AppendLine($"<html>");
        sb.AppendLine($"<body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>");
        sb.AppendLine($"  <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>");
        sb.AppendLine($"    <h2 style='color: #2196F3;'>Booking Status Update</h2>");
        sb.AppendLine($"    <p>Hi {userName},</p>");
        sb.AppendLine($"    <p>Your booking <strong>{bookingId}</strong> status has been updated to:</p>");
        sb.AppendLine($"    <div style='text-align: center; margin: 30px 0;'>");
        sb.AppendLine($"      <span style='background-color: #2196F3; color: white; padding: 12px 24px; border-radius: 4px; display: inline-block; font-weight: bold;'>{status}</span>");
        sb.AppendLine($"    </div>");
        sb.AppendLine($"    <p>Check the app for more details and updates.</p>");
        sb.AppendLine($"    <hr style='margin: 30px 0; border: none; border-top: 1px solid #ddd;'>");
        sb.AppendLine($"    <p style='font-size: 12px; color: #666;'>Best regards,<br>The Home Service Team</p>");
        sb.AppendLine($"  </div>");
        sb.AppendLine($"</body>");
        sb.AppendLine($"</html>");
        return sb.ToString();
    }

    private string BuildPaymentReceiptEmailBody(string userName, PaymentReceiptDetails receipt)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"<!DOCTYPE html>");
        sb.AppendLine($"<html>");
        sb.AppendLine($"<body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>");
        sb.AppendLine($"  <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>");
        sb.AppendLine($"    <h2 style='color: #4CAF50;'>Payment Receipt</h2>");
        sb.AppendLine($"    <p>Hi {userName},</p>");
        sb.AppendLine($"    <p>Thank you for your payment. Here's your receipt:</p>");
        sb.AppendLine($"    <div style='background-color: #f5f5f5; padding: 20px; border-radius: 4px; margin: 20px 0;'>");
        sb.AppendLine($"      <p><strong>Transaction ID:</strong> {receipt.TransactionId}</p>");
        sb.AppendLine($"      <p><strong>Booking ID:</strong> {receipt.BookingId}</p>");
        sb.AppendLine($"      <p><strong>Service:</strong> {receipt.ServiceName}</p>");
        sb.AppendLine($"      <p><strong>Payment Date:</strong> {receipt.PaymentDate:dddd, MMMM dd, yyyy 'at' hh:mm tt}</p>");
        sb.AppendLine($"      <p><strong>Payment Method:</strong> {receipt.PaymentMethod}</p>");
        sb.AppendLine($"      <p><strong>Amount Paid:</strong> {receipt.Amount:N2} {receipt.Currency}</p>");
        sb.AppendLine($"    </div>");
        sb.AppendLine($"    <p>This receipt has been recorded in your account.</p>");
        sb.AppendLine($"    <hr style='margin: 30px 0; border: none; border-top: 1px solid #ddd;'>");
        sb.AppendLine($"    <p style='font-size: 12px; color: #666;'>Best regards,<br>The Home Service Team</p>");
        sb.AppendLine($"  </div>");
        sb.AppendLine($"</body>");
        sb.AppendLine($"</html>");
        return sb.ToString();
    }

    private string BuildProviderApprovalEmailBody(string providerName, bool isApproved, string? reason)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"<!DOCTYPE html>");
        sb.AppendLine($"<html>");
        sb.AppendLine($"<body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>");
        sb.AppendLine($"  <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>");

        if (isApproved)
        {
            sb.AppendLine($"    <h2 style='color: #4CAF50;'>Congratulations! Your Application is Approved</h2>");
            sb.AppendLine($"    <p>Hi {providerName},</p>");
            sb.AppendLine($"    <p>Great news! Your service provider application has been approved.</p>");
            sb.AppendLine($"    <p>You can now start accepting job requests and building your business with Home Service.</p>");
            sb.AppendLine($"    <div style='text-align: center; margin: 30px 0;'>");
            sb.AppendLine($"      <a href='#' style='background-color: #4CAF50; color: white; padding: 12px 24px; text-decoration: none; border-radius: 4px; display: inline-block;'>Start Accepting Jobs</a>");
            sb.AppendLine($"    </div>");
            sb.AppendLine($"    <p>Here's what you can do next:</p>");
            sb.AppendLine($"    <ul>");
            sb.AppendLine($"      <li>Complete your profile and add portfolio images</li>");
            sb.AppendLine($"      <li>Set your service areas and pricing</li>");
            sb.AppendLine($"      <li>Turn online to start receiving job requests</li>");
            sb.AppendLine($"    </ul>");
        }
        else
        {
            sb.AppendLine($"    <h2 style='color: #FF5722;'>Application Status Update</h2>");
            sb.AppendLine($"    <p>Hi {providerName},</p>");
            sb.AppendLine($"    <p>Thank you for your interest in joining Home Service as a provider.</p>");
            sb.AppendLine($"    <p>Unfortunately, we are unable to approve your application at this time.</p>");

            if (!string.IsNullOrEmpty(reason))
            {
                sb.AppendLine($"    <div style='background-color: #fff3e0; padding: 15px; border-left: 4px solid #FF5722; margin: 20px 0;'>");
                sb.AppendLine($"      <p><strong>Reason:</strong> {reason}</p>");
                sb.AppendLine($"    </div>");
            }

            sb.AppendLine($"    <p>If you believe this is an error or would like to address any issues, please contact our support team.</p>");
        }

        sb.AppendLine($"    <hr style='margin: 30px 0; border: none; border-top: 1px solid #ddd;'>");
        sb.AppendLine($"    <p style='font-size: 12px; color: #666;'>Best regards,<br>The Home Service Team</p>");
        sb.AppendLine($"  </div>");
        sb.AppendLine($"</body>");
        sb.AppendLine($"</html>");
        return sb.ToString();
    }

    private string BuildNewJobNotificationEmailBody(string providerName, JobNotificationDetails job)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"<!DOCTYPE html>");
        sb.AppendLine($"<html>");
        sb.AppendLine($"<body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>");
        sb.AppendLine($"  <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>");
        sb.AppendLine($"    <h2 style='color: #2196F3;'>New Job Request</h2>");
        sb.AppendLine($"    <p>Hi {providerName},</p>");
        sb.AppendLine($"    <p>You have a new job request! Here are the details:</p>");
        sb.AppendLine($"    <div style='background-color: #f5f5f5; padding: 20px; border-radius: 4px; margin: 20px 0;'>");
        sb.AppendLine($"      <p><strong>Job ID:</strong> {job.JobId}</p>");
        sb.AppendLine($"      <p><strong>Service:</strong> {job.ServiceName}</p>");
        sb.AppendLine($"      <p><strong>Customer:</strong> {job.CustomerName}</p>");
        sb.AppendLine($"      <p><strong>Date & Time:</strong> {job.ScheduledDate:dddd, MMMM dd, yyyy 'at' hh:mm tt}</p>");
        sb.AppendLine($"      <p><strong>Location:</strong> {job.Location}</p>");
        sb.AppendLine($"      <p><strong>Your Earnings:</strong> {job.Earnings:N2} {job.Currency}</p>");
        sb.AppendLine($"    </div>");
        sb.AppendLine($"    <div style='text-align: center; margin: 30px 0;'>");
        sb.AppendLine($"      <a href='#' style='background-color: #4CAF50; color: white; padding: 12px 24px; text-decoration: none; border-radius: 4px; display: inline-block; margin-right: 10px;'>Accept Job</a>");
        sb.AppendLine($"      <a href='#' style='background-color: #666; color: white; padding: 12px 24px; text-decoration: none; border-radius: 4px; display: inline-block;'>View Details</a>");
        sb.AppendLine($"    </div>");
        sb.AppendLine($"    <p><strong>Note:</strong> This job request will expire in 10 minutes. Please respond quickly!</p>");
        sb.AppendLine($"    <hr style='margin: 30px 0; border: none; border-top: 1px solid #ddd;'>");
        sb.AppendLine($"    <p style='font-size: 12px; color: #666;'>Best regards,<br>The Home Service Team</p>");
        sb.AppendLine($"  </div>");
        sb.AppendLine($"</body>");
        sb.AppendLine($"</html>");
        return sb.ToString();
    }

    #endregion
}
