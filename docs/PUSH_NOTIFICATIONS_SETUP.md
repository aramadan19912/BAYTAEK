# Push Notifications Setup Guide - Firebase Cloud Messaging (FCM)

This guide covers the complete setup and usage of Firebase Cloud Messaging for push notifications in the Home Service application.

## Overview

Push notifications are used for real-time communication with users:
- **Providers:** New job requests, booking updates, payment notifications
- **Customers:** Booking confirmations, provider location updates, chat messages
- **Both:** General announcements, promotional offers

## Features

✅ Single device notifications
✅ Batch notifications (up to 500 devices per batch)
✅ Topic-based notifications (broadcast to all subscribers)
✅ High priority notifications (for time-sensitive alerts)
✅ Silent notifications (for data updates)
✅ Rich notifications with images
✅ Custom data payloads
✅ Platform-specific configuration (Android & iOS)

## Prerequisites

### 1. Firebase Project Setup

1. **Create Firebase Project:**
   - Go to [Firebase Console](https://console.firebase.google.com/)
   - Click "Add project"
   - Enter project name: `home-service-prod` (or your preferred name)
   - Enable Google Analytics (optional)
   - Click "Create project"

2. **Add Apps to Firebase:**

   **For Android:**
   - Click "Add app" → Select Android
   - Register app with package name (e.g., `com.homeservice.app`)
   - Download `google-services.json`
   - Place in `android/app/` directory

   **For iOS:**
   - Click "Add app" → Select iOS
   - Register app with bundle ID (e.g., `com.homeservice.app`)
   - Download `GoogleService-Info.plist`
   - Place in iOS project root

### 2. Generate Service Account Key

1. Go to Project Settings → Service Accounts
2. Click "Generate New Private Key"
3. Save the JSON file securely
4. Rename to `firebase-service-account.json`
5. Place in a secure location (NOT in source control)

### 3. Required NuGet Package

```bash
dotnet add package FirebaseAdmin --version 3.0.0
```

## Backend Configuration

### 1. Application Settings

**Option A: File Path Configuration (Development)**

Update `appsettings.json`:
```json
{
  "Firebase": {
    "ServiceAccountKeyPath": "/path/to/firebase-service-account.json"
  }
}
```

**Option B: JSON Configuration (Alternative)**

```json
{
  "Firebase": {
    "ServiceAccountJson": "{...JSON content...}"
  }
}
```

### 2. User Secrets (Development)

```bash
cd backend/src/HomeService.API

# Set service account path
dotnet user-secrets set "Firebase:ServiceAccountKeyPath" "/secure/path/firebase-service-account.json"
```

### 3. Azure Key Vault (Production)

Store Firebase credentials in Azure Key Vault:

```bash
# Upload service account to Key Vault
az keyvault secret set \
  --vault-name "homeservice-keyvault" \
  --name "Firebase--ServiceAccountJson" \
  --file firebase-service-account.json
```

In `Program.cs`:
```csharp
builder.Configuration.AddAzureKeyVault(
    new Uri($"https://{keyVaultName}.vault.azure.net/"),
    new DefaultAzureCredential());
```

### 4. Environment Variables (Docker/Kubernetes)

```dockerfile
# Dockerfile
ENV Firebase__ServiceAccountKeyPath=/app/secrets/firebase-service-account.json
```

```yaml
# Kubernetes Secret
apiVersion: v1
kind: Secret
metadata:
  name: firebase-secret
type: Opaque
data:
  service-account.json: <base64-encoded-json>
```

## Usage Examples

### 1. Basic Notification

```csharp
public class BookingService
{
    private readonly IPushNotificationService _pushService;

    public async Task SendBasicNotificationAsync(string deviceToken)
    {
        var message = new PushNotificationMessage
        {
            Title = "Hello!",
            Body = "This is a test notification",
            Priority = PushNotificationPriority.Normal,
            Sound = "default"
        };

        await _pushService.SendNotificationAsync(deviceToken, message);
    }
}
```

### 2. New Booking Notification (Provider)

```csharp
public async Task NotifyProviderOfNewBookingAsync(Provider provider, Booking booking)
{
    if (string.IsNullOrEmpty(provider.DeviceToken))
        return;

    var notificationData = new BookingNotificationData
    {
        BookingId = booking.Id.ToString(),
        ServiceName = booking.Service.Name,
        CustomerName = booking.Customer.FullName,
        ScheduledDate = booking.ScheduledDateTime,
        Location = booking.Address,
        Amount = booking.ProviderEarnings,
        Currency = booking.Currency,
        ExpiryMinutes = 10
    };

    await _pushService.SendNewBookingNotificationAsync(
        provider.DeviceToken,
        notificationData
    );
}
```

### 3. Booking Status Update (Customer)

```csharp
public async Task NotifyCustomerOfStatusUpdateAsync(
    Customer customer,
    string bookingId,
    BookingStatus status)
{
    if (string.IsNullOrEmpty(customer.DeviceToken))
        return;

    await _pushService.SendBookingStatusUpdateAsync(
        customer.DeviceToken,
        bookingId,
        status.ToString()
    );
}
```

### 4. Payment Confirmation

```csharp
public async Task NotifyPaymentSuccessAsync(
    Customer customer,
    Payment payment)
{
    if (string.IsNullOrEmpty(customer.DeviceToken))
        return;

    await _pushService.SendPaymentConfirmationAsync(
        customer.DeviceToken,
        payment.BookingId.ToString(),
        payment.Amount,
        payment.Currency
    );
}
```

### 5. Chat Message Notification

```csharp
public async Task NotifyNewMessageAsync(
    User recipient,
    User sender,
    ChatMessage message)
{
    if (string.IsNullOrEmpty(recipient.DeviceToken))
        return;

    await _pushService.SendMessageNotificationAsync(
        recipient.DeviceToken,
        sender.FullName,
        message.Content.Substring(0, Math.Min(100, message.Content.Length)),
        message.ChatId.ToString()
    );
}
```

### 6. Batch Notifications

```csharp
public async Task NotifyMultipleProvidersAsync(
    List<Provider> providers,
    PushNotificationMessage message)
{
    var deviceTokens = providers
        .Where(p => !string.IsNullOrEmpty(p.DeviceToken))
        .Select(p => p.DeviceToken)
        .ToList();

    var result = await _pushService.SendBatchNotificationAsync(
        deviceTokens,
        message
    );

    _logger.LogInformation(
        "Batch notification sent. Success: {Success}, Failed: {Failed}",
        result.SuccessCount,
        result.FailureCount
    );

    // Clean up invalid tokens
    if (result.FailedTokens.Any())
    {
        await RemoveInvalidTokensAsync(result.FailedTokens);
    }
}
```

### 7. Topic-Based Notifications

```csharp
// Subscribe users to topics
public async Task SubscribeToRegionalTopicAsync(User user, string region)
{
    if (string.IsNullOrEmpty(user.DeviceToken))
        return;

    var topic = $"region_{region.ToLower()}";
    await _pushService.SubscribeToTopicAsync(user.DeviceToken, topic);
}

// Send to all users in a region
public async Task SendRegionalAnnouncementAsync(
    string region,
    string title,
    string body)
{
    var topic = $"region_{region.ToLower()}";
    var message = new PushNotificationMessage
    {
        Title = title,
        Body = body,
        Priority = PushNotificationPriority.Normal
    };

    await _pushService.SendTopicNotificationAsync(topic, message);
}
```

### 8. Silent Notification (Location Update)

```csharp
public async Task SendProviderLocationUpdateAsync(
    Customer customer,
    string bookingId,
    double latitude,
    double longitude)
{
    if (string.IsNullOrEmpty(customer.DeviceToken))
        return;

    // Silent notification - only data payload, no UI alert
    await _pushService.SendLocationUpdateAsync(
        customer.DeviceToken,
        bookingId,
        latitude,
        longitude
    );
}
```

### 9. Rich Notification with Image

```csharp
public async Task SendPromotionalNotificationAsync(
    User user,
    Promotion promotion)
{
    if (string.IsNullOrEmpty(user.DeviceToken))
        return;

    var message = new PushNotificationMessage
    {
        Title = promotion.Title,
        Body = promotion.Description,
        ImageUrl = promotion.ImageUrl,
        Priority = PushNotificationPriority.Normal,
        Data = new Dictionary<string, string>
        {
            { "type", "promotion" },
            { "promotionId", promotion.Id.ToString() },
            { "deepLink", $"homeservice://promotions/{promotion.Id}" }
        }
    };

    await _pushService.SendNotificationAsync(user.DeviceToken, message);
}
```

## Mobile App Integration

### Android Setup (Flutter/React Native)

**1. Install Dependencies:**

```bash
# Flutter
flutter pub add firebase_messaging

# React Native
npm install @react-native-firebase/app @react-native-firebase/messaging
```

**2. Request Permission & Get Token:**

```dart
// Flutter
import 'package:firebase_messaging/firebase_messaging.dart';

Future<String?> getDeviceToken() async {
  final fcm = FirebaseMessaging.instance;

  // Request permission
  await fcm.requestPermission(
    alert: true,
    badge: true,
    sound: true,
  );

  // Get token
  String? token = await fcm.getToken();
  print('Device Token: $token');

  return token;
}

// Listen for token refresh
FirebaseMessaging.instance.onTokenRefresh.listen((newToken) {
  // Send new token to backend
  updateDeviceToken(newToken);
});
```

**3. Handle Notifications:**

```dart
// Foreground messages
FirebaseMessaging.onMessage.listen((RemoteMessage message) {
  print('Got a message in foreground!');
  print('Message data: ${message.data}');

  if (message.notification != null) {
    showLocalNotification(message.notification!);
  }
});

// Background messages
FirebaseMessaging.onBackgroundMessage(_firebaseMessagingBackgroundHandler);

Future<void> _firebaseMessagingBackgroundHandler(RemoteMessage message) async {
  print('Handling background message: ${message.messageId}');
  // Handle notification
}

// Notification tap (app opened from notification)
FirebaseMessaging.onMessageOpenedApp.listen((RemoteMessage message) {
  print('Notification tapped!');
  handleNotificationTap(message.data);
});
```

### iOS Setup (Swift/Flutter/React Native)

**1. Enable Push Notifications:**
- Open Xcode project
- Select target → Signing & Capabilities
- Click "+ Capability" → Add "Push Notifications"
- Add "Background Modes" → Check "Remote notifications"

**2. Upload APNs Certificate to Firebase:**
- Generate APNs certificate in Apple Developer Portal
- Upload to Firebase Console → Project Settings → Cloud Messaging → iOS app configuration

**3. Handle Notifications (Swift):**

```swift
import Firebase
import UserNotifications

// In AppDelegate
func application(_ application: UIApplication,
                 didFinishLaunchingWithOptions launchOptions: [UIApplication.LaunchOptionsKey: Any]?) -> Bool {
    FirebaseApp.configure()

    UNUserNotificationCenter.current().delegate = self
    application.registerForRemoteNotifications()

    Messaging.messaging().delegate = self

    return true
}

// Get device token
extension AppDelegate: MessagingDelegate {
    func messaging(_ messaging: Messaging, didReceiveRegistrationToken fcmToken: String?) {
        print("FCM Token: \(fcmToken ?? "")")
        // Send to backend
    }
}

// Handle notifications
extension AppDelegate: UNUserNotificationCenterDelegate {
    func userNotificationCenter(_ center: UNUserNotificationCenter,
                                willPresent notification: UNNotification,
                                withCompletionHandler completionHandler: @escaping (UNNotificationPresentationOptions) -> Void) {
        completionHandler([[.banner, .sound, .badge]])
    }

    func userNotificationCenter(_ center: UNUserNotificationCenter,
                                didReceive response: UNNotificationResponse,
                                withCompletionHandler completionHandler: @escaping () -> Void) {
        let userInfo = response.notification.request.content.userInfo
        handleNotificationTap(userInfo)
        completionHandler()
    }
}
```

## Backend API Endpoints

Create endpoints for device token management:

```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DeviceTokensController : ControllerBase
{
    private readonly IDeviceTokenService _deviceTokenService;
    private readonly IPushNotificationService _pushService;

    [HttpPost("register")]
    public async Task<IActionResult> RegisterDeviceToken([FromBody] RegisterTokenRequest request)
    {
        var userId = User.GetUserId();
        await _deviceTokenService.RegisterTokenAsync(userId, request.Token, request.Platform);
        return Ok(new { success = true });
    }

    [HttpDelete("unregister")]
    public async Task<IActionResult> UnregisterDeviceToken([FromBody] UnregisterTokenRequest request)
    {
        var userId = User.GetUserId();
        await _deviceTokenService.UnregisterTokenAsync(userId, request.Token);
        return Ok(new { success = true });
    }

    [HttpPost("test")]
    public async Task<IActionResult> TestNotification()
    {
        var userId = User.GetUserId();
        var token = await _deviceTokenService.GetUserTokenAsync(userId);

        if (string.IsNullOrEmpty(token))
            return BadRequest("No device token registered");

        var message = new PushNotificationMessage
        {
            Title = "Test Notification",
            Body = "This is a test notification from Home Service!",
            Priority = PushNotificationPriority.Normal
        };

        var result = await _pushService.SendNotificationAsync(token, message);
        return Ok(new { success = result });
    }
}

public class RegisterTokenRequest
{
    public string Token { get; set; } = string.Empty;
    public string Platform { get; set; } = string.Empty; // "android" or "ios"
}
```

## Database Schema for Device Tokens

```sql
CREATE TABLE DeviceTokens (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    UserId UNIQUEIDENTIFIER NOT NULL,
    Token NVARCHAR(500) NOT NULL,
    Platform NVARCHAR(50) NOT NULL, -- 'android' or 'ios'
    IsActive BIT NOT NULL DEFAULT 1,
    LastUsedAt DATETIME2 NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    FOREIGN KEY (UserId) REFERENCES Users(Id),
    INDEX IX_DeviceTokens_UserId (UserId),
    INDEX IX_DeviceTokens_Token (Token)
);
```

## Topics Strategy

Organize users into topics for targeted notifications:

### Regional Topics
- `region_riyadh`
- `region_jeddah`
- `region_cairo`
- `region_alexandria`

### Role-Based Topics
- `role_customer`
- `role_provider`
- `role_admin`

### Service Category Topics
- `service_plumbing`
- `service_electrical`
- `service_ac_maintenance`
- `service_cleaning`

### Implementation:

```csharp
public async Task ManageUserTopicsAsync(User user)
{
    if (string.IsNullOrEmpty(user.DeviceToken))
        return;

    // Regional topic
    var regionTopic = $"region_{user.Region.ToLower()}";
    await _pushService.SubscribeToTopicAsync(user.DeviceToken, regionTopic);

    // Role topic
    var roleTopic = $"role_{user.Role.ToLower()}";
    await _pushService.SubscribeToTopicAsync(user.DeviceToken, roleTopic);

    // Service category topics (for providers)
    if (user is Provider provider)
    {
        foreach (var category in provider.ServiceCategories)
        {
            var categoryTopic = $"service_{category.ToLower().Replace(" ", "_")}";
            await _pushService.SubscribeToTopicAsync(user.DeviceToken, categoryTopic);
        }
    }
}
```

## Best Practices

### 1. Token Management
- Store tokens securely in database
- Associate tokens with user accounts
- Support multiple tokens per user (multiple devices)
- Remove invalid/expired tokens automatically
- Update tokens when they refresh

### 2. Notification Frequency
- Don't spam users with too many notifications
- Implement user preferences for notification types
- Use silent notifications for background data updates
- Batch notifications when appropriate

### 3. Content Guidelines
- Keep titles under 50 characters
- Keep body under 200 characters
- Use emojis sparingly for visual appeal
- Include relevant data payload for deep linking
- Localize content based on user language

### 4. Priority Management
- Use HIGH priority only for time-sensitive notifications
- Use NORMAL priority for informational notifications
- Silent notifications for data updates

### 5. Error Handling
- Log all notification failures
- Implement retry logic with exponential backoff
- Clean up invalid tokens automatically
- Monitor delivery rates in Firebase Console

### 6. Testing
- Test on real devices (emulators may not work reliably)
- Test both foreground and background scenarios
- Test notification taps and deep linking
- Test with different user permissions

## Monitoring & Analytics

### Firebase Console

Monitor push notification performance:
1. Go to Cloud Messaging section
2. View sent messages
3. Check delivery rates
4. Track user engagement

### Application Logging

```csharp
// Success
_logger.LogInformation("Push notification sent successfully to {UserId}", userId);

// Failure
_logger.LogError("Failed to send push notification to {UserId}. Error: {Error}",
    userId, errorMessage);

// Batch results
_logger.LogInformation("Batch notification: {Success} succeeded, {Failed} failed",
    result.SuccessCount, result.FailureCount);
```

### Database Tracking

Create a notifications table to track sent notifications:

```sql
CREATE TABLE NotificationLogs (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    UserId UNIQUEIDENTIFIER NOT NULL,
    Type NVARCHAR(100) NOT NULL,
    Title NVARCHAR(200),
    Body NVARCHAR(500),
    Status NVARCHAR(50) NOT NULL, -- 'sent', 'failed', 'delivered', 'opened'
    SentAt DATETIME2 NOT NULL,
    DeliveredAt DATETIME2 NULL,
    OpenedAt DATETIME2 NULL,
    FOREIGN KEY (UserId) REFERENCES Users(Id)
);
```

## Troubleshooting

### Common Issues

**1. "Service account not found" error**
- Verify the path to `firebase-service-account.json` is correct
- Check file permissions
- Ensure the service account has FCM permissions

**2. Notifications not received on iOS**
- Verify APNs certificate is uploaded to Firebase
- Check if push notifications capability is enabled in Xcode
- Ensure device has granted notification permissions

**3. Notifications not received on Android**
- Verify `google-services.json` is in the correct location
- Check if app is in battery optimization whitelist
- Ensure Google Play Services is available on device

**4. "Invalid token" errors**
- Token may have expired or been invalidated
- User may have uninstalled the app
- Clean up invalid tokens from database

**5. High failure rate**
- Check if tokens are still valid
- Verify Firebase project configuration
- Review notification payload size (max 4KB)

## Regional Considerations

### Saudi Arabia & Egypt

**1. Notification Timing:**
```csharp
// Avoid notifications during prayer times
public bool IsValidNotificationTime(DateTime utcTime, string region)
{
    var localTime = TimeZoneInfo.ConvertTimeFromUtc(utcTime, GetTimeZone(region));
    var hour = localTime.Hour;

    // Avoid late night notifications (11 PM - 7 AM)
    if (hour >= 23 || hour < 7)
        return false;

    // TODO: Check against prayer times API
    return true;
}
```

**2. Arabic Language Support:**
```csharp
var message = new PushNotificationMessage
{
    Title = user.Language == "ar" ? "طلب خدمة جديد" : "New Service Request",
    Body = user.Language == "ar"
        ? $"لديك طلب خدمة جديد من {customerName}"
        : $"You have a new service request from {customerName}",
    // ...
};
```

## Cost Considerations

Firebase Cloud Messaging is **FREE** with no usage limits!

However, consider:
1. **Firebase Hosting/Storage** costs if using other Firebase services
2. **Server costs** for running backend that sends notifications
3. **Network bandwidth** for notification delivery

## Security Best Practices

1. **Never expose service account keys**
   - Store in secure locations only
   - Use environment variables or secret managers
   - Never commit to version control

2. **Validate device tokens**
   - Verify tokens belong to authenticated users
   - Don't allow arbitrary token registration

3. **Sanitize notification content**
   - Validate and escape user-generated content
   - Limit payload size
   - Don't include sensitive data in notifications

4. **Rate limiting**
   - Implement rate limits per user
   - Prevent notification spam
   - Monitor for unusual patterns

## Next Steps

After setup:
1. ✅ Configure Firebase project and service account
2. ✅ Integrate in mobile apps
3. ✅ Test notification delivery
4. ⏳ Implement device token management endpoints
5. ⏳ Create notification preference settings for users
6. ⏳ Set up monitoring and analytics
7. ⏳ Implement notification history/logs

---

**Last Updated:** November 2025
**Version:** 1.0
