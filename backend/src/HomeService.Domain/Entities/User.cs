using HomeService.Domain.Common;
using HomeService.Domain.Enums;

namespace HomeService.Domain.Entities;

public class User : AuditableEntity
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public bool IsEmailVerified { get; set; }
    public bool IsPhoneVerified { get; set; }
    public bool IsTwoFactorEnabled { get; set; }
    public Language PreferredLanguage { get; set; } = Language.Arabic;
    public Region Region { get; set; }
    public string? ProfileImageUrl { get; set; }
    public DateTime? LastLoginAt { get; set; }

    // Navigation properties
    public virtual ICollection<Address> Addresses { get; set; } = new List<Address>();
    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    public virtual ServiceProvider? ServiceProvider { get; set; }
}
