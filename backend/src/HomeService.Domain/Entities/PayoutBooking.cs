using HomeService.Domain.Common;

namespace HomeService.Domain.Entities;

public class PayoutBooking : BaseEntity
{
    public Guid PayoutId { get; set; }
    public Guid BookingId { get; set; }
    public decimal BookingAmount { get; set; }
    public decimal Commission { get; set; }
    public decimal NetAmount { get; set; }

    // Navigation properties
    public virtual Payout Payout { get; set; } = null!;
    public virtual Booking Booking { get; set; } = null!;
}
