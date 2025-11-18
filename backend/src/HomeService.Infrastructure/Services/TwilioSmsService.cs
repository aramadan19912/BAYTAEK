using HomeService.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace HomeService.Infrastructure.Services;

public class TwilioSmsService : ISmsService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<TwilioSmsService> _logger;
    private readonly string _accountSid;
    private readonly string _authToken;
    private readonly string _fromPhoneNumber;

    public TwilioSmsService(
        IConfiguration configuration,
        ILogger<TwilioSmsService> logger)
    {
        _configuration = configuration;
        _logger = logger;

        _accountSid = _configuration["SMS:Twilio:AccountSid"] ?? throw new ArgumentNullException("Twilio AccountSid not configured");
        _authToken = _configuration["SMS:Twilio:AuthToken"] ?? throw new ArgumentNullException("Twilio AuthToken not configured");
        _fromPhoneNumber = _configuration["SMS:Twilio:PhoneNumber"] ?? throw new ArgumentNullException("Twilio PhoneNumber not configured");

        TwilioClient.Init(_accountSid, _authToken);
    }

    public async Task<SmsResult> SendSmsAsync(string phoneNumber, string message, CancellationToken cancellationToken = default)
    {
        try
        {
            var messageResource = await MessageResource.CreateAsync(
                body: message,
                from: new PhoneNumber(_fromPhoneNumber),
                to: new PhoneNumber(phoneNumber)
            );

            if (messageResource.Status == MessageResource.StatusEnum.Failed ||
                messageResource.Status == MessageResource.StatusEnum.Undelivered)
            {
                _logger.LogError("SMS failed to send to {PhoneNumber}. Status: {Status}, Error: {ErrorMessage}",
                    phoneNumber, messageResource.Status, messageResource.ErrorMessage);
                return new SmsResult
                {
                    Success = false,
                    ErrorMessage = messageResource.ErrorMessage
                };
            }

            _logger.LogInformation("SMS sent successfully to {PhoneNumber}. SID: {MessageSid}",
                phoneNumber, messageResource.Sid);

            return new SmsResult
            {
                Success = true,
                MessageId = messageResource.Sid
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending SMS to {PhoneNumber}", phoneNumber);
            return new SmsResult
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    public async Task<SmsBatchResult> SendBatchSmsAsync(List<string> phoneNumbers, string message, CancellationToken cancellationToken = default)
    {
        var results = new List<SmsResult>();
        foreach (var phoneNumber in phoneNumbers)
        {
            var result = await SendSmsAsync(phoneNumber, message, cancellationToken);
            results.Add(result);
        }

        return new SmsBatchResult
        {
            SuccessCount = results.Count(r => r.Success),
            FailureCount = results.Count(r => !r.Success),
            Results = results
        };
    }

    public async Task<SmsResult> SendOtpSmsAsync(string phoneNumber, string otpCode, CancellationToken cancellationToken = default)
    {
        var message = $"Your verification code is: {otpCode}. Valid for 10 minutes.";
        return await SendSmsAsync(phoneNumber, message, cancellationToken);
    }

    public async Task<SmsResult> SendBookingConfirmationSmsAsync(string phoneNumber, string bookingId, string serviceName, DateTime scheduledDate, CancellationToken cancellationToken = default)
    {
        var message = $"Your booking #{bookingId} for {serviceName} is confirmed for {scheduledDate:MMM dd, yyyy HH:mm}. Thank you for using our service!";
        return await SendSmsAsync(phoneNumber, message, cancellationToken);
    }

    public async Task<SmsResult> SendBookingReminderSmsAsync(string phoneNumber, string serviceName, DateTime scheduledDate, CancellationToken cancellationToken = default)
    {
        var message = $"Reminder: Your {serviceName} appointment is scheduled for {scheduledDate:MMM dd, yyyy HH:mm}.";
        return await SendSmsAsync(phoneNumber, message, cancellationToken);
    }

    public async Task<SmsResult> SendProviderOnTheWaySmsAsync(string phoneNumber, string providerName, int estimatedMinutes, CancellationToken cancellationToken = default)
    {
        var message = $"{providerName} is on the way! Estimated arrival in {estimatedMinutes} minutes.";
        return await SendSmsAsync(phoneNumber, message, cancellationToken);
    }

    public async Task<SmsResult> SendServiceCompletedSmsAsync(string phoneNumber, string bookingId, string serviceName, CancellationToken cancellationToken = default)
    {
        var message = $"Your {serviceName} service (Booking #{bookingId}) has been completed. Please rate your experience!";
        return await SendSmsAsync(phoneNumber, message, cancellationToken);
    }

    private string BuildMessageFromTemplate(string templateId, Dictionary<string, string> parameters)
    {
        // Simple template system - in production, use a proper template engine
        var templates = new Dictionary<string, string>
        {
            { "otp_registration", "Your verification code is: {code}" },
            { "booking_confirmed", "Your booking #{bookingId} is confirmed" },
            { "provider_assigned", "Provider {providerName} has been assigned to your booking" }
        };

        if (!templates.TryGetValue(templateId, out var template))
        {
            return "Message from Home Service";
        }

        foreach (var param in parameters)
        {
            template = template.Replace($"{{{param.Key}}}", param.Value);
        }

        return template;
    }
}
