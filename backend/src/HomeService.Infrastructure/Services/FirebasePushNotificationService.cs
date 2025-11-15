using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using HomeService.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace HomeService.Infrastructure.Services;

public class FirebasePushNotificationService : IPushNotificationService
{
    private readonly ILogger<FirebasePushNotificationService> _logger;
    private readonly FirebaseMessaging _firebaseMessaging;

    public FirebasePushNotificationService(
        IConfiguration configuration,
        ILogger<FirebasePushNotificationService> logger)
    {
        _logger = logger;

        // Initialize Firebase Admin SDK if not already initialized
        if (FirebaseApp.DefaultInstance == null)
        {
            var credential = GoogleCredential.FromFile(
                configuration["Firebase:ServiceAccountKeyPath"]
                ?? throw new ArgumentNullException("Firebase:ServiceAccountKeyPath not configured"));

            FirebaseApp.Create(new AppOptions
            {
                Credential = credential
            });
        }

        _firebaseMessaging = FirebaseMessaging.DefaultInstance;
    }

    public async Task<bool> SendNotificationAsync(
        string deviceToken,
        PushNotificationMessage message,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var fcmMessage = BuildMessage(deviceToken, message);
            var response = await _firebaseMessaging.SendAsync(fcmMessage, cancellationToken);

            _logger.LogInformation("Successfully sent notification to {DeviceToken}. Response: {Response}",
                MaskToken(deviceToken), response);

            return true;
        }
        catch (FirebaseMessagingException ex)
        {
            _logger.LogError(ex, "Failed to send notification to {DeviceToken}. Error code: {ErrorCode}",
                MaskToken(deviceToken), ex.MessagingErrorCode);

            // Handle invalid tokens
            if (ex.MessagingErrorCode == MessagingErrorCode.InvalidArgument ||
                ex.MessagingErrorCode == MessagingErrorCode.Unregistered)
            {
                _logger.LogWarning("Device token is invalid or unregistered: {DeviceToken}", MaskToken(deviceToken));
                // TODO: Mark token as invalid in database for cleanup
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while sending notification to {DeviceToken}", MaskToken(deviceToken));
            return false;
        }
    }

    public async Task<PushNotificationBatchResult> SendBatchNotificationAsync(
        List<string> deviceTokens,
        PushNotificationMessage message,
        CancellationToken cancellationToken = default)
    {
        var result = new PushNotificationBatchResult();

        try
        {
            if (!deviceTokens.Any())
            {
                _logger.LogWarning("SendBatchNotificationAsync called with empty device token list");
                return result;
            }

            // Firebase supports up to 500 tokens per batch
            var batches = deviceTokens.Chunk(500);

            foreach (var batch in batches)
            {
                var messages = batch.Select(token => BuildMessage(token, message)).ToList();
                var batchResponse = await _firebaseMessaging.SendAllAsync(messages, cancellationToken);

                result.SuccessCount += batchResponse.SuccessCount;
                result.FailureCount += batchResponse.FailureCount;

                // Collect failed tokens and errors
                for (int i = 0; i < batchResponse.Responses.Count; i++)
                {
                    var response = batchResponse.Responses[i];
                    if (!response.IsSuccess)
                    {
                        result.FailedTokens.Add(batch.ElementAt(i));
                        result.ErrorMessages.Add(response.Exception?.Message ?? "Unknown error");

                        _logger.LogWarning("Failed to send notification to token {Index}: {Error}",
                            i, response.Exception?.Message);
                    }
                }
            }

            _logger.LogInformation("Batch notification sent. Success: {SuccessCount}, Failed: {FailureCount}",
                result.SuccessCount, result.FailureCount);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while sending batch notifications");
            result.FailureCount = deviceTokens.Count;
            result.FailedTokens = deviceTokens;
            result.ErrorMessages.Add(ex.Message);
            return result;
        }
    }

