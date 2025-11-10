using HomeService.Domain.Common;

namespace HomeService.Domain.Entities;

public class PromoCodeUsage : BaseEntity
{
    public Guid PromoCodeId { get; set; }
    public Guid UserId { get; set; }
    public Guid BookingId { get; set; }
    public decimal DiscountAmount { get; set; }
    public DateTime UsedAt { get; set; }

    // Navigation properties
    public virtual PromoCode PromoCode { get; set; } = null!;
    public virtual User User { get; set; } = null!;
    public virtual Booking Booking { get; set; } = null!;
}
