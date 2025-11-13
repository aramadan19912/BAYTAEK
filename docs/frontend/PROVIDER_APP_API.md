# Provider Mobile App - API Integration Guide

## Authentication

### Provider Registration
```http
POST /api/auth/register
Content-Type: application/json

{
  "email": "provider@example.com",
  "password": "SecurePass123!",
  "firstName": "Ahmad",
  "lastName": "AlSaud",
  "phoneNumber": "+966501234567",
  "role": "Provider",
  "businessName": "AlSaud Plumbing Services",
  "description": "Professional plumbing services with 10+ years experience",
  "licenseNumber": "LIC123456",
  "licenseExpiryDate": "2025-12-31"
}
```

## Dashboard

### Get Provider Dashboard
```http
GET /api/providers/dashboard
```

Response:
```json
{
  "isSuccess": true,
  "data": {
    "providerName": "AlSaud Plumbing",
    "averageRating": 4.7,
    "totalReviews": 245,
    "isVerified": true,
    "todayBookings": 3,
    "todayCompleted": 1,
    "weeklyBookings": 15,
    "weeklyCompleted": 12,
    "monthlyBookings": 58,
    "monthlyCompleted": 52,
    "monthlyRevenue": 15750.00,
    "monthlyNetEarnings": 13387.50,
    "availableBalance": 8500.00,
    "pendingPayouts": 2500.00,
    "pendingBookingsCount": 5,
    "unrespondedReviewsCount": 3,
    "pendingBookings": [...],
    "upcomingBookings": [...],
    "recentReviews": [...]
  }
}
```

## Bookings Management

### Get My Bookings
```http
GET /api/bookings/provider-bookings?
  status=Pending&
  startDate=2024-12-01&
  endDate=2024-12-31&
  pageNumber=1&
  pageSize=20
```

### Accept Booking
```http
POST /api/bookings/{bookingId}/accept
Content-Type: application/json

{
  "estimatedDurationMinutes": 120
}
```

### Reject Booking
```http
POST /api/bookings/{bookingId}/reject
Content-Type: application/json

{
  "reason": "Fully booked for that day"
}
```

### Start Service
```http
POST /api/bookings/{bookingId}/start
Content-Type: application/json

{
  "latitude": 24.7136,
  "longitude": 46.6753
}
```

### Complete Service
```http
POST /api/bookings/{bookingId}/complete
Content-Type: application/json

{
  "completionPhotos": [
    "https://storage.blob.core.windows.net/files/photo1.jpg",
    "https://storage.blob.core.windows.net/files/photo2.jpg"
  ],
  "notes": "Service completed successfully. All pipes fixed."
}
```

### Cancel Booking
```http
POST /api/bookings/{bookingId}/cancel
Content-Type: application/json

{
  "reason": "Emergency situation"
}
```

## Availability Management

### Get My Availability
```http
GET /api/providers/{providerId}/availability?
  startDate=2024-12-01&
  endDate=2024-12-31
```

### Set Weekly Schedule
```http
POST /api/providers/availability/weekly
Content-Type: application/json

{
  "schedule": [
    {
      "dayOfWeek": "Monday",
      "startTime": "08:00:00",
      "endTime": "17:00:00",
      "isAvailable": true
    },
    {
      "dayOfWeek": "Tuesday",
      "startTime": "08:00:00",
      "endTime": "17:00:00",
      "isAvailable": true
    },
    {
      "dayOfWeek": "Friday",
      "startTime": "00:00:00",
      "endTime": "00:00:00",
      "isAvailable": false
    }
  ]
}
```

### Block Specific Dates
```http
POST /api/providers/availability/block
Content-Type: application/json

{
  "startDate": "2024-12-25T00:00:00Z",
  "endDate": "2024-12-26T23:59:59Z",
  "reason": "Holiday",
  "isAllDay": true
}
```

### Delete Blocked Date
```http
DELETE /api/providers/availability/block/{blockedDateId}
```

## Earnings & Reports

### Get Earnings Summary
```http
GET /api/providers/earnings?
  startDate=2024-12-01&
  endDate=2024-12-31
```

Response:
```json
{
  "isSuccess": true,
  "data": {
    "providerId": "guid",
    "totalRevenue": 18500.00,
    "platformFees": 2775.00,
    "netEarnings": 15725.00,
    "totalPaidOut": 10000.00,
    "pendingPayouts": 2500.00,
    "availableBalance": 3225.00,
    "totalBookings": 65,
    "monthlyBreakdown": [
      {
        "year": 2024,
        "month": 12,
        "totalRevenue": 18500.00,
        "netEarnings": 15725.00,
        "bookingsCount": 65
      }
    ]
  }
}
```

### Get Detailed Earnings Report
```http
GET /api/providers/earnings/report?
  startDate=2024-12-01&
  endDate=2024-12-31&
  pageNumber=1&
  pageSize=50
```

