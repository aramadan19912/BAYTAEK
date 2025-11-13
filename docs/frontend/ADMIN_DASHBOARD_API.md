# Admin Web Dashboard - API Integration Guide

## Authentication

### Admin Login
```http
POST /api/auth/login
Content-Type: application/json

{
  "email": "admin@baytaek.com",
  "password": "AdminPass123!"
}
```

## Platform Analytics

### Get Platform Analytics & KPIs
```http
GET /api/admin/analytics/platform?
  startDate=2024-12-01&
  endDate=2024-12-31
```

Response:
```json
{
  "isSuccess": true,
  "data": {
    "totalUsers": 15000,
    "activeUsers": 12500,
    "newUsers": 850,
    "totalProviders": 450,
    "verifiedProviders": 380,
    "activeProviders": 420,
    "totalBookings": 8500,
    "bookingsInPeriod": 1200,
    "completedBookings": 1050,
    "cancelledBookings": 85,
    "pendingBookings": 65,
    "completionRate": 87.5,
    "totalRevenue": 285000.00,
    "platformRevenue": 42750.00,
    "providerPayouts": 242250.00,
    "totalReviews": 950,
    "averagePlatformRating": 4.6,
    "dailyMetrics": [...],
    "topServices": [...],
    "topProviders": [...]
  }
}
```

### Get Financial Report
```http
GET /api/admin/financial/report?
  startDate=2024-12-01&
  endDate=2024-12-31
```

Response:
```json
{
  "isSuccess": true,
  "data": {
    "totalRevenue": 285000.00,
    "platformRevenue": 42750.00,
    "providerRevenue": 242250.00,
    "totalTransactions": 1200,
    "averageTransactionValue": 237.50,
    "totalRefunds": 8500.00,
    "refundCount": 42,
    "refundRate": 3.5,
    "totalPayouts": 150000.00,
    "completedPayouts": 145000.00,
    "pendingPayouts": 5000.00,
    "cashInflow": 285000.00,
    "cashOutflow": 153500.00,
    "netCashFlow": 131500.00,
    "paymentMethodBreakdown": [...],
    "dailyFinancials": [...]
  }
}
```

## User Management

### Get All Users
```http
GET /api/admin/users?
  searchTerm=john&
  role=Customer&
  region=Riyadh&
  isVerified=true&
  registeredAfter=2024-01-01&
  pageNumber=1&
  pageSize=20
```

### Get User Details
```http
GET /api/admin/users/{userId}
```

### Manage User
```http
POST /api/admin/users/{userId}/manage
Content-Type: application/json

{
  "action": "Suspend",  // Suspend, Activate, Verify, Unverify, Delete
  "reason": "Policy violation"
}
```

Actions:
- **Suspend**: Deactivate user account
- **Activate**: Reactivate suspended account
- **Verify**: Mark user as verified
- **Unverify**: Remove verification status
- **Delete**: Soft delete user (anonymize data)

## Provider Management

### Get All Providers
```http
GET /api/admin/providers?
  searchTerm=&
  region=Riyadh&
  isVerified=true&
  isActive=true&
  minRating=4.0&
  pageNumber=1&
  pageSize=20
```

### Manage Provider
```http
POST /api/admin/providers/{providerId}/manage
Content-Type: application/json

{
  "action": "Verify",  // Verify, Unverify, Suspend, Activate
  "reason": "Documents approved"
}
```

### Approve Provider Verification
```http
POST /api/admin/providers/{providerId}/verify
Content-Type: application/json

{
  "notes": "All documents verified. License valid until 2025."
}
```

### Reject Provider Verification
```http
POST /api/admin/providers/{providerId}/reject
Content-Type: application/json

{
  "reason": "Invalid license number. Please resubmit correct documentation."
}
```

## Booking Management

### Get All Bookings
```http
GET /api/admin/bookings?
  status=Completed&
  startDate=2024-12-01&
  endDate=2024-12-31&
  region=Riyadh&
  customerId=<guid>&
  providerId=<guid>&
  serviceId=<guid>&
  searchTerm=&
  isPaid=true&
  pageNumber=1&
  pageSize=20
```

### Get Booking Details
```http
GET /api/admin/bookings/{bookingId}
```

## Payment & Payout Management

### Get All Transactions
```http
GET /api/admin/transactions?
  startDate=2024-12-01&
  endDate=2024-12-31&
  status=Completed&
  paymentMethod=Card&
  region=Riyadh&
  customerId=<guid>&
  providerId=<guid>&
  minAmount=100&
  maxAmount=1000&
  pageNumber=1&
  pageSize=20
```

