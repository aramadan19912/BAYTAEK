using HomeService.Domain.Common;
using HomeService.Domain.Enums;

namespace HomeService.Domain.Entities;

public class Payout : AuditableEntity
{
    public string PayoutNumber { get; set; } = string.Empty;
    public Guid ProviderId { get; set; }
    public decimal Amount { get; set; }
    public Currency Currency { get; set; } = Currency.SAR;
    public decimal Fee { get; set; }
    public decimal NetAmount { get; set; }
    public PayoutStatus Status { get; set; } = PayoutStatus.Pending;
    public PayoutMethod Method { get; set; }
    public string? AccountNumber { get; set; }
    public string? IBAN { get; set; }
    public string? BankName { get; set; }
    public DateTime RequestedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public Guid? ProcessedById { get; set; }
    public string? TransactionId { get; set; }
    public string? FailureReason { get; set; }
    public string? Notes { get; set; }

    // Navigation properties
    public virtual ServiceProvider Provider { get; set; } = null!;
    public virtual User? ProcessedBy { get; set; }
    public virtual ICollection<PayoutBooking> PayoutBookings { get; set; } = new List<PayoutBooking>();
}