Response includes transaction-level details:
```json
{
  "isSuccess": true,
  "data": {
    "items": [
      {
        "bookingId": "guid",
        "serviceName": "Plumbing Repair",
        "customerName": "John Doe",
        "completedAt": "2024-12-15T14:30:00Z",
        "grossAmount": 150.00,
        "platformFee": 22.50,
        "netAmount": 127.50,
        "paymentMethod": "Card",
        "transactionId": "txn_123456"
      }
    ],
    "totalCount": 65,
    "pageNumber": 1,
    "pageSize": 50
  }
}
```

### Request Payout
```http
POST /api/payments/payouts/create
Content-Type: application/json

{
  "amount": 3000.00,
  "payoutMethod": "BankTransfer",
  "accountDetails": {
    "accountNumber": "SA1234567890",
    "bankName": "Al Rajhi Bank"
  }
}
```

### Get Payout History
```http
GET /api/payments/payouts?
  status=Completed&
  startDate=2024-01-01&
  endDate=2024-12-31&
  pageNumber=1&
  pageSize=20
```

## Performance Analytics

### Get Performance Metrics
```http
GET /api/providers/performance?
  startDate=2024-12-01&
  endDate=2024-12-31
```

Response:
```json
{
  "isSuccess": true,
  "data": {
    "providerId": "guid",
    "totalBookings": 65,
    "completedBookings": 60,
    "cancelledBookings": 3,
    "rejectedBookings": 2,
    "completionRate": 92.31,
    "cancellationRate": 4.62,
    "acceptanceRate": 96.92,
    "averageRating": 4.7,
    "totalReviews": 58,
    "averageResponseTimeMinutes": 12.5,
    "averageServiceDurationMinutes": 95.3,
    "serviceBreakdown": [
      {
        "serviceId": "guid",
        "serviceName": "Plumbing Repair",
        "bookingsCount": 35,
        "totalRevenue": 10500.00
      }
    ]
  }
}
```

## Portfolio Management

### Update Portfolio
```http
PUT /api/providers/portfolio
Content-Type: application/json

{
  "description": "Updated business description with 15+ years experience...",
  "portfolioImages": [
    "https://storage.blob.core.windows.net/files/work1.jpg",
    "https://storage.blob.core.windows.net/files/work2.jpg"
  ],
  "certificationDocuments": [
    "https://storage.blob.core.windows.net/files/license.pdf",
    "https://storage.blob.core.windows.net/files/cert.pdf"
  ]
}
```

## Reviews Management

### Get Reviews for My Services
```http
GET /api/reviews/provider/{providerId}?
  minRating=1&
  withImagesOnly=false&
  pageNumber=1&
  pageSize=20
```

Response includes rating statistics:
```json
{
  "isSuccess": true,
  "data": {
    "ratingStatistics": {
      "totalReviews": 245,
      "averageRating": 4.7,
      "fiveStarCount": 180,
      "fourStarCount": 45,
      "threeStarCount": 15,
      "twoStarCount": 3,
      "oneStarCount": 2,
      "fiveStarPercentage": 73.47,
      "fourStarPercentage": 18.37
    },
    "reviews": {
      "items": [...],
      "totalCount": 245,
      "pageNumber": 1,
      "pageSize": 20
    }
  }
}
```

### Respond to Review
```http
POST /api/reviews/{reviewId}/respond
Content-Type: application/json

{
  "response": "Thank you for your feedback! We're glad you're satisfied with our service."
}
```

## Notifications

### Get Notifications
```http
GET /api/notifications?
  isRead=false&
  category=Booking&
  pageNumber=1&
  pageSize=20
```

### Update Notification Settings
```http
PUT /api/notifications/settings
Content-Type: application/json

{
  "bookingNotifications": {
    "push": true,
    "email": true,
    "sms": true
  },
  "paymentNotifications": {
    "push": true,
    "email": true,
    "sms": false
  },
  "reviewNotifications": {
    "push": true,
    "email": false,
    "sms": false
  }
}
```

## Messages

### Get Conversations
```http
GET /api/messages/conversations
```

### Send Message
```http
POST /api/messages/send
Content-Type: application/json

{
  "conversationId": "guid",
  "content": "I'll arrive at 10 AM as scheduled",
  "type": "Text"
}
```

## File Uploads

### Upload Booking Photos
```http
POST /api/files/booking-photos
Content-Type: multipart/form-data

file: <binary>
```

### Upload Documents
```http
POST /api/files/documents
Content-Type: multipart/form-data

file: <binary>
```

## Profile Management

### Get My Profile
```http
GET /api/auth/profile
```

### Update Provider Profile
```http
PUT /api/providers/profile
Content-Type: application/json

{
  "businessName": "Updated Business Name",
  "description": "Updated description",
  "phoneNumber": "+966501234567"
}
```

## Real-time Updates