### Process Payout
```http
POST /api/payments/payouts/{payoutId}/process
Content-Type: application/json

{
  "notes": "Processed via bank transfer"
}
```

## Dispute Management

### Get Disputes
```http
GET /api/admin/disputes?
  status=Open&
  priority=High&
  assignedTo=<adminUserId>&
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
        "bookingId": "guid",
        "serviceName": "Plumbing Repair",
        "raisedBy": "guid",
        "raisedByName": "John Doe",
        "type": "QualityIssue",
        "title": "Service not completed properly",
        "description": "The leak was not fixed...",
        "status": "Open",
        "priority": "High",
        "assignedTo": null,
        "evidenceUrls": [...],
        "createdAt": "2024-12-15T10:00:00Z"
      }
    ],
    "totalCount": 45,
    "pageNumber": 1,
    "pageSize": 20
  }
}
```

### Manage Dispute
```http
POST /api/admin/disputes/{disputeId}/manage
Content-Type: application/json

{
  "action": "Assign",  // Assign, UpdateStatus, UpdatePriority, Resolve, Close, Escalate
  "assignTo": "admin-user-id",
  "newStatus": "UnderReview",
  "newPriority": "High",
  "resolution": "Issued full refund to customer"
}
```

Dispute Actions:
- **Assign**: Assign dispute to admin staff
- **UpdateStatus**: Change dispute status
- **UpdatePriority**: Adjust priority level
- **Resolve**: Mark dispute as resolved with resolution text
- **Close**: Close the dispute
- **Escalate**: Escalate to Critical priority

## Review Moderation

### Get Pending Reviews
```http
GET /api/reviews/admin/pending?
  onlyUnverified=true&
  pageNumber=1&
  pageSize=20
```

### Moderate Review
```http
POST /api/reviews/{reviewId}/moderate
Content-Type: application/json

{
  "action": "Approve",  // Approve, Reject, Hide, Show
  "reason": "Review violates community guidelines"
}
```

## Service Management

### Get All Services
```http
GET /api/admin/services?
  categoryId=<guid>&
  region=Riyadh&
  isActive=true&
  searchTerm=plumbing&
  minPrice=50&
  maxPrice=500&
  pageNumber=1&
  pageSize=20
```

### Update Service Status
```http
PUT /api/admin/services/{serviceId}/status
Content-Type: application/json

{
  "isActive": false,
  "reason": "Service temporarily suspended"
}
```

## Reports Management

### Get Reports
```http
GET /api/admin/reports?
  status=Pending&
  type=User&
  reason=Harassment&
  reporterId=<guid>&
  reportedUserId=<guid>&
  pageNumber=1&
  pageSize=20
```

### Review Report
```http
PUT /api/admin/reports/{reportId}/review
Content-Type: application/json

{
  "status": "Resolved",
  "actionTaken": "Warning",
  "adminNotes": "Issued warning to user",
  "suspendUser": false,
  "suspensionDays": null,
  "sendWarning": true,
  "warningMessage": "This behavior violates our community guidelines"
}
```

## System Configuration

### Get System Configurations
```http
GET /api/admin/config?
  category=Payment&
  onlyPublic=false
```

Response:
```json
{
  "isSuccess": true,
  "data": [
    {
      "id": "guid",
      "key": "PlatformCommissionRate",
      "value": "0.15",
      "category": "Payment",
      "description": "Platform commission rate (15%)",
      "isPublic": false,
      "updatedAt": "2024-12-01T10:00:00Z"
    },
    {
      "key": "MinimumPayoutAmount",
      "value": "50",
      "category": "Payment",
      "description": "Minimum payout threshold",
      "isPublic": false
    }
  ]
}
```

### Update System Configuration
```http
POST /api/admin/config
Content-Type: application/json

{
  "key": "PlatformCommissionRate",
  "value": "0.15",
  "category": "Payment",
  "description": "Platform commission rate",
  "isPublic": false
}
```

## Support Tickets

### Get All Tickets
```http
GET /api/admin/tickets?
  status=Open&
  category=Technical&
  priority=High&
  userId=<guid>&
  assignedToUserId=<guid>&
  pageNumber=1&
  pageSize=20
```

### Update Ticket Status
```http
PUT /api/admin/tickets/{ticketId}/status
Content-Type: application/json

{
  "status": "InProgress",
  "notes": "Working on the issue",
  "assignToUserId": "admin-user-id"
}
```

