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

    public async Task<bool> SendSmsAsync(string phoneNumber, string message, CancellationToken cancellationToken = default)
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
                return false;
            }

            _logger.LogInformation("SMS sent successfully to {PhoneNumber}. SID: {MessageSid}",
                phoneNumber, messageResource.Sid);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending SMS to {PhoneNumber}", phoneNumber);
            return false;
        }
    }

    public async Task<bool> SendSmsWithTemplateAsync(string phoneNumber, string templateId, Dictionary<string, string> parameters, CancellationToken cancellationToken = default)
    {
        // Twilio doesn't have built-in templates like SendGrid
        // You would build the message from a template repository
        try
        {
            // For now, just send a simple message
            var message = BuildMessageFromTemplate(templateId, parameters);
            return await SendSmsAsync(phoneNumber, message, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending SMS with template {TemplateId} to {PhoneNumber}", templateId, phoneNumber);
            return false;
        }
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
