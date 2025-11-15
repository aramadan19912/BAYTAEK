# Customer Mobile App - API Integration Guide

## Authentication

### Register
```http
POST /api/auth/register
Content-Type: application/json

{
  "email": "customer@example.com",
  "password": "SecurePass123!",
  "firstName": "John",
  "lastName": "Doe",
  "phoneNumber": "+966501234567",
  "role": "Customer"
}
```

### Login
```http
POST /api/auth/login
Content-Type: application/json

{
  "email": "customer@example.com",
  "password": "SecurePass123!"
}
```

Response:
```json
{
  "isSuccess": true,
  "data": {
    "accessToken": "eyJhbGciOiJIUzI1NiIs...",
    "refreshToken": "a1b2c3d4...",
    "expiresAt": "2024-12-01T10:00:00Z",
    "user": {
      "id": "guid",
      "email": "customer@example.com",
      "firstName": "John",
      "lastName": "Doe",
      "role": "Customer"
    }
  }
}
```

## Services

### Browse Services
```http
GET /api/services/search?
  categoryId=<guid>&
  region=Riyadh&
  searchTerm=plumbing&
  minPrice=50&
  maxPrice=500&
  sortBy=price&
  pageNumber=1&
  pageSize=20
```

Response:
```json
{
  "isSuccess": true,
  "data": {
    "items": [
      {
        "id": "guid",
        "nameEn": "Plumbing Repair",
        "nameAr": "إصلاح السباكة",
        "descriptionEn": "Professional plumbing services",
        "descriptionAr": "خدمات سباكة احترافية",
        "price": 150.00,
        "currency": "SAR",
        "categoryId": "guid",
        "categoryName": "Home Maintenance",
        "imageUrl": "https://...",
        "averageRating": 4.5,
        "totalReviews": 120
      }
    ],
    "totalCount": 45,
    "pageNumber": 1,
    "pageSize": 20
  }
}
```

### Get Service Details
```http
GET /api/services/{serviceId}
```

## Service Categories
```http
GET /api/categories
```

## Providers

### Browse Providers
```http
GET /api/providers/search?
  searchTerm=&
  region=Riyadh&
  categoryId=<guid>&
  minRating=4&
  isVerified=true&
  isAvailable=true&
  sortBy=rating&
  pageNumber=1&
  pageSize=20
```

### Get Provider Details
```http
GET /api/providers/{providerId}
```

### Get Provider Availability
```http
GET /api/providers/{providerId}/availability?
  startDate=2024-12-01&
  endDate=2024-12-31
```

Response:
```json
{
  "isSuccess": true,
  "data": {
    "providerId": "guid",
    "weeklySchedule": [
      {
        "dayOfWeek": "Monday",
        "startTime": "08:00:00",
        "endTime": "17:00:00",
        "isAvailable": true
      }
    ],
    "blockedDates": [
      {
        "id": "guid",
        "startDate": "2024-12-25T00:00:00Z",
        "endDate": "2024-12-26T00:00:00Z",
        "reason": "Holiday",
        "isAllDay": true
      }
    ]
  }
}
```

## Bookings

### Create Booking
```http
POST /api/bookings
Content-Type: application/json

{
  "serviceId": "guid",
  "providerId": "guid",
  "addressId": "guid",
  "scheduledAt": "2024-12-15T10:00:00Z",
  "specialInstructions": "Please bring extra tools",
  "promoCode": "SAVE20"
}
```

### Get My Bookings
```http
GET /api/bookings/my-bookings?
  status=Confirmed&
  startDate=2024-12-01&
  pageNumber=1&
  pageSize=20
```

### Get Booking Details
```http
GET /api/bookings/{bookingId}
```

### Cancel Booking
```http
POST /api/bookings/{bookingId}/cancel
Content-Type: application/json

{
  "reason": "Schedule conflict"
}
```

