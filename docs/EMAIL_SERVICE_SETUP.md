# Email Service Setup Guide - SendGrid Integration

This guide covers the setup and usage of the SendGrid email service integration for the Home Service application.

## Overview

The email service provides automated email notifications for various user interactions including:
- Welcome emails with verification links
- Password reset emails
- Booking confirmations
- Payment receipts
- Provider approval notifications
- Job notifications for providers

## Prerequisites

1. **SendGrid Account**
   - Sign up at [SendGrid](https://sendgrid.com/)
   - Free tier includes 100 emails/day
   - Paid plans available for higher volumes

2. **Required NuGet Package**
   ```bash
   dotnet add package SendGrid
   ```

## Configuration

### 1. SendGrid API Key Setup

1. **Create API Key:**
   - Log in to SendGrid dashboard
   - Navigate to Settings → API Keys
   - Click "Create API Key"
   - Name: `HomeService-Production` (or appropriate name)
   - Permissions: Select "Full Access" or "Mail Send" at minimum
   - Copy the API key (you won't be able to see it again)

2. **Verify Sender Identity:**
   - Navigate to Settings → Sender Authentication
   - Choose one:
     - **Single Sender Verification** (Quick, for testing)
       - Add and verify your email address
     - **Domain Authentication** (Recommended for production)
       - Authenticate your domain with DNS records

### 2. Application Configuration

Update `appsettings.json`:

```json
{
  "Email": {
    "Provider": "SendGrid",
    "SendGrid": {
      "ApiKey": "SG.your-actual-api-key-here",
      "FromEmail": "noreply@homeservice.com",
      "FromName": "Home Service"
    }
  }
}
```

**Important:**
- `FromEmail` must be a verified sender or from an authenticated domain
- Never commit API keys to version control
- Use User Secrets for development, Azure Key Vault for production

### 3. User Secrets (Development)

```bash
cd backend/src/HomeService.API

# Initialize user secrets
dotnet user-secrets init

# Set SendGrid API key
dotnet user-secrets set "Email:SendGrid:ApiKey" "SG.your-api-key-here"

# Set sender email
dotnet user-secrets set "Email:SendGrid:FromEmail" "your-verified-email@domain.com"
```

### 4. Azure Key Vault (Production)

```csharp
// In Program.cs
builder.Configuration.AddAzureKeyVault(
    new Uri($"https://{keyVaultName}.vault.azure.net/"),
    new DefaultAzureCredential());
```

Store secrets:
- `Email--SendGrid--ApiKey`
- `Email--SendGrid--FromEmail`

## Usage Examples

### Basic Email

```csharp
public class YourService
{
    private readonly IEmailService _emailService;

    public YourService(IEmailService emailService)
    {
        _emailService = emailService;
    }

    public async Task SendBasicEmailAsync()
    {
        await _emailService.SendEmailAsync(
            toEmail: "customer@example.com",
            toName: "John Doe",
            subject: "Your Subject",
            body: "<h1>HTML Body</h1><p>Email content</p>"
        );
    }
}
```

### Welcome Email

```csharp
await _emailService.SendWelcomeEmailAsync(
    toEmail: user.Email,
    userName: user.FullName,
    verificationLink: $"https://app.homeservice.com/verify?token={token}"
);
```

### Password Reset

```csharp
await _emailService.SendPasswordResetEmailAsync(
    toEmail: user.Email,
    userName: user.FullName,
    resetLink: $"https://app.homeservice.com/reset-password?token={token}"
);
```

### Booking Confirmation

```csharp
var bookingDetails = new BookingEmailDetails
{
    BookingId = booking.Id.ToString(),
    ServiceName = service.Name,
    ProviderName = provider.FullName,
    ScheduledDate = booking.ScheduledDateTime,
    Location = booking.Address,
    TotalAmount = booking.TotalAmount,
    Currency = "SAR"
};

await _emailService.SendBookingConfirmationEmailAsync(
    toEmail: customer.Email,
    userName: customer.FullName,
    booking: bookingDetails
);
```

### Payment Receipt

```csharp
var receipt = new PaymentReceiptDetails
{
    TransactionId = payment.TransactionId,
    BookingId = payment.BookingId.ToString(),
    PaymentDate = payment.CreatedAt,
    Amount = payment.Amount,
    Currency = payment.Currency,
    PaymentMethod = payment.PaymentMethod.ToString(),
    ServiceName = booking.ServiceName
};

await _emailService.SendPaymentReceiptEmailAsync(
    toEmail: customer.Email,
    userName: customer.FullName,
    receipt: receipt
);
```

### Provider Approval

```csharp
// Approved
await _emailService.SendProviderApprovalEmailAsync(
    toEmail: provider.Email,
    providerName: provider.FullName,
    isApproved: true
);

// Rejected
await _emailService.SendProviderApprovalEmailAsync(
    toEmail: provider.Email,
    providerName: provider.FullName,
    isApproved: false,
    reason: "Incomplete documentation. Please resubmit with all required certificates."
);
```

### New Job Notification

```csharp
var jobDetails = new JobNotificationDetails
{
    JobId = booking.Id.ToString(),
    ServiceName = service.Name,
    CustomerName = customer.FullName,
    ScheduledDate = booking.ScheduledDateTime,
    Location = booking.Address,
    Earnings = booking.ProviderEarnings,
    Currency = "SAR"
};

await _emailService.SendNewJobNotificationEmailAsync(
    toEmail: provider.Email,
    providerName: provider.FullName,
    job: jobDetails
);
```

## SendGrid Templates (Advanced)

For more professional emails with dynamic content:

### 1. Create Template in SendGrid

1. Navigate to Email API → Dynamic Templates
2. Click "Create a Dynamic Template"
3. Name your template (e.g., "Booking Confirmation")
4. Add a version and design your email
5. Use handlebars syntax for variables: `{{userName}}`, `{{bookingId}}`
6. Copy the Template ID

### 2. Use Template in Code

```csharp
var templateData = new Dictionary<string, string>
{
    { "userName", customer.FullName },
    { "bookingId", booking.Id.ToString() },
    { "serviceName", service.Name },
    { "scheduledDate", booking.ScheduledDateTime.ToString("dddd, MMMM dd, yyyy") }
};

await _emailService.SendTemplateEmailAsync(
    toEmail: customer.Email,
    toName: customer.FullName,
    templateId: "d-1234567890abcdef",
    templateData: templateData
);
```

## Email Localization (Arabic Support)

For Arabic language support:

### Option 1: Separate Templates

Create separate SendGrid templates for Arabic and English:
- `d-welcome-en`
- `d-welcome-ar`

Select template based on user's language preference:

```csharp
var templateId = user.Language == "ar"
    ? "d-welcome-ar"
    : "d-welcome-en";

await _emailService.SendTemplateEmailAsync(
    toEmail: user.Email,
    toName: user.FullName,
    templateId: templateId,
    templateData: data
);
```

### Option 2: Custom Email Builder

Create a custom email builder service that generates HTML with proper RTL support:

```csharp
public string BuildEmailBody(string template, string language, Dictionary<string, string> data)
{
    var direction = language == "ar" ? "rtl" : "ltr";
    var font = language == "ar" ? "Tahoma, Arial" : "Arial, sans-serif";

    var html = $@"
        <!DOCTYPE html>
        <html dir='{direction}'>
        <body style='font-family: {font};'>
            {GetLocalizedContent(template, language, data)}
        </body>
        </html>
    ";

    return html;
}
```

## Testing

### Test Mode

SendGrid's test mode won't send actual emails but will validate the request:

```csharp
// In SendGridEmailService constructor
var options = new SendGridClientOptions
{
    ApiKey = apiKey,
    HttpErrorAsException = true
};

if (environment.IsDevelopment())
{
    options.ReliabilitySettings = new ReliabilitySettings(1, TimeSpan.FromSeconds(1));
}

_sendGridClient = new SendGridClient(options);
```

### Test with Real Emails

Use a test email address or email testing service:
- [Mailtrap.io](https://mailtrap.io/) - Email sandbox
- [MailHog](https://github.com/mailhog/MailHog) - Local email testing

### Unit Testing

Mock the email service in tests:

```csharp
var mockEmailService = new Mock<IEmailService>();
mockEmailService
    .Setup(x => x.SendWelcomeEmailAsync(
        It.IsAny<string>(),
        It.IsAny<string>(),
        It.IsAny<string>(),
        It.IsAny<CancellationToken>()))
    .ReturnsAsync(true);
```

## Monitoring & Analytics

### SendGrid Dashboard

Monitor email performance:
- Delivery rates
- Open rates
- Click rates
- Bounce rates
- Spam reports

### Application Logging

The email service logs all operations:

```csharp
// Success
_logger.LogInformation("Email sent successfully to {Email}", toEmail);

// Failure
_logger.LogError("Failed to send email to {Email}. Status: {StatusCode}",
    toEmail, response.StatusCode);
```

Check logs for email sending issues.

## Best Practices

1. **Rate Limiting**
   - Free tier: 100 emails/day
   - Implement queuing for bulk emails
   - Consider background jobs for non-critical emails

2. **Email Deliverability**
   - Always use authenticated domains in production
   - Implement proper SPF, DKIM, and DMARC records
   - Keep bounce rates low (<5%)
   - Honor unsubscribe requests immediately

3. **Content Guidelines**
   - Keep emails concise and mobile-friendly
   - Use responsive HTML templates
   - Include plain text alternatives
   - Add unsubscribe links for marketing emails

4. **Security**
   - Never expose API keys
   - Use HTTPS for all links
   - Validate email addresses before sending
   - Implement retry logic with exponential backoff

5. **Performance**
   - Use async methods everywhere
   - Implement caching for templates
   - Consider bulk send APIs for multiple recipients
   - Use background jobs for non-critical emails

## Troubleshooting

### Common Issues

**1. "Forbidden" Error (403)**
- Verify API key is correct
- Check API key permissions (must have Mail Send permission)
- Ensure API key is active

**2. "From email address not verified" Error**
- Complete sender verification in SendGrid
- Use only verified email addresses or authenticated domains

**3. Emails not received**
- Check spam folder
- Verify recipient email is valid
- Check SendGrid activity logs
- Review bounce/block lists

**4. High bounce rate**
- Validate email addresses before sending
- Remove invalid addresses from lists
- Implement double opt-in for subscriptions

## Regional Considerations

### Saudi Arabia & Egypt

**Time Zones:**
```csharp
var saudiTime = TimeZoneInfo.ConvertTimeFromUtc(
    DateTime.UtcNow,
    TimeZoneInfo.FindSystemTimeZoneById("Arab Standard Time")
);
```

**Currency Display:**
```csharp
var formattedAmount = booking.TotalAmount.ToString("N2") + " " + booking.Currency;
// Output: "150.00 SAR"
```

**Language Detection:**
```csharp
var language = user.PreferredLanguage ??
    (user.Country == "SA" || user.Country == "EG" ? "ar" : "en");
```

## Cost Optimization

**Free Tier (100 emails/day):**
- Suitable for early testing
- Use for critical emails only (password reset, booking confirmations)

**Essentials Plan ($19.95/month for 50k emails):**
- Suitable for production launch
- Includes basic templates
- Email API access

**Pro Plan ($89.95/month for 100k emails):**
- Dedicated IP address
- Advanced analytics
- Priority support

**Cost-Saving Tips:**
1. Consolidate non-urgent emails into digests
2. Use SMS for time-critical notifications
3. Implement user preferences for email frequency
4. Archive old marketing emails

## Support & Resources

- **SendGrid Documentation:** https://docs.sendgrid.com/
- **SendGrid API Reference:** https://docs.sendgrid.com/api-reference/
- **Status Page:** https://status.sendgrid.com/
- **Support:** https://support.sendgrid.com/

## Next Steps

After setup:
1. ✅ Configure API keys and sender verification
2. ✅ Test basic email sending
3. ⏳ Create custom templates for Arabic/English
4. ⏳ Implement email event webhooks
5. ⏳ Set up email analytics tracking
6. ⏳ Configure unsubscribe management

---

**Last Updated:** November 2025
**Version:** 1.0
