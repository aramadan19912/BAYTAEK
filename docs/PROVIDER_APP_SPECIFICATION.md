# Service Provider Mobile App & Portal - Professional Features & Specifications

**Home Service Application - Provider Experience**

| Item | Details |
|------|---------|
| Document Version | 1.0 |
| Date | November 2025 |
| Platforms | iOS, Android, Web (Angular 18) |
| Markets | Saudi Arabia, Egypt |
| Languages | Arabic, English |

## 1. Introduction

### 1.1 Purpose
This document outlines the comprehensive feature set for Service Providers using the home service platform. It covers registration, verification, job management, earnings, and all tools providers need to build successful service businesses.

### 1.2 Provider Value Proposition
- Grow your business with consistent bookings
- Flexible schedule - work when you want
- Guaranteed payments with automatic transfers
- Build your reputation with ratings and reviews
- Professional tools to manage your services
- Marketing and customer reach without advertising costs
- Dedicated support team

### 1.3 Design Principles
- **Efficiency-First**: Complete job management in minimal taps
- **Transparency**: Clear earnings breakdown and payment tracking
- **Professionalism**: Tools that enhance provider credibility
- **Reliability**: Stable performance even with poor connectivity
- **Support**: Help always available when needed

## 2. Registration and Onboarding

### 2.1 Provider Registration

#### 2.1.1 Welcome & Requirements
- Onboarding slides explaining platform benefits
- Requirements checklist:
  - Valid ID or professional license
  - Bank account details
  - Service category expertise
  - Background verification consent
- Estimated time to complete: 15-20 minutes

#### 2.1.2 Personal Information
- Full Name (as per ID)
- Phone Number (with OTP verification)
- Email Address
- Date of Birth
- Nationality
- National ID or Iqama Number (Saudi)
- Passport Number (Egypt)
- Current Address
- Profile Photo Upload

#### 2.1.3 Professional Information
- Business Name (optional, for companies)
- Service Categories (multi-select)
- Years of Experience
- Brief Description (300 characters)
- Service Areas (select regions/cities)
- Working Days and Hours
- Languages Spoken

#### 2.1.4 Document Upload
- Identity Document (Front & Back)
- Professional License (if applicable)
- Certifications (optional, multiple files)
- Portfolio Images (previous work, up to 10)
- Document quality requirements:
  - Clear, well-lit photos
  - All text must be readable
  - Accepted formats: JPG, PNG, PDF
  - Max file size: 10MB per document

#### 2.1.5 Banking Information
- Bank Name
- Account Holder Name
- IBAN (Saudi Arabia)
- Account Number (Egypt)
- Bank Branch Code
- Bank Statement Upload (for verification)
- Secure encryption notice displayed

#### 2.1.6 Background Check Consent
- Background check information and purpose
- Consent checkbox (required)
- Processing time: 3-5 business days
- Privacy policy link

#### 2.1.7 Terms & Conditions
- Provider Terms of Service
- Commission structure disclosure
- Cancellation policies
- Quality standards agreement
- Acceptance checkbox and signature

### 2.2 Verification Process
- Application submitted confirmation screen
- Status tracking dashboard
- Progress indicators:
  - Documents under review
  - Background check in progress
  - Final approval pending
- Email/SMS notifications at each stage
- Estimated completion time
- Contact support option

### 2.3 Profile Setup (Post-Approval)
- Approval celebration screen
- Complete profile setup wizard:
  - Set service pricing
  - Define availability schedule
  - Upload additional portfolio images
  - Set service radius/areas
- Onboarding tutorial (interactive)
- Go Live button

## 3. Dashboard and Home Screen

### 3.1 Dashboard Overview

#### 3.1.1 Today's Summary Cards
- Today's Bookings (count)
- Earnings Today (amount)
- Pending Approvals (count)
- Current Rating (stars)

