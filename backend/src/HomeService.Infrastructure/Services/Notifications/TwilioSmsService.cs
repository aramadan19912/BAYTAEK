using HomeService.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace HomeService.Infrastructure.Services.Notifications;

public class TwilioSmsService : ISmsService
{
    private readonly ILogger<TwilioSmsService> _logger;
    private readonly IConfiguration _configuration;
    private readonly string _accountSid;
    private readonly string _authToken;
    private readonly string _fromPhoneNumber;
    private readonly bool _isConfigured;

    public TwilioSmsService(
        ILogger<TwilioSmsService> logger,
        IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
        _accountSid = configuration["Twilio:AccountSid"] ?? string.Empty;
        _authToken = configuration["Twilio:AuthToken"] ?? string.Empty;
        _fromPhoneNumber = configuration["Twilio:PhoneNumber"] ?? string.Empty;

        _isConfigured = !string.IsNullOrEmpty(_accountSid) && !string.IsNullOrEmpty(_authToken);

        if (_isConfigured)
        {
            TwilioClient.Init(_accountSid, _authToken);
        }
        else
        {
            _logger.LogWarning("Twilio not configured. SMS will be simulated.");
        }
    }

    public async Task<SmsResult> SendSmsAsync(
        string phoneNumber,
        string message,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!_isConfigured)
            {
                _logger.LogWarning("Twilio not configured. Simulating SMS to {PhoneNumber}: {Message}",
                    phoneNumber, message);
                return new SmsResult
                {
                    Success = true,
                    MessageId = $"sim_{Guid.NewGuid():N}"
                };
            }

            var messageResource = await MessageResource.CreateAsync(
                to: new PhoneNumber(phoneNumber),
                from: new PhoneNumber(_fromPhoneNumber),
                body: message
            );

            var result = new SmsResult
            {
                Success = messageResource.Status != MessageResource.StatusEnum.Failed &&
                         messageResource.Status != MessageResource.StatusEnum.Undelivered,
                MessageId = messageResource.Sid,
                ErrorMessage = messageResource.ErrorMessage
            };

            if (result.Success)
            {
                _logger.LogInformation("SMS sent successfully to {PhoneNumber}. SID: {MessageId}",
                    phoneNumber, result.MessageId);
            }
            else
            {
                _logger.LogError("SMS failed to {PhoneNumber}. Error: {Error}",
                    phoneNumber, result.ErrorMessage);
            }

            return result;
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

    public async Task<SmsBatchResult> SendBatchSmsAsync(
        List<string> phoneNumbers,
        string message,
        CancellationToken cancellationToken = default)
    {
        var result = new SmsBatchResult();

        foreach (var phoneNumber in phoneNumbers)
        {
            var smsResult = await SendSmsAsync(phoneNumber, message, cancellationToken);
            result.Results.Add(smsResult);

            if (smsResult.Success)
            {
                result.SuccessCount++;
                if (smsResult.Cost.HasValue)
                    result.TotalCost = (result.TotalCost ?? 0) + smsResult.Cost.Value;
            }
            else
            {
                result.FailureCount++;
            }
        }

        _logger.LogInformation("Batch SMS completed. Success: {SuccessCount}, Failed: {FailureCount}",
            result.SuccessCount, result.FailureCount);

        return result;
    }

    public async Task<SmsResult> SendOtpSmsAsync(
        string phoneNumber,
        string otpCode,
        CancellationToken cancellationToken = default)
    {
        var message = $"Your verification code is: {otpCode}. This code will expire in 10 minutes. " +
                     $"Do not share this code with anyone.";

        return await SendSmsAsync(phoneNumber, message, cancellationToken);
    }

    public async Task<SmsResult> SendBookingConfirmationSmsAsync(
        string phoneNumber,
        string bookingId,
        string serviceName,
        DateTime scheduledDate,
        CancellationToken cancellationToken = default)
    {
        var message = $"Booking confirmed! Service: {serviceName}, " +
                     $"Date: {scheduledDate:MMM dd, yyyy 'at' hh:mm tt}. " +
                     $"Booking ID: {bookingId}. " +
                     $"Thank you for choosing Home Service!";

        return await SendSmsAsync(phoneNumber, message, cancellationToken);
    }

    public async Task<SmsResult> SendBookingReminderSmsAsync(
        string phoneNumber,
        string serviceName,
        DateTime scheduledDate,
        CancellationToken cancellationToken = default)
    {
        var hoursUntil = (scheduledDate - DateTime.UtcNow).TotalHours;
        var timeText = hoursUntil <= 1 ? "in 1 hour" : $"in {Math.Round(hoursUntil)} hours";

        var message = $"Reminder: Your {serviceName} service is scheduled {timeText} " +
                     $"at {scheduledDate:hh:mm tt}. Please be available.";

        return await SendSmsAsync(phoneNumber, message, cancellationToken);
    }

    public async Task<SmsResult> SendProviderOnTheWaySmsAsync(
        string phoneNumber,
        string providerName,
        int estimatedMinutes,
        CancellationToken cancellationToken = default)
    {
        var message = $"{providerName} is on the way! " +
                     $"Estimated arrival: {estimatedMinutes} minutes. " +
                     $"Track your provider in the app.";

        return await SendSmsAsync(phoneNumber, message, cancellationToken);
    }

    public async Task<SmsResult> SendServiceCompletedSmsAsync(
        string phoneNumber,
        string bookingId,
        string serviceName,
        CancellationToken cancellationToken = default)
    {
        var message = $"Service completed! Booking ID: {bookingId}. " +
                     $"Please rate your {serviceName} experience in the app. " +
                     $"Thank you for using Home Service!";

        return await SendSmsAsync(phoneNumber, message, cancellationToken);
    }
}