### Add Admin Response
```http
POST /api/admin/tickets/{ticketId}/messages
Content-Type: application/json

{
  "message": "We're looking into this issue. Will update you soon.",
  "attachments": []
}
```

## Content Management

### Get Content
```http
GET /api/admin/content?
  type=FAQ&
  category=Payments&
  isPublished=true&
  pageNumber=1&
  pageSize=20
```

### Create Content
```http
POST /api/admin/content
Content-Type: application/json

{
  "type": "FAQ",
  "titleEn": "How do payments work?",
  "titleAr": "كيف تعمل المدفوعات؟",
  "contentEn": "Payments are processed securely...",
  "contentAr": "تتم معالجة المدفوعات بشكل آمن...",
  "slug": "how-payments-work",
  "category": "Payments",
  "tags": ["payments", "security"],
  "isPublished": true,
  "displayOrder": 1
}
```

### Update Content
```http
PUT /api/admin/content/{contentId}
Content-Type: application/json

{
  "titleEn": "Updated title",
  "contentEn": "Updated content",
  "isPublished": true
}
```

### Delete Content
```http
DELETE /api/admin/content/{contentId}
```

## Dashboard Statistics

### Get Dashboard Stats
```http
GET /api/admin/dashboard/stats?
  startDate=2024-12-01&
  endDate=2024-12-31
```

Response includes comprehensive platform statistics.

## Example Integration (React)

### API Service Setup
```javascript
// adminApi.js
import axios from 'axios';

const API_BASE_URL = 'https://api.baytaek.com/api/admin';

class AdminAPI {
  constructor() {
    this.client = axios.create({
      baseURL: API_BASE_URL,
      headers: {
        'Content-Type': 'application/json',
      },
    });

    // Add auth token
    this.client.interceptors.request.use((config) => {
      const token = localStorage.getItem('adminToken');
      if (token) {
        config.headers.Authorization = `Bearer ${token}`;
      }
      return config;
    });

    // Handle errors
    this.client.interceptors.response.use(
      (response) => response,
      (error) => {
        if (error.response?.status === 401) {
          localStorage.removeItem('adminToken');
          window.location.href = '/admin/login';
        }
        return Promise.reject(error);
      }
    );
  }

  // Platform Analytics
  async getPlatformAnalytics(startDate, endDate) {
    const response = await this.client.get('/analytics/platform', {
      params: { startDate, endDate },
    });
    return response.data;
  }

  // User Management
  async getUsers(filters) {
    const response = await this.client.get('/users', { params: filters });
    return response.data;
  }

  async manageUser(userId, action, reason) {
    const response = await this.client.post(`/users/${userId}/manage`, {
      action,
      reason,
    });
    return response.data;
  }

  // Dispute Management
  async getDisputes(filters) {
    const response = await this.client.get('/disputes', { params: filters });
    return response.data;
  }

  async manageDispute(disputeId, payload) {
    const response = await this.client.post(`/disputes/${disputeId}/manage`, payload);
    return response.data;
  }

  // Financial Reports
  async getFinancialReport(startDate, endDate) {
    const response = await this.client.get('/financial/report', {
      params: { startDate, endDate },
    });
    return response.data;
  }
}

export default new AdminAPI();
```

