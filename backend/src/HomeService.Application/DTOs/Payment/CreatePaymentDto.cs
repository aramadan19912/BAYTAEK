using HomeService.Domain.Enums;

namespace HomeService.Application.DTOs.Payment;

public class CreatePaymentDto
{
    public Guid BookingId { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "SAR"; // SAR, EGP, USD

    // For card payments
    public string? CardToken { get; set; }
    public string? CardLast4 { get; set; }
    public string? CardBrand { get; set; }

    // For wallet payments
    public string? WalletId { get; set; }

    // Customer details
    public string CustomerEmail { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;

    // Billing address
    public string? BillingAddress { get; set; }
    public string? City { get; set; }
    public string? PostalCode { get; set; }
    public string? Country { get; set; }
}

public class PaymentResponseDto
{
    public Guid PaymentId { get; set; }
    public string TransactionId { get; set; } = string.Empty;
    public PaymentStatus Status { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public PaymentMethod PaymentMethod { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? ErrorMessage { get; set; }
    public string? GatewayResponse { get; set; }

    // For 3D Secure or additional authentication
    public bool RequiresAction { get; set; }
    public string? ClientSecret { get; set; }
    public string? RedirectUrl { get; set; }
}

public class ProcessRefundDto
{
    public Guid PaymentId { get; set; }
    public decimal Amount { get; set; }
    public string Reason { get; set; } = string.Empty;
    public Guid AdminUserId { get; set; }
}

public class RefundResponseDto
{
    public Guid RefundId { get; set; }
    public string RefundTransactionId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime ProcessedAt { get; set; }
    public string? ErrorMessage { get; set; }
}

public class PaymentIntentDto
{
    public string ClientSecret { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public Guid BookingId { get; set; }
}
