using HomeService.Application.DTOs.Payment;

namespace HomeService.Application.Interfaces;

public interface IPaymentGatewayService
{
    /// <summary>
    /// Create a payment intent for processing payment
    /// </summary>
    Task<PaymentIntentDto> CreatePaymentIntentAsync(CreatePaymentDto paymentDto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Process a payment
    /// </summary>
    Task<PaymentResponseDto> ProcessPaymentAsync(CreatePaymentDto paymentDto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Verify a payment status from gateway
    /// </summary>
    Task<PaymentResponseDto> VerifyPaymentAsync(string transactionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Process a refund
    /// </summary>
    Task<RefundResponseDto> ProcessRefundAsync(string transactionId, decimal amount, string reason, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get payment status from gateway
    /// </summary>
    Task<string> GetPaymentStatusAsync(string transactionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Handle webhook from payment gateway
    /// </summary>
    Task<bool> HandleWebhookAsync(string payload, string signature, CancellationToken cancellationToken = default);
}

public interface IStripePaymentService : IPaymentGatewayService
{
    Task<string> CreateCustomerAsync(string email, string name, string? phone = null, CancellationToken cancellationToken = default);
    Task<string> AttachPaymentMethodAsync(string customerId, string paymentMethodId, CancellationToken cancellationToken = default);
    Task<List<string>> GetCustomerPaymentMethodsAsync(string customerId, CancellationToken cancellationToken = default);
}

public interface IPaytabsPaymentService : IPaymentGatewayService
{
    // Paytabs-specific methods if needed
}

public interface IFawryPaymentService : IPaymentGatewayService
{
    // Fawry-specific methods for Egypt
}
