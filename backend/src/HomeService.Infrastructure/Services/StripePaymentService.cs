using HomeService.Application.DTOs.Payment;
using HomeService.Application.Interfaces;
using HomeService.Domain.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Stripe;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace HomeService.Infrastructure.Services;

public class StripePaymentService : IStripePaymentService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<StripePaymentService> _logger;
    private readonly string _secretKey;
    private readonly string _webhookSecret;

    public StripePaymentService(
        IConfiguration configuration,
        ILogger<StripePaymentService> logger)
    {
        _configuration = configuration;
        _logger = logger;
        _secretKey = _configuration["Stripe:SecretKey"] ?? throw new ArgumentNullException("Stripe:SecretKey is not configured");
        _webhookSecret = _configuration["Stripe:WebhookSecret"] ?? string.Empty;

        StripeConfiguration.ApiKey = _secretKey;
    }

    public async Task<PaymentIntentDto> CreatePaymentIntentAsync(CreatePaymentDto paymentDto, CancellationToken cancellationToken = default)
    {
        try
        {
            var options = new PaymentIntentCreateOptions
            {
                Amount = (long)(paymentDto.Amount * 100), // Convert to cents
                Currency = paymentDto.Currency.ToLower(),
                PaymentMethodTypes = new List<string> { "card" },
                Metadata = new Dictionary<string, string>
                {
                    { "booking_id", paymentDto.BookingId.ToString() },
                    { "customer_email", paymentDto.CustomerEmail },
                    { "customer_name", paymentDto.CustomerName }
                },
                ReceiptEmail = paymentDto.CustomerEmail,
                Description = $"Payment for booking {paymentDto.BookingId}"
            };

            var service = new PaymentIntentService();
            var paymentIntent = await service.CreateAsync(options, cancellationToken: cancellationToken);

            _logger.LogInformation("Stripe payment intent created: {PaymentIntentId} for booking {BookingId}",
                paymentIntent.Id, paymentDto.BookingId);

            return new PaymentIntentDto
            {
                ClientSecret = paymentIntent.ClientSecret,
                Amount = paymentDto.Amount,
                Currency = paymentDto.Currency,
                BookingId = paymentDto.BookingId
            };
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Stripe error creating payment intent for booking {BookingId}", paymentDto.BookingId);
            throw new ApplicationException($"Payment gateway error: {ex.Message}", ex);
        }
    }

    public async Task<PaymentResponseDto> ProcessPaymentAsync(CreatePaymentDto paymentDto, CancellationToken cancellationToken = default)
    {
        try
        {
            // For card payments with token
            if (paymentDto.PaymentMethod == Domain.Enums.PaymentMethod.CreditCard && !string.IsNullOrEmpty(paymentDto.CardToken))
            {
                var chargeOptions = new ChargeCreateOptions
                {
                    Amount = (long)(paymentDto.Amount * 100),
                    Currency = paymentDto.Currency.ToLower(),
                    Source = paymentDto.CardToken,
                    Description = $"Payment for booking {paymentDto.BookingId}",
                    Metadata = new Dictionary<string, string>
                    {
                        { "booking_id", paymentDto.BookingId.ToString() },
                        { "customer_email", paymentDto.CustomerEmail }
                    },
                    ReceiptEmail = paymentDto.CustomerEmail
                };

                var chargeService = new ChargeService();
                var charge = await chargeService.CreateAsync(chargeOptions, cancellationToken: cancellationToken);

                _logger.LogInformation("Stripe charge created: {ChargeId} for booking {BookingId}",
                    charge.Id, paymentDto.BookingId);

                return new PaymentResponseDto
                {
                    TransactionId = charge.Id,
                    Status = MapStripeStatus(charge.Status),
                    Amount = paymentDto.Amount,
                    Currency = paymentDto.Currency,
                    PaymentMethod = paymentDto.PaymentMethod,
                    CreatedAt = DateTime.UtcNow,
                    GatewayResponse = charge.Status,
                    RequiresAction = false
                };
            }

            throw new NotImplementedException("Payment method not supported");
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Stripe error processing payment for booking {BookingId}", paymentDto.BookingId);
            return new PaymentResponseDto
            {
                Status = Domain.Enums.PaymentStatus.Failed,
                Amount = paymentDto.Amount,
                Currency = paymentDto.Currency,
                PaymentMethod = paymentDto.PaymentMethod,
                CreatedAt = DateTime.UtcNow,
                ErrorMessage = ex.Message,
                GatewayResponse = ex.StripeError?.Code
            };
        }
    }

    public async Task<PaymentResponseDto> VerifyPaymentAsync(string transactionId, CancellationToken cancellationToken = default)
    {
        try
        {
            var service = new PaymentIntentService();
            var paymentIntent = await service.GetAsync(transactionId, cancellationToken: cancellationToken);

            return new PaymentResponseDto
            {
                TransactionId = paymentIntent.Id,
                Status = MapStripePaymentIntentStatus(paymentIntent.Status),
                Amount = paymentIntent.Amount / 100m,
                Currency = paymentIntent.Currency.ToUpper(),
                CreatedAt = paymentIntent.Created,
                GatewayResponse = paymentIntent.Status
            };
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Error verifying payment {TransactionId}", transactionId);
            throw;
        }
    }

    public async Task<RefundResponseDto> ProcessRefundAsync(string transactionId, decimal amount, string reason, CancellationToken cancellationToken = default)
    {
        try
        {
            var options = new RefundCreateOptions
            {
                Charge = transactionId,
                Amount = (long)(amount * 100),
                Reason = "requested_by_customer",
                Metadata = new Dictionary<string, string>
                {
                    { "reason", reason }
                }
            };

            var service = new RefundService();
            var refund = await service.CreateAsync(options, cancellationToken: cancellationToken);

            _logger.LogInformation("Stripe refund created: {RefundId} for charge {ChargeId}",
                refund.Id, transactionId);

            return new RefundResponseDto
            {
                RefundTransactionId = refund.Id,
                Amount = amount,
                Status = refund.Status,
                ProcessedAt = DateTime.UtcNow
            };
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Error processing refund for transaction {TransactionId}", transactionId);
            return new RefundResponseDto
            {
                Amount = amount,
                Status = "failed",
                ProcessedAt = DateTime.UtcNow,
                ErrorMessage = ex.Message
            };
        }
    }

    public async Task<string> GetPaymentStatusAsync(string transactionId, CancellationToken cancellationToken = default)
    {
        try
        {
            var service = new PaymentIntentService();
            var paymentIntent = await service.GetAsync(transactionId, cancellationToken: cancellationToken);
            return paymentIntent.Status;
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Error getting payment status for {TransactionId}", transactionId);
            throw;
        }
    }

    public async Task<bool> HandleWebhookAsync(string payload, string signature, CancellationToken cancellationToken = default)
    {
        try
        {
            var stripeEvent = EventUtility.ConstructEvent(
                payload,
                signature,
                _webhookSecret
            );

            _logger.LogInformation("Processing Stripe webhook event: {EventType}", stripeEvent.Type);

            // Handle different event types
            switch (stripeEvent.Type)
            {
                case Events.PaymentIntentSucceeded:
                    var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
                    _logger.LogInformation("Payment succeeded: {PaymentIntentId}", paymentIntent?.Id);
                    // Update booking payment status
                    break;

                case Events.PaymentIntentPaymentFailed:
                    var failedPayment = stripeEvent.Data.Object as PaymentIntent;
                    _logger.LogWarning("Payment failed: {PaymentIntentId}", failedPayment?.Id);
                    // Handle failed payment
                    break;

                case Events.ChargeRefunded:
                    var refund = stripeEvent.Data.Object as Charge;
                    _logger.LogInformation("Charge refunded: {ChargeId}", refund?.Id);
                    // Handle refund
                    break;

                default:
                    _logger.LogInformation("Unhandled event type: {EventType}", stripeEvent.Type);
                    break;
            }

            return await Task.FromResult(true);
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Error processing Stripe webhook");
            return false;
        }
    }

    public async Task<string> CreateCustomerAsync(string email, string name, string? phone = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var options = new CustomerCreateOptions
            {
                Email = email,
                Name = name,
                Phone = phone
            };

            var service = new CustomerService();
            var customer = await service.CreateAsync(options, cancellationToken: cancellationToken);

            _logger.LogInformation("Stripe customer created: {CustomerId} for {Email}", customer.Id, email);

            return customer.Id;
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Error creating Stripe customer for {Email}", email);
            throw;
        }
    }

    public async Task<string> AttachPaymentMethodAsync(string customerId, string paymentMethodId, CancellationToken cancellationToken = default)
    {
        try
        {
            var options = new PaymentMethodAttachOptions
            {
                Customer = customerId
            };

            var service = new PaymentMethodService();
            var paymentMethod = await service.AttachAsync(paymentMethodId, options, cancellationToken: cancellationToken);

            _logger.LogInformation("Payment method {PaymentMethodId} attached to customer {CustomerId}",
                paymentMethodId, customerId);

            return paymentMethod.Id;
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Error attaching payment method {PaymentMethodId} to customer {CustomerId}",
                paymentMethodId, customerId);
            throw;
        }
    }

    public async Task<List<string>> GetCustomerPaymentMethodsAsync(string customerId, CancellationToken cancellationToken = default)
    {
        try
        {
            var options = new PaymentMethodListOptions
            {
                Customer = customerId,
                Type = "card"
            };

            var service = new PaymentMethodService();
            var paymentMethods = await service.ListAsync(options, cancellationToken: cancellationToken);

            return paymentMethods.Data.Select(pm => pm.Id).ToList();
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Error getting payment methods for customer {CustomerId}", customerId);
            throw;
        }
    }

    private Domain.Enums.PaymentStatus MapStripeStatus(string stripeStatus)
    {
        return stripeStatus.ToLower() switch
        {
            "succeeded" => Domain.Enums.PaymentStatus.Completed,
            "pending" => Domain.Enums.PaymentStatus.Pending,
            "failed" => Domain.Enums.PaymentStatus.Failed,
            _ => Domain.Enums.PaymentStatus.Pending
        };
    }

    private Domain.Enums.PaymentStatus MapStripePaymentIntentStatus(string status)
    {
        return status.ToLower() switch
        {
            "succeeded" => Domain.Enums.PaymentStatus.Completed,
            "processing" => Domain.Enums.PaymentStatus.Processing,
            "requires_payment_method" => Domain.Enums.PaymentStatus.Pending,
            "requires_confirmation" => Domain.Enums.PaymentStatus.Pending,
            "requires_action" => Domain.Enums.PaymentStatus.Pending,
            "canceled" => Domain.Enums.PaymentStatus.Failed,
            _ => Domain.Enums.PaymentStatus.Pending
        };
    }
}
