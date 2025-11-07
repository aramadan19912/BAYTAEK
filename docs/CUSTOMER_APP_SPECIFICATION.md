# Customer Mobile App & Web Portal - Feature Specifications

**Home Service Application - Customer Experience**

| Item | Details |
|------|---------|
| Document Version | 1.0 |
| Date | November 2025 |
| Platforms | iOS, Android, Web (Angular 18) |
| Markets | Saudi Arabia, Egypt |
| Languages | Arabic, English |

## 1. Introduction

### 1.1 Purpose
This document defines the complete feature set and user experience for the Customer-facing mobile applications (iOS & Android) and web portal. It covers all customer journeys from registration through service booking, payment, and post-service interactions.

### 1.2 Customer Value Proposition
- Easy access to trusted, verified home service professionals
- Book services in minutes with transparent pricing
- Real-time tracking and communication with providers
- Secure payments with multiple payment options
- Quality assurance through ratings and reviews
- 24/7 customer support in Arabic and English

### 1.3 Design Principles
- **User-First Design**: Intuitive, clean interface requiring minimal learning
- **Speed & Efficiency**: Complete booking in under 2 minutes
- **Trust & Transparency**: Clear pricing, verified providers, honest reviews
- **Localization**: Culturally appropriate for Saudi Arabia and Egypt
- **Accessibility**: WCAG 2.1 Level AA compliance

## 2. Onboarding and Authentication

### 2.1 App Launch Experience

#### 2.1.1 Splash Screen
- Animated logo with brand colors
- Loading indicator
- Duration: 2-3 seconds maximum

#### 2.1.2 Welcome Screens (First-time users)
- 3-4 swipeable screens showcasing key features
- Visual illustrations with short, compelling copy
- Language selection (Arabic/English) on first screen
- Skip option available
- Call-to-action: Sign Up or Log In

### 2.2 Registration

#### 2.2.1 Registration Methods
- Phone Number (Primary method with OTP verification)
- Email Address (with verification link)
- Social Media Login:
  - Google Account
  - Facebook Account
  - Apple ID (iOS only)

#### 2.2.2 Registration Form Fields
- Full Name (Required)
- Phone Number (Required, auto-detect country code)
- Email Address (Optional but recommended)
- Password (8+ characters, with strength indicator)
- Terms of Service acceptance checkbox
- Privacy Policy acceptance checkbox

#### 2.2.3 Phone Verification
- 6-digit OTP sent via SMS
- Auto-read SMS permission (Android)
- Resend OTP after 60 seconds
- Alternative: Call me with code
- OTP expires in 10 minutes

### 2.3 Login
- Phone Number or Email + Password
- Social media quick login
- Remember Me option
- Biometric authentication (Fingerprint/Face ID)
- Forgot Password link
- Auto-logout after 30 days of inactivity (configurable)

### 2.4 Password Recovery
- Enter registered phone or email
- Receive reset code via SMS/Email
- Verify code and create new password
- Immediate notification of password change

## 3. Home Screen

### 3.1 Home Screen Layout

#### 3.1.1 Header Section
- Location Display (Current address or city)
- Change Location button
- Notification Bell with badge count
- Profile Avatar (tap to access menu)

#### 3.1.2 Search Bar
- Prominent search box at top
- Placeholder: 'What service do you need?' (in selected language)
- Voice search icon
- Auto-suggestions as user types
- Recent searches (last 5)

#### 3.1.3 Banner/Carousel Section
- Auto-rotating promotional banners
- Featured services or seasonal offers
- Swipeable with page indicators
- Tap to view offer details

#### 3.1.4 Service Categories
- Grid layout with category cards
- Icon + Category Name
- Categories:
  - Cleaning Services
  - Plumbing
  - Electrical
  - Carpentry
  - Appliance Repair
  - Painting
  - Pest Control
  - AC Maintenance
  - Moving & Packing
  - View All Categories
- Horizontal scrollable on mobile

#### 3.1.5 Featured Services Section
- Most Popular Services
- Service card showing:
  - Service image
  - Service name
  - Starting price
  - Average rating (stars)
  - Number of reviews
- Horizontal scrollable list

#### 3.1.6 Recent Bookings (for returning users)
- Quick access to last 3 bookings
- Status indicator (Upcoming, Completed)
- 'Book Again' quick action button

#### 3.1.7 Bottom Navigation Bar
- Home (Icon + Label)
- Bookings (Icon + Label + Badge for active)
- Messages (Icon + Label + Unread badge)
- Profile (Icon + Label)

## 4. Service Discovery and Booking

### 4.1 Browse Services

#### 4.1.1 Category View
- List of services within selected category
- Filter options:
  - Price Range (slider)
  - Rating (4+ stars, 3+ stars, etc.)
  - Availability (Today, This Week, Specific Date)
  - Distance from location
- Sort by:
  - Recommended
  - Price: Low to High
  - Price: High to Low
  - Highest Rated
  - Most Booked

#### 4.1.2 Service Detail Page
- Service Photo Gallery (swipeable, pinch to zoom)
- Service Name and Description
- Pricing Information:
  - Base Price (clearly displayed)
  - Hourly Rate vs Fixed Rate indicator
  - Additional charges breakdown
