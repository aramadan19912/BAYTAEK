using HomeService.Domain.Common;
using HomeService.Domain.Enums;

namespace HomeService.Domain.Entities;

public class Payment : AuditableEntity
{
    public Guid BookingId { get; set; }
    public decimal Amount { get; set; }
    public Currency Currency { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public PaymentStatus Status { get; set; }
    public string? TransactionId { get; set; }
    public string? PaymentGateway { get; set; }
    public string? GatewayResponse { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public string? FailureReason { get; set; }
    public decimal? RefundAmount { get; set; }
    public DateTime? RefundedAt { get; set; }
    public string? RefundReason { get; set; }
    public string? InvoiceUrl { get; set; }

    // Navigation properties
    public virtual Booking Booking { get; set; } = null!;
}