### Reschedule Booking
```http
POST /api/bookings/{bookingId}/reschedule
Content-Type: application/json

{
  "newScheduledAt": "2024-12-16T14:00:00Z",
  "reason": "Customer request"
}
```

## Payments

### Create Payment Intent
```http
POST /api/payments/create-intent
Content-Type: application/json

{
  "bookingId": "guid",
  "paymentMethod": "Card"
}
```

Response:
```json
{
  "isSuccess": true,
  "data": {
    "clientSecret": "pi_xxx_secret_xxx",
    "paymentIntentId": "pi_xxx",
    "amount": 150.00,
    "currency": "SAR"
  }
}
```

### Get Payment History
```http
GET /api/payments/history?
  startDate=2024-01-01&
  endDate=2024-12-31&
  status=Completed&
  pageNumber=1&
  pageSize=20
```

## Reviews

### Submit Review
```http
POST /api/reviews
Content-Type: application/json

{
  "bookingId": "guid",
  "rating": 5,
  "comment": "Excellent service!",
  "imageUrls": [
    "https://storage.blob.core.windows.net/files/img1.jpg"
  ]
}
```

### Update Review (within 48 hours)
```http
PUT /api/reviews/{reviewId}
Content-Type: application/json

{
  "rating": 4,
  "comment": "Updated: Good service",
  "imageUrls": []
}
```

### Delete Review
```http
DELETE /api/reviews/{reviewId}
```

### Get My Reviews
```http
GET /api/reviews/my-reviews?
  pageNumber=1&
  pageSize=20
```

### Get Service Reviews
```http
GET /api/reviews/service/{serviceId}?
  minRating=4&
  withImagesOnly=true&
  pageNumber=1&
  pageSize=20
```

### Mark Review as Helpful
```http
POST /api/reviews/{reviewId}/helpful
```

## Addresses

### Get My Addresses
```http
GET /api/addresses
```

### Create Address
```http
POST /api/addresses
Content-Type: application/json

{
  "street": "King Fahd Road",
  "city": "Riyadh",
  "region": "Riyadh",
  "postalCode": "12345",
  "country": "Saudi Arabia",
  "latitude": 24.7136,
  "longitude": 46.6753,
  "isDefault": true,
  "label": "Home"
}
```

### Update Address
```http
PUT /api/addresses/{addressId}
Content-Type: application/json

{
  "street": "Updated Street",
  "city": "Riyadh",
  "isDefault": true
}
```

### Delete Address
```http
DELETE /api/addresses/{addressId}
```

## Notifications

### Get My Notifications
```http
GET /api/notifications?
  isRead=false&
  category=Booking&
  pageNumber=1&
  pageSize=20
```

### Mark as Read
```http
PUT /api/notifications/{notificationId}/read
```

### Mark All as Read
```http
PUT /api/notifications/read-all
```

### Delete Notification
```http
DELETE /api/notifications/{notificationId}
```

### Get Notification Settings
```http
GET /api/notifications/settings
```

### Update Notification Settings
```http
PUT /api/notifications/settings
Content-Type: application/json

{
  "bookingNotifications": {
    "push": true,
    "email": true,
    "sms": false
  },
  "paymentNotifications": {
    "push": true,
    "email": true,
    "sms": true
  },
  "messageNotifications": {
    "push": true,
    "email": false,
    "sms": false
  }
}
```

### Register Device Token (for Push Notifications)
```http
POST /api/notifications/device-token
Content-Type: application/json

{
  "token": "fcm-device-token",
  "platform": "iOS",
  "appVersion": "1.0.0"
}
```

## Messages

### Get Conversations
```http
GET /api/messages/conversations
```

### Get Messages in Conversation
```http
GET /api/messages/{conversationId}/messages?
  pageNumber=1&
  pageSize=50
```

### Send Message
```http
POST /api/messages/send
Content-Type: application/json

{
  "conversationId": "guid",
  "content": "Hello, when can you start?",
  "type": "Text",
  "attachmentUrl": null
}
```

