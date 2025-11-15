using HomeService.Domain.Common;
using HomeService.Domain.Enums;

namespace HomeService.Domain.Entities;

public class Booking : AuditableEntity
{
    public Guid CustomerId { get; set; }
    public Guid ServiceId { get; set; }
    public Guid? ProviderId { get; set; }
    public Guid AddressId { get; set; }
    public BookingStatus Status { get; set; }
    public DateTime ScheduledAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? CancelledAt { get; set; }
    public string? CancellationReason { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal VatAmount { get; set; }
    public decimal VatPercentage { get; set; }
    public Currency Currency { get; set; }
    public string? SpecialInstructions { get; set; }
    public bool IsRecurring { get; set; }
    public string? RecurrencePattern { get; set; }
    public string? CompletionPhotos { get; set; }
    public Guid? PromoCodeId { get; set; }
    public decimal? DiscountAmount { get; set; }
    public bool ReminderSent { get; set; }
    public bool ReviewRequestSent { get; set; }

    // Navigation properties
    public virtual User Customer { get; set; } = null!;
    public virtual Service Service { get; set; } = null!;
    public virtual ServiceProvider? Provider { get; set; }
    public virtual Address Address { get; set; } = null!;
    public virtual Payment? Payment { get; set; }
    public virtual Review? Review { get; set; }
    public virtual PromoCode? PromoCode { get; set; }
    public virtual Conversation? Conversation { get; set; }
    public virtual ICollection<BookingHistory> History { get; set; } = new List<BookingHistory>();
    public virtual ICollection<PayoutBooking> PayoutBookings { get; set; } = new List<PayoutBooking>();
}
