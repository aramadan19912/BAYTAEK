using HomeService.Domain.Common;
using HomeService.Domain.Enums;

namespace HomeService.Domain.Entities;

public class BookingHistory : BaseEntity
{
    public Guid BookingId { get; set; }
    public BookingStatus Status { get; set; }
    public Guid? ChangedById { get; set; }
    public string? ChangeReason { get; set; }
    public string? Notes { get; set; }
    public DateTime ChangedAt { get; set; }

    // Navigation properties
    public virtual Booking Booking { get; set; } = null!;
    public virtual User? ChangedBy { get; set; }
}