### Booking Status Updates
```javascript
const connection = new signalR.HubConnectionBuilder()
  .withUrl("https://api.baytaek.com/hubs/notifications", {
    accessTokenFactory: () => accessToken
  })
  .withAutomaticReconnect()
  .build();

// New booking request
connection.on("NewBookingRequest", (booking) => {
  console.log("New booking request:", booking);
  showBookingNotification(booking);
  playNotificationSound();
});

// Booking cancelled by customer
connection.on("BookingCancelled", (booking) => {
  console.log("Booking cancelled:", booking);
  updateBookingsList();
});

// Payment received
connection.on("PaymentReceived", (payment) => {
  console.log("Payment received:", payment);
  updateEarningsDisplay();
});

// New review
connection.on("NewReview", (review) => {
  console.log("New review:", review);
  showReviewNotification(review);
});

await connection.start();
```

## Example Integration (React Native)

### API Service Setup
```javascript
// api.js
import axios from 'axios';
import AsyncStorage from '@react-native-async-storage/async-storage';

const API_BASE_URL = 'https://api.baytaek.com/api';

const apiClient = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Add token to requests
apiClient.interceptors.request.use(async (config) => {
  const token = await AsyncStorage.getItem('accessToken');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

// Handle token refresh
apiClient.interceptors.response.use(
  (response) => response,
  async (error) => {
    const originalRequest = error.config;

    if (error.response?.status === 401 && !originalRequest._retry) {
      originalRequest._retry = true;

      const refreshToken = await AsyncStorage.getItem('refreshToken');
      const accessToken = await AsyncStorage.getItem('accessToken');

      try {
        const response = await axios.post(`${API_BASE_URL}/auth/refresh-token`, {
          accessToken,
          refreshToken,
        });

        const { accessToken: newToken, refreshToken: newRefreshToken } = response.data.data;

        await AsyncStorage.setItem('accessToken', newToken);
        await AsyncStorage.setItem('refreshToken', newRefreshToken);

        originalRequest.headers.Authorization = `Bearer ${newToken}`;
        return apiClient(originalRequest);
      } catch (refreshError) {
        // Refresh failed, logout user
        await AsyncStorage.clear();
        // Navigate to login
        return Promise.reject(refreshError);
      }
    }

    return Promise.reject(error);
  }
);

export default apiClient;
```

### Provider Dashboard Screen
```javascript
// ProviderDashboard.js
import React, { useEffect, useState } from 'react';
import { View, Text, ScrollView, RefreshControl } from 'react-native';
import apiClient from './api';

const ProviderDashboard = () => {
  const [dashboard, setDashboard] = useState(null);
  const [loading, setLoading] = useState(true);
  const [refreshing, setRefreshing] = useState(false);

  const fetchDashboard = async () => {
    try {
      const response = await apiClient.get('/providers/dashboard');
      if (response.data.isSuccess) {
        setDashboard(response.data.data);
      }
    } catch (error) {
      console.error('Error fetching dashboard:', error);
    } finally {
      setLoading(false);
      setRefreshing(false);
    }
  };

  useEffect(() => {
    fetchDashboard();
  }, []);

  const onRefresh = () => {
    setRefreshing(true);
    fetchDashboard();
  };

  if (loading) {
    return <LoadingSpinner />;
  }

  return (
    <ScrollView
      refreshControl={
        <RefreshControl refreshing={refreshing} onRefresh={onRefresh} />
      }
    >
      <View>
        <Text>Monthly Revenue: ${dashboard.monthlyRevenue}</Text>
        <Text>Available Balance: ${dashboard.availableBalance}</Text>
        <Text>Pending Bookings: {dashboard.pendingBookingsCount}</Text>
        <Text>Rating: {dashboard.averageRating} ({dashboard.totalReviews} reviews)</Text>
      </View>

      {/* Render pending bookings, upcoming bookings, etc. */}
    </ScrollView>
  );
};
```

### Accept Booking Example
```javascript
const acceptBooking = async (bookingId, estimatedDuration) => {
  try {
    const response = await apiClient.post(`/bookings/${bookingId}/accept`, {
      estimatedDurationMinutes: estimatedDuration,
    });

    if (response.data.isSuccess) {
      Alert.alert('Success', 'Booking accepted successfully');
      // Update UI
      refreshBookings();
    } else {
      Alert.alert('Error', response.data.error);
    }
  } catch (error) {
    Alert.alert('Error', 'Failed to accept booking');
  }
};
```

## Best Practices

1. **Offline Support**
   - Cache critical data locally
   - Queue actions for when connection returns
   - Show offline indicator

2. **Location Services**
   - Request location permissions properly
   - Use background location for service tracking
   - Respect user privacy

3. **Push Notifications**
   - Request permission at appropriate time
   - Handle notification taps
   - Update UI based on notification data

4. **Performance**
   - Implement pull-to-refresh
   - Use pagination for lists
   - Optimize images before upload

5. **User Experience**
   - Show loading states
   - Provide immediate feedback
   - Handle errors gracefully
