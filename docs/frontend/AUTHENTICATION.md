# Authentication & Security Guide

## Overview

BAYTAEK platform uses JWT (JSON Web Tokens) for authentication with refresh token rotation for enhanced security.

## Authentication Flow

### 1. Registration & Login

#### Registration
```http
POST /api/auth/register
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "SecurePass123!",
  "firstName": "John",
  "lastName": "Doe",
  "phoneNumber": "+966501234567",
  "role": "Customer"  // or "Provider"
}
```

Response:
```json
{
  "isSuccess": true,
  "data": {
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "a1b2c3d4e5f6g7h8i9j0...",
    "expiresAt": "2024-12-01T10:00:00Z",
    "user": {
      "id": "uuid",
      "email": "user@example.com",
      "firstName": "John",
      "lastName": "Doe",
      "role": "Customer"
    }
  }
}
```

#### Login
```http
POST /api/auth/login
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "SecurePass123!"
}
```

Same response format as registration.

### 2. Token Storage

#### Mobile Apps (React Native)
```javascript
import AsyncStorage from '@react-native-async-storage/async-storage';
import * as Keychain from 'react-native-keychain';

// Secure storage for tokens (recommended)
const storeTokens = async (accessToken, refreshToken) => {
  await Keychain.setGenericPassword(
    'auth',
    JSON.stringify({ accessToken, refreshToken }),
    { service: 'com.baytaek.app' }
  );
};

const getTokens = async () => {
  const credentials = await Keychain.getGenericPassword({
    service: 'com.baytaek.app'
  });
  if (credentials) {
    return JSON.parse(credentials.password);
  }
  return null;
};

// Alternative: AsyncStorage (less secure)
await AsyncStorage.setItem('accessToken', accessToken);
await AsyncStorage.setItem('refreshToken', refreshToken);
```

#### Web Apps
```javascript
// Store in localStorage (not ideal but acceptable for web)
localStorage.setItem('accessToken', accessToken);
localStorage.setItem('refreshToken', refreshToken);

// Better: Use httpOnly cookies (requires backend setup)
// Tokens are automatically sent with requests
```

### 3. Using Access Tokens

#### HTTP Headers
```http
GET /api/bookings/my-bookings
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

#### JavaScript/TypeScript
```javascript
const accessToken = await getAccessToken();

const response = await fetch('https://api.baytaek.com/api/bookings/my-bookings', {
  method: 'GET',
  headers: {
    'Authorization': `Bearer ${accessToken}`,
    'Content-Type': 'application/json'
  }
});
```

#### Axios Interceptor
```javascript
import axios from 'axios';

const apiClient = axios.create({
  baseURL: 'https://api.baytaek.com/api'
});

apiClient.interceptors.request.use(async (config) => {
  const accessToken = await getAccessToken();
  if (accessToken) {
    config.headers.Authorization = `Bearer ${accessToken}`;
  }
  return config;
});
```

### 4. Token Refresh Flow

When an access token expires (401 Unauthorized), automatically refresh it:

```javascript
import axios from 'axios';

let isRefreshing = false;
let failedQueue = [];

const processQueue = (error, token = null) => {
  failedQueue.forEach(prom => {
    if (error) {
      prom.reject(error);
    } else {
      prom.resolve(token);
    }
  });

  failedQueue = [];
};

apiClient.interceptors.response.use(
  (response) => response,
  async (error) => {
    const originalRequest = error.config;

    if (error.response?.status === 401 && !originalRequest._retry) {
      if (isRefreshing) {
        return new Promise((resolve, reject) => {
          failedQueue.push({ resolve, reject });
        }).then(token => {
          originalRequest.headers.Authorization = `Bearer ${token}`;
          return apiClient(originalRequest);
        });
      }

      originalRequest._retry = true;
      isRefreshing = true;

      try {
        const { accessToken, refreshToken } = await getTokens();

        const response = await axios.post(
          'https://api.baytaek.com/api/auth/refresh-token',
          {
            accessToken,
            refreshToken
          }
        );

        if (response.data.isSuccess) {
          const { accessToken: newToken, refreshToken: newRefreshToken } = response.data.data;

          await storeTokens(newToken, newRefreshToken);

          apiClient.defaults.headers.common['Authorization'] = `Bearer ${newToken}`;
          originalRequest.headers.Authorization = `Bearer ${newToken}`;

          processQueue(null, newToken);

          return apiClient(originalRequest);
        }
      } catch (refreshError) {
        processQueue(refreshError, null);
        await logout();
        return Promise.reject(refreshError);
      } finally {
        isRefreshing = false;
      }
    }

    return Promise.reject(error);
  }
);
```

### 5. Logout

#### Revoke Current Token
```http
POST /api/auth/logout
Authorization: Bearer <access-token>
```

#### Revoke All Tokens
```http
POST /api/auth/revoke-all-tokens
Authorization: Bearer <access-token>
```

#### Client-side Cleanup
```javascript
const logout = async () => {
  try {
    // Call backend logout endpoint
    await apiClient.post('/auth/logout');
  } catch (error) {
    // Continue logout even if API call fails
    console.error('Logout error:', error);
  } finally {
    // Clear tokens
    await Keychain.resetGenericPassword({ service: 'com.baytaek.app' });
    // or
    localStorage.removeItem('accessToken');
    localStorage.removeItem('refreshToken');

    // Clear user data
    // Navigate to login screen
  }
};
```

## Password Management

### Change Password
```http
PUT /api/auth/change-password
Authorization: Bearer <access-token>
Content-Type: application/json

