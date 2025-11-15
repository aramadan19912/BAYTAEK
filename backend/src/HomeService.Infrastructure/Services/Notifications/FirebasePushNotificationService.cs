using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using HomeService.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace HomeService.Infrastructure.Services.Notifications;

public class FirebasePushNotificationService : IPushNotificationService
{
    private readonly ILogger<FirebasePushNotificationService> _logger;
    private readonly IConfiguration _configuration;
    private readonly FirebaseApp _firebaseApp;

    public FirebasePushNotificationService(
        ILogger<FirebasePushNotificationService> logger,
        IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;

        try
        {
            // Initialize Firebase if not already initialized
            if (FirebaseApp.DefaultInstance == null)
            {
                var credentialPath = configuration["Firebase:CredentialPath"];

                if (!string.IsNullOrEmpty(credentialPath) && File.Exists(credentialPath))
                {
                    _firebaseApp = FirebaseApp.Create(new AppOptions
                    {
                        Credential = GoogleCredential.FromFile(credentialPath)
                    });
                }
                else
                {
                    _logger.LogWarning("Firebase credential file not found. Push notifications will be simulated.");
                    _firebaseApp = null!;
                }
            }
            else
            {
                _firebaseApp = FirebaseApp.DefaultInstance;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing Firebase");
            _firebaseApp = null!;
        }
    }

    public async Task<bool> SendNotificationAsync(
        string deviceToken,
        PushNotificationMessage message,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (_firebaseApp == null)
            {
                _logger.LogWarning("Firebase not initialized. Simulating push notification to {DeviceToken}", deviceToken);
                return true; // Simulate success in development
            }

            var fcmMessage = new Message
            {
                Token = deviceToken,
                Notification = new Notification
                {
                    Title = message.Title,
                    Body = message.Body,
                    ImageUrl = message.ImageUrl
                },
                Data = message.Data,
                Android = new AndroidConfig
                {
                    Priority = message.Priority == PushNotificationPriority.High
                        ? Priority.High
                        : Priority.Normal,
                    Notification = new AndroidNotification
                    {
                        Sound = message.Sound,
                        ClickAction = message.ClickAction
                    }
                },
                Apns = new ApnsConfig
                {
                    Aps = new Aps
                    {
                        Sound = message.Sound,
                        Badge = message.Badge
                    }
                }
            };

            var response = await FirebaseMessaging.DefaultInstance.SendAsync(fcmMessage, cancellationToken);

            _logger.LogInformation("Push notification sent successfully. Message ID: {MessageId}", response);
            return true;
        }
        catch (FirebaseMessagingException ex)
        {
            _logger.LogError(ex, "Firebase messaging error sending notification to {DeviceToken}: {ErrorCode}",
                deviceToken, ex.MessagingErrorCode);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending push notification to {DeviceToken}", deviceToken);
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
            if (_firebaseApp == null)
            {
                _logger.LogWarning("Firebase not initialized. Simulating batch push notification");
                result.SuccessCount = deviceTokens.Count;
                return result;
            }

            var multicastMessage = new MulticastMessage
            {
                Tokens = deviceTokens,
                Notification = new Notification
                {
                    Title = message.Title,
                    Body = message.Body,
                    ImageUrl = message.ImageUrl
                },
                Data = message.Data
            };

            var batchResponse = await FirebaseMessaging.DefaultInstance.SendEachForMulticastAsync(
                multicastMessage,
                cancellationToken);

            result.SuccessCount = batchResponse.SuccessCount;
            result.FailureCount = batchResponse.FailureCount;

            for (int i = 0; i < batchResponse.Responses.Count; i++)
            {
                var response = batchResponse.Responses[i];
                if (!response.IsSuccess)
                {
                    result.FailedTokens.Add(deviceTokens[i]);
                    result.ErrorMessages.Add(response.Exception?.Message ?? "Unknown error");
                }
            }

            _logger.LogInformation("Batch notification sent. Success: {SuccessCount}, Failed: {FailureCount}",
                result.SuccessCount, result.FailureCount);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending batch push notification");
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
            if (_firebaseApp == null)
            {
                _logger.LogWarning("Firebase not initialized. Simulating topic notification to {Topic}", topic);
                return true;
            }

            var fcmMessage = new Message
            {
                Topic = topic,
                Notification = new Notification
                {
                    Title = message.Title,
                    Body = message.Body,
                    ImageUrl = message.ImageUrl
                },
                Data = message.Data
            };

            var response = await FirebaseMessaging.DefaultInstance.SendAsync(fcmMessage, cancellationToken);

            _logger.LogInformation("Topic notification sent to {Topic}. Message ID: {MessageId}", topic, response);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending topic notification to {Topic}", topic);
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
            if (_firebaseApp == null)
            {
                _logger.LogWarning("Firebase not initialized. Simulating topic subscription");
                return true;
            }

            var response = await FirebaseMessaging.DefaultInstance.SubscribeToTopicAsync(
                new List<string> { deviceToken },
                topic,
                cancellationToken);

            _logger.LogInformation("Device subscribed to topic {Topic}. Success: {SuccessCount}",
                topic, response.SuccessCount);

            return response.SuccessCount > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error subscribing device to topic {Topic}", topic);
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
            if (_firebaseApp == null)
            {
                _logger.LogWarning("Firebase not initialized. Simulating topic unsubscription");
                return true;
            }

            var response = await FirebaseMessaging.DefaultInstance.UnsubscribeFromTopicAsync(
                new List<string> { deviceToken },
                topic,
                cancellationToken);

            _logger.LogInformation("Device unsubscribed from topic {Topic}. Success: {SuccessCount}",
                topic, response.SuccessCount);

            return response.SuccessCount > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unsubscribing device from topic {Topic}", topic);
            return false;
        }
    }

