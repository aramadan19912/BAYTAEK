using HomeService.Domain.Enums;

namespace HomeService.Application.DTOs.Admin;

public class AdminTransactionDto
{
    public Guid Id { get; set; }
    public string TransactionNumber { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }

    // Booking Information
    public Guid BookingId { get; set; }
    public string BookingNumber { get; set; } = string.Empty;

    // Customer Information
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;

    // Provider Information
    public Guid? ProviderId { get; set; }
    public string? ProviderName { get; set; }
    public string? ProviderEmail { get; set; }

    // Service Information
    public string ServiceName { get; set; } = string.Empty;

    // Payment Details
    public decimal Amount { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public PaymentStatus Status { get; set; }
    public string? TransactionId { get; set; }
    public string? GatewayResponse { get; set; }

    // Financial Breakdown
    public decimal ServicePrice { get; set; }
    public decimal VatAmount { get; set; }
    public decimal PlatformCommission { get; set; }
    public decimal ProviderEarnings { get; set; }

    // Regional Information
    public Region Region { get; set; }
}
