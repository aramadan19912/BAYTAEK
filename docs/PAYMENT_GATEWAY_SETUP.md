# Payment Gateway Setup Guide

## Stripe Integration - COMPLETED ✅

### What Was Implemented

1. **Backend Services**
   - `StripePaymentService` - Complete Stripe integration
   - Payment processing with card tokens
   - Payment Intent creation for 3D Secure
   - Refund processing
   - Customer management
   - Payment method management
   - Webhook handling

2. **API Endpoints** (`/api/v1/payments`)
   - `POST /create-intent` - Create payment intent
   - `POST /process` - Process payment
   - `GET /{paymentId}/verify` - Verify payment status
   - `POST /{paymentId}/refund` - Process refund (Admin only)
   - `GET /history` - Get payment history
   - `POST /webhook/stripe` - Stripe webhook handler

3. **DTOs**
   - `CreatePaymentDto` - Payment creation
   - `PaymentResponseDto` - Payment response
   - `ProcessRefundDto` - Refund processing
   - `RefundResponseDto` - Refund response
   - `PaymentIntentDto` - Payment intent

### Required Setup Steps

#### 1. Install Stripe NuGet Package
```bash
cd backend/src/HomeService.Infrastructure
dotnet add package Stripe.net
```

#### 2. Get Stripe API Keys
1. Go to https://dashboard.stripe.com/apikeys
2. Copy your **Publishable Key** (starts with `pk_test_`)
3. Copy your **Secret Key** (starts with `sk_test_`)

#### 3. Update appsettings.json
Replace the placeholder values:
```json
"Stripe": {
  "PublishableKey": "pk_test_YOUR_ACTUAL_KEY_HERE",
  "SecretKey": "sk_test_YOUR_ACTUAL_KEY_HERE",
  "WebhookSecret": "whsec_YOUR_WEBHOOK_SECRET_HERE"
}
```

#### 4. Setup Stripe Webhooks (Optional but Recommended)
1. Go to https://dashboard.stripe.com/webhooks
2. Click "Add endpoint"
3. Enter your webhook URL: `https://your-domain.com/api/v1/payments/webhook/stripe`
4. Select events to listen to:
   - `payment_intent.succeeded`
   - `payment_intent.payment_failed`
   - `charge.refunded`
5. Copy the **Webhook Secret** and add to appsettings.json

### Testing

#### Test Cards (Stripe Test Mode)
- **Success**: `4242 4242 4242 4242`
- **Decline**: `4000 0000 0000 0002`
- **3D Secure**: `4000 0027 6000 3184`

Any future expiry date and any 3-digit CVC.

### Frontend Integration

#### Install Stripe.js
```bash
cd frontend
npm install @stripe/stripe-js
```

#### Example Usage
```typescript
import { loadStripe } from '@stripe/stripe-js';

// Initialize Stripe
const stripe = await loadStripe('pk_test_YOUR_PUBLISHABLE_KEY');

// Create payment intent
const response = await this.http.post('/api/v1/payments/create-intent', {
  bookingId: 'xxx',
  amount: 299.00,
  currency: 'SAR',
  paymentMethod: 'CreditCard',
  customerEmail: 'user@example.com',
  customerName: 'John Doe'
}).toPromise();

// Confirm payment
const { error } = await stripe.confirmCardPayment(
  response.data.clientSecret,
  {
    payment_method: {
      card: cardElement,
      billing_details: { name: 'John Doe' }
    }
  }
);
```

### Features Supported

✅ Credit card payments
✅ Payment intents (3D Secure ready)
✅ Refunds
✅ Customer management
✅ Payment method storage
✅ Webhook handling
✅ Multi-currency (SAR, EGP, USD)
✅ Transaction tracking

### Security

- API keys stored in configuration (use Azure Key Vault in production)
- Webhook signature verification
- PCI DSS compliance through Stripe
- No card details stored on server
- Secure tokenization

### Next Steps

1. Add frontend payment form
2. Implement payment method management UI
3. Add invoice generation
4. Setup additional payment gateways (Paytabs, Fawry)
5. Implement provider payout system

---

## Paytabs Integration (Saudi Arabia) - TODO

### Required for
- Saudi Arabia region primary gateway
- Apple Pay support
- MADA card support

### Setup Steps
1. Register at https://paytabs.com
2. Get merchant credentials
3. Implement Paytabs service
4. Add to DependencyInjection

---

## Fawry Integration (Egypt) - TODO

### Required for
- Egypt region cash payments
- Mobile wallet integration
- Bank transfer support

### Setup Steps
1. Register at https://fawry.com
2. Get merchant code and security key
3. Implement Fawry service
4. Add to DependencyInjection

---

**Status**: Stripe integration complete and ready for testing!
**Time Spent**: 2 hours
**Next Priority**: OTP Verification System