- Duration Estimate
- What's Included (checkmarks)
- Materials Required (if any)
- Warranty/Guarantee Information
- FAQs (Expandable accordion)
- Customer Reviews Section
- Related Services
- Floating 'Book Now' button at bottom
- Share service (WhatsApp, SMS, etc.)

### 4.2 Booking Flow

#### 4.2.1 Step 1: Service Selection
- Selected service summary
- Quantity selector (if applicable)
- Additional options/add-ons
- Estimated total price

#### 4.2.2 Step 2: Location & Schedule
- Service Location:
  - Select from saved addresses
  - Add new address (with map picker)
  - Use current location
- Address Details:
  - Building/Apartment number
  - Floor number
  - Delivery instructions
- Date & Time Selection:
  - Calendar view (upcoming 30 days)
  - Time slots (disabled slots grayed out)
  - 'As Soon As Possible' option
  - Recurring service option (weekly, monthly)

#### 4.2.3 Step 3: Provider Selection
- Available providers list based on filters
- Each provider card shows:
  - Profile photo
  - Name and 'Verified' badge
  - Rating (stars) and number of reviews
  - Years of experience
  - Jobs completed
  - Distance from location
  - Price (if different from base)
- View full provider profile button
- 'Auto-assign Best Match' option
- Select provider and continue

#### 4.2.4 Step 4: Review & Additional Details
- Booking Summary:
  - Service details
  - Location
  - Date and time
  - Provider info
- Special Instructions (text area)
- Promo Code Entry
- Price Breakdown:
  - Service cost
  - Additional charges
  - Discount (if any)
  - VAT (15% SA / 14% EG)
  - Total Amount (bold, prominent)

#### 4.2.5 Step 5: Payment
- Payment Method Selection:
  - Credit/Debit Card (Saved cards + Add new)
  - Apple Pay / Google Pay
  - Mada (Saudi Arabia)
  - STC Pay (Saudi Arabia)
  - Fawry (Egypt)
  - Cash on Service Completion
- Save card for future use checkbox
- Secure payment badge (PCI DSS compliant)
- Cancellation policy link
- 'Confirm & Pay' button

#### 4.2.6 Booking Confirmation
- Success animation (checkmark)
- Booking Reference Number (large, copyable)
- Booking summary card
- Provider contact info (Call/Message buttons)
- Add to Calendar button
- Receipt/Invoice download
- Track booking status
- Action buttons: View Details, Go to Home

## 5. Bookings Management

### 5.1 My Bookings Screen

#### 5.1.1 Booking Tabs
- Upcoming (Active bookings)
- In Progress (Provider on the way or working)
- Completed (Past bookings)
- Cancelled

#### 5.1.2 Booking Card Information
- Booking ID
- Service icon and name
- Provider photo, name, and rating
- Date and time
- Status badge (color-coded)
- Total amount
- Quick actions based on status

### 5.2 Booking Detail View
- Complete booking information
- Status timeline/progress indicator
- Provider Information:
  - Photo and full name
  - Rating and reviews
  - Call and Message buttons
- Service Details
- Location with map
- Special instructions
- Payment Information
- Invoice/Receipt download
- Actions: Reschedule, Cancel, Rate, Book Again

### 5.3 Live Tracking (During Service)
- Real-time map showing provider location
- Estimated arrival time
- Provider route visualization
- Status updates:
  - Provider on the way
  - Provider arrived
  - Service in progress
  - Service completed
- Quick call/message provider
- Share live location with family/friends

### 5.4 Reschedule Booking
- Available only for upcoming bookings
- Select new date and time
- Check provider availability
- Free reschedule policy (24 hours before)
- Confirmation and notification sent

### 5.5 Cancel Booking
- Cancellation policy displayed
- Refund calculation shown
- Cancellation reason selection (required):
  - Change of plans
  - Found another provider
  - Service no longer needed
  - Provider delayed
  - Other (text field)
- Confirmation dialog with warning
- Refund processing timeline notification

## 6. Reviews and Ratings

### 6.1 Rate Service Experience
- Prompt appears after service completion
- Overall Rating (1-5 stars, required)
- Detailed Ratings (optional):
  - Service Quality
  - Professionalism
  - Punctuality
  - Value for Money
- Written Review (optional, character limit: 500)
- Upload Photos (optional, up to 5)
- Quick Tags (one-tap options):
  - Great Service
  - Highly Professional
  - On Time
  - Good Value
  - Will Book Again
- Tip Option (add tip for excellent service)
- Submit button (review posted immediately)

### 6.2 View Reviews
- On provider profile and service pages
- Overall rating summary
- Rating distribution chart (5★, 4★, 3★, 2★, 1★)
- Filter reviews:
  - All
  - 5 stars only
  - With photos
  - Most helpful
- Individual review display:
  - Customer name (partial for privacy)
  - Rating stars
  - Review text
  - Review photos
  - Date posted
  - Verified booking badge
  - Provider response (if any)
  - Helpful button (upvote)

## 7. Messages and Communication

