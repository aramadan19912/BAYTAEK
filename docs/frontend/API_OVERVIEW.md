# BAYTAEK Home Service Platform - API Overview

## Base URL
```
Development: https://localhost:7001/api
Production: https://api.baytaek.com/api
```

## Authentication

### JWT Bearer Token Authentication
All authenticated endpoints require a JWT token in the Authorization header:

```http
Authorization: Bearer <your-jwt-token>
```

### Refresh Token Flow
When access token expires, use refresh token to get new tokens:

```http
POST /api/auth/refresh-token
Content-Type: application/json

{
  "accessToken": "expired-token",
  "refreshToken": "refresh-token"
}
```

Response:
```json
{
  "isSuccess": true,
  "data": {
    "accessToken": "new-access-token",
    "refreshToken": "new-refresh-token",
    "expiresAt": "2024-12-01T10:00:00Z"
  }
}
```

## API Response Format

### Success Response
```json
{
  "isSuccess": true,
  "data": { ... },
  "message": "Operation successful"
}
```

### Error Response
```json
{
  "isSuccess": false,
  "error": "Error message",
  "errors": ["Detailed error 1", "Detailed error 2"],
  "message": "Operation failed"
}
```

### Paginated Response
```json
{
  "isSuccess": true,
  "data": {
    "items": [...],
    "totalCount": 150,
    "pageNumber": 1,
    "pageSize": 20,
    "totalPages": 8
  }
}
```

## Available Applications

1. **Customer Mobile App** - iOS/Android app for customers to book services
2. **Provider Mobile App** - iOS/Android app for service providers
3. **Admin Web Dashboard** - Web dashboard for platform administration

## Core Features by Application

### Customer App Features
- User registration & authentication
- Browse services and providers
- Book services
- Real-time booking tracking
- Chat with providers
- Payment processing
- Leave reviews and ratings
- View booking history
- Manage profile and addresses
- Notifications

### Provider App Features
- Provider registration & verification
- Availability calendar management
- Booking management (accept/reject/complete)
- Real-time booking updates
- Chat with customers
- Earnings dashboard
- Performance analytics
- Review management
- Portfolio management
- Notifications

### Admin Dashboard Features
- Platform analytics and KPIs
- User management
- Provider verification
- Booking oversight
- Payment and payout management
- Dispute resolution
- Review moderation
- Financial reports
- System configuration

## Real-time Features (SignalR)

### Connection
```javascript
import * as signalR from "@microsoft/signalr";

const connection = new signalR.HubConnectionBuilder()
  .withUrl("https://api.baytaek.com/hubs/notifications", {
    accessTokenFactory: () => yourAccessToken
  })
  .withAutomaticReconnect()
  .build();

await connection.start();
```

### Available Hubs
1. **NotificationsHub** (`/hubs/notifications`)
   - Receive real-time notifications
   - Booking status updates
   - New messages

2. **ChatHub** (`/hubs/chat`)
   - Real-time messaging
   - Typing indicators
   - Message delivery status

## File Upload

### Upload Endpoint
```http
POST /api/files/{type}
Content-Type: multipart/form-data

file: <binary>
```

Types: `profile-image`, `booking-photos`, `documents`, `chat-attachments`

Maximum file sizes:
- Images: 5 MB
- Documents: 10 MB
- Videos: 100 MB

### Response
```json
{
  "isSuccess": true,
  "data": {
    "url": "https://storage.blob.core.windows.net/files/abc123.jpg",
    "fileName": "profile.jpg",
    "contentType": "image/jpeg",
    "size": 102400
  }
}
```

## Supported Languages

The API supports bilingual content (English and Arabic):
- All service names, descriptions returned in both languages
- Notifications sent in both languages
- Admin can manage content in both languages

```json
{
  "nameEn": "Plumbing Services",
  "nameAr": "خدمات السباكة",
  "descriptionEn": "Professional plumbing...",
  "descriptionAr": "خدمات سباكة احترافية..."
}
```

## Rate Limiting

API endpoints are rate-limited to prevent abuse:
- Authentication endpoints: 5 requests per minute
- Standard endpoints: 100 requests per minute
- File upload endpoints: 10 requests per minute

Rate limit headers:
```http
X-RateLimit-Limit: 100
X-RateLimit-Remaining: 95
X-RateLimit-Reset: 1638360000
```

## Error Codes

| Status Code | Description |
|-------------|-------------|
| 200 | Success |
| 201 | Created |
| 400 | Bad Request |
| 401 | Unauthorized |
| 403 | Forbidden |
| 404 | Not Found |
| 409 | Conflict |
| 422 | Validation Error |
| 429 | Too Many Requests |
| 500 | Internal Server Error |

## Next Steps

1. [Customer App API Guide](./CUSTOMER_APP_API.md)
2. [Provider App API Guide](./PROVIDER_APP_API.md)
3. [Admin Dashboard API Guide](./ADMIN_DASHBOARD_API.md)
4. [Authentication Guide](./AUTHENTICATION.md)
5. [Real-time Integration Guide](./REALTIME_GUIDE.md)