    #region Specific Notification Methods

    public async Task<bool> SendNewBookingNotificationAsync(
        string deviceToken,
        BookingNotificationData data,
        CancellationToken cancellationToken = default)
    {
        var message = new PushNotificationMessage
        {
            Title = "New Booking Request",
            Body = $"New booking from {data.CustomerName} for {data.ServiceName}",
            Priority = PushNotificationPriority.High,
            Sound = "booking_alert",
            Data = new Dictionary<string, string>
            {
                ["type"] = "new_booking",
                ["bookingId"] = data.BookingId,
                ["serviceName"] = data.ServiceName,
                ["customerName"] = data.CustomerName,
                ["scheduledDate"] = data.ScheduledDate.ToString("O"),
                ["amount"] = data.Amount.ToString(),
                ["currency"] = data.Currency,
                ["expiryMinutes"] = data.ExpiryMinutes.ToString()
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
        var message = new PushNotificationMessage
        {
            Title = "Booking Status Update",
            Body = $"Your booking status has been updated to: {status}",
            Priority = PushNotificationPriority.High,
            Data = new Dictionary<string, string>
            {
                ["type"] = "booking_status",
                ["bookingId"] = bookingId,
                ["status"] = status
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
            Title = "Payment Confirmed",
            Body = $"Payment of {amount} {currency} has been processed successfully",
            Priority = PushNotificationPriority.Normal,
            Data = new Dictionary<string, string>
            {
                ["type"] = "payment_confirmation",
                ["bookingId"] = bookingId,
                ["amount"] = amount.ToString(),
                ["currency"] = currency
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
            Title = $"New message from {senderName}",
            Body = messagePreview,
            Priority = PushNotificationPriority.Normal,
            Data = new Dictionary<string, string>
            {
                ["type"] = "new_message",
                ["chatId"] = chatId,
                ["senderName"] = senderName
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
        var message = new PushNotificationMessage
        {
            Title = "Provider Location Update",
            Body = "Your service provider is on the way",
            Priority = PushNotificationPriority.Normal,
            Data = new Dictionary<string, string>
            {
                ["type"] = "location_update",
                ["bookingId"] = bookingId,
                ["latitude"] = latitude.ToString(),
                ["longitude"] = longitude.ToString()
            }
        };

        return await SendNotificationAsync(deviceToken, message, cancellationToken);
    }

    #endregion
}