### 7.1 Messaging Interface
- Conversation list showing all chats
- Each conversation shows:
  - Provider photo and name
  - Last message preview
  - Timestamp
  - Unread badge count
  - Booking reference (subtle)
- Search conversations

### 7.2 Chat Features
- Text messaging with emoji support
- Photo/Image sharing
- Voice messages (record and send)
- Location sharing
- Message status indicators:
  - Sent (single checkmark)
  - Delivered (double checkmark)
  - Read (blue double checkmark)
- Typing indicator
- Quick reply suggestions (AI-powered)
- Message timestamps
- Archive conversation

### 7.3 Call Provider
- In-app calling or system dialer
- Call privacy (masked numbers option)
- Call history tracking

### 7.4 Support Chat
- AI Chatbot (Semantic Kernel powered)
- Natural language understanding (Arabic/English)
- Common queries quick responses
- Escalate to human agent option
- Attach booking reference automatically
- Satisfaction rating after chat

## 8. Profile and Settings

### 8.1 My Profile
- Profile Photo (upload/change/remove)
- Personal Information:
  - Full Name
  - Email Address
  - Phone Number (verified)
  - Date of Birth
  - Gender
- Edit profile button
- Account statistics (Total bookings, Money saved, etc.)

### 8.2 Saved Addresses
- List of saved addresses with labels (Home, Work, Other)
- Add new address button
- Edit/Delete existing addresses
- Set default address
- Map preview for each address

### 8.3 Payment Methods
- List of saved payment methods
- Each card shows:
  - Card brand logo
  - Masked card number (••••1234)
  - Expiry date
  - Default badge
- Add new payment method
- Set as default
- Remove payment method

### 8.4 Transaction History
- Chronological list of all transactions
- Filter by:
  - Date range
  - Transaction type (Payment, Refund)
  - Status
- Each transaction shows:
  - Transaction ID
  - Service name
  - Amount
  - Payment method
  - Date
  - Status
- Download receipt/invoice
- View related booking

### 8.5 Notifications Settings
- Push Notifications toggle
- Granular controls:
  - Booking updates
  - Provider messages
  - Offers and promotions
  - Payment confirmations
  - Review reminders
- Email Notifications toggle
- SMS Notifications toggle

### 8.6 App Settings
- Language (Arabic/English)
- Region (Saudi Arabia/Egypt)
- Currency display
- Dark mode toggle (if supported)
- Clear cache

### 8.7 Help & Support
- FAQ Section (searchable)
- Contact Support (opens chat)
- Call Support (phone number)
- Submit Feedback
- Report a Problem

### 8.8 Legal & About
- Terms of Service
- Privacy Policy
- About Us
- App Version
- Rate the App (link to store)

### 8.9 Account Management
- Change Password
- Enable Two-Factor Authentication
- Connected Social Accounts
- Active Sessions (view and revoke)
- Download My Data
- Deactivate Account
- Delete Account (with confirmation)

## 9. Additional Features

### 9.1 Referral Program
- Unique referral code for each user
- Share via WhatsApp, SMS, Email, Social Media
- Referral tracking dashboard
- Rewards earned display
- Terms and conditions

### 9.2 Offers & Promotions
- Dedicated Offers section
- Active promotions list with expiry
- Promo code entry at checkout
- First booking discount
- Seasonal offers
- Loyalty rewards

### 9.3 Favorites/Wishlist
- Save favorite services
- Save preferred providers
- Quick book from favorites
- Manage favorites list

### 9.4 AI Assistant (Semantic Kernel)
- Smart service recommendations based on history
- Predict next service needs
- Natural language booking assistance
- Voice commands (optional)
- Contextual help and suggestions

### 9.5 Accessibility Features
- Screen reader support
- Voice navigation
- Adjustable font sizes
- High contrast mode
- Color blind friendly design
- Haptic feedback

## 10. UI/UX Design Guidelines

### 10.1 Visual Design
- Modern, clean interface with card-based layouts
- Primary Brand Colors: Blue gradient (#0070C0 to #00B0F0)
- Accent Colors for actions and CTAs
- Consistent iconography (Material Design or SF Symbols)
- High-quality images and illustrations
- Generous white space for readability

### 10.2 Typography
- Primary Font: System fonts (San Francisco for iOS, Roboto for Android)
- Arabic Font: Optimized Arabic typeface with good readability
- Clear hierarchy: Headers (28-32pt), Body (16pt), Captions (14pt)
- Line height: 1.5 for body text

### 10.3 Interactions
- Smooth animations and transitions (300ms standard)
- Haptic feedback for key actions
- Loading states with skeleton screens
- Pull-to-refresh on list screens
- Swipe gestures for common actions
- Confirmation dialogs for destructive actions

### 10.4 RTL Support
- Complete layout flip for Arabic
- Mirror all directional UI elements
- Maintain visual balance in RTL mode
- Test all screens in both LTR and RTL

### 10.5 Performance
- App launch time < 2 seconds
- Screen transition time < 300ms
- Image loading optimization with lazy loading
- Offline capability for core features
- Battery optimization

---

**End of Document**