### Dashboard Component
```jsx
// AdminDashboard.jsx
import React, { useEffect, useState } from 'react';
import adminAPI from './adminApi';
import { Card, Row, Col, DatePicker, Spin } from 'antd';

const AdminDashboard = () => {
  const [analytics, setAnalytics] = useState(null);
  const [loading, setLoading] = useState(true);
  const [dateRange, setDateRange] = useState([
    moment().subtract(30, 'days'),
    moment(),
  ]);

  useEffect(() => {
    fetchAnalytics();
  }, [dateRange]);

  const fetchAnalytics = async () => {
    setLoading(true);
    try {
      const [startDate, endDate] = dateRange;
      const result = await adminAPI.getPlatformAnalytics(
        startDate.format('YYYY-MM-DD'),
        endDate.format('YYYY-MM-DD')
      );

      if (result.isSuccess) {
        setAnalytics(result.data);
      }
    } catch (error) {
      console.error('Error fetching analytics:', error);
    } finally {
      setLoading(false);
    }
  };

  if (loading) {
    return <Spin size="large" />;
  }

  return (
    <div>
      <DatePicker.RangePicker
        value={dateRange}
        onChange={setDateRange}
        style={{ marginBottom: 24 }}
      />

      <Row gutter={[16, 16]}>
        <Col span={6}>
          <Card title="Total Users">
            <h2>{analytics.totalUsers.toLocaleString()}</h2>
            <p>+{analytics.newUsers} new users</p>
          </Card>
        </Col>

        <Col span={6}>
          <Card title="Total Revenue">
            <h2>${analytics.totalRevenue.toLocaleString()}</h2>
            <p>Platform: ${analytics.platformRevenue.toLocaleString()}</p>
          </Card>
        </Col>

        <Col span={6}>
          <Card title="Bookings">
            <h2>{analytics.bookingsInPeriod.toLocaleString()}</h2>
            <p>{analytics.completionRate}% completion rate</p>
          </Card>
        </Col>

        <Col span={6}>
          <Card title="Average Rating">
            <h2>{analytics.averagePlatformRating}</h2>
            <p>{analytics.totalReviews} reviews</p>
          </Card>
        </Col>
      </Row>

      {/* Charts, tables, etc. */}
    </div>
  );
};

export default AdminDashboard;
```

### User Management Component
```jsx
// UserManagement.jsx
import React, { useState, useEffect } from 'react';
import { Table, Button, Modal, Select, Input, message } from 'antd';
import adminAPI from './adminApi';

const UserManagement = () => {
  const [users, setUsers] = useState([]);
  const [loading, setLoading] = useState(false);
  const [pagination, setPagination] = useState({ current: 1, pageSize: 20 });
  const [filters, setFilters] = useState({});

  useEffect(() => {
    fetchUsers();
  }, [pagination.current, filters]);

  const fetchUsers = async () => {
    setLoading(true);
    try {
      const result = await adminAPI.getUsers({
        ...filters,
        pageNumber: pagination.current,
        pageSize: pagination.pageSize,
      });

      if (result.isSuccess) {
        setUsers(result.data.items);
        setPagination({
          ...pagination,
          total: result.data.totalCount,
        });
      }
    } catch (error) {
      message.error('Failed to fetch users');
    } finally {
      setLoading(false);
    }
  };

  const handleManageUser = async (userId, action, reason) => {
    try {
      const result = await adminAPI.manageUser(userId, action, reason);
      if (result.isSuccess) {
        message.success(`User ${action.toLowerCase()}ed successfully`);
        fetchUsers();
      }
    } catch (error) {
      message.error(`Failed to ${action.toLowerCase()} user`);
    }
  };

  const columns = [
    {
      title: 'Name',
      dataIndex: 'fullName',
      key: 'name',
    },
    {
      title: 'Email',
      dataIndex: 'email',
      key: 'email',
    },
    {
      title: 'Role',
      dataIndex: 'role',
      key: 'role',
    },
    {
      title: 'Status',
      dataIndex: 'isActive',
      key: 'status',
      render: (isActive) => isActive ? 'Active' : 'Suspended',
    },
    {
      title: 'Actions',
      key: 'actions',
      render: (_, record) => (
        <>
          <Button
            type="link"
            onClick={() => handleManageUser(record.id, 'Suspend', null)}
            disabled={!record.isActive}
          >
            Suspend
          </Button>
          <Button
            type="link"
            onClick={() => handleManageUser(record.id, 'Activate', null)}
            disabled={record.isActive}
          >
            Activate
          </Button>
        </>
      ),
    },
  ];

  return (
    <div>
      <Table
        dataSource={users}
        columns={columns}
        loading={loading}
        pagination={pagination}
        onChange={(newPagination) => setPagination(newPagination)}
        rowKey="id"
      />
    </div>
  );
};

export default UserManagement;
```

## Best Practices

1. **Security**
   - Implement role-based access control
   - Audit all admin actions
   - Use secure session management
   - Implement IP whitelisting for sensitive operations

2. **Performance**
   - Implement caching for dashboard data
   - Use server-side pagination
   - Optimize database queries
   - Use CDN for static assets

3. **User Experience**
   - Provide clear feedback on actions
   - Implement undo functionality where possible
   - Show loading states
   - Validate inputs client-side

4. **Data Management**
   - Export data in multiple formats (CSV, Excel, PDF)
   - Implement advanced filtering
   - Provide bulk operations
   - Keep audit trails

5. **Monitoring**
   - Track admin activity
   - Monitor API performance
   - Set up alerts for critical events
   - Regular security audits