#### 3.1.2 Quick Stats
- This Week Earnings
- This Month Earnings
- Total Jobs Completed
- Acceptance Rate (%)
- Customer Satisfaction Score

#### 3.1.3 Status Toggle
- Prominent Online/Offline toggle switch
- Status indicator (green = online, gray = offline)
- Automatic notifications when offline
- Schedule online hours (optional)

#### 3.1.4 Upcoming Jobs Section
- Next 3 scheduled bookings
- Each job card shows:
  - Customer name and photo
  - Service type
  - Time and date
  - Location (distance)
  - Earnings amount
- Quick actions: Navigate, Call Customer, View Details

#### 3.1.5 Earnings Chart
- 7-day earnings trend line graph
- Comparison with previous week
- Tap to view detailed earnings

#### 3.1.6 Quick Actions
- View All Bookings
- Earnings & Payouts
- My Schedule
- Update Availability

### 3.2 Bottom Navigation
- Home (Dashboard)
- Jobs (with pending count badge)
- Earnings
- Messages (with unread badge)
- Profile

## 4. Job Management

### 4.1 New Job Requests

#### 4.1.1 Job Request Notification
- Push notification with sound/vibration
- Brief job details in notification
- Tap to view full details
- Auto-expire after configured time (e.g., 10 minutes)

#### 4.1.2 Job Request Details
**Service Information:**
- Service name and category
- Service description
- Estimated duration

**Customer Information:**
- Name
- Photo
- Rating (if repeat customer)
- Phone number (masked)

**Location Details:**
- Full address
- Map preview with pin
- Distance from current location
- Estimated travel time

**Schedule:**
- Preferred date and time
- Flexibility indicator

**Earnings:**
- Total amount
- Platform commission
- Your earnings (bold, prominent)

**Additional:**
- Special Instructions (if any)
- Time remaining to respond (countdown timer)

#### 4.1.3 Accept/Decline Actions
- Large 'Accept Job' button (green)
- 'Decline' button (with reason selection)
- Decline reasons:
  - Too far
  - Schedule conflict
  - Outside service area
  - Not enough details
  - Other (text field)
- Impact on acceptance rate displayed
- Confirmation after action

### 4.2 Active Jobs

#### 4.2.1 Job List View
- Tabs: Pending, Today, Upcoming, Completed
- Each job card displays:
  - Booking ID
  - Customer info
  - Service type
  - Date and time
  - Location (brief)
  - Status badge
  - Earnings amount
- Search and filter options
- Sort by: Date, Earnings, Distance

#### 4.2.2 Job Detail View
- Complete booking information
- Status timeline with progress
- Customer contact options (Call, Message)
- Navigation to location
- Special instructions highlighted
- Earnings breakdown
- Action buttons based on status

### 4.3 Job Execution Flow

#### 4.3.1 On the Way
- 'Start Journey' button
- Live location sharing enabled
- Turn-by-turn navigation
- ETA displayed to customer
- Quick call customer button
- 'I have arrived' button when near

#### 4.3.2 Arrived at Location
- Confirm arrival
- Notify customer automatically
- Timer starts for job duration tracking
- 'Start Service' button

#### 4.3.3 Service in Progress
- Running timer display
- Add notes (optional)
- Take before photos (optional but recommended)
- Report issues button (if problems arise)
- Request additional items/materials
- 'Complete Service' button

#### 4.3.4 Service Completion
- Take after photos (required)
- Add completion notes
- Additional charges (if any):
  - Extra materials used
  - Additional time
  - Other charges with reason
- Final amount calculation
- Customer signature/confirmation
- 'Mark as Complete' button
- Earnings added to balance notification

### 4.4 Job Issues & Cancellation
**Report Issue:**
- Customer not available
- Wrong address
- Unsafe working conditions
- Customer behavior issue
- Equipment/materials not available
- Other (with description)

**Actions:**
- Upload evidence (photos)
- Contact support for mediation
- Cancel Job (with compensation policy)
- Resolution tracking