    public async Task<bool> SendTopicNotificationAsync(
        string topic,
        PushNotificationMessage message,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var fcmMessage = BuildTopicMessage(topic, message);
            var response = await _firebaseMessaging.SendAsync(fcmMessage, cancellationToken);

            _logger.LogInformation("Successfully sent notification to topic {Topic}. Response: {Response}",
                topic, response);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send notification to topic {Topic}", topic);
            return false;
        }
    }

    public async Task<bool> SubscribeToTopicAsync(
        string deviceToken,
        string topic,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _firebaseMessaging.SubscribeToTopicAsync(
                new List<string> { deviceToken },
                topic,
                cancellationToken);

            if (response.SuccessCount > 0)
            {
                _logger.LogInformation("Successfully subscribed {DeviceToken} to topic {Topic}",
                    MaskToken(deviceToken), topic);
                return true;
            }

            _logger.LogWarning("Failed to subscribe {DeviceToken} to topic {Topic}. Errors: {Errors}",
                MaskToken(deviceToken), topic, response.FailureCount);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while subscribing to topic {Topic}", topic);
            return false;
        }
    }

    public async Task<bool> UnsubscribeFromTopicAsync(
        string deviceToken,
        string topic,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _firebaseMessaging.UnsubscribeFromTopicAsync(
                new List<string> { deviceToken },
                topic,
                cancellationToken);

            if (response.SuccessCount > 0)
            {
                _logger.LogInformation("Successfully unsubscribed {DeviceToken} from topic {Topic}",
                    MaskToken(deviceToken), topic);
                return true;
            }

            _logger.LogWarning("Failed to unsubscribe {DeviceToken} from topic {Topic}. Errors: {Errors}",
                MaskToken(deviceToken), topic, response.FailureCount);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while unsubscribing from topic {Topic}", topic);
            return false;
        }
    }

    public async Task<bool> SendNewBookingNotificationAsync(
        string deviceToken,
        BookingNotificationData data,
        CancellationToken cancellationToken = default)
    {
        var message = new PushNotificationMessage
        {
            Title = "New Job Request! 🎯",
            Body = $"{data.ServiceName} - {data.Amount:N0} {data.Currency}\n{data.CustomerName} • {data.ScheduledDate:MMM dd, hh:mm tt}",
            Priority = PushNotificationPriority.High,
            Sound = "default",
            ClickAction = "NEW_BOOKING",
            Data = new Dictionary<string, string>
            {
                { "type", "new_booking" },
                { "bookingId", data.BookingId },
                { "serviceName", data.ServiceName },
                { "customerName", data.CustomerName },
                { "scheduledDate", data.ScheduledDate.ToString("o") },
                { "location", data.Location },
                { "amount", data.Amount.ToString() },
                { "currency", data.Currency },
                { "expiryMinutes", data.ExpiryMinutes.ToString() }
            }
        };

        return await SendNotificationAsync(deviceToken, message, cancellationToken);
    }

    public async Task<bool> SendBookingStatusUpdateAsync(
        string deviceToken,
        string bookingId,
        string status,
        CancellationToken cancellationToken = default)
    {
        var (emoji, title) = GetStatusEmoji(status);

        var message = new PushNotificationMessage
        {
            Title = $"{title} {emoji}",
            Body = GetStatusMessage(status),
            Priority = PushNotificationPriority.Normal,
            Sound = "default",
            ClickAction = "BOOKING_DETAILS",
            Data = new Dictionary<string, string>
            {
                { "type", "booking_status_update" },
                { "bookingId", bookingId },
                { "status", status }
            }
        };

        return await SendNotificationAsync(deviceToken, message, cancellationToken);
    }

    public async Task<bool> SendPaymentConfirmationAsync(
        string deviceToken,
        string bookingId,
        decimal amount,
        string currency,
        CancellationToken cancellationToken = default)
    {
        var message = new PushNotificationMessage
        {
            Title = "Payment Successful ✅",
            Body = $"Your payment of {amount:N2} {currency} has been processed successfully.",
            Priority = PushNotificationPriority.Normal,
            Sound = "default",
            ClickAction = "BOOKING_DETAILS",
            Data = new Dictionary<string, string>
            {
                { "type", "payment_confirmation" },
                { "bookingId", bookingId },
                { "amount", amount.ToString() },
                { "currency", currency }
            }
        };

        return await SendNotificationAsync(deviceToken, message, cancellationToken);
    }

    public async Task<bool> SendMessageNotificationAsync(
        string deviceToken,
        string senderName,
        string messagePreview,
        string chatId,
        CancellationToken cancellationToken = default)
    {
        var message = new PushNotificationMessage
        {
            Title = senderName,
            Body = messagePreview,
            Priority = PushNotificationPriority.High,
            Sound = "default",
            ClickAction = "CHAT",
            Badge = 1,
            Data = new Dictionary<string, string>
            {
                { "type", "new_message" },
                { "chatId", chatId },
                { "senderName", senderName }
            }
        };

        return await SendNotificationAsync(deviceToken, message, cancellationToken);
    }

    public async Task<bool> SendLocationUpdateAsync(
        string deviceToken,
        string bookingId,
        double latitude,
        double longitude,
        CancellationToken cancellationToken = default)
    {
        // Silent notification for location updates
        var message = new PushNotificationMessage
        {
            Title = string.Empty,
            Body = string.Empty,
            Priority = PushNotificationPriority.Normal,
            Sound = null, // Silent
            Data = new Dictionary<string, string>
            {
                { "type", "location_update" },
                { "bookingId", bookingId },
                { "latitude", latitude.ToString() },
                { "longitude", longitude.ToString() },
                { "timestamp", DateTime.UtcNow.ToString("o") }
            }
        };

        return await SendNotificationAsync(deviceToken, message, cancellationToken);
    }

    #region Helper Methods

    private Message BuildMessage(string deviceToken, PushNotificationMessage message)
    {
        var notification = new Notification
        {
            Title = message.Title,
            Body = message.Body,
            ImageUrl = message.ImageUrl
        };

        var androidConfig = new AndroidConfig
        {
            Priority = message.Priority == PushNotificationPriority.High
                ? Priority.High
                : Priority.Normal,
            Notification = new AndroidNotification
            {
                Title = message.Title,
                Body = message.Body,
                Icon = "ic_notification",
                Color = "#4CAF50",
                Sound = message.Sound,
                ClickAction = message.ClickAction
            }
        };

        var apnsConfig = new ApnsConfig
        {
            Aps = new Aps
            {
                Alert = new ApsAlert
                {
                    Title = message.Title,
                    Body = message.Body
                },
                Sound = message.Sound,
                Badge = message.Badge,
                ContentAvailable = string.IsNullOrEmpty(message.Title) // For silent notifications
            },
            Headers = new Dictionary<string, string>
            {
                { "apns-priority", message.Priority == PushNotificationPriority.High ? "10" : "5" }
            }
        };

        return new Message
        {
            Token = deviceToken,
            Notification = notification,
            Data = message.Data,
            Android = androidConfig,
            Apns = apnsConfig
        };
    }

    private Message BuildTopicMessage(string topic, PushNotificationMessage message)
    {
        var notification = new Notification
        {
            Title = message.Title,
            Body = message.Body,
            ImageUrl = message.ImageUrl
        };

        return new Message
        {
            Topic = topic,
            Notification = notification,
            Data = message.Data,
            Android = new AndroidConfig
            {
                Priority = message.Priority == PushNotificationPriority.High
                    ? Priority.High
                    : Priority.Normal
            },
            Apns = new ApnsConfig
            {
                Aps = new Aps
                {
                    Alert = new ApsAlert
                    {
                        Title = message.Title,
                        Body = message.Body
                    },
                    Sound = message.Sound
                }
            }
        };
    }

    private (string emoji, string title) GetStatusEmoji(string status)
    {
        return status.ToLower() switch
        {
            "confirmed" => ("✅", "Booking Confirmed"),
            "accepted" => ("👍", "Provider Accepted"),
            "on_the_way" => ("🚗", "Provider On The Way"),
            "arrived" => ("📍", "Provider Arrived"),
            "in_progress" => ("🔧", "Service In Progress"),
            "completed" => ("✨", "Service Completed"),
            "cancelled" => ("❌", "Booking Cancelled"),
            "rescheduled" => ("📅", "Booking Rescheduled"),
            _ => ("ℹ️", "Booking Update")
        };
    }

    private string GetStatusMessage(string status)
    {
        return status.ToLower() switch
        {
            "confirmed" => "Your booking has been confirmed. The provider will contact you soon.",
            "accepted" => "The provider has accepted your booking.",
            "on_the_way" => "The provider is on the way to your location.",
            "arrived" => "The provider has arrived at your location.",
            "in_progress" => "The service is now in progress.",
            "completed" => "The service has been completed. Please rate your experience.",
            "cancelled" => "Your booking has been cancelled.",
            "rescheduled" => "Your booking has been rescheduled.",
            _ => "Your booking status has been updated."
        };
    }

    private string MaskToken(string token)
    {
        if (string.IsNullOrEmpty(token) || token.Length < 10)
            return "***";

        return $"{token.Substring(0, 5)}...{token.Substring(token.Length - 5)}";
    }

    #endregion
}