{
  "currentPassword": "OldPass123!",
  "newPassword": "NewPass456!"
}
```

### Forgot Password
```http
POST /api/auth/forgot-password
Content-Type: application/json

{
  "email": "user@example.com"
}
```

User receives reset link via email.

### Reset Password
```http
POST /api/auth/reset-password
Content-Type: application/json

{
  "email": "user@example.com",
  "token": "reset-token-from-email",
  "newPassword": "NewPass456!"
}
```

## Email & Phone Verification

### Request Email Verification
```http
POST /api/auth/request-email-verification
Authorization: Bearer <access-token>
```

### Verify Email
```http
POST /api/auth/verify-email
Content-Type: application/json

{
  "userId": "uuid",
  "token": "verification-token"
}
```

### Request Phone Verification
```http
POST /api/auth/request-phone-verification
Authorization: Bearer <access-token>
```

User receives OTP via SMS.

### Verify Phone
```http
POST /api/auth/verify-phone
Authorization: Bearer <access-token>
Content-Type: application/json

{
  "otp": "123456"
}
```

## Role-Based Access Control (RBAC)

### Available Roles
- **Customer**: Can book services, make payments, leave reviews
- **Provider**: Can offer services, manage bookings, receive payments
- **Admin**: Can manage platform, users, and content
- **SuperAdmin**: Full platform access

### Checking Permissions

Access tokens contain user role in JWT claims. Decode and check on client-side for UI rendering:

```javascript
import jwtDecode from 'jwt-decode';

const getTokenClaims = (token) => {
  try {
    return jwtDecode(token);
  } catch (error) {
    return null;
  }
};

const getUserRole = async () => {
  const accessToken = await getAccessToken();
  const claims = getTokenClaims(accessToken);
  return claims?.role;
};

const hasRole = async (requiredRole) => {
  const userRole = await getUserRole();
  return userRole === requiredRole;
};

// Usage
const isProvider = await hasRole('Provider');
if (isProvider) {
  // Show provider-specific UI
}
```

## Security Best Practices

### 1. Token Storage
- **Mobile**: Use secure storage (Keychain/Keystore)
- **Web**: Use httpOnly cookies if possible, localStorage as fallback
- Never store tokens in:
  - Plain text files
  - Insecure storage
  - URL parameters
  - Local variables without encryption

### 2. Token Transmission
- Always use HTTPS
- Never send tokens in URL parameters
- Use Authorization header
- Implement certificate pinning for mobile apps

### 3. Token Lifetime
- Access tokens: Short-lived (15-30 minutes)
- Refresh tokens: Long-lived (7-30 days)
- Implement automatic refresh before expiry

### 4. Password Requirements
- Minimum 8 characters
- At least one uppercase letter
- At least one lowercase letter
- At least one number
- At least one special character
- Validation regex: `^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$`

### 5. Rate Limiting
- Login attempts: 5 per 15 minutes per IP
- Password reset: 3 per hour per email
- Implement exponential backoff on failures

### 6. Session Management
- Implement session timeout
- Clear sensitive data on logout
- Handle multiple device sessions
- Provide "logout all devices" option

### 7. Error Handling
Never expose sensitive information in errors:

```javascript
// Bad
if (error.response?.status === 401) {
  alert('Invalid token: ' + error.response.data.error);
}

// Good
if (error.response?.status === 401) {
  alert('Authentication failed. Please login again.');
  await logout();
}
```

## Example: Complete Auth Service

```javascript
// authService.js
import axios from 'axios';
import AsyncStorage from '@react-native-async-storage/async-storage';
import * as Keychain from 'react-native-keychain';

const API_BASE_URL = 'https://api.baytaek.com/api';