## 5. Earnings and Payments

### 5.1 Earnings Dashboard
- Total Earnings (lifetime)
- Available Balance (ready for withdrawal)
- Pending Earnings (jobs completed, payment processing)
- This Week Earnings
- This Month Earnings
- Last Payout (date and amount)
- Next Payout Date

### 5.2 Earnings Breakdown
- Interactive chart (weekly/monthly/yearly view)
- Earnings by service category
- Peak earning days and times
- Average earnings per job
- Tips received

### 5.3 Transaction History
- Chronological transaction list
- Each transaction shows:
  - Transaction ID
  - Job/Booking reference
  - Customer name
  - Service provided
  - Gross amount
  - Commission deducted
  - Net earnings
  - Date
  - Status (Completed, Pending, Paid out)
- Filter by date range, status, service type
- Export statements (PDF/Excel)
- Tax summary view

### 5.4 Payout Management

#### 5.4.1 Automatic Payouts
- Payout schedule display (e.g., Weekly every Monday)
- Minimum payout threshold
- Next payout calculation
- Holding period explanation (e.g., 7 days)
- Bank account verification status

#### 5.4.2 Instant Payout (if available)
- Request instant payout for available balance
- Small fee disclosure (e.g., 2% or fixed amount)
- Expected arrival time (within hours)
- Daily/Weekly limit displayed

#### 5.4.3 Payout History
- List of all payouts
- Each payout shows:
  - Payout ID
  - Amount
  - Bank account (masked)
  - Initiation date
  - Completion date
  - Status (Processing, Completed, Failed)
- Download payout receipt
- Contact support for issues

### 5.5 Banking Settings
- View saved bank account
- Update bank account details
- Verification status indicator
- Re-verify bank account
- Security notice for changes

## 6. Schedule and Availability

### 6.1 Calendar View
- Monthly calendar with job indicators
- Color-coded dots for different job statuses
- Tap date to view day schedule
- Toggle between month/week/day views
- Today button for quick navigation

### 6.2 Availability Management
- Weekly schedule template
- Set working hours for each day
- Add breaks/unavailable periods
- Mark days off
- Add vacation/time off
- Quick repeat for regular schedule
- Override for specific dates

### 6.3 Service Areas
- Map view with service radius
- Adjust radius (slider)
- Select specific neighborhoods/districts
- Multiple service areas support
- Travel charge settings by distance

## 7. Profile and Settings

### 7.1 Professional Profile
- Profile photo
- Verified badge display
- Business name (if applicable)
- Bio/Description (editable)
- Years of experience
- Service categories
- Languages spoken
- Certifications and licenses
- Portfolio gallery (before/after photos)

### 7.2 Performance Metrics
- Overall Rating (stars)
- Total Jobs Completed
- Acceptance Rate
- Completion Rate
- Cancellation Rate
- Average Response Time
- Customer Satisfaction Score
- On-Time Percentage

### 7.3 Reviews & Ratings
- All customer reviews
- Rating breakdown (5★ to 1★)
- Filter: Recent, Highest, Lowest, With photos
- Respond to reviews
- Report inappropriate reviews

### 7.4 Services & Pricing
- List of services offered
- Add new services
- Edit service pricing
- Set hourly vs fixed rates
- Additional charges configuration
- Service descriptions
- Enable/disable specific services

### 7.5 App Settings
- Language preference
- Notification settings:
  - New job requests
  - Customer messages
  - Payment confirmations
  - Schedule reminders
  - Marketing communications
- Sound and vibration preferences
- Location services

### 7.6 Help & Support
- Provider Help Center (FAQ)
- Contact Support (Chat)
- Call Support
- Video Tutorials
- Community Forum
- Report a Problem

### 7.7 Legal & Documents
- Provider Agreement
- Privacy Policy
- Terms of Service
- Tax Information
- Commission Structure

---

**End of Document**
