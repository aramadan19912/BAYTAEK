using HomeService.Domain.Common;

namespace HomeService.Domain.Entities;

public class NotificationSettings : BaseEntity
{
    public Guid UserId { get; set; }

    // Communication Channels
    public bool EmailNotifications { get; set; } = true;
    public bool SmsNotifications { get; set; } = true;
    public bool PushNotifications { get; set; } = true;

    // Notification Types
    public bool BookingUpdates { get; set; } = true;
    public bool PaymentUpdates { get; set; } = true;
    public bool Messages { get; set; } = true;
    public bool Reminders { get; set; } = true;
    public bool Reviews { get; set; } = true;
    public bool Promotions { get; set; } = false;
    public bool Newsletter { get; set; } = false;

    // Provider-specific
    public bool NewBookingRequests { get; set; } = true;
    public bool BookingCancellations { get; set; } = true;
    public bool PayoutNotifications { get; set; } = true;

    // Navigation properties
    public virtual User User { get; set; } = null!;
}