### Mark Messages as Read
```http
PUT /api/messages/{conversationId}/read
```

## Profile

### Get My Profile
```http
GET /api/auth/profile
```

### Update Profile
```http
PUT /api/users/profile
Content-Type: application/json

{
  "firstName": "John",
  "lastName": "Doe",
  "phoneNumber": "+966501234567",
  "profilePhotoUrl": "https://..."
}
```

### Change Password
```http
PUT /api/auth/change-password
Content-Type: application/json

{
  "currentPassword": "OldPass123!",
  "newPassword": "NewPass123!"
}
```

### Upload Profile Photo
```http
POST /api/files/profile-image
Content-Type: multipart/form-data

file: <binary>
```

## Real-time Updates (SignalR)

### Connect to Notifications Hub
```javascript
import * as signalR from "@microsoft/signalr";

const connection = new signalR.HubConnectionBuilder()
  .withUrl("https://api.baytaek.com/hubs/notifications", {
    accessTokenFactory: () => accessToken
  })
  .withAutomaticReconnect()
  .build();

// Listen for notifications
connection.on("ReceiveNotification", (notification) => {
  console.log("New notification:", notification);
  showNotification(notification);
});

// Listen for booking updates
connection.on("BookingStatusChanged", (booking) => {
  console.log("Booking updated:", booking);
  updateBookingUI(booking);
});

await connection.start();
```

### Connect to Chat Hub
```javascript
const chatConnection = new signalR.HubConnectionBuilder()
  .withUrl("https://api.baytaek.com/hubs/chat", {
    accessTokenFactory: () => accessToken
  })
  .withAutomaticReconnect()
  .build();

// Listen for new messages
chatConnection.on("ReceiveMessage", (message) => {
  console.log("New message:", message);
  addMessageToChat(message);
});

// Listen for typing indicators
chatConnection.on("UserTyping", (data) => {
  console.log(`${data.userName} is typing...`);
  showTypingIndicator(data.conversationId);
});

await chatConnection.start();
```

## Error Handling Example

```javascript
try {
  const response = await fetch('https://api.baytaek.com/api/bookings', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${accessToken}`
    },
    body: JSON.stringify(bookingData)
  });

  const result = await response.json();

  if (!result.isSuccess) {
    // Handle API error
    console.error('API Error:', result.error);
    if (result.errors && result.errors.length > 0) {
      // Show validation errors
      result.errors.forEach(error => console.error(error));
    }
    return;
  }

  // Success
  console.log('Booking created:', result.data);
} catch (error) {
  // Handle network error
  console.error('Network error:', error);
}
```

## Pagination Helper

```javascript
function buildPaginatedUrl(baseUrl, params) {
  const queryParams = new URLSearchParams({
    pageNumber: params.pageNumber || 1,
    pageSize: params.pageSize || 20,
    ...params.filters
  });
  return `${baseUrl}?${queryParams.toString()}`;
}

// Usage
const url = buildPaginatedUrl('/api/bookings/my-bookings', {
  pageNumber: 1,
  pageSize: 20,
  filters: {
    status: 'Confirmed',
    startDate: '2024-12-01'
  }
});
```

## Best Practices

1. **Token Management**
   - Store tokens securely (Keychain/Keystore)
   - Implement automatic token refresh
   - Clear tokens on logout

2. **Error Handling**
   - Handle network errors gracefully
   - Show user-friendly error messages
   - Implement retry logic for failed requests

3. **Performance**
   - Implement pagination for lists
   - Cache frequently accessed data
   - Use optimistic updates for better UX

4. **Real-time**
   - Implement reconnection logic
   - Handle connection state changes
   - Show connection status to users

5. **Security**
   - Never log sensitive data
   - Validate all user inputs
   - Use HTTPS only
   - Implement certificate pinning