class AuthService {
  // Store tokens securely
  async storeTokens(accessToken, refreshToken, expiresAt) {
    await Keychain.setGenericPassword(
      'auth',
      JSON.stringify({ accessToken, refreshToken, expiresAt }),
      { service: 'com.baytaek.app' }
    );
  }

  // Get stored tokens
  async getTokens() {
    try {
      const credentials = await Keychain.getGenericPassword({
        service: 'com.baytaek.app'
      });
      if (credentials) {
        return JSON.parse(credentials.password);
      }
    } catch (error) {
      console.error('Error getting tokens:', error);
    }
    return null;
  }

  // Register new user
  async register(userData) {
    try {
      const response = await axios.post(`${API_BASE_URL}/auth/register`, userData);

      if (response.data.isSuccess) {
        const { accessToken, refreshToken, expiresAt, user } = response.data.data;
        await this.storeTokens(accessToken, refreshToken, expiresAt);
        return { success: true, user };
      }

      return { success: false, error: response.data.error };
    } catch (error) {
      return { success: false, error: error.message };
    }
  }

  // Login user
  async login(email, password) {
    try {
      const response = await axios.post(`${API_BASE_URL}/auth/login`, {
        email,
        password
      });

      if (response.data.isSuccess) {
        const { accessToken, refreshToken, expiresAt, user } = response.data.data;
        await this.storeTokens(accessToken, refreshToken, expiresAt);
        return { success: true, user };
      }

      return { success: false, error: response.data.error };
    } catch (error) {
      return { success: false, error: error.message };
    }
  }

  // Refresh access token
  async refreshToken() {
    try {
      const tokens = await this.getTokens();
      if (!tokens) {
        throw new Error('No tokens found');
      }

      const response = await axios.post(`${API_BASE_URL}/auth/refresh-token`, {
        accessToken: tokens.accessToken,
        refreshToken: tokens.refreshToken
      });

      if (response.data.isSuccess) {
        const { accessToken, refreshToken, expiresAt } = response.data.data;
        await this.storeTokens(accessToken, refreshToken, expiresAt);
        return { success: true, accessToken };
      }

      throw new Error('Token refresh failed');
    } catch (error) {
      await this.logout();
      throw error;
    }
  }

  // Logout user
  async logout() {
    try {
      const tokens = await this.getTokens();
      if (tokens) {
        await axios.post(`${API_BASE_URL}/auth/logout`, null, {
          headers: { Authorization: `Bearer ${tokens.accessToken}` }
        });
      }
    } catch (error) {
      console.error('Logout error:', error);
    } finally {
      await Keychain.resetGenericPassword({ service: 'com.baytaek.app' });
    }
  }

  // Check if user is authenticated
  async isAuthenticated() {
    const tokens = await this.getTokens();
    if (!tokens) return false;

    // Check if token is expired
    const expiresAt = new Date(tokens.expiresAt);
    if (expiresAt <= new Date()) {
      try {
        await this.refreshToken();
        return true;
      } catch (error) {
        return false;
      }
    }

    return true;
  }

  // Get current user
  async getCurrentUser() {
    try {
      const tokens = await this.getTokens();
      if (!tokens) return null;

      const response = await axios.get(`${API_BASE_URL}/auth/profile`, {
        headers: { Authorization: `Bearer ${tokens.accessToken}` }
      });

      if (response.data.isSuccess) {
        return response.data.data;
      }

      return null;
    } catch (error) {
      console.error('Get current user error:', error);
      return null;
    }
  }
}

export default new AuthService();
```

## Testing Authentication

### Unit Tests
```javascript
describe('AuthService', () => {
  it('should login successfully', async () => {
    const result = await authService.login('user@example.com', 'password');
    expect(result.success).toBe(true);
    expect(result.user).toBeDefined();
  });

  it('should handle invalid credentials', async () => {
    const result = await authService.login('user@example.com', 'wrong');
    expect(result.success).toBe(false);
    expect(result.error).toBeDefined();
  });

  it('should refresh token automatically', async () => {
    const authenticated = await authService.isAuthenticated();
    expect(authenticated).toBe(true);
  });
});
```

## Troubleshooting

### Common Issues

1. **401 Unauthorized**
   - Token expired: Implement automatic refresh
   - Token invalid: Clear tokens and re-login
   - Token missing: Check Authorization header

2. **Token Refresh Fails**
   - Refresh token expired: Redirect to login
   - Network error: Retry with exponential backoff
   - Invalid refresh token: Clear tokens and re-login

3. **CORS Errors (Web)**
   - Ensure API allows your domain
   - Check credentials: 'include' for cookies
   - Verify preflight requests (OPTIONS)

4. **Tokens Not Persisting**
   - Check storage permissions
   - Verify async/await usage
   - Ensure proper error handling
